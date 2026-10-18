using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Actions.Providers;
using DroidDeck.Models;
using DroidDeck.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DroidDeck.Tests
{
    public class ConditionsTests
    {
        private static Conditions Make(DateTime? now = null)
        {
            var c = new Conditions(new ServiceCollection().BuildServiceProvider());
            if (now is { } n) c.Now = () => n;
            return c;
        }

        [Theory]
        [InlineData("08:00-18:00", 9, true)]
        [InlineData("08:00-18:00", 18, false)]
        [InlineData("22:00-06:00", 23, true)]
        [InlineData("22:00-06:00", 3, true)]
        [InlineData("22:00-06:00", 12, false)]
        [InlineData("lixo", 12, true)]
        public void TimeBetween(string range, int hour, bool expected) =>
            Assert.Equal(expected, Conditions.TimeBetween(range, new DateTime(2026, 10, 18, hour, 0, 0)));

        [Theory]
        [InlineData("1-5", DayOfWeek.Monday, true)]
        [InlineData("1-5", DayOfWeek.Sunday, false)]
        [InlineData("0,6", DayOfWeek.Saturday, true)]
        [InlineData("5-1", DayOfWeek.Sunday, true)]
        public void Weekday(string days, DayOfWeek day, bool expected)
        {
            var date = new DateTime(2026, 10, 18); // domingo
            while (date.DayOfWeek != day) date = date.AddDays(1);
            Assert.Equal(expected, Conditions.Weekday(days, date));
        }

        [Fact]
        public void SemCondicao_OuDesconhecida_RodaONegarInverte()
        {
            var c = Make(new DateTime(2026, 10, 18, 10, 0, 0));
            Assert.True(c.Evaluate(null));
            Assert.True(c.Evaluate(new StepCondition { Check = "nao_existe" }));
            Assert.True(c.Evaluate(new StepCondition { Check = "time_between", Value = "09:00-11:00" }));
            Assert.False(c.Evaluate(new StepCondition { Check = "time_between", Value = "09:00-11:00", Negate = true }));
            // Sem OBS registrado: a live nao esta no ar.
            Assert.False(c.Evaluate(new StepCondition { Check = "obs_streaming" }));
            Assert.True(c.Evaluate(new StepCondition { Check = "obs_streaming", Negate = true }));
            Assert.False(c.Evaluate(new StepCondition { Check = "app_running", Value = "processo-que-nao-existe-123" }));
        }

        [Fact]
        public async Task MultiAcao_PulaOPassoQueNaoPassa()
        {
            var ran = new List<string>();
            var steps = new[]
            {
                new MultiStep { Type = "a", If = new StepCondition { Check = "time_between", Value = "00:00-00:01", Negate = true } },
                new MultiStep { Type = "b", If = new StepCondition { Check = "time_between", Value = "00:00-00:01" } },
                new MultiStep { Type = "c" },
            };
            var action = new DeckAction
            {
                Type = "multi",
                Parameters = { ["steps"] = JsonSerializer.Serialize(steps, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) },
            };
            var provider = new MultiProvider(NullLogger<MultiProvider>.Instance, Make(new DateTime(2026, 10, 18, 12, 0, 0)));
            await provider.ExecuteAsync(new ActionContext(action, null, (a, _) => { ran.Add(a.Type); return Task.CompletedTask; }));

            Assert.Equal(new[] { "a", "c" }, ran);
        }
    }
}
