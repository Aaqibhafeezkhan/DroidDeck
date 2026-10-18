using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Auth;
using DroidDeck.Integrations;
using DroidDeck.Lib;
using DroidDeck.Models;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using MQTTnet.Protocol;
using System.Net.Http.Json;
using Xunit;

namespace DroidDeck.Tests
{
    internal static class Ctx
    {
        public static ActionContext Of(string type, params (string key, string value)[] parameters)
        {
            var action = new DeckAction { Type = type };
            foreach (var (k, v) in parameters) action.Parameters[k] = v;
            return new ActionContext(action, null, (_, _) => Task.CompletedTask);
        }
    }

    // ---- Voicemeeter ----

    public class VoicemeeterTests
    {
        private class FakeApi : IVoicemeeterApi
        {
            public Dictionary<string, float> Values { get; } = new();
            public List<(int button, float value)> Macro { get; } = new();

            public float Get(string parameter) => Values.TryGetValue(parameter, out var v) ? v : 0;
            public void Set(string parameter, float value) => Values[parameter] = value;
            public void SetMacroButton(int button, float value) => Macro.Add((button, value));
        }

        [Theory]
        [InlineData("strip", 1, "Mute", "Strip[0].Mute")]
        [InlineData("bus", 3, "Gain", "Bus[2].Gain")]
        [InlineData("BUS", 99, "Mute", "Bus[7].Mute")]
        [InlineData("qualquer", 0, "Mute", "Strip[0].Mute")]
        public void Parametro_ContaAPartirDeUm(string target, int number, string name, string expected) =>
            Assert.Equal(expected, VoicemeeterProvider.Parameter(target, number, name));

        [Theory]
        [InlineData("\"C:\\Program Files (x86)\\VB\\Voicemeeter\\voicemeeterprosetup.exe\" /uninstall", @"C:\Program Files (x86)\VB\Voicemeeter")]
        [InlineData(@"C:\Program Files (x86)\VB\Voicemeeter\voicemeetersetup.exe", @"C:\Program Files (x86)\VB\Voicemeeter")]
        [InlineData(@"C:\VB\Voicemeeter\setup.exe -u", @"C:\VB\Voicemeeter")]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void PastaDoDesinstalador(string? value, string? expected) =>
            Assert.Equal(expected, VoicemeeterRemote.DirFromUninstallString(value));

        [Theory]
        [InlineData("Strip[0].A1")]
        [InlineData("Fx.Reverb.On")]
        [InlineData("Bus[1].Gain")]
        public void ParametroValido(string p) => Assert.Equal(p, VoicemeeterProvider.CheckParameter(p));

        [Theory]
        [InlineData("")]
        [InlineData("Strip[0].A1; Command.Shutdown = 1")]
        [InlineData("Command.Load = \"c:\\x.xml\"")]
        public void ParametroInvalido_Lanca(string p) =>
            Assert.Throws<InvalidOperationException>(() => VoicemeeterProvider.CheckParameter(p));

        [Fact]
        public async Task Mudo_Alterna()
        {
            var api = new FakeApi();
            var p = new VoicemeeterProvider(api);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "toggle_mute"), ("target", "bus"), ("index", "2")));
            Assert.Equal(1, api.Values["Bus[1].Mute"]);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "toggle_mute"), ("target", "bus"), ("index", "2")));
            Assert.Equal(0, api.Values["Bus[1].Mute"]);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "mute"), ("index", "1")));
            Assert.Equal(1, api.Values["Strip[0].Mute"]);
        }

        [Fact]
        public async Task Ganho_SomaERespeitaOLimite()
        {
            var api = new FakeApi();
            api.Values["Strip[0].Gain"] = 10;
            var p = new VoicemeeterProvider(api);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "adjust_gain"), ("step", "6")));
            Assert.Equal(12, api.Values["Strip[0].Gain"]);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "set_gain"), ("gain", "-90")));
            Assert.Equal(-60, api.Values["Strip[0].Gain"]);
        }

        [Fact]
        public async Task MacroButton_ApertaESolta()
        {
            var api = new FakeApi();
            await new VoicemeeterProvider(api).ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "macro"), ("button", "5")));
            Assert.Equal(new[] { (5, 1f), (5, 0f) }, api.Macro);
        }

        [Fact]
        public async Task Parametro_Alterna()
        {
            var api = new FakeApi();
            var p = new VoicemeeterProvider(api);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "param_toggle"), ("parameter", "Strip[0].A1")));
            Assert.Equal(1, api.Values["Strip[0].A1"]);
            await p.ExecuteAsync(Ctx.Of("voicemeeter", ("operation", "param_set"), ("parameter", "Strip[0].A1"), ("value", "0")));
            Assert.Equal(0, api.Values["Strip[0].A1"]);
        }
    }

    // ---- Elgato ----

    public class KeyLightTests
    {
        private class FakeHandler : HttpMessageHandler
        {
            public string GetBody { get; set; } = "{}";
            public List<(HttpMethod method, Uri uri, string? body)> Requests { get; } = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(ct);
                Requests.Add((request.Method, request.RequestUri!, body));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(request.Method == HttpMethod.Get ? GetBody : "{}"),
                };
            }
        }

        private class Factory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;
            public Factory(HttpMessageHandler handler) => _handler = handler;
            public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
        }

        [Theory]
        [InlineData("192.168.0.50", "http://192.168.0.50:9123/elgato/lights")]
        [InlineData(" 192.168.0.50:9000 ", "http://192.168.0.50:9000/elgato/lights")]
        [InlineData("http://keylight.local/", "http://keylight.local:9123/elgato/lights")]
        public void Url(string host, string expected) => Assert.Equal(expected, KeyLightProvider.LightsUrl(host).ToString());

        [Theory]
        [InlineData("")]
        [InlineData("192.168.0.50/outra/coisa")]
        public void UrlInvalida_Lanca(string host) =>
            Assert.Throws<InvalidOperationException>(() => KeyLightProvider.LightsUrl(host));

        [Theory]
        [InlineData(7000, 143)]
        [InlineData(2900, 344)]
        [InlineData(5000, 200)]
        [InlineData(100000, 143)]
        public void Temperatura_EmMired(int kelvin, int elgato) => Assert.Equal(elgato, KeyLightProvider.ToElgato(kelvin));

        [Fact]
        public void Alternar_InverteCadaLuz()
        {
            var current = JsonNode.Parse("""{"numberOfLights":2,"lights":[{"on":1,"brightness":20},{"on":0,"brightness":20}]}""") as JsonObject;
            var body = KeyLightProvider.BuildPayload("toggle", current, 0, 0, 0, 0);
            Assert.Equal(2, body["numberOfLights"]!.GetValue<int>());
            Assert.Equal(0, body["lights"]![0]!["on"]!.GetValue<int>());
            Assert.Equal(1, body["lights"]![1]!["on"]!.GetValue<int>());
        }

        [Fact]
        public void Brilho_SomaComLimite()
        {
            var current = JsonNode.Parse("""{"lights":[{"on":1,"brightness":95}]}""") as JsonObject;
            var body = KeyLightProvider.BuildPayload("brightness_step", current, 0, 10, 0, 0);
            Assert.Equal(100, body["lights"]![0]!["brightness"]!.GetValue<int>());
            Assert.Equal(1, body["lights"]![0]!["on"]!.GetValue<int>());
        }

        [Fact]
        public void Temperatura_DefinidaSemMexerNoResto()
        {
            var body = KeyLightProvider.BuildPayload("temperature", null, 0, 0, 5000, 0);
            var light = body["lights"]![0]!.AsObject();
            Assert.Equal(200, light["temperature"]!.GetValue<int>());
            Assert.False(light.ContainsKey("on"));
        }

        [Fact]
        public async Task Alternar_LeEDepoisGrava()
        {
            var handler = new FakeHandler { GetBody = """{"numberOfLights":1,"lights":[{"on":0,"brightness":40,"temperature":200}]}""" };
            await new KeyLightProvider(new Factory(handler)).ExecuteAsync(Ctx.Of("keylight", ("host", "10.0.0.9"), ("operation", "toggle")));
            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(HttpMethod.Get, handler.Requests[0].method);
            Assert.Equal(HttpMethod.Put, handler.Requests[1].method);
            Assert.Equal("http://10.0.0.9:9123/elgato/lights", handler.Requests[1].uri.ToString());
            Assert.Equal(1, JsonNode.Parse(handler.Requests[1].body!)!["lights"]![0]!["on"]!.GetValue<int>());
        }

        [Fact]
        public async Task Ligar_NaoPrecisaLer()
        {
            var handler = new FakeHandler();
            await new KeyLightProvider(new Factory(handler)).ExecuteAsync(Ctx.Of("keylight", ("host", "10.0.0.9"), ("operation", "on")));
            Assert.Single(handler.Requests);
            Assert.Equal("""{"numberOfLights":1,"lights":[{"on":1}]}""", handler.Requests[0].body);
        }
    }

    // ---- Philips Hue ----

    public class HueTests
    {
        [Fact]
        public void Pareamento_DevolveOUsuario() =>
            Assert.Equal("abc123", HueIntegration.ParsePairing("""[{"success":{"username":"abc123"}}]"""));

        [Fact]
        public void Pareamento_SemApertarOBotao_PedeParaApertar()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => HueIntegration.ParsePairing(
                """[{"error":{"type":101,"address":"","description":"link button not pressed"}}]"""));
            Assert.Contains("botão", ex.Message);
        }

        [Fact]
        public void ErroDoBridge_ViraExcecao()
        {
            Assert.Throws<InvalidOperationException>(() => HueIntegration.ThrowIfError(JsonNode.Parse(
                """[{"error":{"type":1,"address":"/lights","description":"unauthorized user"}}]""")));
            HueIntegration.ThrowIfError(JsonNode.Parse("""[{"success":{"/lights/1/state/on":true}}]"""));
            HueIntegration.ThrowIfError(JsonNode.Parse("""{"1":{"name":"Sala"}}"""));
        }

        [Theory]
        [InlineData("192.168.1.2", "192.168.1.2")]
        [InlineData(" http://192.168.1.2/ ", "192.168.1.2")]
        [InlineData("", "")]
        public void Endereco(string input, string expected) => Assert.Equal(expected, HueIntegration.NormalizeHost(input));

        [Fact]
        public void Estado_LigarDesligarAlternarBrilho()
        {
            Assert.Equal("""{"on":false}""", HueProvider.BuildState("toggle", true, 0, 0).ToJsonString());
            Assert.Equal("""{"on":true}""", HueProvider.BuildState("toggle", false, 0, 0).ToJsonString());
            Assert.Equal("""{"on":true,"bri":254}""", HueProvider.BuildState("brightness", null, 100, 0).ToJsonString());
            // Minimo de 1 %.
            Assert.Equal("""{"on":true,"bri":3}""", HueProvider.BuildState("brightness", null, 0, 0).ToJsonString());
            Assert.Equal("""{"on":true,"bri_inc":25}""", HueProvider.BuildState("brightness_step", null, 0, 10).ToJsonString());
            Assert.Equal("""{"bri_inc":-25}""", HueProvider.BuildState("brightness_step", null, 0, -10).ToJsonString());
        }

        [Fact]
        public void Aceso_LuzOuSala()
        {
            Assert.True(HueProvider.IsOn(JsonNode.Parse("""{"state":{"on":true}}"""), group: false));
            Assert.True(HueProvider.IsOn(JsonNode.Parse("""{"state":{"all_on":false,"any_on":true}}"""), group: true));
            Assert.False(HueProvider.IsOn(JsonNode.Parse("""{"state":{"any_on":false}}"""), group: true));
        }

        [Fact]
        public void Opcoes_FiltramGruposInternosECenasTemporarias()
        {
            var groups = JsonNode.Parse("""
                {"1":{"name":"Sala","type":"Room"},"2":{"name":"Jogo","type":"Entertainment"},"3":{"name":"Andar de cima","type":"Zone"}}
                """)!.AsObject();
            var g = HueIntegration.ParseOptions("groups", groups);
            Assert.Equal(new[] { "0", "3", "1" }, g.Select(o => o.Value));

            var scenes = JsonNode.Parse("""
                {"aB1":{"name":"Relaxar","group":"1","type":"GroupScene"},
                 "tmp":{"name":"x","recycle":true},
                 "cD2":{"name":"Leitura","type":"LightScene"}}
                """)!.AsObject();
            var s = HueIntegration.ParseOptions("scenes", scenes, groups);
            Assert.Equal(new[] { "Leitura", "Relaxar (Sala)" }, s.Select(o => o.Label));
        }
    }

    // ---- Govee ----

    public class GoveeTests
    {
        [Fact]
        public void Mensagens_NoFormatoDaApiLan()
        {
            Assert.Equal("""{"msg":{"cmd":"turn","data":{"value":1}}}""", GoveeLan.Turn(true).ToJsonString());
            Assert.Equal("""{"msg":{"cmd":"brightness","data":{"value":100}}}""", GoveeLan.Brightness(250).ToJsonString());
            Assert.Equal("""{"msg":{"cmd":"scan","data":{"account_topic":"reserve"}}}""", GoveeLan.Scan().ToJsonString());
            Assert.Equal("""{"msg":{"cmd":"colorwc","data":{"color":{"r":255,"g":128,"b":0},"colorTemInKelvin":0}}}""",
                GoveeLan.Color(255, 128, 0).ToJsonString());
            Assert.Equal(9000, GoveeLan.Color(0, 0, 0, 12000)["msg"]!["data"]!["colorTemInKelvin"]!.GetValue<int>());
        }

        [Theory]
        [InlineData("#FF8000", 255, 128, 0)]
        [InlineData("00ff00", 0, 255, 0)]
        [InlineData("#abc", 0xAA, 0xBB, 0xCC)]
        public void Cor_Hexadecimal(string hex, int r, int g, int b) => Assert.Equal((r, g, b), GoveeLan.ParseHex(hex));

        [Fact]
        public void CorInvalida_Lanca() => Assert.Throws<InvalidOperationException>(() => GoveeLan.ParseHex("laranja"));

        [Fact]
        public void RespostaDaBusca()
        {
            var d = GoveeLan.ParseScan(JsonNode.Parse("""
                {"msg":{"cmd":"scan","data":{"ip":"192.168.1.23","device":"1F:80:C5:32:32:36:72:4E","sku":"H618E","bleVersionHard":"3.01.01"}}}
                """))!;
            Assert.Equal(new GoveeDevice("192.168.1.23", "H618E", "1F:80:C5:32:32:36:72:4E"), d);
            Assert.Null(GoveeLan.ParseScan(JsonNode.Parse("""{"msg":{"cmd":"devStatus","data":{}}}""")));
            Assert.Null(GoveeLan.ParseScan(JsonNode.Parse("""{"msg":{"cmd":"scan","data":{"ip":"nao-e-ip"}}}""")));
        }

        [Fact]
        public void RespostaDoStatus()
        {
            Assert.True(GoveeLan.ParseOn(JsonNode.Parse("""{"msg":{"cmd":"devStatus","data":{"onOff":1,"brightness":100}}}""")));
            Assert.False(GoveeLan.ParseOn(JsonNode.Parse("""{"msg":{"cmd":"devStatus","data":{"onOff":0}}}""")));
            Assert.Null(GoveeLan.ParseOn(JsonNode.Parse("""{"msg":{"cmd":"scan","data":{}}}""")));
        }

        [Theory]
        [InlineData("")]
        [InlineData("::1")]
        [InlineData("lampada")]
        public void IpInvalido_Lanca(string ip) => Assert.Throws<InvalidOperationException>(() => GoveeLan.ParseIp(ip));
    }

    // ---- MQTT ----

    public class MqttTests
    {
        [Theory]
        [InlineData("192.168.0.10", "192.168.0.10", 1883, false)]
        [InlineData("broker.local:1884", "broker.local", 1884, false)]
        [InlineData("mqtt://10.0.0.2", "10.0.0.2", 1883, false)]
        [InlineData("mqtts://broker.exemplo.com", "broker.exemplo.com", 8883, true)]
        [InlineData("ssl://broker:9999/", "broker", 9999, true)]
        public void Endereco(string input, string host, int port, bool tls) =>
            Assert.Equal(new MqttIntegration.BrokerAddress(host, port, tls), MqttIntegration.ParseServer(input));

        [Theory]
        [InlineData("")]
        [InlineData("mqtt://")]
        [InlineData("broker/caminho")]
        public void EnderecoInvalido_Lanca(string input) =>
            Assert.Throws<InvalidOperationException>(() => MqttIntegration.ParseServer(input));

        [Fact]
        public void Mensagem()
        {
            var m = MqttIntegration.BuildMessage(" casa/sala/luz ", "ON", 1, true);
            Assert.Equal("casa/sala/luz", m.Topic);
            Assert.Equal("ON", System.Text.Encoding.UTF8.GetString(m.PayloadSegment));
            Assert.Equal(MqttQualityOfServiceLevel.AtLeastOnce, m.QualityOfServiceLevel);
            Assert.True(m.Retain);
            Assert.Equal(MqttQualityOfServiceLevel.ExactlyOnce, MqttIntegration.BuildMessage("a", "", 7, false).QualityOfServiceLevel);
        }

        [Theory]
        [InlineData("")]
        [InlineData("casa/#")]
        [InlineData("casa/+/luz")]
        public void TopicoComCuringa_Lanca(string topic) =>
            Assert.Throws<InvalidOperationException>(() => MqttIntegration.BuildMessage(topic, "", 0, false));
    }

    // ---- Schema e traducoes ----

    public class LocalIntegrationsSchemaTests : IClassFixture<WebApplicationFactory<DroidDeck.Program>>
    {
        private static readonly string[] Types = { "voicemeeter", "keylight", "hue", "govee", "mqtt", "streamerbot", "vtubestudio" };

        private readonly WebApplicationFactory<DroidDeck.Program> _factory;

        public LocalIntegrationsSchemaTests(WebApplicationFactory<DroidDeck.Program> factory) => _factory = factory;

        private async Task<List<ActionSchema>> Schemas()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-API-KEY", ApiKeyProvider.GetKey());
            var all = (await client.GetFromJsonAsync<List<ActionSchema>>("/api/actions/schema"))!;
            return all.Where(s => Types.Contains(s.Type)).ToList();
        }

        [Fact]
        public async Task TodosRegistrados_NenhumPrivilegiado()
        {
            var schemas = await Schemas();
            Assert.Equal(Types.OrderBy(t => t), schemas.Select(s => s.Type).OrderBy(t => t));
            Assert.All(schemas, s => Assert.False(s.Privileged));
            // Os que dependem de configuracao avisam quando estao desconectados.
            Assert.All(schemas.Where(s => s.Settings != null), s => Assert.NotNull(s.StateEvent));
        }

        private static Dictionary<string, string> Dictionary(string name) =>
            (Dictionary<string, string>)typeof(L).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        [Fact]
        public async Task TodoTextoTemInglesEEspanhol()
        {
            var en = Dictionary("En");
            var es = Dictionary("Es");
            var texts = new List<string>();
            foreach (var s in await Schemas())
            {
                if (s.Description != null) texts.Add(s.Description);
                foreach (var f in s.Fields)
                {
                    texts.Add(f.Label);
                    if (f.Help != null) texts.Add(f.Help);
                    texts.AddRange((f.Options ?? new()).Select(o => o.Label));
                }
            }
            Assert.Empty(texts.Where(t => !en.ContainsKey(t)));
            Assert.Empty(texts.Where(t => !es.ContainsKey(t)));
        }
    }
}
