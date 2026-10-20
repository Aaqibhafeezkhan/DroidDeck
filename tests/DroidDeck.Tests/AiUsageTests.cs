using System;
using System.IO;
using System.Linq;
using DroidDeck.Services;
using Xunit;

namespace DroidDeck.Tests
{
    public class AiUsageTests
    {
        [Fact]
        public void Claude_Limites_LeSessaoESemana()
        {
            var plan = AiUsageService.ParseClaudePlan("""
                {"five_hour":{"utilization":17.0,"resets_at":"2026-10-20T20:40:00.501183+00:00"},
                 "seven_day":{"utilization":24.0,"resets_at":"2026-10-26T13:00:00+00:00"},
                 "seven_day_opus":null}
                """);
            Assert.Equal(17, plan.Session!.Percent);
            Assert.Equal(24, plan.Week!.Percent);
            Assert.Equal(new DateTimeOffset(2026, 10, 26, 13, 0, 0, TimeSpan.Zero), plan.Week.ResetsAt);
        }

        [Fact]
        public void Claude_Limites_SemSemanaNaoQuebra()
        {
            var plan = AiUsageService.ParseClaudePlan("""{"five_hour":{"utilization":3},"seven_day":null}""");
            Assert.Equal(3, plan.Session!.Percent);
            Assert.Null(plan.Session.ResetsAt);
            Assert.Null(plan.Week);
        }

        [Fact]
        public void Claude_Registro_SomaTokensSemRepetirAMesmaMensagem()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dd-ai-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "proj"));
            try
            {
                var now = DateTimeOffset.UtcNow;
                string Line(string id, int input, int output, DateTimeOffset at) =>
                    "{\"type\":\"assistant\",\"timestamp\":\"" + at.ToString("O") + "\",\"requestId\":\"r" + id
                    + "\",\"message\":{\"id\":\"" + id + "\",\"usage\":{\"input_tokens\":" + input + ",\"output_tokens\":" + output
                    + ",\"cache_creation_input_tokens\":10,\"cache_read_input_tokens\":5}}}";
                var file = Path.Combine(dir, "proj", "a.jsonl");
                // A mesma resposta em duas linhas (um bloco de texto e uma ferramenta) e uma linha
                // de usuário que só cita "usage" no texto.
                File.WriteAllText(file,
                    Line("m1", 100, 20, now) + "\n" +
                    Line("m1", 100, 20, now) + "\n" +
                    """{"type":"user","message":{"content":"o campo \"usage\" do JSON"}}""" + "\n" +
                    Line("m2", 1, 2, now.AddDays(-3)) + "\n");

                var scanner = new AiUsageService.LogScanner(AiUsageService.ParseClaudeLine, "\"usage\"");
                var entries = scanner.Entries(dir);
                Assert.Equal(2, entries.Count);
                Assert.Equal(135, entries.Single(e => e.Id!.StartsWith("m1")).Tokens);

                // Linha pela metade (ainda sendo escrita) só conta quando termina.
                File.AppendAllText(file, Line("m3", 1000, 0, now)[..^1]);
                Assert.Equal(2, scanner.Entries(dir).Count);
                File.AppendAllText(file, "}\n");
                var after = scanner.Entries(dir);
                Assert.Equal(3, after.Count);

                var sum = AiUsageService.Sum(after, now.ToLocalTime());
                Assert.Equal(135 + 1015, sum.Today);
                Assert.Equal(135 + 1015 + 18, sum.Week);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Codex_TokenCount_TrazTokensELimites()
        {
            var e = AiUsageService.ParseCodexLine("""
                {"timestamp":"2026-10-20T12:00:00.000Z","type":"event_msg","payload":{"type":"token_count",
                 "info":{"total_token_usage":{"total_tokens":9000},"last_token_usage":{"input_tokens":700,"output_tokens":50,"total_tokens":750}},
                 "rate_limits":{"primary":{"used_percent":12.5,"window_minutes":300,"resets_in_seconds":600},
                                "secondary":{"used_percent":40,"window_minutes":10080,"resets_at":1793000000}}}}
                """.Replace("\n", ""));
            Assert.NotNull(e);
            Assert.Equal(750, e!.Tokens);
            Assert.Equal(12.5, e.Plan!.Session!.Percent);
            Assert.Equal(new DateTimeOffset(2026, 10, 20, 12, 10, 0, TimeSpan.Zero), e.Plan.Session.ResetsAt);
            Assert.Equal(40, e.Plan.Week!.Percent);
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1793000000), e.Plan.Week.ResetsAt);
        }

        [Fact]
        public void Codex_OutrosEventos_Ignorados()
        {
            Assert.Null(AiUsageService.ParseCodexLine("""{"timestamp":"2026-10-20T12:00:00Z","type":"event_msg","payload":{"type":"agent_message","message":"token_count"}}"""));
        }

        [Fact]
        public void Soma_SeparaHojeDosUltimosSeteDias()
        {
            var now = new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.FromHours(-3));
            var entries = new[]
            {
                new AiUsageService.Entry(now.AddHours(-1), 10, null),
                new AiUsageService.Entry(now.AddHours(-10), 20, null),  // ontem à noite
                new AiUsageService.Entry(now.AddDays(-6).AddHours(-8), 40, null), // dia 14, ainda na semana
                new AiUsageService.Entry(now.AddDays(-7), 80, null),    // fora
            };
            var sum = AiUsageService.Sum(entries, now);
            Assert.Equal(10, sum.Today);
            Assert.Equal(70, sum.Week);
        }
    }
}
