using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Integrations;
using DroidDeck.Services.Tuya;
using DroidDeck.Lib;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;

namespace DroidDeck.Controllers
{
    /// <summary>
    /// Botao controle deslizante: arrastar o dedo muda um valor no PC. Todos os alvos falam
    /// em 0 a <c>max</c> (100, menos o volume de uma pessoa no Discord, que vai a 200, e a
    /// posicao da musica, em segundos).
    ///
    /// target:
    /// - "output" (padrao) / "input": volume do som / microfone padrao do Windows;
    /// - "app" (app = processo sem .exe): volume de um app;
    /// - "monitor": brilho dos monitores;
    /// - "media": posicao da faixa que esta tocando;
    /// - "obs" (inputName): volume de uma fonte de audio do OBS;
    /// - "discord_user" (userId): volume de uma pessoa na call;
    /// - "tuya" (deviceId, code), "homeassistant" (entityId), "hue" (hueTarget, id),
    ///   "govee" (ip), "keylight" (host): brilho de uma luz.
    /// </summary>
    [ApiController]
    [Route("api/slider")]
    public class SliderController : ControllerBase
    {
        private readonly MixerService _mixer;
        private readonly IServiceProvider _services;

        public SliderController(MixerService mixer, IServiceProvider services)
        {
            _mixer = mixer;
            _services = services;
        }

        public class SliderRequest
        {
            public string? Target { get; set; }
            public string? App { get; set; }
            public int Volume { get; set; }
            /// <summary>Parametros do alvo (dispositivo, fonte, pessoa...).</summary>
            public Dictionary<string, string>? Params { get; set; }
        }

        /// <summary>Estado do controle. Text: o que mostrar no lugar do numero (posicao "1:23").</summary>
        public record SliderState(double Volume, bool Mute, string? Label, double Max = 100, string? Text = null);

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? target, [FromQuery] string? app)
        {
            var p = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            if (app != null) p["app"] = app;
            return Result(await TryApply(target, p, null));
        }

        [HttpPut]
        public async Task<IActionResult> Set([FromBody] SliderRequest request)
        {
            var p = request.Params ?? new Dictionary<string, string>();
            if (request.App != null) p["app"] = request.App;
            return Result(await TryApply(request.Target, p, Math.Max(0, request.Volume)));
        }

        /// <summary>
        /// Escolhas do editor para alvos sem lista propria: lampadas Tuya com brilho e pessoas
        /// na call do Discord. As outras listas reaproveitam as rotas das acoes.
        /// </summary>
        [HttpGet("options")]
        public IActionResult Options([FromQuery] string? target)
        {
            switch (target)
            {
                case "tuya":
                    var tuya = _services.GetRequiredService<TuyaService>();
                    return Ok(tuya.Devices
                        .Where(d => BrightnessCode(d) != null)
                        .Select(d => new FieldOption(d.Id, d.Name))
                        .ToList());
                case "discord_user":
                    var discord = _services.GetRequiredService<DiscordRpcService>();
                    var people = JsonSerializer.SerializeToElement(discord.Participants);
                    return Ok(people.EnumerateArray()
                        .Select(p => new FieldOption(
                            p.GetProperty("id").GetString() ?? "",
                            (p.TryGetProperty("nick", out var n) && n.GetString() is { Length: > 0 } nick ? nick : null)
                                ?? p.GetProperty("username").GetString() ?? ""))
                        .Where(o => o.Value.Length > 0)
                        .ToList());
                default:
                    return Ok(Array.Empty<FieldOption>());
            }
        }

        private IActionResult Result((SliderState? state, string? error) r) =>
            r.state != null ? Ok(r.state)
            : r.error != null ? StatusCode(503, new { error = r.error })
            : NotFound();

        private async Task<(SliderState?, string?)> TryApply(string? target, Dictionary<string, string> p, int? value)
        {
            try
            {
                return (await Apply((target ?? "output").ToLowerInvariant(), p, value), null);
            }
            catch (Exception ex) when (ex is InvalidOperationException or TimeoutException or HttpRequestException)
            {
                // Integracao desconectada, aparelho fora da rede: o celular mostra o aviso.
                return (null, ex.Message);
            }
        }

        private static string P(Dictionary<string, string> p, string key) =>
            p.TryGetValue(key, out var v) ? v.Trim() : "";

        /// <summary>Le (value null) ou muda. Null se o alvo nao existe agora.</summary>
        private async Task<SliderState?> Apply(string target, Dictionary<string, string> p, int? value)
        {
            switch (target)
            {
                case "app":
                    return AppVolume(P(p, "app"), value);
                case "monitor":
                    return Monitor(value);
                case "media":
                    return await Media(value);
                case "obs":
                    return await Obs(P(p, "inputName"), value);
                case "discord_user":
                    return await DiscordUser(P(p, "userId"), value);
                case "tuya":
                    return await Tuya(P(p, "deviceId"), P(p, "code"), value);
                case "homeassistant":
                    return await HomeAssistant(P(p, "entityId"), value);
                case "hue":
                    return await Hue(P(p, "hueTarget") == "group" ? "groups" : "lights", P(p, "id"), value);
                case "govee":
                    return await Govee(P(p, "ip"), value);
                case "keylight":
                    return await KeyLight(P(p, "host"), value);
            }

            var flow = target == "input" ? DataFlow.Capture : DataFlow.Render;
            var id = _mixer.GetDefaultDeviceId(flow);
            if (id == null) return null;
            var state = _mixer.SetDeviceAudio(id, volume: value.HasValue ? Math.Min(value.Value, 100) : null);
            return state == null ? null : new SliderState(state.Volume, state.Mute, state.Title);
        }

        // ---- PC ----

        /// <summary>Volume das sessoes de audio de um app no dispositivo de saida padrao.</summary>
        private static SliderState? AppVolume(string app, int? volume)
        {
            if (app.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) app = app[..^4];
            if (app.Length == 0) return null;

            using var en = new MMDeviceEnumerator();
            using var device = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;
            int? current = null;
            var mute = false;
            for (var i = 0; i < sessions.Count; i++)
            {
                using var session = sessions[i];
                if (session.IsSystemSoundsSession) continue;
                string name;
                try
                {
                    using var proc = Process.GetProcessById((int)session.GetProcessID);
                    name = proc.ProcessName;
                }
                catch
                {
                    continue;
                }
                if (!name.Equals(app, StringComparison.OrdinalIgnoreCase)) continue;
                // Um app pode ter varias sessoes (navegador com abas): mexe em todas.
                if (volume.HasValue) session.SimpleAudioVolume.Volume = Math.Min(volume.Value, 100) / 100f;
                current ??= (int)Math.Round(session.SimpleAudioVolume.Volume * 100);
                mute |= session.SimpleAudioVolume.Mute;
            }
            return current == null ? null : new SliderState(current.Value, mute, app);
        }

        private static SliderState? Monitor(int? value)
        {
            if (value.HasValue && !MonitorBrightness.Set(Math.Min(value.Value, 100)))
                throw new InvalidOperationException(L.T("Nenhum monitor aceitou o brilho (ative o DDC/CI no menu do monitor)."));
            var current = value ?? MonitorBrightness.Get();
            return current == null ? null : new SliderState(current.Value, false, L.T("Brilho"));
        }

        private async Task<SliderState?> Media(int? value)
        {
            var media = _services.GetRequiredService<MediaControlService>();
            var now = await media.GetNowPlayingAsync(false);
            if (now == null || now.Duration <= 0) return null;
            var position = now.Position;
            if (value.HasValue)
            {
                position = Math.Min(value.Value, now.Duration);
                await media.SeekAsync(position);
            }
            return new SliderState(Math.Round(position), false, now.Title, Math.Round(now.Duration), Clock(position));
        }

        private static string Clock(double seconds)
        {
            var t = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");
        }

        // ---- OBS e Discord ----

        private async Task<SliderState?> Obs(string inputName, int? value)
        {
            if (inputName.Length == 0) return null;
            var obs = _services.GetRequiredService<ObsService>();
            // O OBS guarda um multiplicador. O fader dele e em dB, entao 50% no multiplicador
            // ja soa quase cheio; a raiz cubica deixa o arrasto mais parecido com o do OBS.
            if (value.HasValue)
                await obs.SetInputVolumeAsync(inputName, Math.Pow(Math.Min(value.Value, 100) / 100.0, 3));
            var mul = await obs.GetInputVolumeAsync(inputName);
            return new SliderState(Math.Round(Math.Cbrt(mul) * 100), mul <= 0, inputName);
        }

        private async Task<SliderState?> DiscordUser(string userId, int? value)
        {
            if (userId.Length == 0) return null;
            var discord = _services.GetRequiredService<DiscordRpcService>();
            if (value.HasValue) await discord.SetUserVoiceAsync(userId, null, Math.Min(value.Value, 200));
            // O Discord nao tem leitura por pessoa: vem do canal de voz (ou do ultimo ajuste).
            var volume = discord.GetUserVolume(userId) ?? 100;
            return new SliderState(Math.Round(volume), false, ParticipantName(discord, userId), 200);
        }

        /// <summary>Apelido (ou usuario) de quem esta na call; null se a pessoa saiu.</summary>
        private static string? ParticipantName(DiscordRpcService discord, string userId)
        {
            foreach (var p in JsonSerializer.SerializeToElement(discord.Participants).EnumerateArray())
            {
                if (p.GetProperty("id").GetString() != userId) continue;
                return p.TryGetProperty("nick", out var n) && n.GetString() is { Length: > 0 } nick
                    ? nick
                    : p.GetProperty("username").GetString();
            }
            return null;
        }

        // ---- Luzes ----

        private async Task<SliderState?> Tuya(string deviceId, string code, int? value)
        {
            var tuya = _services.GetRequiredService<TuyaService>();
            var device = tuya.GetDevice(deviceId);
            if (device == null) return null;
            if (code.Length == 0) code = BrightnessCode(device) ?? "";
            if (code.Length == 0) return null;
            var (min, max) = TuyaRange(device, code);
            if (value.HasValue)
            {
                var raw = (long)Math.Round(min + (max - min) * Math.Min(value.Value, 100) / 100.0);
                await tuya.SendCommandAsync(deviceId, code, raw);
                device.Status[code] = raw;
            }
            var current = device.Status.TryGetValue(code, out var v) ? ToDouble(v) : min;
            var percent = max > min ? (current - min) * 100 / (max - min) : 0;
            return new SliderState(Math.Round(Math.Clamp(percent, 0, 100)), false, device.Name);
        }

        /// <summary>O DP de brilho mais comum de lampadas Tuya.</summary>
        private static string? BrightnessCode(TuyaDevice d) =>
            new[] { "bright_value_v2", "bright_value", "bright_value_1" }.FirstOrDefault(d.Functions.ContainsKey);

        private static (double min, double max) TuyaRange(TuyaDevice d, string code)
        {
            try
            {
                if (d.Functions.TryGetValue(code, out var f) && f.Values != null)
                {
                    var node = JsonNode.Parse(f.Values);
                    var min = node?["min"]?.GetValue<double>() ?? 0;
                    var max = node?["max"]?.GetValue<double>() ?? 100;
                    if (max > min) return (min, max);
                }
            }
            catch (JsonException) { }
            return (0, 100);
        }

        private static double ToDouble(object? v) => v switch
        {
            JsonElement e when e.ValueKind == JsonValueKind.Number => e.GetDouble(),
            IConvertible c => Convert.ToDouble(c, CultureInfo.InvariantCulture),
            _ => 0,
        };

        private async Task<SliderState?> HomeAssistant(string entityId, int? value)
        {
            if (entityId.Length == 0) return null;
            var ha = _services.GetRequiredService<HomeAssistantIntegration>();
            if (value.HasValue)
            {
                var pct = Math.Min(value.Value, 100);
                if (pct == 0) await ha.CallServiceAsync("light", "turn_off", entityId);
                else await ha.CallServiceAsync("light", "turn_on", entityId, new JsonObject { ["brightness_pct"] = pct });
                return new SliderState(pct, false, ha.Find(entityId)?.Name);
            }
            var e = ha.Find(entityId);
            if (e == null) return null;
            return new SliderState(e.State == "on" ? e.Brightness ?? 100 : 0, false, e.Name);
        }

        private async Task<SliderState?> Hue(string kind, string id, int? value)
        {
            if (id.Length == 0) return null;
            var hue = _services.GetRequiredService<HueIntegration>();
            var stateKey = kind == "groups" ? "action" : "state";
            if (value.HasValue)
            {
                var pct = Math.Min(value.Value, 100);
                var body = pct == 0
                    ? new JsonObject { ["on"] = false }
                    : new JsonObject { ["on"] = true, ["bri"] = HueProvider.ToHue(pct) };
                await hue.CallAsync(HttpMethod.Put, $"{kind}/{id}/{stateKey}", body);
            }
            var node = await hue.CallAsync(HttpMethod.Get, $"{kind}/{id}");
            var state = node?[stateKey];
            var on = state?["on"]?.GetValue<bool>() ?? false;
            var bri = state?["bri"]?.GetValue<int>() ?? 0;
            return new SliderState(on ? Math.Round(bri * 100 / 254.0) : 0, false, node?["name"]?.GetValue<string>());
        }

        private async Task<SliderState?> Govee(string ip, int? value)
        {
            if (ip.Length == 0) return null;
            var govee = _services.GetRequiredService<GoveeLan>();
            if (value.HasValue)
            {
                var pct = Math.Min(value.Value, 100);
                await govee.SendAsync(ip, pct == 0 ? GoveeLan.Turn(false) : GoveeLan.Brightness(pct));
                if (pct > 0) await govee.SendAsync(ip, GoveeLan.Turn(true));
                return new SliderState(pct, false, null);
            }
            var on = await govee.IsOnAsync(ip, TimeSpan.FromSeconds(1.5));
            if (on == false) return new SliderState(0, false, null);
            var level = await govee.BrightnessAsync(ip, TimeSpan.FromSeconds(1.5));
            return level == null ? throw new TimeoutException(L.T("A luz Govee não respondeu.")) : new SliderState(level.Value, false, null);
        }

        private async Task<SliderState?> KeyLight(string host, int? value)
        {
            if (host.Length == 0) return null;
            var http = _services.GetRequiredService<IHttpClientFactory>().CreateClient();
            http.Timeout = TimeSpan.FromSeconds(3);
            var url = KeyLightProvider.LightsUrl(host);
            if (value.HasValue)
            {
                var pct = Math.Min(value.Value, 100);
                var body = new JsonObject
                {
                    ["numberOfLights"] = 1,
                    ["lights"] = new JsonArray(pct == 0
                        ? new JsonObject { ["on"] = 0 }
                        : new JsonObject { ["on"] = 1, ["brightness"] = Math.Max(3, pct) }),
                };
                using var put = await http.PutAsync(url, new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json"));
                put.EnsureSuccessStatusCode();
            }
            var json = JsonNode.Parse(await http.GetStringAsync(url));
            var light = json?["lights"]?[0];
            var on = light?["on"]?.GetValue<int>() == 1;
            return new SliderState(on ? light?["brightness"]?.GetValue<int>() ?? 0 : 0, false, null);
        }
    }
}
