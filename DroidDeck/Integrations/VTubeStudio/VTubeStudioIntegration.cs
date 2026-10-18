using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Hubs;
using DroidDeck.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// VTube Studio pela API publica (WebSocket, padrao ws://localhost:8001). Na primeira vez o
    /// DroidDeck pede um token e o VTube Studio pergunta, na tela dele, se permite o plugin; o
    /// token fica cifrado e serve para as proximas conexoes. Recusado, nao pergunta de novo
    /// sozinho: so quando o usuario tocar em Conectar.
    /// </summary>
    public class VTubeStudioIntegration : AccountIntegration
    {
        public const string Event = "ReceiveVTubeStudioState";
        public const string DefaultUrl = "ws://localhost:8001";
        public const string PluginName = "DroidDeck";
        public const string PluginDeveloper = "DroidDeck";

        private JsonWebSocket? _socket;
        private bool _connected;
        private bool _wantToken;
        private bool _awaitingApproval;
        private string? _error;
        private string? _model;
        private CancellationTokenSource? _reconnect;
        private int _nextId;

        public VTubeStudioIntegration(ILogger<VTubeStudioIntegration> logger, IHubContext<DeckHub> hub, StreamDeckConfigService config)
            : base(logger, hub, config) { }

        public override string Name => "vtubestudio";
        public override string Title => "VTube Studio";
        public override string StateEvent => Event;

        private string? Url => Get("url");
        private string? Token => Get("token");

        public override IntegrationStatus GetStatus() => new()
        {
            Connected = _connected,
            Configured = Url != null && Token != null,
            Account = _connected ? (_model ?? "VTube Studio") : null,
            Message = _connected ? null : _awaitingApproval ? "Permita o DroidDeck na janela que abriu no VTube Studio." : _error,
            LoginPending = _awaitingApproval,
            Fields = new() { ["url"] = Url ?? DefaultUrl },
        };

        public override object GetState() => new { connected = _connected, model = _model };

        public static Uri ParseUrl(string? value)
        {
            var s = (value ?? "").Trim();
            if (s.Length == 0) s = DefaultUrl;
            if (!s.Contains("://")) s = "ws://" + s;
            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || (uri.Scheme != "ws" && uri.Scheme != "wss"))
                throw new InvalidOperationException("Endereço inválido. Ex.: ws://localhost:8001");
            return uri;
        }

        public override async Task<IntegrationStatus> ConfigureAsync(Dictionary<string, string> fields)
        {
            var url = ParseUrl(fields.GetValueOrDefault("url"));
            // Outro VTube Studio (outro PC): o token antigo nao vale la.
            if (Url != null && ParseUrl(Url) != url) Set(("token", null));
            Set(("url", url.ToString()));
            _wantToken = Token == null;
            Restart();
            for (var i = 0; i < 40 && !_connected && !_awaitingApproval && _error == null; i++) await Task.Delay(250);
            return GetStatus();
        }

        public override Task LogoutAsync()
        {
            Clear();
            _wantToken = false;
            Restart();
            return Task.CompletedTask;
        }

        private void Restart()
        {
            _error = null;
            _reconnect?.Cancel();
            _socket?.Abort();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(5);
            while (!stoppingToken.IsCancellationRequested)
            {
                _reconnect = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                if (Url == null || (Token == null && !_wantToken))
                {
                    await Wait(TimeSpan.FromDays(1), _reconnect.Token);
                    continue;
                }
                try
                {
                    await RunAsync(_reconnect.Token);
                    delay = TimeSpan.FromSeconds(5);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    if (!_reconnect.IsCancellationRequested)
                    {
                        _error = ex is WebSocketException or TimeoutException
                            ? "Não foi possível conectar ao VTube Studio. Ligue a API em Configurações → Iniciar API."
                            : ex.Message;
                        Logger.LogInformation("VTube Studio: {Error}", ex.Message);
                    }
                }
                _awaitingApproval = false;
                if (_connected)
                {
                    _connected = false;
                    _model = null;
                    await BroadcastAsync();
                }
                if (!_reconnect.IsCancellationRequested)
                {
                    await Wait(delay, _reconnect.Token);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var socket = new JsonWebSocket(m => m["requestID"]?.ToString());
            _socket = socket;
            using (var connect = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                connect.CancelAfter(TimeSpan.FromSeconds(5));
                await socket.ConnectAsync(ParseUrl(Url), connect.Token);
            }
            var run = socket.RunAsync(null, ct);

            if (Token == null)
            {
                // O VTube Studio mostra a pergunta e so responde quando o usuario escolhe.
                _awaitingApproval = true;
                JsonNode reply;
                try
                {
                    reply = await RequestOn(socket, "AuthenticationTokenRequest",
                        new JsonObject { ["pluginName"] = PluginName, ["pluginDeveloper"] = PluginDeveloper },
                        TimeSpan.FromMinutes(2), ct);
                }
                catch (InvalidOperationException)
                {
                    _wantToken = false;
                    throw new InvalidOperationException("O VTube Studio negou o acesso. Toque em Conectar para pedir de novo.");
                }
                catch (TimeoutException)
                {
                    _wantToken = false;
                    throw new InvalidOperationException("Ninguém respondeu no VTube Studio. Toque em Conectar para pedir de novo.");
                }
                finally
                {
                    _awaitingApproval = false;
                }
                var token = reply["data"]?["authenticationToken"]?.GetValue<string>()
                    ?? throw new InvalidOperationException("Resposta inesperada do VTube Studio.");
                Set(("token", token));
                _wantToken = false;
            }

            var auth = await RequestOn(socket, "AuthenticationRequest", new JsonObject
            {
                ["pluginName"] = PluginName,
                ["pluginDeveloper"] = PluginDeveloper,
                ["authenticationToken"] = Token,
            }, TimeSpan.FromSeconds(5), ct);
            if (auth["data"]?["authenticated"]?.GetValue<bool>() != true)
            {
                // Token revogado no VTube Studio: esquece e espera o usuario pedir outro.
                Set(("token", null));
                throw new InvalidOperationException("O VTube Studio recusou o token. Toque em Conectar para pedir de novo.");
            }

            _connected = true;
            _error = null;
            try
            {
                var model = await RequestOn(socket, "CurrentModelRequest", null, TimeSpan.FromSeconds(5), ct);
                _model = model["data"]?["modelName"]?.GetValue<string>();
            }
            catch { }
            await BroadcastAsync();
            await run;
        }

        /// <summary>Envelope da API: {"apiName":"VTubeStudioPublicAPI","apiVersion":"1.0",...}.</summary>
        public static JsonObject Envelope(string requestId, string messageType, JsonObject? data)
        {
            var msg = new JsonObject
            {
                ["apiName"] = "VTubeStudioPublicAPI",
                ["apiVersion"] = "1.0",
                ["requestID"] = requestId,
                ["messageType"] = messageType,
            };
            if (data != null) msg["data"] = data;
            return msg;
        }

        /// <summary>Resposta de erro (messageType APIError) vira excecao com a mensagem do VTube Studio.</summary>
        public static JsonNode ThrowIfError(JsonNode reply)
        {
            if (reply["messageType"]?.GetValue<string>() == "APIError")
                throw new InvalidOperationException(reply["data"]?["message"]?.GetValue<string>() ?? "O VTube Studio recusou.");
            return reply;
        }

        private async Task<JsonNode> RequestOn(JsonWebSocket socket, string type, JsonObject? data, TimeSpan timeout, CancellationToken ct)
        {
            var id = "dd" + Interlocked.Increment(ref _nextId);
            return ThrowIfError(await socket.RequestAsync(id, Envelope(id, type, data), timeout, ct));
        }

        public async Task<JsonNode> RequestAsync(string type, JsonObject? data)
        {
            var socket = _socket;
            if (!_connected || socket == null) throw new InvalidOperationException("VTube Studio desconectado.");
            try { return await RequestOn(socket, type, data, TimeSpan.FromSeconds(8), CancellationToken.None); }
            catch (TimeoutException) { throw new TimeoutException("O VTube Studio não respondeu."); }
        }

        public Task TriggerHotkeyAsync(string hotkey) =>
            RequestAsync("HotkeyTriggerRequest", new JsonObject { ["hotkeyID"] = hotkey.Trim() });

        public async Task LoadModelAsync(string modelId)
        {
            await RequestAsync("ModelLoadRequest", new JsonObject { ["modelID"] = modelId.Trim() });
            try
            {
                var model = await RequestAsync("CurrentModelRequest", null);
                _model = model["data"]?["modelName"]?.GetValue<string>();
                await BroadcastAsync();
            }
            catch { }
        }

        // ---- Listas do editor ----

        public override async Task<List<FieldOption>> OptionsAsync(string list, IQueryCollection query)
        {
            switch (list)
            {
                case "hotkeys":
                    return ParseHotkeys(await RequestAsync("HotkeysInCurrentModelRequest", null));
                case "models":
                    return ParseModels(await RequestAsync("AvailableModelsRequest", null));
                default:
                    return await base.OptionsAsync(list, query);
            }
        }

        /// <summary>Atalhos do modelo carregado: id e "nome (tipo)".</summary>
        public static List<FieldOption> ParseHotkeys(JsonNode reply) =>
            (reply["data"]?["availableHotkeys"] as JsonArray ?? new JsonArray())
                .Where(h => h?["hotkeyID"] != null)
                .Select(h =>
                {
                    var name = h!["name"]?.GetValue<string>();
                    var type = h["type"]?.GetValue<string>();
                    var label = string.IsNullOrEmpty(name) ? h["hotkeyID"]!.ToString() : name;
                    return new FieldOption(h["hotkeyID"]!.ToString(), string.IsNullOrEmpty(type) ? label : $"{label} ({type})");
                })
                .ToList();

        public static List<FieldOption> ParseModels(JsonNode reply) =>
            (reply["data"]?["availableModels"] as JsonArray ?? new JsonArray())
                .Where(m => m?["modelID"] != null)
                .Select(m => new FieldOption(m!["modelID"]!.ToString(), m["modelName"]?.GetValue<string>() ?? m["modelID"]!.ToString()))
                .OrderBy(o => o.Label, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
    }
}
