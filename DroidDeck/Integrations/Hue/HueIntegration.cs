using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Hubs;
using DroidDeck.Lib;
using DroidDeck.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// Philips Hue pela API local (v1) do Hue Bridge, sem conta na nuvem. O pareamento e o
    /// do proprio Hue: aperta o botao do bridge e, em ate 30 s, o DroidDeck pede um usuario
    /// (POST /api). O usuario fica cifrado com o resto da configuracao.
    /// </summary>
    public class HueIntegration : AccountIntegration
    {
        public const string Event = "ReceiveHueState";
        private const string DiscoveryUrl = "https://discovery.meethue.com/";

        private readonly IHttpClientFactory _http;
        private bool _connected;
        private string? _error;
        private string? _bridgeName;
        private CancellationTokenSource? _recheck;

        public HueIntegration(ILogger<HueIntegration> logger, IHubContext<DeckHub> hub, StreamDeckConfigService config,
            IHttpClientFactory http) : base(logger, hub, config)
        {
            _http = http;
        }

        public override string Name => "hue";
        public override string Title => "Philips Hue";
        public override string StateEvent => Event;

        private string? Bridge => Get("bridge");
        private string? Username => Get("username");

        public override IntegrationStatus GetStatus() => new()
        {
            Connected = _connected,
            Configured = Bridge != null && Username != null,
            Account = _connected ? (_bridgeName ?? Bridge) : null,
            Message = _connected ? null : _error,
            Fields = new() { ["bridge"] = Bridge ?? "" },
        };

        public override object GetState() => new { connected = _connected };

        public override async Task<IntegrationStatus> ConfigureAsync(Dictionary<string, string> fields)
        {
            var bridge = NormalizeHost(fields.GetValueOrDefault("bridge"));
            if (bridge.Length == 0)
            {
                bridge = await DiscoverAsync()
                    ?? throw new InvalidOperationException("Nenhum Hue Bridge encontrado. Digite o IP dele (aparece no app Hue, em Configurações → Bridges).");
            }

            // Mesmo bridge e usuario que ainda funciona: so confere.
            if (bridge != Bridge || Username == null || !await CheckAsync())
            {
                var username = await PairAsync(bridge);
                Set(("bridge", bridge), ("username", username));
            }
            await CheckAsync();
            _recheck?.Cancel();
            return GetStatus();
        }

        public override Task LogoutAsync()
        {
            Clear();
            _connected = false;
            _error = null;
            _bridgeName = null;
            _ = BroadcastAsync();
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Sem conexao permanente: confere o bridge ao abrir e de tempos em tempos, para o
            // aviso de "desconectado" do editor ficar certo.
            while (!stoppingToken.IsCancellationRequested)
            {
                _recheck = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                if (Bridge != null && Username != null) await CheckAsync();
                await Wait(TimeSpan.FromMinutes(_connected ? 5 : 1), _recheck.Token);
            }
        }

        /// <summary>Le a configuracao do bridge (so responde com nome a um usuario valido).</summary>
        private async Task<bool> CheckAsync()
        {
            var was = _connected;
            try
            {
                var config = await SendAsync(HttpMethod.Get, "config");
                _bridgeName = config?["name"]?.GetValue<string>();
                _connected = _bridgeName != null || config?["bridgeid"] != null;
                _error = _connected ? null : "O Hue Bridge recusou o acesso. Pareie de novo.";
            }
            catch (Exception ex)
            {
                _connected = false;
                _error = ex is HttpRequestException or TaskCanceledException
                    ? "Não foi possível falar com o Hue Bridge. Confira o IP."
                    : ex.Message;
            }
            if (was != _connected) await BroadcastAsync();
            return _connected;
        }

        private async Task<string?> DiscoverAsync()
        {
            try
            {
                var client = _http.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                var list = JsonNode.Parse(await client.GetStringAsync(DiscoveryUrl)) as JsonArray;
                return list?.Select(b => b?["internalipaddress"]?.GetValue<string>()).FirstOrDefault(ip => !string.IsNullOrEmpty(ip));
            }
            catch (Exception ex)
            {
                Logger.LogInformation("Hue: descoberta falhou ({Error})", ex.Message);
                return null;
            }
        }

        /// <summary>Pede um usuario ao bridge. So funciona logo depois de apertar o botao dele.</summary>
        private async Task<string> PairAsync(string bridge)
        {
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            var body = new JsonObject { ["devicetype"] = "droiddeck#" + DeviceName() };
            using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try { response = await client.PostAsync($"http://{bridge}/api", content); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new InvalidOperationException("Não foi possível falar com o Hue Bridge. Confira o IP.");
            }
            using (response)
                return ParsePairing(await response.Content.ReadAsStringAsync());
        }

        /// <summary>Nome do PC no "devicetype" (o Hue aceita ate 19 caracteres depois do #).</summary>
        private static string DeviceName()
        {
            var name = new string(Environment.MachineName.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            return name.Length == 0 ? "pc" : name[..Math.Min(name.Length, 19)];
        }

        /// <summary>Resposta do POST /api: o usuario, ou o erro (101 = botao nao apertado).</summary>
        public static string ParsePairing(string body)
        {
            var first = (JsonNode.Parse(body) as JsonArray)?.FirstOrDefault();
            if (first?["success"]?["username"]?.GetValue<string>() is { Length: > 0 } user) return user;
            var type = first?["error"]?["type"]?.GetValue<int>();
            if (type == 101) throw new InvalidOperationException("Aperte o botão redondo do Hue Bridge e toque em Parear de novo (em até 30 segundos).");
            throw new InvalidOperationException(first?["error"]?["description"]?.GetValue<string>() ?? "O Hue Bridge recusou o pareamento.");
        }

        public static string NormalizeHost(string? value)
        {
            var h = (value ?? "").Trim();
            foreach (var prefix in new[] { "http://", "https://" })
                if (h.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) h = h[prefix.Length..];
            return h.Trim('/');
        }

        // ---- Chamadas da API ----

        /// <summary>
        /// Chamada em /api/{usuario}/{caminho}. Erros do bridge (lista com "error") viram excecao.
        /// </summary>
        public async Task<JsonNode?> SendAsync(HttpMethod method, string path, JsonObject? body = null)
        {
            var bridge = Bridge;
            var user = Username;
            if (bridge == null || user == null) throw new InvalidOperationException("Philips Hue não está pareado.");
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            using var request = new HttpRequestMessage(method, $"http://{bridge}/api/{user}/{path}");
            if (body != null) request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var node = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            ThrowIfError(node);
            return node;
        }

        /// <summary>O bridge responde 200 mesmo com erro: [{"error":{"type":1,"description":"..."}}].</summary>
        public static void ThrowIfError(JsonNode? node)
        {
            if (node is not JsonArray list) return;
            var error = list.Select(i => i?["error"]).FirstOrDefault(e => e != null);
            if (error == null) return;
            if (error["type"]?.GetValue<int>() == 1) throw new InvalidOperationException("O Hue Bridge recusou o acesso. Pareie de novo.");
            throw new InvalidOperationException(error["description"]?.GetValue<string>() ?? "O Hue Bridge recusou.");
        }

        /// <summary>Executa e atualiza o "conectado" conforme o bridge respondeu.</summary>
        public async Task<JsonNode?> CallAsync(HttpMethod method, string path, JsonObject? body = null)
        {
            try
            {
                var result = await SendAsync(method, path, body);
                if (!_connected) { _connected = true; _error = null; await BroadcastAsync(); }
                return result;
            }
            catch (HttpRequestException)
            {
                if (_connected) { _connected = false; await BroadcastAsync(); }
                _error = "Não foi possível falar com o Hue Bridge. Confira o IP.";
                throw new InvalidOperationException(_error);
            }
        }

        // ---- Listas do editor ----

        public override async Task<List<FieldOption>> OptionsAsync(string list, IQueryCollection query)
        {
            if (list == "items") list = query["target"] == "group" ? "groups" : "lights";
            if (list is not ("lights" or "groups" or "scenes")) return await base.OptionsAsync(list, query);
            var node = await CallAsync(HttpMethod.Get, list) as JsonObject;
            // Cenas levam o nome da sala: "Relaxar" existe em cada uma.
            var groups = list == "scenes" ? await CallAsync(HttpMethod.Get, "groups") as JsonObject : null;
            return ParseOptions(list, node, groups);
        }

        /// <summary>Luzes, grupos ou cenas como opcoes do select (id e nome).</summary>
        public static List<FieldOption> ParseOptions(string list, JsonObject? node, JsonObject? groups = null)
        {
            var options = new List<FieldOption>();
            if (list == "groups") options.Add(new FieldOption("0", L.T("Todas as luzes")));
            if (node == null) return options;
            foreach (var (id, item) in node)
            {
                var name = item?["name"]?.GetValue<string>() ?? id;
                // Grupos de entretenimento e internos nao fazem sentido no deck.
                if (list == "groups" && item?["type"]?.GetValue<string>() is "Entertainment" or "LightSource" or "Luminaire") continue;
                if (list == "scenes")
                {
                    // Cenas temporarias que os apps criam e o bridge apaga sozinho.
                    if (item?["recycle"]?.GetValue<bool>() == true) continue;
                    if (item?["group"]?.GetValue<string>() is { } group &&
                        groups?[group]?["name"]?.GetValue<string>() is { } room)
                        name = $"{name} ({room})";
                }
                options.Add(new FieldOption(id, name));
            }
            return options.OrderBy(o => o.Value == "0" ? "" : o.Label, StringComparer.CurrentCultureIgnoreCase).ToList();
        }
    }
}
