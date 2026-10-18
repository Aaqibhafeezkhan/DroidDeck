// Idioma do site (landing, galeria e perfil compartilhado): ?lang= > localStorage "dd-lang" > navegador.
(function () {
  const OK = ["en", "pt", "es"];

  function pick() {
    let l = null;
    try { l = new URLSearchParams(location.search).get("lang"); } catch (e) {}
    if (!OK.includes(l)) { try { l = localStorage.getItem("dd-lang"); } catch (e) {} }
    if (!OK.includes(l)) {
      const b = (navigator.language || "").toLowerCase();
      l = b.startsWith("pt") ? "pt" : b.startsWith("es") ? "es" : "en";
    }
    return l;
  }

  // A documentação existe em inglês e português; em espanhol os links vão para a versão em inglês.
  function apply(lang) {
    document.documentElement.lang = lang === "pt" ? "pt-BR" : lang;
    document.querySelectorAll("[data-docs]").forEach(a => {
      if (!a.dataset.base) a.dataset.base = a.getAttribute("href");
      a.setAttribute("href", lang === "pt" ? a.dataset.base.replace(/(^|\/)docs\//, "$1docs/pt/") : a.dataset.base);
    });
    document.querySelectorAll(".lang button").forEach(b => b.setAttribute("aria-pressed", String(b.dataset.lang === lang)));
    try { localStorage.setItem("dd-lang", lang); } catch (e) {}
    try {
      const u = new URL(location.href);
      if (u.searchParams.has("lang")) { u.searchParams.set("lang", lang); history.replaceState(null, "", u); }
    } catch (e) {}
  }

  function wire(onChange) {
    document.querySelectorAll(".lang button").forEach(b => b.addEventListener("click", () => onChange(b.dataset.lang)));
  }

  window.DDLang = { pick, apply, wire };
})();
