using System;
using System.Collections.Generic;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Traducoes para espanhol, chave = texto exato em portugues usado no codigo. Mudou o texto
    /// em portugues, mude a chave aqui tambem (senao volta a aparecer em ingles/portugues).
    /// </summary>
    public static partial class L
    {
        private static readonly Dictionary<string, string> Es = new(StringComparer.Ordinal)
        {
            // ---- Categorias ----
            ["Sistema"] = "Sistema",
            ["Mídia"] = "Multimedia",
            ["Streaming"] = "Streaming",
            ["Casa"] = "Hogar",
            ["Utilidades"] = "Utilidades",
            ["Navegação"] = "Navegación",

            // ---- Atalho, programa, janela ----
            ["Atalho de teclado"] = "Atajo de teclado",
            ["Teclas"] = "Teclas",
            ["^ = Ctrl, + = Shift, % = Alt, # = Win, {ENTER}, {F5}…"] = "^ = Ctrl, + = Shift, % = Alt, # = Win, {ENTER}, {F5}…",
            ["Abrir programa"] = "Abrir programa",
            ["Abre um programa, arquivo ou endereço no PC."] = "Abre un programa, archivo o dirección en la PC.",
            ["Caminho"] = "Ruta",
            ["Também aceita um arquivo ou uma URL."] = "También acepta un archivo o una URL.",
            ["Argumentos (opcional)"] = "Argumentos (opcional)",
            ["Ativar janela"] = "Activar ventana",
            ["Título exato da janela"] = "Título exacto de la ventana",

            // ---- Midia e mixer ----
            ["Comando"] = "Comando",
            ["Play / Pause"] = "Play / Pausa",
            ["Próxima faixa"] = "Siguiente pista",
            ["Faixa anterior"] = "Pista anterior",
            ["Parar"] = "Detener",
            ["Volume e mudo"] = "Volumen y silencio",

            // ---- Multi-acao, perfis ----
            ["Multi-ação"] = "Multiacción",
            ["Vários passos em sequência, com espera entre eles."] = "Varios pasos en secuencia, con espera entre ellos.",
            ["Abrir perfil"] = "Abrir perfil",
            ["Abre outro perfil como uma pasta."] = "Abre otro perfil como si fuera una carpeta.",
            ["Voltar"] = "Volver",
            ["Volta ao perfil anterior."] = "Vuelve al perfil anterior.",

            // ---- OBS ----
            ["Ação do OBS"] = "Acción de OBS",
            ["Trocar de cena"] = "Cambiar de escena",
            ["Gravar (alternar)"] = "Grabar (alternar)",
            ["Transmitir / Live (alternar)"] = "Transmitir / En vivo (alternar)",
            ["Câmera virtual (alternar)"] = "Cámara virtual (alternar)",
            ["Replay buffer (alternar)"] = "Búfer de repetición (alternar)",
            ["Salvar replay"] = "Guardar repetición",
            ["Mutar fonte de áudio (alternar)"] = "Silenciar fuente de audio (alternar)",
            ["Mostrar / ocultar fonte"] = "Mostrar / ocultar fuente",
            ["Ligar / desligar filtro"] = "Activar / desactivar filtro",
            ["Pausar gravação (alternar)"] = "Pausar grabación (alternar)",
            ["Marcador de capítulo na gravação"] = "Marcador de capítulo en la grabación",
            ["Captura de tela da cena"] = "Captura de pantalla de la escena",
            ["Cena"] = "Escena",
            ["Fonte"] = "Fuente",
            ["Fonte ou cena do filtro"] = "Fuente o escena del filtro",
            ["Filtro"] = "Filtro",
            ["Nome do capítulo (opcional)"] = "Nombre del capítulo (opcional)",
            ["Exige OBS 30.2 ou mais novo, gravando em MP4 híbrido."] = "Requiere OBS 30.2 o posterior, grabando en MP4 híbrido.",
            ["Fonte de áudio"] = "Fuente de audio",

            // ---- Tuya ----
            ["Casa inteligente (Tuya)"] = "Hogar inteligente (Tuya)",

            // ---- Windows ----
            ["Ação"] = "Acción",
            ["Trocar o dispositivo de áudio padrão"] = "Cambiar el dispositivo de audio predeterminado",
            ["Travar o computador"] = "Bloquear la computadora",
            ["Suspender"] = "Suspender",
            ["Desligar a tela"] = "Apagar la pantalla",
            ["Tipo"] = "Tipo",
            ["Saída (fone, caixa de som)"] = "Salida (audífonos, bocinas)",
            ["Entrada (microfone)"] = "Entrada (micrófono)",
            ["Dispositivo"] = "Dispositivo",
            ["Alternar com (opcional)"] = "Alternar con (opcional)",
            ["Com um segundo dispositivo, cada toque alterna entre os dois (fone ↔ caixa de som)."] =
                "Con un segundo dispositivo, cada toque alterna entre los dos (audífonos ↔ bocinas).",
            ["Nenhum (só define o de cima)"] = "Ninguno (solo define el de arriba)",

            // ---- HTTP ----
            ["Requisição HTTP (webhook)"] = "Solicitud HTTP (webhook)",
            ["Chama uma URL a partir do PC: webhooks do n8n, Home Assistant, IFTTT ou qualquer API."] =
                "Llama a una URL desde la PC: webhooks de n8n, Home Assistant, IFTTT o cualquier API.",
            ["Método"] = "Método",
            ["https://exemplo.com/webhook"] = "https://ejemplo.com/webhook",
            ["Cabeçalhos (opcional)"] = "Encabezados (opcional)",
            ["Um por linha, no formato Nome: valor."] = "Uno por línea, con el formato Nombre: valor.",
            ["Formato do corpo"] = "Formato del cuerpo",
            ["Formulário"] = "Formulario",
            ["Texto"] = "Texto",
            ["Corpo (opcional)"] = "Cuerpo (opcional)",
            ["{\"evento\": \"deck\"}"] = "{\"evento\": \"deck\"}",

            // ---- Timer ----
            ["Timer, pomodoro e contador"] = "Temporizador, pomodoro y contador",
            ["O botão mostra o tempo ao vivo. Quando uma contagem termina, o PC toca um aviso e o celular vibra."] =
                "El botón muestra el tiempo en vivo. Cuando termina una cuenta regresiva, la PC suena una alerta y el celular vibra.",
            ["Timer (contagem regressiva)"] = "Temporizador (cuenta regresiva)",
            ["Cronômetro"] = "Cronómetro",
            ["Pomodoro (foco e pausa)"] = "Pomodoro (enfoque y descanso)",
            ["Contador"] = "Contador",
            ["Duração"] = "Duración",
            ["Foco"] = "Enfoque",
            ["Pausa"] = "Descanso",
            ["Ao tocar"] = "Al tocar",
            ["Iniciar / pausar (contador: somar)"] = "Iniciar / pausar (contador: sumar)",
            ["Zerar"] = "Reiniciar",
            ["Pular para a próxima fase (pomodoro)"] = "Saltar a la siguiente fase (pomodoro)",
            ["Somar (contador)"] = "Sumar (contador)",
            ["Subtrair (contador)"] = "Restar (contador)",
            ["Passo"] = "Paso",
            ["Timer controlado"] = "Temporizador controlado",
            ["Para um botão de \"zerar\" separado, escolha o timer de outro botão."] =
                "Para un botón de \"reiniciar\" aparte, elige el temporizador de otro botón.",
            ["Este botão"] = "Este botón",

            // ---- Steam ----
            ["Jogo da Steam"] = "Juego de Steam",
            ["Jogo"] = "Juego",

            // ---- Home Assistant ----
            ["Luzes, tomadas, cenas, scripts e sensores do Home Assistant, com o estado real."] =
                "Luces, enchufes, escenas, scripts y sensores de Home Assistant, con su estado real.",
            ["O que fazer"] = "Qué hacer",
            ["Liga/desliga"] = "Encender/apagar",
            ["Ligar"] = "Encender",
            ["Desligar"] = "Apagar",
            ["Brilho (+/-)"] = "Brillo (+/-)",
            ["Ativar cena"] = "Activar escena",
            ["Rodar script"] = "Ejecutar script",
            ["Só mostrar o estado"] = "Solo mostrar el estado",
            ["Chamar um serviço"] = "Llamar a un servicio",
            ["Entidade"] = "Entidad",
            ["Passo do brilho"] = "Paso de brillo",
            ["Serviço"] = "Servicio",
            ["domínio.serviço"] = "dominio.servicio",
            ["Dados (JSON, opcional)"] = "Datos (JSON, opcional)",

            // ---- Spotify ----
            ["Controla o Spotify da sua conta (Premium), com capa, curtida e aleatório no botão."] =
                "Controla Spotify en tu cuenta (Premium), con portada, me gusta y aleatorio en el botón.",
            ["Tocar/pausar"] = "Reproducir/pausar",
            ["Próxima"] = "Siguiente",
            ["Anterior"] = "Anterior",
            ["Curtir/descurtir a faixa"] = "Dar/quitar me gusta a la pista",
            ["Aleatório liga/desliga"] = "Aleatorio sí/no",
            ["Repetir (desligado → playlist → faixa)"] = "Repetir (desactivado → playlist → pista)",
            ["Volume (+/-)"] = "Volumen (+/-)",
            ["Tocar uma playlist"] = "Reproducir una playlist",
            ["Mostrar o que está tocando"] = "Mostrar lo que suena",
            ["Passo do volume"] = "Paso de volumen",
            ["Começar no aleatório"] = "Empezar en aleatorio",

            // ---- Twitch ----
            ["Marcador, clipe, anúncio, chat, enquete e título do seu canal; mostra se está ao vivo."] =
                "Marcador, clip, anuncio, chat, encuesta y título de tu canal; muestra si estás en vivo.",
            ["Criar marcador"] = "Crear marcador",
            ["Criar clipe"] = "Crear clip",
            ["Rodar anúncio"] = "Lanzar anuncio",
            ["Mandar mensagem no chat"] = "Enviar mensaje al chat",
            ["Abrir enquete"] = "Iniciar encuesta",
            ["Trocar o título"] = "Cambiar el título",
            ["Só mostrar ao vivo/espectadores"] = "Solo mostrar en vivo/espectadores",
            ["Descrição (opcional)"] = "Descripción (opcional)",
            ["Momento épico"] = "Momento épico",
            ["Mensagem"] = "Mensaje",
            ["Sigam o canal!"] = "¡Sigan el canal!",
            ["Pergunta"] = "Pregunta",
            ["Qual jogo agora?"] = "¿Qué juego sigue?",
            ["Opções (uma por linha, 2 a 5)"] = "Opciones (una por línea, de 2 a 5)",
            ["Sim"] = "Sí",
            ["Não"] = "No",
            ["Novo título"] = "Nuevo título",

            // ---- YouTube ----
            ["Manda mensagens no chat da sua live e mostra se está ao vivo e quantos assistem."] =
                "Envía mensajes al chat de tu transmisión y muestra si estás en vivo y cuántos la ven.",
            ["Obrigado por assistir!"] = "¡Gracias por ver!",

            // ==== Mensagens de erro e de status que chegam ao app ====

            // Seguranca (acoes privilegiadas so no PC)
            ["Botões que abrem programas, enviam atalhos, ativam janelas ou fazem requisições HTTP só podem ser criados ou alterados no configurador do PC (http://localhost:4787)."] =
                "Los botones que abren programas, envían atajos, activan ventanas o hacen solicitudes HTTP solo se pueden crear o cambiar en el configurador de la PC (http://localhost:4787).",
            [" Botão: \"{label}\"."] = " Botón: \"{label}\".",
            ["Gravar macro só funciona no configurador do PC (http://localhost:4787)."] =
                "Grabar macros solo funciona en el configurador de la PC (http://localhost:4787).",
            ["O backup tem botões que abrem programas, enviam atalhos ou fazem requisições. Importe pelo configurador no PC (http://localhost:4787)."] =
                "La copia de seguridad tiene botones que abren programas, envían atajos o hacen solicitudes. Impórtala desde el configurador de la PC (http://localhost:4787).",

            // Backup
            ["Não é um backup do DroidDeck (o arquivo não é um .droiddeck válido)."] = "No es una copia de seguridad de DroidDeck (el archivo no es un .droiddeck válido).",
            ["Não é um backup do DroidDeck (falta o manifest.json)."] = "No es una copia de seguridad de DroidDeck (falta el manifest.json).",
            ["Não é um backup do DroidDeck."] = "No es una copia de seguridad de DroidDeck.",
            ["Backup de uma versão mais nova do DroidDeck. Atualize o DroidDeck no PC."] = "Copia de seguridad de una versión más nueva de DroidDeck. Actualiza DroidDeck en la PC.",
            ["Arquivo de backup corrompido."] = "Archivo de copia de seguridad dañado.",
            ["O arquivo não parece um perfil do Stream Deck."] = "El archivo no parece un perfil de Stream Deck.",

            // Integracoes com conta
            ["Integração desconhecida."] = "Integración desconocida.",
            ["Esta integração não usa login pelo navegador."] = "Esta integración no usa inicio de sesión en el navegador.",
            ["Entre na conta de novo."] = "Vuelve a iniciar sesión.",
            ["Login cancelado."] = "Inicio de sesión cancelado.",
            ["Este link de login expirou. Comece de novo pelo DroidDeck."] = "Este enlace de inicio de sesión expiró. Vuelve a empezar desde DroidDeck.",
            ["Spotify conectado"] = "Spotify conectado",
            ["YouTube conectado"] = "YouTube conectado",
            ["Conta {account}. Pode fechar esta aba."] = "Cuenta {account}. Ya puedes cerrar esta pestaña.",
            ["Canal {channel}. Pode fechar esta aba."] = "Canal {channel}. Ya puedes cerrar esta pestaña.",
            ["O Spotify recusou: {error}"] = "Spotify rechazó: {error}",
            ["O Google recusou: {error}"] = "Google rechazó: {error}",
            ["Não deu certo: {error}"] = "No funcionó: {error}",
            ["Endereço inválido. Ex.: http://homeassistant.local:8123"] = "Dirección no válida. Ej.: http://homeassistant.local:8123",
            ["Falta o token de acesso."] = "Falta el token de acceso.",
            ["Não foi possível conectar ao Home Assistant. Confira o endereço."] = "No se pudo conectar con Home Assistant. Revisa la dirección.",
            ["Resposta inesperada do Home Assistant."] = "Respuesta inesperada de Home Assistant.",
            ["Token recusado pelo Home Assistant. Gere outro no seu perfil."] = "Home Assistant rechazó el token. Genera otro en tu perfil.",
            ["O Home Assistant recusou."] = "Home Assistant lo rechazó.",
            ["Home Assistant desconectado."] = "Home Assistant desconectado.",
            ["O Home Assistant não respondeu."] = "Home Assistant no respondió.",
            ["Serviço no formato domínio.serviço"] = "Servicio con el formato dominio.servicio",
            ["Cole o Client ID do app que você criou no Spotify for Developers."] = "Pega el Client ID de la app que creaste en Spotify for Developers.",
            ["Nada tocando no Spotify."] = "No suena nada en Spotify.",
            ["Escolha a playlist."] = "Elige la playlist.",
            ["Cole o Client ID do app que você criou no console da Twitch."] = "Pega el Client ID de la app que creaste en la consola de Twitch.",
            ["O código expirou. Tente de novo."] = "El código expiró. Inténtalo de nuevo.",
            ["Sem token."] = "Sin token.",
            ["Twitch desconectada."] = "Twitch desconectado.",
            ["A enquete precisa de 2 opções."] = "La encuesta necesita 2 opciones.",
            ["O Client ID termina em .apps.googleusercontent.com."] = "El Client ID termina en .apps.googleusercontent.com.",
            ["Falta o Client Secret."] = "Falta el Client Secret.",
            ["Esta conta Google não tem canal no YouTube."] = "Esta cuenta de Google no tiene canal de YouTube.",
            ["Nenhuma transmissão ao vivo no canal."] = "No hay ninguna transmisión en vivo en el canal.",

            // OBS, Discord, Tuya, Soundboard
            ["OBS não respondeu (handshake). O obs-websocket está ativo?"] = "OBS no respondió (handshake). ¿Está activado obs-websocket?",
            ["OBS não está conectado."] = "OBS no está conectado.",
            ["OBS recusou o pedido."] = "OBS rechazó la solicitud.",
            ["Configure o Client ID e o Secret do Discord primeiro."] = "Configura primero el Client ID y el Secret de Discord.",
            ["Discord não respondeu ao handshake (READY). Se continuar, feche o Discord pela bandeja (Sair do Discord) e abra de novo."] =
                "Discord no respondió al handshake (READY). Si sigue pasando, cierra Discord desde la bandeja (Salir de Discord) y ábrelo de nuevo.",
            ["A autorização do Discord expirou: abra Configurações → Discord e toque em Conectar."] =
                "La autorización de Discord expiró: abre Configuración → Discord y toca Conectar.",
            ["Discord não está conectado."] = "Discord no está conectado.",
            ["Pipe do Discord não encontrado — o Discord está aberto?"] = "No se encontró el pipe de Discord — ¿Discord está abierto?",
            ["Discord fechou a conexão (CLOSE)."] = "Discord cerró la conexión (CLOSE).",
            ["Pipe fechado."] = "Pipe cerrado.",
            ["clientId e clientSecret obrigatórios"] = "clientId y clientSecret son obligatorios",
            ["soundId é obrigatório"] = "soundId es obligatorio",
            ["QR expirado; gere outro."] = "El código QR expiró; genera otro.",
            ["Tuya nao pareado."] = "Tuya no está vinculado.",
            ["Tuya nao conectado."] = "Tuya no está conectado.",
            ["userCode obrigatorio"] = "userCode es obligatorio",
            ["deviceId e code obrigatorios"] = "deviceId y code son obligatorios",
            ["url é obrigatório"] = "url es obligatorio",
            ["URL de som invalida (so http/https)."] = "URL de sonido no válida (solo http/https).",
            ["Arquivo de som grande demais."] = "El archivo de sonido es demasiado grande.",
        };
    }
}
