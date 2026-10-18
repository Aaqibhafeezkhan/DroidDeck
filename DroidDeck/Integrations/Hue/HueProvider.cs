using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DroidDeck.Actions;

namespace DroidDeck.Integrations
{
    /// <summary>Acao "hue": liga/desliga, brilho e cenas de luzes e salas do Philips Hue.</summary>
    public class HueProvider : IStatefulActionProvider
    {
        public const string TypeName = "hue";

        private readonly HueIntegration _hue;

        public HueProvider(HueIntegration hue) => _hue = hue;

        public string Type => TypeName;
        public string StateEvent => HueIntegration.Event;
        public object GetState() => _hue.GetState();

        public ActionSchema Describe() => new()
        {
            Type = TypeName,
            Title = "Philips Hue",
            Icon = "emoji_objects",
            Category = "Casa",
            Description = "Luzes, salas e cenas do Philips Hue pelo Hue Bridge da sua rede, sem conta na nuvem.",
            Settings = "hue",
            StateEvent = StateEvent,
            Fields =
            {
                ActionField.Select("operation", "O que fazer",
                    ("toggle", "Liga/desliga"),
                    ("on", "Ligar"),
                    ("off", "Desligar"),
                    ("brightness", "Definir o brilho"),
                    ("brightness_step", "Brilho (+/-)"),
                    ("scene", "Ativar cena")),
                ActionField.Select("target", "Controlar",
                    ("light", "Uma luz"),
                    ("group", "Uma sala ou zona"))
                    .When("operation", "toggle", "on", "off", "brightness", "brightness_step"),
                ActionField.Dynamic("id", "Luz ou sala", "/api/integrations/hue/options/items?target={target}")
                    .When("operation", "toggle", "on", "off", "brightness", "brightness_step"),
                ActionField.Dynamic("scene", "Cena", "/api/integrations/hue/options/scenes").When("operation", "scene"),
                ActionField.Number("brightness", "Brilho", 100, 1, 100, "%").When("operation", "brightness"),
                ActionField.Number("step", "Passo do brilho", 10, -100, 100, "%").When("operation", "brightness_step"),
            },
        };

        public async Task ExecuteAsync(ActionContext ctx)
        {
            var op = ctx.Operation("toggle");
            if (op == "scene")
            {
                var scene = ctx.Get("scene").Trim();
                if (scene.Length == 0) throw new InvalidOperationException("Escolha uma cena.");
                // O grupo 0 (todas as luzes) aceita qualquer cena, de sala ou nao.
                await _hue.CallAsync(HttpMethod.Put, "groups/0/action", new JsonObject { ["scene"] = scene });
                return;
            }

            var group = ctx.Get("target", "light") == "group";
            var id = ctx.Get("id").Trim();
            if (id.Length == 0) throw new InvalidOperationException("Escolha uma luz ou sala.");
            var path = group ? $"groups/{Uri.EscapeDataString(id)}" : $"lights/{Uri.EscapeDataString(id)}";

            bool? isOn = null;
            if (op == "toggle")
            {
                var current = await _hue.CallAsync(HttpMethod.Get, path);
                isOn = IsOn(current, group);
            }
            var body = BuildState(op, isOn, ctx.Int("brightness", 100), ctx.Int("step", 10));
            await _hue.CallAsync(HttpMethod.Put, path + (group ? "/action" : "/state"), body);
        }

        /// <summary>Luz: state.on. Sala: alguma luz acesa (state.any_on).</summary>
        public static bool IsOn(JsonNode? node, bool group) =>
            (group ? node?["state"]?["any_on"] : node?["state"]?["on"])?.GetValue<bool>() == true;

        /// <summary>Corpo do PUT. O brilho do Hue vai de 1 a 254.</summary>
        public static JsonObject BuildState(string op, bool? isOn, int brightness, int step) => op switch
        {
            "on" => new JsonObject { ["on"] = true },
            "off" => new JsonObject { ["on"] = false },
            "toggle" => new JsonObject { ["on"] = isOn != true },
            "brightness" => new JsonObject { ["on"] = true, ["bri"] = ToHue(brightness) },
            // bri_inc soma ao brilho atual; diminuir nao acende uma luz apagada.
            "brightness_step" => step > 0
                ? new JsonObject { ["on"] = true, ["bri_inc"] = Math.Clamp((int)Math.Round(step * 254 / 100.0), -254, 254) }
                : new JsonObject { ["bri_inc"] = Math.Clamp((int)Math.Round(step * 254 / 100.0), -254, 254) },
            _ => throw new InvalidOperationException("Operação desconhecida."),
        };

        public static int ToHue(int percent) => Math.Clamp((int)Math.Round(Math.Clamp(percent, 1, 100) * 254 / 100.0), 1, 254);
    }
}
