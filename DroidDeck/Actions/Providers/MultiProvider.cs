using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DroidDeck.Models;
using Microsoft.Extensions.Logging;

namespace DroidDeck.Actions.Providers
{
    /// <summary>
    /// Executa uma sequência de ações. parameters["steps"] = JSON de
    /// [{type, parameters, delayMs, if}]; delayMs é aguardado ANTES de cada passo e "if"
    /// (opcional) decide se o passo roda (ver Services/Conditions.cs).
    /// </summary>
    public class MultiProvider : IActionProvider
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private readonly ILogger<MultiProvider> _logger;
        private readonly Services.Conditions _conditions;

        public MultiProvider(ILogger<MultiProvider> logger, Services.Conditions conditions)
        {
            _logger = logger;
            _conditions = conditions;
        }

        public string Type => "multi";

        public ActionSchema Describe() => new()
        {
            Type = Type,
            Title = "Multi-ação",
            Icon = "playlist_play",
            Category = "Utilidades",
            Description = "Vários passos em sequência, com espera entre eles.",
            Editor = "multi",
        };

        public async Task ExecuteAsync(ActionContext c)
        {
            var stepsJson = c.Get("steps");
            if (string.IsNullOrWhiteSpace(stepsJson)) return;

            List<MultiStep>? steps;
            try
            {
                steps = JsonSerializer.Deserialize<List<MultiStep>>(stepsJson, JsonOpts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Multi-ação: JSON de passos inválido");
                return;
            }
            if (steps == null) return;

            foreach (var step in steps)
            {
                if (string.IsNullOrWhiteSpace(step.Type)) continue;
                if (string.Equals(step.Type, Type, StringComparison.OrdinalIgnoreCase))
                    continue; // não aninha multi
                if (step.DelayMs > 0)
                    await Task.Delay(step.DelayMs);
                // "So se...": a condicao e olhada na hora do passo (depois da espera).
                if (!_conditions.Evaluate(step.If)) continue;
                await c.ExecuteAsync(new DeckAction
                {
                    Type = step.Type!,
                    Parameters = step.Parameters ?? new Dictionary<string, string>()
                });
            }
        }
    }
}
