// Perfis do DroidDeck em texto curto (link e QR): JSON comprimido com deflate em base64url,
// o mesmo formato do PC (DroidDeck/Services/ProfileShare.cs). Usado pela página de um perfil
// compartilhado, pela galeria e pela landing.
(function () {
  const b64url = bytes => {
    let s = "";
    for (const b of bytes) s += String.fromCharCode(b);
    return btoa(s).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
  };
  const fromB64url = text => {
    const b64 = text.replace(/-/g, "+").replace(/_/g, "/");
    const bin = atob(b64 + "===".slice((b64.length + 3) % 4));
    return Uint8Array.from(bin, c => c.charCodeAt(0));
  };
  async function pipe(bytes, stream) {
    const out = new Blob([bytes]).stream().pipeThrough(stream);
    return new Uint8Array(await new Response(out).arrayBuffer());
  }

  // Navegadores antigos não têm CompressionStream: sem ele não dá para montar nem ler links.
  const supported = typeof CompressionStream === "function" && typeof DecompressionStream === "function";

  async function encode(profile) {
    if (!supported) throw new Error("unsupported");
    const json = new TextEncoder().encode(JSON.stringify({ v: 1, profile }));
    return b64url(await pipe(json, new CompressionStream("deflate-raw")));
  }

  async function decode(data) {
    if (!supported) throw new Error("unsupported");
    const json = await pipe(fromB64url(data), new DecompressionStream("deflate-raw"));
    const shared = JSON.parse(new TextDecoder().decode(json));
    if (!shared || !shared.profile) throw new Error("bad");
    return shared.profile;
  }

  // Nomes de ícone do app que não são iguais aos do Material Symbols.
  const ICONS = { play: "play_arrow", brightness: "brightness_6", lightbulb_outline: "lightbulb", web_asset: "web_asset" };
  const INFO = {
    clock: "schedule", weather: "partly_cloudy_day", now_playing: "music_note", cpu_monitor: "memory",
    gpu_monitor: "developer_board", memory_monitor: "memory_alt", network_monitor: "swap_vert",
    volume_slider: "tune", notifications: "notifications", command: "terminal", url: "link",
  };
  const esc = s => String(s ?? "").replace(/[&<>"]/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));

  // Grade de pré-visualização: botões grandes ocupam várias casas, como no app.
  function render(profile, el) {
    const buttons = profile.buttons || [];
    const cols = Math.max(5, ...buttons.map(b => (b.column || 0) + (b.columnSpan || 1)));
    const rows = Math.max(3, ...buttons.map(b => (b.row || 0) + (b.rowSpan || 1)));
    el.style.gridTemplateColumns = `repeat(${cols}, minmax(0, 1fr))`;
    el.style.gridTemplateRows = `repeat(${rows}, auto)`;
    el.setAttribute("role", "img");
    el.setAttribute("aria-label", buttons.map(b => b.label).filter(Boolean).join(", "));
    el.innerHTML = buttons.map(b => {
      const icon = b.dynamicType && INFO[b.dynamicType] ? INFO[b.dynamicType] : ICONS[b.iconName] || b.iconName;
      const style = [
        `grid-column:${(b.column || 0) + 1} / span ${b.columnSpan || 1}`,
        `grid-row:${(b.row || 0) + 1} / span ${b.rowSpan || 1}`,
        b.backgroundColor ? `background:${esc(b.backgroundColor)}` : "",
      ].join(";");
      return `<div class="key" style="${style}">` +
        (icon ? `<span class="ms" aria-hidden="true">${esc(icon)}</span>` : "") +
        `<span class="lbl">${esc(b.label || "")}</span></div>`;
    }).join("");
  }

  const STR = {
    en: {
      import: "Import into DroidDeck", send: "Send to my PC", view: "Open link", wait: "Preparing…",
      must: "DroidDeck must be open on this PC.", go: "Import now",
      retry: "Nothing happened? Open DroidDeck from the Start menu and try again.",
      phone: "Importing happens on the Windows PC. Send yourself this link and open it there.",
      copied: "Link copied. Open it on your PC to import.",
      copyFail: "Couldn't copy. Open this page on your PC to import.",
      unsupported: "This browser can't build profile links. Try a recent Chrome, Edge, Firefox or Safari.",
    },
    pt: {
      import: "Importar no DroidDeck", send: "Mandar para o meu PC", view: "Abrir link", wait: "Preparando…",
      must: "O DroidDeck precisa estar aberto neste PC.", go: "Importar agora",
      retry: "Não aconteceu nada? Abra o DroidDeck pelo menu Iniciar e tente de novo.",
      phone: "A importação acontece no PC com Windows. Mande este link para você e abra lá.",
      copied: "Link copiado. Abra no seu PC para importar.",
      copyFail: "Não deu para copiar. Abra esta página no seu PC para importar.",
      unsupported: "Este navegador não consegue montar links de perfil. Use um Chrome, Edge, Firefox ou Safari atualizado.",
    },
    es: {
      import: "Importar en DroidDeck", send: "Enviar a mi PC", view: "Abrir enlace", wait: "Preparando…",
      must: "DroidDeck debe estar abierto en este PC.", go: "Importar ahora",
      retry: "¿No pasó nada? Abre DroidDeck desde el menú Inicio y vuelve a intentarlo.",
      phone: "La importación se hace en el PC con Windows. Envíate este enlace y ábrelo allí.",
      copied: "Enlace copiado. Ábrelo en tu PC para importar.",
      copyFail: "No se pudo copiar. Abre esta página en tu PC para importar.",
      unsupported: "Este navegador no puede crear enlaces de perfil. Usa un Chrome, Edge, Firefox o Safari actualizado.",
    },
  };

  // Importar só funciona no PC com Windows onde o DroidDeck roda; no celular, mandar o link para o PC.
  const onWindows = /Windows NT/.test(navigator.userAgent);
  let panelSeq = 0;

  // Botões "Importar"/"Mandar para o meu PC" (+ "Abrir link", opcional) com o painel de aviso.
  // Ficam desativados até o link ficar pronto. `data` é uma Promise do texto do perfil.
  function mountActions(root, { lang, data, title, shareHref, viewHref, phoneNote }) {
    const t = STR[lang] || STR.en;
    const id = `imp-panel-${++panelSeq}`;
    root.innerHTML =
      `<div class="actions"><button type="button" class="btn btn-primary imp" aria-disabled="true"></button>` +
      (viewHref ? `<a class="btn btn-ghost view" aria-disabled="true"></a>` : "") + `</div>` +
      `<div class="imp-panel" id="${id}" hidden><p class="imp-must"></p><a class="btn btn-primary imp-go"></a><p class="imp-retry"></p></div>` +
      `<p class="imp-status" role="status"></p>`;
    const imp = root.querySelector(".imp"), view = root.querySelector(".view"), panel = root.querySelector(".imp-panel");
    const status = root.querySelector(".imp-status");
    imp.textContent = t.wait;
    if (view) view.textContent = t.view;
    root.querySelector(".imp-must").textContent = t.must;
    root.querySelector(".imp-go").textContent = t.go;
    root.querySelector(".imp-retry").textContent = t.retry;
    let ready = null;

    imp.addEventListener("click", async () => {
      if (!ready) return;
      if (onWindows) {
        panel.hidden = !panel.hidden;
        imp.setAttribute("aria-expanded", String(!panel.hidden));
        if (!panel.hidden) root.querySelector(".imp-go").focus();
        return;
      }
      const url = new URL(shareHref(ready), location.href).href;
      if (navigator.share) {
        try { await navigator.share({ title, url }); return; } catch (e) { if (e && e.name === "AbortError") return; }
      }
      try { await navigator.clipboard.writeText(url); status.textContent = t.copied; }
      catch (e) { status.textContent = t.copyFail; }
    });

    Promise.resolve(data).then(d => {
      ready = d;
      imp.textContent = onWindows ? t.import : t.send;
      imp.removeAttribute("aria-disabled");
      if (onWindows) imp.setAttribute("aria-controls", id), imp.setAttribute("aria-expanded", "false");
      else if (phoneNote) status.textContent = t.phone;
      root.querySelector(".imp-go").href = importUrl(d);
      if (view) { view.href = viewHref(d); view.removeAttribute("aria-disabled"); }
    }).catch(() => {
      imp.textContent = onWindows ? t.import : t.send;
      status.textContent = t.unsupported;
      status.classList.add("error");
    });
  }

  const importUrl = data => `http://localhost:4787/?import=${data}`;
  window.DDShare = { encode, decode, render, importUrl, mountActions, supported, onWindows, STR };
})();
