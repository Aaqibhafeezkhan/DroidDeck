/// Traduções das integrações locais (Philips Hue, MQTT, Streamer.bot, VTube Studio): veja en.dart.
const Map<String, String> enLocalIntegrations = {
  // Lista de integrações
  'Philips Hue': 'Philips Hue',
  'Luzes, salas e cenas pelo Hue Bridge': 'Lights, rooms and scenes via the Hue Bridge',
  'MQTT': 'MQTT',
  'Publicar mensagens no seu broker': 'Publish messages to your broker',
  'Streamer.bot': 'Streamer.bot',
  'Ações do Streamer.bot, com argumentos': 'Streamer.bot actions, with arguments',
  'VTube Studio': 'VTube Studio',
  'Atalhos e modelos do VTube Studio': 'VTube Studio hotkeys and models',

  // Philips Hue
  'Parear': 'Pair',
  'IP do Hue Bridge': 'Hue Bridge IP',
  'Vazio = procurar na rede': 'Empty = search the network',
  'O PC e o Hue Bridge precisam estar na mesma rede.':
      'The PC and the Hue Bridge must be on the same network.',
  'Digite o IP do bridge (no app Hue: Configurações → Bridges) ou deixe vazio para procurar.':
      'Type the bridge IP (in the Hue app: Settings → Bridges) or leave it empty to search.',
  'Aperte o botão redondo em cima do Hue Bridge.':
      'Press the round button on top of the Hue Bridge.',
  'Em até 30 segundos, toque em Parear aqui.': 'Within 30 seconds, tap Pair here.',

  // MQTT
  'Broker': 'Broker',
  'Usuário (opcional)': 'Username (optional)',
  'Senha (opcional)': 'Password (optional)',
  'Use o endereço do seu broker (Mosquitto, o add-on do Home Assistant, EMQX...). A porta padrão é 1883.':
      'Use your broker address (Mosquitto, the Home Assistant add-on, EMQX...). The default port is 1883.',
  'Para TLS, comece com mqtts:// (porta padrão 8883).':
      'For TLS, start with mqtts:// (default port 8883).',
  'Depois, crie botões do tipo MQTT com o tópico e a mensagem a publicar.':
      'Then create MQTT buttons with the topic and the message to publish.',

  // Streamer.bot
  'Endereço do WebSocket Server': 'WebSocket Server address',
  'Senha (se tiver)': 'Password (if any)',
  'No Streamer.bot, abra Servers/Clients → WebSocket Server.':
      'In Streamer.bot, open Servers/Clients → WebSocket Server.',
  'Marque "Auto Start" e clique em Start Server (o padrão é 127.0.0.1, porta 8080).':
      'Check "Auto Start" and click Start Server (the default is 127.0.0.1, port 8080).',
  'Streamer.bot em outro PC: lá, troque o endereço para 0.0.0.0; aqui, use o IP daquele PC.':
      'Streamer.bot on another PC: there, change the address to 0.0.0.0; here, use that PC\'s IP.',
  'Se ligou "Authentication", preencha a mesma senha.':
      'If you turned on "Authentication", fill in the same password.',

  // VTube Studio
  'Endereço da API': 'API address',
  'No VTube Studio, abra as Configurações (engrenagem) e ligue "Iniciar API (permitir plugins)".':
      'In VTube Studio, open Settings (gear) and turn on "Start API (allow plugins)".',
  'Toque em Conectar aqui: o VTube Studio pergunta se permite o DroidDeck. Clique em Permitir.':
      'Tap Connect here: VTube Studio asks whether to allow DroidDeck. Click Allow.',
  'A porta padrão é 8001; se mudou no VTube Studio, mude aqui também.':
      'The default port is 8001; if you changed it in VTube Studio, change it here too.',
};
