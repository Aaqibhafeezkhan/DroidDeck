using System;
using System.Collections.Generic;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Traducoes das integracoes locais (Voicemeeter, Elgato, Hue, Govee, MQTT, Streamer.bot,
    /// VTube Studio), num arquivo a parte para nao inchar Localization.en.cs. Entram nos
    /// dicionarios principais no construtor estatico; mesma regra: chave = texto em portugues.
    /// </summary>
    public static partial class L
    {
        static L()
        {
            foreach (var (pt, en) in LocalEn) En[pt] = en;
            foreach (var (pt, es) in LocalEs) Es[pt] = es;
        }

        private static readonly Dictionary<string, string> LocalEn = new(StringComparer.Ordinal)
        {
            // ---- Comuns ----
            ["Onde"] = "Where",
            ["Número"] = "Number",
            ["Ganho"] = "Gain",
            ["Valor"] = "Value",
            ["Brilho"] = "Brightness",
            ["Temperatura"] = "Temperature",
            ["Cor"] = "Color",
            ["Definir o brilho"] = "Set brightness",
            ["Definir a temperatura"] = "Set temperature",
            ["Temperatura (+/-)"] = "Temperature (+/-)",
            ["Passo da temperatura"] = "Temperature step",
            ["Operação desconhecida."] = "Unknown operation.",

            // ---- Voicemeeter ----
            ["Mudo e ganho das entradas e saídas do Voicemeeter, botões do Macro Buttons e parâmetros."] =
                "Mute and gain of Voicemeeter inputs and outputs, Macro Buttons and parameters.",
            ["Mudo (alternar)"] = "Mute (toggle)",
            ["Mutar"] = "Mute",
            ["Desmutar"] = "Unmute",
            ["Definir o ganho"] = "Set gain",
            ["Ganho (+/-)"] = "Gain (+/-)",
            ["Apertar botão do Macro Buttons"] = "Press a Macro Buttons button",
            ["Definir um parâmetro"] = "Set a parameter",
            ["Alternar um parâmetro (0/1)"] = "Toggle a parameter (0/1)",
            ["Entrada (Strip)"] = "Input (Strip)",
            ["Saída (Bus)"] = "Output (Bus)",
            ["Contando da esquerda, como no Voicemeeter (1 = a primeira)."] = "Counting from the left, as in Voicemeeter (1 = the first).",
            ["Número do botão"] = "Button number",
            ["O id do botão no Macro Buttons (o primeiro é 0)."] = "The button id in Macro Buttons (the first is 0).",
            ["Parâmetro"] = "Parameter",
            ["Nome do parâmetro na Remote API do Voicemeeter."] = "Parameter name in the Voicemeeter Remote API.",
            ["Parâmetro do Voicemeeter desconhecido."] = "Unknown Voicemeeter parameter.",
            ["Voicemeeter não está instalado neste PC."] = "Voicemeeter is not installed on this PC.",
            ["O Voicemeeter não está aberto."] = "Voicemeeter is not running.",
            ["Não foi possível falar com o Voicemeeter."] = "Couldn't talk to Voicemeeter.",

            // ---- Elgato ----
            ["Liga, desliga, brilho e temperatura de cor das luzes Elgato (Key Light, Light Strip) pela rede local."] =
                "On, off, brightness and color temperature of Elgato lights (Key Light, Light Strip) over the local network.",
            ["IP da luz"] = "Light IP",
            ["Aparece no Elgato Control Center, nas configurações da luz. Porta diferente: 192.168.0.50:9123."] =
                "Shown in Elgato Control Center, in the light's settings. Different port: 192.168.0.50:9123.",
            ["Positivo deixa a luz mais fria (azulada)."] = "Positive makes the light cooler (bluer).",
            ["IP da luz inválido."] = "Invalid light IP.",

            // ---- Philips Hue ----
            ["Luzes, salas e cenas do Philips Hue pelo Hue Bridge da sua rede, sem conta na nuvem."] =
                "Philips Hue lights, rooms and scenes through the Hue Bridge on your network, no cloud account.",
            ["Controlar"] = "Control",
            ["Uma luz"] = "A light",
            ["Uma sala ou zona"] = "A room or zone",
            ["Luz ou sala"] = "Light or room",
            ["Todas as luzes"] = "All lights",
            ["Escolha uma cena."] = "Choose a scene.",
            ["Escolha uma luz ou sala."] = "Choose a light or room.",
            ["Nenhum Hue Bridge encontrado. Digite o IP dele (aparece no app Hue, em Configurações → Bridges)."] =
                "No Hue Bridge found. Type its IP (shown in the Hue app, under Settings → Bridges).",
            ["Aperte o botão redondo do Hue Bridge e toque em Parear de novo (em até 30 segundos)."] =
                "Press the round button on the Hue Bridge and tap Pair again (within 30 seconds).",
            ["O Hue Bridge recusou o pareamento."] = "The Hue Bridge refused the pairing.",
            ["O Hue Bridge recusou o acesso. Pareie de novo."] = "The Hue Bridge refused access. Pair again.",
            ["O Hue Bridge recusou."] = "The Hue Bridge refused.",
            ["Não foi possível falar com o Hue Bridge. Confira o IP."] = "Couldn't reach the Hue Bridge. Check the IP.",
            ["Philips Hue não está pareado."] = "Philips Hue is not paired.",

            // ---- Govee ----
            ["Lâmpadas e fitas Govee pela rede local (precisa do Controle LAN ligado no app Govee Home)."] =
                "Govee bulbs and strips over the local network (needs LAN Control enabled in the Govee Home app).",
            ["Aparelho"] = "Device",
            ["Procura na rede ao abrir. Se não aparecer, digite o IP do aparelho."] =
                "Searches the network when opened. If it doesn't show up, type the device IP.",
            ["Definir a cor"] = "Set color",
            ["Definir a temperatura (branco)"] = "Set temperature (white)",
            ["No formato #RRGGBB."] = "In #RRGGBB format.",
            ["Cor inválida. Use o formato #RRGGBB."] = "Invalid color. Use the #RRGGBB format.",
            ["IP do aparelho Govee inválido."] = "Invalid Govee device IP.",
            ["O aparelho Govee não respondeu. Confira o IP e o Controle LAN no app Govee Home."] =
                "The Govee device didn't respond. Check the IP and LAN Control in the Govee Home app.",
            ["A porta UDP 4002 está em uso por outro programa (precisa dela para falar com a Govee)."] =
                "UDP port 4002 is in use by another program (it's needed to talk to Govee).",

            // ---- MQTT ----
            ["Publica uma mensagem num tópico do seu broker MQTT (Mosquitto, Home Assistant, Node-RED, Zigbee2MQTT...)."] =
                "Publishes a message to a topic on your MQTT broker (Mosquitto, Home Assistant, Node-RED, Zigbee2MQTT...).",
            ["Tópico"] = "Topic",
            ["casa/sala/luz/set"] = "home/living-room/light/set",
            ["Texto ou JSON. Pode ficar vazia."] = "Text or JSON. Can be empty.",
            ["QoS"] = "QoS",
            ["0 — no máximo uma vez"] = "0 — at most once",
            ["1 — pelo menos uma vez"] = "1 — at least once",
            ["2 — exatamente uma vez"] = "2 — exactly once",
            ["Reter (retain)"] = "Retain",
            ["O broker guarda a última mensagem do tópico para quem assinar depois."] =
                "The broker keeps the topic's last message for whoever subscribes later.",
            ["Endereço do broker inválido. Ex.: mqtt://192.168.0.10:1883"] = "Invalid broker address. E.g.: mqtt://192.168.0.10:1883",
            ["O broker MQTT recusou o usuário ou a senha."] = "The MQTT broker rejected the username or password.",
            ["Não foi possível conectar ao broker MQTT. Confira o endereço."] = "Couldn't connect to the MQTT broker. Check the address.",
            ["MQTT desconectado."] = "MQTT disconnected.",
            ["O broker MQTT recusou a mensagem."] = "The MQTT broker refused the message.",
            ["Tópico MQTT inválido."] = "Invalid MQTT topic.",

            // ---- Streamer.bot ----
            ["Dispara uma ação do Streamer.bot (com argumentos), pelo WebSocket Server dele."] =
                "Runs a Streamer.bot action (with arguments) through its WebSocket Server.",
            ["Também aceita o nome da ação digitado."] = "Also accepts a typed action name.",
            ["chave=valor"] = "key=value",
            ["Um por linha (chave=valor) ou um objeto JSON. Viram %chave% no Streamer.bot."] =
                "One per line (key=value) or a JSON object. They become %key% in Streamer.bot.",
            ["Endereço inválido. Ex.: ws://127.0.0.1:8080/"] = "Invalid address. E.g.: ws://127.0.0.1:8080/",
            ["Não foi possível conectar ao Streamer.bot. Ligue o WebSocket Server dele (Servers/Clients)."] =
                "Couldn't connect to Streamer.bot. Turn on its WebSocket Server (Servers/Clients).",
            ["O Streamer.bot fechou a conexão."] = "Streamer.bot closed the connection.",
            ["O Streamer.bot pede senha. Preencha a senha do WebSocket Server."] =
                "Streamer.bot asks for a password. Fill in the WebSocket Server password.",
            ["Senha recusada pelo Streamer.bot."] = "Streamer.bot rejected the password.",
            ["Streamer.bot desconectado."] = "Streamer.bot disconnected.",
            ["O Streamer.bot não respondeu."] = "Streamer.bot didn't respond.",
            ["O Streamer.bot recusou."] = "Streamer.bot refused.",
            ["Escolha uma ação do Streamer.bot."] = "Choose a Streamer.bot action.",
            ["Argumentos em JSON inválidos."] = "Invalid JSON arguments.",

            // ---- VTube Studio ----
            ["Dispara atalhos (expressões, animações) do modelo no VTube Studio ou troca de modelo."] =
                "Triggers the model's hotkeys (expressions, animations) in VTube Studio or switches models.",
            ["Disparar atalho"] = "Trigger hotkey",
            ["Carregar modelo"] = "Load model",
            ["Atalho"] = "Hotkey",
            ["Modelo"] = "Model",
            ["Atalhos do modelo carregado agora. Também aceita o nome digitado."] =
                "Hotkeys of the currently loaded model. Also accepts a typed name.",
            ["Escolha um atalho."] = "Choose a hotkey.",
            ["Escolha um modelo."] = "Choose a model.",
            ["Endereço inválido. Ex.: ws://localhost:8001"] = "Invalid address. E.g.: ws://localhost:8001",
            ["Permita o DroidDeck na janela que abriu no VTube Studio."] = "Allow DroidDeck in the window that opened in VTube Studio.",
            ["Não foi possível conectar ao VTube Studio. Ligue a API em Configurações → Iniciar API."] =
                "Couldn't connect to VTube Studio. Turn on the API in Settings → Start API.",
            ["O VTube Studio negou o acesso. Toque em Conectar para pedir de novo."] =
                "VTube Studio denied access. Tap Connect to ask again.",
            ["Ninguém respondeu no VTube Studio. Toque em Conectar para pedir de novo."] =
                "Nobody answered in VTube Studio. Tap Connect to ask again.",
            ["Resposta inesperada do VTube Studio."] = "Unexpected response from VTube Studio.",
            ["O VTube Studio recusou o token. Toque em Conectar para pedir de novo."] =
                "VTube Studio rejected the token. Tap Connect to ask again.",
            ["VTube Studio desconectado."] = "VTube Studio disconnected.",
            ["O VTube Studio não respondeu."] = "VTube Studio didn't respond.",
            ["O VTube Studio recusou."] = "VTube Studio refused.",
        };
    }
}
