using System;
using System.Threading.Tasks;
using DroidDeck.Actions;

namespace DroidDeck.Integrations
{
    /// <summary>Acao "vtubestudio": dispara um atalho (hotkey) do modelo ou troca de modelo.</summary>
    public class VTubeStudioProvider : IStatefulActionProvider
    {
        public const string TypeName = "vtubestudio";

        private readonly VTubeStudioIntegration _vts;

        public VTubeStudioProvider(VTubeStudioIntegration vts) => _vts = vts;

        public string Type => TypeName;
        public string StateEvent => VTubeStudioIntegration.Event;
        public object GetState() => _vts.GetState();

        public ActionSchema Describe() => new()
        {
            Type = TypeName,
            Title = "VTube Studio",
            Icon = "face",
            Category = "Streaming",
            Description = "Dispara atalhos (expressões, animações) do modelo no VTube Studio ou troca de modelo.",
            Settings = "vtubestudio",
            StateEvent = StateEvent,
            Fields =
            {
                ActionField.Select("operation", "O que fazer",
                    ("hotkey", "Disparar atalho"),
                    ("model", "Carregar modelo")),
                ActionField.Dynamic("hotkey", "Atalho", "/api/integrations/vtubestudio/options/hotkeys",
                    "Atalhos do modelo carregado agora. Também aceita o nome digitado.", allowCustom: true)
                    .When("operation", "hotkey"),
                ActionField.Dynamic("model", "Modelo", "/api/integrations/vtubestudio/options/models")
                    .When("operation", "model"),
            },
        };

        public Task ExecuteAsync(ActionContext ctx)
        {
            if (ctx.Operation("hotkey") == "model")
            {
                var model = ctx.Get("model");
                if (model.Trim().Length == 0) throw new InvalidOperationException("Escolha um modelo.");
                return _vts.LoadModelAsync(model);
            }
            var hotkey = ctx.Get("hotkey");
            if (hotkey.Trim().Length == 0) throw new InvalidOperationException("Escolha um atalho.");
            return _vts.TriggerHotkeyAsync(hotkey);
        }
    }
}
