/// Traducciones de las integraciones locales: mismas claves que en/local_integrations.dart.
const Map<String, String> esLocalIntegrations = {
  // Lista de integrações
  'Philips Hue': 'Philips Hue',
  'Luzes, salas e cenas pelo Hue Bridge': 'Luces, habitaciones y escenas con el Hue Bridge',
  'MQTT': 'MQTT',
  'Publicar mensagens no seu broker': 'Publicar mensajes en tu broker',
  'Streamer.bot': 'Streamer.bot',
  'Ações do Streamer.bot, com argumentos': 'Acciones de Streamer.bot, con argumentos',
  'VTube Studio': 'VTube Studio',
  'Atalhos e modelos do VTube Studio': 'Atajos y modelos de VTube Studio',

  // Philips Hue
  'Parear': 'Vincular',
  'IP do Hue Bridge': 'IP del Hue Bridge',
  'Vazio = procurar na rede': 'Vacío = buscar en la red',
  'O PC e o Hue Bridge precisam estar na mesma rede.':
      'El PC y el Hue Bridge tienen que estar en la misma red.',
  'Digite o IP do bridge (no app Hue: Configurações → Bridges) ou deixe vazio para procurar.':
      'Escribe la IP del bridge (en la app Hue: Ajustes → Bridges) o déjala vacía para buscar.',
  'Aperte o botão redondo em cima do Hue Bridge.':
      'Pulsa el botón redondo de encima del Hue Bridge.',
  'Em até 30 segundos, toque em Parear aqui.':
      'En menos de 30 segundos, toca Vincular aquí.',

  // MQTT
  'Broker': 'Broker',
  'Usuário (opcional)': 'Usuario (opcional)',
  'Senha (opcional)': 'Contraseña (opcional)',
  'Use o endereço do seu broker (Mosquitto, o add-on do Home Assistant, EMQX...). A porta padrão é 1883.':
      'Usa la dirección de tu broker (Mosquitto, el complemento de Home Assistant, EMQX...). El puerto por defecto es 1883.',
  'Para TLS, comece com mqtts:// (porta padrão 8883).':
      'Para TLS, empieza con mqtts:// (puerto por defecto 8883).',
  'Depois, crie botões do tipo MQTT com o tópico e a mensagem a publicar.':
      'Después, crea botones de tipo MQTT con el tema y el mensaje a publicar.',

  // Streamer.bot
  'Endereço do WebSocket Server': 'Dirección del WebSocket Server',
  'Senha (se tiver)': 'Contraseña (si tiene)',
  'No Streamer.bot, abra Servers/Clients → WebSocket Server.':
      'En Streamer.bot, abre Servers/Clients → WebSocket Server.',
  'Marque "Auto Start" e clique em Start Server (o padrão é 127.0.0.1, porta 8080).':
      'Marca "Auto Start" y haz clic en Start Server (por defecto 127.0.0.1, puerto 8080).',
  'Streamer.bot em outro PC: lá, troque o endereço para 0.0.0.0; aqui, use o IP daquele PC.':
      'Streamer.bot en otro PC: allí, cambia la dirección a 0.0.0.0; aquí, usa la IP de ese PC.',
  'Se ligou "Authentication", preencha a mesma senha.':
      'Si activaste "Authentication", rellena la misma contraseña.',

  // VTube Studio
  'Endereço da API': 'Dirección de la API',
  'No VTube Studio, abra as Configurações (engrenagem) e ligue "Iniciar API (permitir plugins)".':
      'En VTube Studio, abre los Ajustes (engranaje) y activa "Iniciar API (permitir plugins)".',
  'Toque em Conectar aqui: o VTube Studio pergunta se permite o DroidDeck. Clique em Permitir.':
      'Toca Conectar aquí: VTube Studio pregunta si permite DroidDeck. Haz clic en Permitir.',
  'A porta padrão é 8001; se mudou no VTube Studio, mude aqui também.':
      'El puerto por defecto es 8001; si lo cambiaste en VTube Studio, cámbialo aquí también.',
};
