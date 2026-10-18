using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
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
    /// Streamer.bot pelo servidor WebSocket dele (Servers/Clients → WebSocket Server, padrao
    /// ws://127.0.0.1:8080/). Os botoes disparam acoes do Streamer.bot (DoAction), com argumentos.
    /// Se o servidor tiver senha, o aperto de mao e o mesmo do obs-websocket (SHA-256 com sal).
    /// </summary>
    public class StreamerBotIntegration : AccountIntegration
    {
        public const string Event = "ReceiveStreamerBotState";
        public const string DefaultUrl = "ws://127.0.0.1:8080/";

        private JsonWebSocket? _socket;
        private bool _connected;
        private string? _error;
        private string? _version;
        private CancellationTokenSource? _reconnect;
        private int _nextId;

        public StreamerBotIntegration(ILogger<StreamerBotIntegration> logger, IHubContext<DeckHub> hub, StreamDeckConfigService config)
            : base(logger, hub, config) { }

        public override string Name => "streamerbot";
        public override string Title => "Streamer.bot";
        public override string StateEvent => Event;

        private string? Url => Get("url");

        public override IntegrationStatus GetStatus() => new()
        {
            Connected = _connected,
            Configured = Url != null,
            Account = _connected ? (_version != null ? $"Streamer.bot {_version}" : Url) : null,
            Message = _connected ? null : _error,
            Fields = new() { ["url"] = Url ?? DefaultUrl },
        };

        public override object GetState() => new { connected = _connected };

        /// <summary>"127.0.0.1:8080" ou "ws://..." para uma URL ws:// valida.</summary>
        public static Uri ParseUrl(string? value)
        {
            var s = (value ?? "").Trim();
            if (s.Length == 0) s = DefaultUrl;
            if (!s.Contains("://")) s = "ws://" + s;
            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || (uri.Scheme != "ws" && uri.Scheme != "wss"))
                throw new InvalidOperationException("Endereço inválido. Ex.: ws://127.0.0.1:8080/");
            return uri;
        }

        public override async Task<IntegrationStatus> ConfigureAsync(Dictionary<string, string> fields)
        {
            var url = ParseUrl(fields.GetValueOrDefault("url"));
            var password = fields.GetValueOrDefault("password") ?? "";
            Set(("url", url.ToString()), ("password", password.Length > 0 ? password : Get("password")));
            Restart();
            for (var i = 0; i < 40 && !_connected && _error == null; i++) await Task.Delay(250);
            return GetStatus();
        }

        public override Task LogoutAsync()
        {
            Clear();
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
                if (Url == null)
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
                            ? "Não foi possível conectar ao Streamer.bot. Ligue o WebSocket Server dele (Servers/Clients)."
                            : ex.Message;
                        Logger.LogInformation("Streamer.bot: {Error}", ex.Message);
                    }
                }
                if (_connected)
                {
                    _connected = false;
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
            using var socket = new JsonWebSocket(m => m["id"]?.ToString());
            _socket = socket;
            using (var connect = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                connect.CancelAfter(TimeSpan.FromSeconds(5));
                await socket.ConnectAsync(ParseUrl(Url), connect.Token);
            }

            // Versoes novas mandam "Hello" logo ao conectar (com o desafio, se houver senha);
            // as antigas nao mandam nada. Cancelar uma leitura derruba o WebSocket, entao o
            // Hello vem pelo laco de leitura, com prazo.
            var helloTcs = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
            var run = socket.RunAsync(m =>
            {
                if (m["request"]?.GetValue<string>() == "Hello") helloTcs.TrySetResult(m);
            }, ct);
            var first = await Task.WhenAny(helloTcs.Task, run, Task.Delay(TimeSpan.FromSeconds(2), ct));
            if (first == run)
            {
                await run;
                throw new WebSocketException("O Streamer.bot fechou a conexão.");
            }
            ct.ThrowIfCancellationRequested();
            var hello = helloTcs.Task.IsCompletedSuccessfully ? helloTcs.Task.Result : null;
            _version = hello?["info"]?["version"]?.GetValue<string>();

            if (hello?["authentication"] is JsonObject auth)
            {
                var password = Get("password") ?? throw new InvalidOperationException("O Streamer.bot pede senha. Preencha a senha do WebSocket Server.");
                var id = NextId();
                var reply = await socket.RequestAsync(id, new JsonObject
                {
                    ["request"] = "Authenticate",
                    ["id"] = id,
                    ["authentication"] = AuthResponse(password, auth["salt"]?.GetValue<string>() ?? "", auth["challenge"]?.GetValue<string>() ?? ""),
                }, TimeSpan.FromSeconds(5), ct);
                if (reply["status"]?.GetValue<string>() != "ok")
                    throw new InvalidOperationException("Senha recusada pelo Streamer.bot.");
            }

            _connected = true;
            _error = null;
            await BroadcastAsync();
            await run;
        }

        private string NextId() => "dd" + Interlocked.Increment(ref _nextId);

        /// <summary>base64(sha256(base64(sha256(senha + sal)) + desafio)), como no obs-websocket.</summary>
        public static string AuthResponse(string password, string salt, string challenge)
        {
            var secret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret + challenge)));
        }

        /// <summary>
        /// Pedido DoAction. Um GUID vai como id; qualquer outro texto, como nome da acao.
        /// Argumentos: JSON ({"chave": "valor"}) ou uma linha "chave=valor" por argumento.
        /// </summary>
        public static JsonObject BuildDoAction(string id, string action, string args)
        {
            var a = action.Trim();
            if (a.Length == 0) throw new InvalidOperationException("Escolha uma ação do Streamer.bot.");
            var target = Guid.TryParse(a, out _) ? new JsonObject { ["id"] = a } : new JsonObject { ["name"] = a };
            var request = new JsonObject { ["request"] = "DoAction", ["id"] = id, ["action"] = target };
            if (ParseArgs(args) is { Count: > 0 } parsed) request["args"] = parsed;
            return request;
        }

        public static JsonObject? ParseArgs(string args)
        {
            var text = args.Trim();
            if (text.Length == 0) return null;
            if (text.StartsWith('{'))
            {
                try { return JsonNode.Parse(text) as JsonObject ?? throw new FormatException(); }
                catch (Exception) { throw new InvalidOperationException("Argumentos em JSON inválidos."); }
            }
            var result = new JsonObject();
            foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var eq = line.IndexOf('=');
                if (eq <= 0) continue;
                result[line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }
            return result;
        }

        private async Task<JsonNode> RequestAsync(Func<string, JsonObject> build)
        {
            var socket = _socket;
            if (!_connected || socket == null) throw new InvalidOperationException("Streamer.bot desconectado.");
            var id = NextId();
            JsonNode reply;
            try { reply = await socket.RequestAsync(id, build(id), TimeSpan.FromSeconds(8)); }
            catch (TimeoutException) { throw new TimeoutException("O Streamer.bot não respondeu."); }
            if (reply["status"]?.GetValue<string>() is { } status && status != "ok")
                throw new InvalidOperationException(reply["error"]?.GetValue<string>() ?? "O Streamer.bot recusou.");
            return reply;
        }

        public Task DoActionAsync(string action, string args) => RequestAsync(id => BuildDoAction(id, action, args));

        // ---- Listas do editor ----

        public override async Task<List<FieldOption>> OptionsAsync(string list, IQueryCollection query)
        {
            if (list != "actions") return await base.OptionsAsync(list, query);
            var reply = await RequestAsync(id => new JsonObject { ["request"] = "GetActions", ["id"] = id });
            return ParseActions(reply);
        }

        /// <summary>Acoes ativas, agrupadas pelo grupo do Streamer.bot ("Grupo / Acao").</summary>
        public static List<FieldOption> ParseActions(JsonNode reply) =>
            (reply["actions"] as JsonArray ?? new JsonArray())
                .Where(a => a?["enabled"]?.GetValue<bool>() != false && a?["id"] != null)
                .Select(a =>
                {
                    var name = a!["name"]?.GetValue<string>() ?? a["id"]!.ToString();
                    var group = a["group"]?.GetValue<string>();
                    return new FieldOption(a["id"]!.ToString(), string.IsNullOrEmpty(group) ? name : $"{group} / {name}");
                })
                .OrderBy(o => o.Label, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
    }
}
