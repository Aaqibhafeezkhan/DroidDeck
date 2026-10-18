using System;
using System.Collections.Generic;

namespace DroidDeck.Lib
{
    /// <summary>Espanhol das integracoes locais: mesmas chaves de Localization.local.en.cs.</summary>
    public static partial class L
    {
        private static readonly Dictionary<string, string> LocalEs = new(StringComparer.Ordinal)
        {
            // ---- Comuns ----
            ["Onde"] = "Dónde",
            ["Número"] = "Número",
            ["Ganho"] = "Ganancia",
            ["Valor"] = "Valor",
            ["Brilho"] = "Brillo",
            ["Temperatura"] = "Temperatura",
            ["Cor"] = "Color",
            ["Definir o brilho"] = "Fijar el brillo",
            ["Definir a temperatura"] = "Fijar la temperatura",
            ["Temperatura (+/-)"] = "Temperatura (+/-)",
            ["Passo da temperatura"] = "Paso de la temperatura",
            ["Operação desconhecida."] = "Operación desconocida.",

            // ---- Voicemeeter ----
            ["Mudo e ganho das entradas e saídas do Voicemeeter, botões do Macro Buttons e parâmetros."] =
                "Silencio y ganancia de las entradas y salidas de Voicemeeter, botones de Macro Buttons y parámetros.",
            ["Mudo (alternar)"] = "Silencio (alternar)",
            ["Mutar"] = "Silenciar",
            ["Desmutar"] = "Quitar silencio",
            ["Definir o ganho"] = "Fijar la ganancia",
            ["Ganho (+/-)"] = "Ganancia (+/-)",
            ["Apertar botão do Macro Buttons"] = "Pulsar un botón de Macro Buttons",
            ["Definir um parâmetro"] = "Fijar un parámetro",
            ["Alternar um parâmetro (0/1)"] = "Alternar un parámetro (0/1)",
            ["Entrada (Strip)"] = "Entrada (Strip)",
            ["Saída (Bus)"] = "Salida (Bus)",
            ["Contando da esquerda, como no Voicemeeter (1 = a primeira)."] = "Contando desde la izquierda, como en Voicemeeter (1 = la primera).",
            ["Número do botão"] = "Número del botón",
            ["O id do botão no Macro Buttons (o primeiro é 0)."] = "El id del botón en Macro Buttons (el primero es 0).",
            ["Parâmetro"] = "Parámetro",
            ["Nome do parâmetro na Remote API do Voicemeeter."] = "Nombre del parámetro en la Remote API de Voicemeeter.",
            ["Parâmetro do Voicemeeter desconhecido."] = "Parámetro de Voicemeeter desconocido.",
            ["Voicemeeter não está instalado neste PC."] = "Voicemeeter no está instalado en este PC.",
            ["O Voicemeeter não está aberto."] = "Voicemeeter no está abierto.",
            ["Não foi possível falar com o Voicemeeter."] = "No se pudo comunicar con Voicemeeter.",

            // ---- Elgato ----
            ["Liga, desliga, brilho e temperatura de cor das luzes Elgato (Key Light, Light Strip) pela rede local."] =
                "Enciende, apaga, brillo y temperatura de color de las luces Elgato (Key Light, Light Strip) por la red local.",
            ["IP da luz"] = "IP de la luz",
            ["Aparece no Elgato Control Center, nas configurações da luz. Porta diferente: 192.168.0.50:9123."] =
                "Aparece en Elgato Control Center, en la configuración de la luz. Otro puerto: 192.168.0.50:9123.",
            ["Positivo deixa a luz mais fria (azulada)."] = "Positivo hace la luz más fría (azulada).",
            ["IP da luz inválido."] = "IP de la luz no válida.",

            // ---- Philips Hue ----
            ["Luzes, salas e cenas do Philips Hue pelo Hue Bridge da sua rede, sem conta na nuvem."] =
                "Luces, habitaciones y escenas de Philips Hue a través del Hue Bridge de tu red, sin cuenta en la nube.",
            ["Controlar"] = "Controlar",
            ["Uma luz"] = "Una luz",
            ["Uma sala ou zona"] = "Una habitación o zona",
            ["Luz ou sala"] = "Luz o habitación",
            ["Todas as luzes"] = "Todas las luces",
            ["Escolha uma cena."] = "Elige una escena.",
            ["Escolha uma luz ou sala."] = "Elige una luz o habitación.",
            ["Nenhum Hue Bridge encontrado. Digite o IP dele (aparece no app Hue, em Configurações → Bridges)."] =
                "No se encontró ningún Hue Bridge. Escribe su IP (aparece en la app Hue, en Ajustes → Bridges).",
            ["Aperte o botão redondo do Hue Bridge e toque em Parear de novo (em até 30 segundos)."] =
                "Pulsa el botón redondo del Hue Bridge y toca Vincular de nuevo (en menos de 30 segundos).",
            ["O Hue Bridge recusou o pareamento."] = "El Hue Bridge rechazó la vinculación.",
            ["O Hue Bridge recusou o acesso. Pareie de novo."] = "El Hue Bridge rechazó el acceso. Vincula de nuevo.",
            ["O Hue Bridge recusou."] = "El Hue Bridge lo rechazó.",
            ["Não foi possível falar com o Hue Bridge. Confira o IP."] = "No se pudo comunicar con el Hue Bridge. Revisa la IP.",
            ["Philips Hue não está pareado."] = "Philips Hue no está vinculado.",

            // ---- Govee ----
            ["Lâmpadas e fitas Govee pela rede local (precisa do Controle LAN ligado no app Govee Home)."] =
                "Bombillas y tiras Govee por la red local (requiere el Control LAN activado en la app Govee Home).",
            ["Aparelho"] = "Dispositivo",
            ["Procura na rede ao abrir. Se não aparecer, digite o IP do aparelho."] =
                "Busca en la red al abrir. Si no aparece, escribe la IP del dispositivo.",
            ["Definir a cor"] = "Fijar el color",
            ["Definir a temperatura (branco)"] = "Fijar la temperatura (blanco)",
            ["No formato #RRGGBB."] = "En formato #RRGGBB.",
            ["Cor inválida. Use o formato #RRGGBB."] = "Color no válido. Usa el formato #RRGGBB.",
            ["IP do aparelho Govee inválido."] = "IP del dispositivo Govee no válida.",
            ["O aparelho Govee não respondeu. Confira o IP e o Controle LAN no app Govee Home."] =
                "El dispositivo Govee no respondió. Revisa la IP y el Control LAN en la app Govee Home.",
            ["A porta UDP 4002 está em uso por outro programa (precisa dela para falar com a Govee)."] =
                "El puerto UDP 4002 está en uso por otro programa (hace falta para hablar con Govee).",

            // ---- MQTT ----
            ["Publica uma mensagem num tópico do seu broker MQTT (Mosquitto, Home Assistant, Node-RED, Zigbee2MQTT...)."] =
                "Publica un mensaje en un tema de tu broker MQTT (Mosquitto, Home Assistant, Node-RED, Zigbee2MQTT...).",
            ["Tópico"] = "Tema (topic)",
            ["casa/sala/luz/set"] = "casa/salon/luz/set",
            ["Texto ou JSON. Pode ficar vazia."] = "Texto o JSON. Puede quedar vacío.",
            ["QoS"] = "QoS",
            ["0 — no máximo uma vez"] = "0 — como máximo una vez",
            ["1 — pelo menos uma vez"] = "1 — al menos una vez",
            ["2 — exatamente uma vez"] = "2 — exactamente una vez",
            ["Reter (retain)"] = "Retener (retain)",
            ["O broker guarda a última mensagem do tópico para quem assinar depois."] =
                "El broker guarda el último mensaje del tema para quien se suscriba después.",
            ["Endereço do broker inválido. Ex.: mqtt://192.168.0.10:1883"] = "Dirección del broker no válida. Ej.: mqtt://192.168.0.10:1883",
            ["O broker MQTT recusou o usuário ou a senha."] = "El broker MQTT rechazó el usuario o la contraseña.",
            ["Não foi possível conectar ao broker MQTT. Confira o endereço."] = "No se pudo conectar al broker MQTT. Revisa la dirección.",
            ["MQTT desconectado."] = "MQTT desconectado.",
            ["O broker MQTT recusou a mensagem."] = "El broker MQTT rechazó el mensaje.",
            ["Tópico MQTT inválido."] = "Tema MQTT no válido.",

            // ---- Streamer.bot ----
            ["Dispara uma ação do Streamer.bot (com argumentos), pelo WebSocket Server dele."] =
                "Ejecuta una acción de Streamer.bot (con argumentos) a través de su WebSocket Server.",
            ["Também aceita o nome da ação digitado."] = "También acepta el nombre de la acción escrito.",
            ["chave=valor"] = "clave=valor",
            ["Um por linha (chave=valor) ou um objeto JSON. Viram %chave% no Streamer.bot."] =
                "Uno por línea (clave=valor) o un objeto JSON. Se convierten en %clave% en Streamer.bot.",
            ["Endereço inválido. Ex.: ws://127.0.0.1:8080/"] = "Dirección no válida. Ej.: ws://127.0.0.1:8080/",
            ["Não foi possível conectar ao Streamer.bot. Ligue o WebSocket Server dele (Servers/Clients)."] =
                "No se pudo conectar a Streamer.bot. Activa su WebSocket Server (Servers/Clients).",
            ["O Streamer.bot fechou a conexão."] = "Streamer.bot cerró la conexión.",
            ["O Streamer.bot pede senha. Preencha a senha do WebSocket Server."] =
                "Streamer.bot pide contraseña. Rellena la contraseña del WebSocket Server.",
            ["Senha recusada pelo Streamer.bot."] = "Streamer.bot rechazó la contraseña.",
            ["Streamer.bot desconectado."] = "Streamer.bot desconectado.",
            ["O Streamer.bot não respondeu."] = "Streamer.bot no respondió.",
            ["O Streamer.bot recusou."] = "Streamer.bot lo rechazó.",
            ["Escolha uma ação do Streamer.bot."] = "Elige una acción de Streamer.bot.",
            ["Argumentos em JSON inválidos."] = "Argumentos JSON no válidos.",

            // ---- VTube Studio ----
            ["Dispara atalhos (expressões, animações) do modelo no VTube Studio ou troca de modelo."] =
                "Activa atajos (expresiones, animaciones) del modelo en VTube Studio o cambia de modelo.",
            ["Disparar atalho"] = "Activar atajo",
            ["Carregar modelo"] = "Cargar modelo",
            ["Atalho"] = "Atajo",
            ["Modelo"] = "Modelo",
            ["Atalhos do modelo carregado agora. Também aceita o nome digitado."] =
                "Atajos del modelo cargado ahora. También acepta el nombre escrito.",
            ["Escolha um atalho."] = "Elige un atajo.",
            ["Escolha um modelo."] = "Elige un modelo.",
            ["Endereço inválido. Ex.: ws://localhost:8001"] = "Dirección no válida. Ej.: ws://localhost:8001",
            ["Permita o DroidDeck na janela que abriu no VTube Studio."] = "Permite DroidDeck en la ventana que se abrió en VTube Studio.",
            ["Não foi possível conectar ao VTube Studio. Ligue a API em Configurações → Iniciar API."] =
                "No se pudo conectar a VTube Studio. Activa la API en Ajustes → Iniciar API.",
            ["O VTube Studio negou o acesso. Toque em Conectar para pedir de novo."] =
                "VTube Studio denegó el acceso. Toca Conectar para pedirlo de nuevo.",
            ["Ninguém respondeu no VTube Studio. Toque em Conectar para pedir de novo."] =
                "Nadie respondió en VTube Studio. Toca Conectar para pedirlo de nuevo.",
            ["Resposta inesperada do VTube Studio."] = "Respuesta inesperada de VTube Studio.",
            ["O VTube Studio recusou o token. Toque em Conectar para pedir de novo."] =
                "VTube Studio rechazó el token. Toca Conectar para pedirlo de nuevo.",
            ["VTube Studio desconectado."] = "VTube Studio desconectado.",
            ["O VTube Studio não respondeu."] = "VTube Studio no respondió.",
            ["O VTube Studio recusou."] = "VTube Studio lo rechazó.",
        };
    }
}
