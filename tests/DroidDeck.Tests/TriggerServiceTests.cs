using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Models;
using DroidDeck.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DroidDeck.Tests
{
    public class TriggerServiceTests
    {
        /// <summary>Provedor falso: anota o que foi executado.</summary>
        private class Recorder : IActionProvider
        {
            public readonly List<string> Ran = new();
            public string Type => "rec";
            public ActionSchema Describe() => new() { Type = Type, Title = "rec" };
            public Task ExecuteAsync(ActionContext c)
            {
                Ran.Add(c.Get("tag"));
                return Task.CompletedTask;
            }
        }

        private static (TriggerService service, Recorder rec, DateTime[] clock, HashSet<string> procs) Make(
            params TriggerService.Trigger[] triggers)
        {
            var config = new StreamDeckConfigService(NullLogger<StreamDeckConfigService>.Instance);
            var profileId = "trig-" + Guid.NewGuid().ToString("N");
            config.SaveProfile(new DeckProfile
            {
                Id = profileId,
                Name = "Gatilhos",
                Buttons =
                {
                    new DeckButton { Id = "b1", Action = new DeckAction { Type = "rec", Parameters = { ["tag"] = "b1" } } },
                    new DeckButton { Id = "b2", Action = new DeckAction { Type = "rec", Parameters = { ["tag"] = "b2" } } },
                },
            });
            foreach (var t in triggers) t.ProfileId = profileId;

            var rec = new Recorder();
            var executor = new ActionExecutorService(NullLogger<ActionExecutorService>.Instance, new IActionProvider[] { rec });
            var conditions = new Conditions(new ServiceCollection().BuildServiceProvider());
            var service = new TriggerService(config, new ButtonToggleService(NullLogger<ButtonToggleService>.Instance),
                executor, conditions, NullLogger<TriggerService>.Instance);
            service.SaveAll(new List<TriggerService.Trigger>(triggers));

            var clock = new[] { new DateTime(2026, 10, 19, 8, 59, 50) }; // segunda-feira
            var procs = new HashSet<string> { "explorer" };
            service.Now = () => clock[0];
            conditions.Now = () => clock[0];
            service.RunningProcesses = () => new HashSet<string>(procs);
            return (service, rec, clock, procs);
        }

        [Fact]
        public async Task Horario_DisparaUmaVezNoMinuto()
        {
            var (service, rec, clock, _) = Make(
                new TriggerService.Trigger { Event = "schedule", Time = "09:00", Days = "1-5", ButtonId = "b1" },
                new TriggerService.Trigger { Event = "schedule", Time = "09:00", Days = "0,6", ButtonId = "b2" });

            await service.TickAsync();
            Assert.Empty(rec.Ran);

            clock[0] = clock[0].AddSeconds(12); // 09:00:02
            await service.TickAsync();
            clock[0] = clock[0].AddSeconds(2);
            await service.TickAsync();

            Assert.Equal(new[] { "b1" }, rec.Ran); // b2 so no fim de semana
        }

        [Fact]
        public async Task AppAbrindoEFechando()
        {
            var (service, rec, clock, procs) = Make(
                new TriggerService.Trigger { Event = "app_start", App = "OBS64.exe", ButtonId = "b1" },
                new TriggerService.Trigger { Event = "app_exit", App = "obs64", ButtonId = "b2" });

            await service.TickAsync();
            procs.Add("obs64");
            clock[0] = clock[0].AddSeconds(2);
            await service.TickAsync();
            procs.Remove("obs64");
            clock[0] = clock[0].AddSeconds(2);
            await service.TickAsync();

            Assert.Equal(new[] { "b1", "b2" }, rec.Ran);
        }

        [Fact]
        public async Task VoltaDaSuspensao_ECondicao()
        {
            var (service, rec, clock, _) = Make(
                new TriggerService.Trigger { Event = "resume", ButtonId = "b1" },
                new TriggerService.Trigger
                {
                    Event = "resume", ButtonId = "b2",
                    If = new StepCondition { Check = "time_between", Value = "20:00-22:00" },
                });

            await service.TickAsync();
            clock[0] = clock[0].AddHours(2); // dormiu
            await service.TickAsync();

            Assert.Equal(new[] { "b1" }, rec.Ran);
        }

        [Fact]
        public async Task Desligado_NaoDispara_EBotaoApagado_NaoQuebra()
        {
            var (service, rec, clock, _) = Make(
                new TriggerService.Trigger { Event = "resume", ButtonId = "b1", Enabled = false },
                new TriggerService.Trigger { Event = "resume", ButtonId = "nao-existe" });

            await service.TickAsync();
            clock[0] = clock[0].AddMinutes(5);
            await service.TickAsync();

            Assert.Empty(rec.Ran);
        }
    }
}
