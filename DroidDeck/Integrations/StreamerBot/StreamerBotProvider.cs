using System.Threading.Tasks;
using DroidDeck.Actions;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// Acao "streamerbot": dispara uma acao do Streamer.bot. Nao e privilegiada: so roda o que o
    /// usuario ja montou no Streamer.bot, como uma cena do OBS ou um script do Home Assistant.
    /// </summary>
    public class StreamerBotProvider : IStatefulActionProvider
    {
        public const string TypeName = "streamerbot";

        private readonly StreamerBotIntegration _bot;

        public StreamerBotProvider(StreamerBotIntegration bot) => _bot = bot;

        public string Type => TypeName;
        public string StateEvent => StreamerBotIntegration.Event;
        public object GetState() => _bot.GetState();

        public ActionSchema Describe() => new()
        {
            Type = TypeName,
            Title = "Streamer.bot",
            Icon = "smart_toy",
            Category = "Streaming",
            Description = "Dispara uma ação do Streamer.bot (com argumentos), pelo WebSocket Server dele.",
            Settings = "streamerbot",
            StateEvent = StateEvent,
            Fields =
            {
                ActionField.Dynamic("action", "Ação", "/api/integrations/streamerbot/options/actions",
                    "Também aceita o nome da ação digitado.", allowCustom: true),
                ActionField.Multiline("args", "Argumentos (opcional)", "chave=valor",
                    "Um por linha (chave=valor) ou um objeto JSON. Viram %chave% no Streamer.bot."),
            },
        };

        public Task ExecuteAsync(ActionContext ctx) => _bot.DoActionAsync(ctx.Get("action"), ctx.Get("args"));
    }
}
