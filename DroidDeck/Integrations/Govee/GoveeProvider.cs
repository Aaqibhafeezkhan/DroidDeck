using System;
using System.Threading.Tasks;
using DroidDeck.Actions;

namespace DroidDeck.Integrations
{
    /// <summary>Acao "govee": liga/desliga, brilho, cor e temperatura pela API LAN da Govee.</summary>
    public class GoveeProvider : IActionProvider
    {
        public const string TypeName = "govee";

        private readonly GoveeLan _lan;

        public GoveeProvider(GoveeLan lan) => _lan = lan;

        public string Type => TypeName;

        public ActionSchema Describe() => new()
        {
            Type = TypeName,
            Title = "Govee",
            Icon = "light_mode",
            Category = "Casa",
            Description = "Lâmpadas e fitas Govee pela rede local (precisa do Controle LAN ligado no app Govee Home).",
            Fields =
            {
                ActionField.Dynamic("ip", "Aparelho", "/api/govee/devices",
                    "Procura na rede ao abrir. Se não aparecer, digite o IP do aparelho.", allowCustom: true),
                ActionField.Select("operation", "O que fazer",
                    ("toggle", "Liga/desliga"),
                    ("on", "Ligar"),
                    ("off", "Desligar"),
                    ("brightness", "Definir o brilho"),
                    ("color", "Definir a cor"),
                    ("temperature", "Definir a temperatura (branco)")),
                ActionField.Number("brightness", "Brilho", 100, 1, 100, "%").When("operation", "brightness"),
                ActionField.Text("color", "Cor", "#FF8000", "No formato #RRGGBB.").WithDefault("#FF8000").When("operation", "color"),
                ActionField.Number("kelvin", "Temperatura", 4000, 2000, 9000, "K").When("operation", "temperature"),
            },
        };

        public async Task ExecuteAsync(ActionContext ctx)
        {
            var ip = ctx.Get("ip").Trim();
            switch (ctx.Operation("toggle"))
            {
                case "on":
                    await _lan.SendAsync(ip, GoveeLan.Turn(true));
                    break;
                case "off":
                    await _lan.SendAsync(ip, GoveeLan.Turn(false));
                    break;
                case "toggle":
                    var on = await _lan.IsOnAsync(ip, TimeSpan.FromSeconds(2))
                        ?? throw new InvalidOperationException("O aparelho Govee não respondeu. Confira o IP e o Controle LAN no app Govee Home.");
                    await _lan.SendAsync(ip, GoveeLan.Turn(!on));
                    break;
                case "brightness":
                    await _lan.SendAsync(ip, GoveeLan.Brightness(ctx.Int("brightness", 100)));
                    break;
                case "color":
                    var (r, g, b) = GoveeLan.ParseHex(ctx.Get("color", "#FF8000"));
                    await _lan.SendAsync(ip, GoveeLan.Color(r, g, b));
                    break;
                case "temperature":
                    await _lan.SendAsync(ip, GoveeLan.Color(0, 0, 0, ctx.Int("kelvin", 4000)));
                    break;
                default:
                    throw new InvalidOperationException("Operação desconhecida.");
            }
        }
    }
}
