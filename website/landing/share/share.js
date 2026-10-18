// Perfis do DroidDeck em texto curto (link e QR): JSON comprimido com deflate em base64url,
// o mesmo formato do PC (DroidDeck/Services/ProfileShare.cs). Usado pela página de um perfil
// compartilhado e pela galeria.
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

  async function encode(profile) {
    const json = new TextEncoder().encode(JSON.stringify({ v: 1, profile }));
    return b64url(await pipe(json, new CompressionStream("deflate-raw")));
  }

  async function decode(data) {
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
    el.style.gridTemplateColumns = `repeat(${cols}, 1fr)`;
    el.style.gridTemplateRows = `repeat(${rows}, auto)`;
    el.innerHTML = buttons.map(b => {
      const icon = b.dynamicType && INFO[b.dynamicType] ? INFO[b.dynamicType] : ICONS[b.iconName] || b.iconName;
      const style = [
        `grid-column:${(b.column || 0) + 1} / span ${b.columnSpan || 1}`,
        `grid-row:${(b.row || 0) + 1} / span ${b.rowSpan || 1}`,
        b.backgroundColor ? `background:${esc(b.backgroundColor)}` : "",
      ].join(";");
      return `<div class="key" style="${style}">` +
        (icon ? `<span class="ms">${esc(icon)}</span>` : "") +
        `<span class="lbl">${esc(b.label || "")}</span></div>`;
    }).join("");
  }

  window.DDShare = { encode, decode, render, importUrl: data => `http://localhost:4787/?import=${data}` };
})();
