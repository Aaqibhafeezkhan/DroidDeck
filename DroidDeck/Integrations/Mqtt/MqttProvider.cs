using System.Threading.Tasks;
using DroidDeck.Actions;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// Acao "mqtt": publica uma mensagem num topico do broker configurado. Nao e privilegiada:
    /// so fala com o broker que o usuario escolheu, do mesmo jeito que o Home Assistant.
    /// </summary>
    public class MqttProvider : IStatefulActionProvider
    {
        public const string TypeName = "mqtt";

        private readonly MqttIntegration _mqtt;

        public MqttProvider(MqttIntegration mqtt) => _mqtt = mqtt;

        public string Type => TypeName;
        public string StateEvent => MqttIntegration.Event;
        public object GetState() => _mqtt.GetState();

        public ActionSchema Describe() => new()
        {
            Type = TypeName,
            Title = "MQTT",
            Icon = "hub",
            Category = "Casa",
            Description = "Publica uma mensagem num tópico do seu broker MQTT (Mosquitto, Home Assistant, Node-RED, Zigbee2MQTT...).",
            Settings = "mqtt",
            StateEvent = StateEvent,
            Fields =
            {
                ActionField.Text("topic", "Tópico", "casa/sala/luz/set", required: true),
                ActionField.Multiline("payload", "Mensagem", "ON", "Texto ou JSON. Pode ficar vazia."),
                ActionField.Select("qos", "QoS",
                    ("0", "0 — no máximo uma vez"),
                    ("1", "1 — pelo menos uma vez"),
                    ("2", "2 — exatamente uma vez")),
                ActionField.Toggle("retain", "Reter (retain)", false, "O broker guarda a última mensagem do tópico para quem assinar depois."),
            },
        };

        public Task ExecuteAsync(ActionContext ctx) =>
            _mqtt.PublishAsync(ctx.Get("topic"), ctx.Get("payload"), ctx.Int("qos", 0),
                ctx.Get("retain").Equals("true", System.StringComparison.OrdinalIgnoreCase));
    }
}
