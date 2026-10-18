// Perfis prontos da galeria, usados pela galeria e pela landing. Rótulos, nomes e descrições
// em inglês, português e espanhol: o perfil importado já chega no idioma de quem importou.
// Perfis em 3×5 (cabem em qualquer celular deitado). Nomes de ícone do app (button_icons.dart).
(function () {
  const G = "#37474F", RED = "#B71C1C", GREEN = "#1B5E20", BLUE = "#0D47A1", PURPLE = "#4A148C";
  const act = (type, parameters) => ({ type, parameters });
  const L = (en, pt, es) => ({ en, pt: pt ?? en, es: es ?? en });
  const btn = (row, column, label, iconName, action, extra = {}) =>
    ({ id: `${row}-${column}`, row, column, label, iconName, backgroundColor: G, action, ...extra });
  const micMute = act("mixer", { operation: "toggleMute", deviceId: "default", deviceKind: "input" });

  const PREV = L("Prev", "Voltar", "Atrás"), PLAY = L("Play"), NEXT = L("Next", "Próxima", "Siguiente");
  const MIC = L("Mic"), DEAF = L("Deafen", "Surdo", "Sordo"), LOCK = L("Lock", "Travar", "Bloquear");
  const CLOCK = L("Clock", "Hora", "Hora"), CLIP = L("Clip", "Clipe", "Clip");

  const PROFILES = [
    {
      key: "streamer",
      name: L("Streamer"),
      desc: L("OBS live and recording, Discord mic, break timer, a counter, music and volume.",
        "Live e gravação do OBS, microfone do Discord, timer de pausa, contador, música e volume.",
        "Directo y grabación de OBS, micrófono de Discord, temporizador de pausa, contador, música y volumen."),
      buttons: [
        btn(0, 0, L("Live", "Live", "Directo"), "smart_display", act("obs", { operation: "toggleStream" }), { backgroundColor: RED }),
        btn(0, 1, L("REC"), "videocam", act("obs", { operation: "toggleRecord" }), { backgroundColor: RED }),
        btn(0, 2, CLIP, "save", act("obs", { operation: "saveReplay" })),
        btn(0, 3, L("Marker", "Marca", "Marca"), "flag", act("obs", { operation: "createChapter" })),
        btn(0, 4, L("Cam", "Câmera", "Cámara"), "camera", act("obs", { operation: "toggleVirtualCam" })),
        btn(1, 0, MIC, "mic", act("discord", { operation: "toggleMute" }), { backgroundColor: PURPLE, activeLabel: L("Muted", "Mudo", "Silencio"), activeIconName: "mic_off" }),
        btn(1, 1, DEAF, "headphones", act("discord", { operation: "toggleDeafen" }), { backgroundColor: PURPLE }),
        btn(1, 2, L("Break", "Pausa", "Pausa"), "timer", act("timer", { mode: "countdown", durationMin: "5", operation: "toggle" })),
        btn(1, 3, L("Deaths", "Mortes", "Muertes"), "sports_esports", act("timer", { mode: "counter", operation: "increment", step: "1" })),
        btn(1, 4, L("CPU"), null, null, { dynamicType: "cpu_monitor" }),
        btn(2, 0, PREV, "arrow_back", act("media", { command: "previous" })),
        btn(2, 1, PLAY, "play", act("media", { command: "playpause" }), { backgroundColor: GREEN }),
        btn(2, 2, L("Music", "Música", "Música"), null, null, { dynamicType: "now_playing", columnSpan: 2 }),
        btn(2, 4, L("Volume", "Volume", "Volumen"), null, null, { dynamicType: "volume_slider", dynamicParameters: { target: "output" } }),
      ],
    },
    {
      key: "meetings",
      name: L("Meetings", "Reuniões", "Reuniones"),
      desc: L("Mute and camera for Teams, Zoom and Meet, your PC mic, notifications and lock.",
        "Mudo e câmera do Teams, Zoom e Meet, microfone do PC, notificações e travar.",
        "Silencio y cámara de Teams, Zoom y Meet, micrófono del PC, notificaciones y bloqueo."),
      buttons: [
        btn(0, 0, MIC, "mic", micMute, { backgroundColor: RED, toggle: false }),
        btn(0, 1, L("Teams mic", "Mic Teams", "Mic Teams"), "mic_off", act("hotkey", { keys: "^+m" })),
        btn(0, 2, L("Teams cam", "Cam Teams", "Cám Teams"), "videocam_off", act("hotkey", { keys: "^+o" })),
        btn(0, 3, L("Hand", "Mão", "Mano"), "flag", act("hotkey", { keys: "^+k" })),
        btn(0, 4, L("Leave", "Sair", "Salir"), "call_end", act("hotkey", { keys: "^+h" }), { backgroundColor: RED }),
        btn(1, 0, L("Zoom mic", "Mic Zoom", "Mic Zoom"), "mic_off", act("hotkey", { keys: "%a" }), { backgroundColor: BLUE }),
        btn(1, 1, L("Zoom cam", "Cam Zoom", "Cám Zoom"), "videocam_off", act("hotkey", { keys: "%v" }), { backgroundColor: BLUE }),
        btn(1, 2, L("Meet mic", "Mic Meet", "Mic Meet"), "mic_off", act("hotkey", { keys: "^d" }), { backgroundColor: GREEN }),
        btn(1, 3, L("Meet cam", "Cam Meet", "Cám Meet"), "videocam_off", act("hotkey", { keys: "^e" }), { backgroundColor: GREEN }),
        btn(1, 4, CLOCK, null, null, { dynamicType: "clock" }),
        btn(2, 0, L("Vol −"), "volume_off", act("mixer", { operation: "volumeDown", deviceId: "default" })),
        btn(2, 1, L("Vol +"), "volume_up", act("mixer", { operation: "volumeUp", deviceId: "default" })),
        btn(2, 2, L("Alerts", "Avisos", "Avisos"), null, null, { dynamicType: "notifications" }),
        btn(2, 3, LOCK, "lock", act("windows", { operation: "lock" })),
        btn(2, 4, L("Focus", "Foco", "Foco"), "notifications_off", act("hotkey", { keys: "#n" })),
      ],
    },
    {
      key: "music",
      name: L("Music", "Música", "Música"),
      desc: L("Now playing, media keys and volume sliders for the PC and Spotify.",
        "Tocando agora, teclas de mídia e controles de volume do PC e do Spotify.",
        "Reproduciendo ahora, teclas multimedia y controles de volumen del PC y de Spotify."),
      buttons: [
        btn(0, 0, L(""), null, null, { dynamicType: "now_playing", columnSpan: 2, rowSpan: 2 }),
        btn(0, 2, PREV, "arrow_back", act("media", { command: "previous" })),
        btn(0, 3, PLAY, "play", act("media", { command: "playpause" }), { backgroundColor: GREEN }),
        btn(0, 4, L("Volume", "Volume", "Volumen"), null, null, { dynamicType: "volume_slider", dynamicParameters: { target: "output" }, rowSpan: 3 }),
        btn(1, 2, NEXT, "playlist_play", act("media", { command: "next" })),
        btn(1, 3, L("Mute", "Mudo", "Silencio"), "volume_off", act("mixer", { operation: "toggleMute", deviceId: "default" })),
        btn(2, 0, L("Spotify"), null, null, { dynamicType: "volume_slider", dynamicParameters: { target: "app", app: "Spotify" }, columnSpan: 4 }),
      ],
    },
    {
      key: "productivity",
      name: L("Productivity", "Produtividade", "Productividad"),
      desc: L("Windows shortcuts, copy and paste, a pomodoro, clock and PC monitors.",
        "Atalhos do Windows, copiar e colar, pomodoro, relógio e monitores do PC.",
        "Atajos de Windows, copiar y pegar, pomodoro, reloj y monitores del PC."),
      buttons: [
        btn(0, 0, L("Desktop", "Desktop", "Escritorio"), "computer", act("hotkey", { keys: "#d" })),
        btn(0, 1, L("Files", "Arquivos", "Archivos"), "folder", act("hotkey", { keys: "#e" })),
        btn(0, 2, L("History", "Histórico", "Historial"), "web_asset", act("hotkey", { keys: "#v" })),
        btn(0, 3, L("Snip", "Recorte", "Recorte"), "camera", act("hotkey", { keys: "#+s" })),
        btn(0, 4, L("Tasks", "Processos", "Procesos"), "tune", act("hotkey", { keys: "^+{ESC}" })),
        btn(1, 0, L("Copy", "Copiar", "Copiar"), "keyboard", act("hotkey", { keys: "^c" })),
        btn(1, 1, L("Paste", "Colar", "Pegar"), "keyboard", act("hotkey", { keys: "^v" })),
        btn(1, 2, L("Undo", "Desfazer", "Deshacer"), "keyboard", act("hotkey", { keys: "^z" })),
        btn(1, 3, L("Pomodoro"), "timer", act("timer", { mode: "pomodoro", workMin: "25", breakMin: "5", operation: "toggle" }), { columnSpan: 2 }),
        btn(2, 0, CLOCK, null, null, { dynamicType: "clock" }),
        btn(2, 1, L("CPU"), null, null, { dynamicType: "cpu_monitor" }),
        btn(2, 2, L("RAM"), null, null, { dynamicType: "memory_monitor" }),
        btn(2, 3, LOCK, "lock", act("windows", { operation: "lock" })),
        btn(2, 4, L("Sleep", "Suspender", "Suspender"), "power_off", act("windows", { operation: "sleep" }), { backgroundColor: RED }),
      ],
    },
    {
      key: "gaming",
      name: L("Gaming", "Jogos", "Juegos"),
      desc: L("Live CPU/GPU/RAM, Discord, instant replay, screenshot and volume sliders.",
        "CPU/GPU/RAM ao vivo, Discord, replay instantâneo, captura de tela e volumes.",
        "CPU/GPU/RAM en vivo, Discord, repetición instantánea, captura de pantalla y volúmenes."),
      buttons: [
        btn(0, 0, L("CPU"), null, null, { dynamicType: "cpu_monitor" }),
        btn(0, 1, L("GPU"), null, null, { dynamicType: "gpu_monitor" }),
        btn(0, 2, L("RAM"), null, null, { dynamicType: "memory_monitor" }),
        btn(0, 3, L("Net", "Rede", "Red"), null, null, { dynamicType: "network_monitor" }),
        btn(0, 4, L("Game", "Jogo", "Juego"), null, null, { dynamicType: "volume_slider", dynamicParameters: { target: "output" }, rowSpan: 3 }),
        btn(1, 0, MIC, "mic", act("discord", { operation: "toggleMute" }), { backgroundColor: PURPLE }),
        btn(1, 1, DEAF, "headphones", act("discord", { operation: "toggleDeafen" }), { backgroundColor: PURPLE }),
        btn(1, 2, CLIP, "save", act("obs", { operation: "saveReplay" }), { backgroundColor: RED }),
        btn(1, 3, L("Screenshot", "Captura", "Captura"), "camera", act("hotkey", { keys: "#%{PRTSC}" })),
        btn(2, 0, L("Discord"), null, null, { dynamicType: "volume_slider", dynamicParameters: { target: "app", app: "Discord" }, columnSpan: 2 }),
        btn(2, 2, PLAY, "play", act("media", { command: "playpause" }), { backgroundColor: GREEN }),
        btn(2, 3, NEXT, "playlist_play", act("media", { command: "next" })),
      ],
    },
  ];

  // Perfis no idioma pedido, no formato que o DroidDeck importa.
  function build(lang) {
    const pick = v => (v && typeof v === "object" ? v[lang] ?? v.en : v);
    return PROFILES.map(p => ({
      key: p.key,
      desc: pick(p.desc),
      profile: {
        name: pick(p.name),
        buttons: p.buttons.map(b => {
          const out = { ...b, label: pick(b.label) };
          if (b.activeLabel) out.activeLabel = pick(b.activeLabel);
          return out;
        }),
      },
    }));
  }

  window.DDProfiles = { build };
})();
