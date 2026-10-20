using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DroidDeck.Lib;

namespace DroidDeck.Services
{
    /// <summary>
    /// Uso de IA para o botão "Uso de IA": a porcentagem dos limites do plano (janela de 5 h e
    /// semana) e os tokens de hoje e dos últimos 7 dias. Tudo sai do que as ferramentas já
    /// guardam no PC; nada é enviado a terceiros.
    /// <list type="bullet">
    /// <item>Claude: limites pela mesma consulta que o /usage do Claude Code faz, com o login que ele
    /// salvou em ~/.claude/.credentials.json (só leitura: o login nunca é renovado daqui, para não
    /// derrubar o do Claude Code). Tokens pelos registros em ~/.claude/projects.</item>
    /// <item>Codex: limites e tokens pelos eventos token_count em ~/.codex/sessions.</item>
    /// </list>
    /// </summary>
    public class AiUsageService
    {
        public const string Claude = "claude";
        public const string Codex = "codex";

        private const string ClaudeUsageUrl = "https://api.anthropic.com/api/oauth/usage";
        private static readonly TimeSpan PlanCache = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan Keep = TimeSpan.FromDays(8);

        private readonly IHttpClientFactory _http;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private (DateTime at, Plan plan)? _claudePlan;
        private readonly LogScanner _claudeLogs = new(ParseClaudeLine, "\"usage\"");
        private readonly LogScanner _codexLogs = new(ParseCodexLine, "\"token_count\"");

        public AiUsageService(IHttpClientFactory http) => _http = http;

        /// <summary>Uma janela de limite: quanto já foi usado (0–100) e quando zera.</summary>
        public record Window(double Percent, DateTimeOffset? ResetsAt);

        public record Plan(Window? Session, Window? Week);

        public record Tokens(long Today, long Week);

        /// <summary>Uma linha lida de um registro: quando, quantos tokens e o id (para não contar repetido).</summary>
        public record Entry(DateTimeOffset At, long Tokens, string? Id, Plan? Plan = null);

        public static string ClaudeDir =>
            Environment.GetEnvironmentVariable("CLAUDE_CONFIG_DIR") is { Length: > 0 } d
                ? d
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

        public static string CodexDir =>
            Environment.GetEnvironmentVariable("CODEX_HOME") is { Length: > 0 } d
                ? d
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex");

        // ---- Limites do plano ----

        public async Task<Plan> PlanAsync(string provider)
        {
            if (provider == Codex)
            {
                var plan = await Task.Run(() => _codexLogs.LatestPlan(Path.Combine(CodexDir, "sessions")));
                return plan ?? throw new InvalidOperationException(L.Ui("Sem registros do Codex neste PC."));
            }

            await _lock.WaitAsync();
            try
            {
                if (_claudePlan is { } c && DateTime.UtcNow - c.at < PlanCache) return c.plan;
                var plan = await FetchClaudePlanAsync();
                _claudePlan = (DateTime.UtcNow, plan);
                return plan;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<Plan> FetchClaudePlanAsync()
        {
            var file = Path.Combine(ClaudeDir, ".credentials.json");
            if (!File.Exists(file))
                throw new InvalidOperationException(L.Ui("Entre no Claude Code neste PC para ver os limites."));

            string? token;
            long expires;
            using (var creds = JsonDocument.Parse(await File.ReadAllTextAsync(file)))
            {
                if (!creds.RootElement.TryGetProperty("claudeAiOauth", out var o))
                    throw new InvalidOperationException(L.Ui("Entre no Claude Code neste PC para ver os limites."));
                token = o.TryGetProperty("accessToken", out var t) ? t.GetString() : null;
                expires = o.TryGetProperty("expiresAt", out var e) && e.TryGetInt64(out var ms) ? ms : 0;
            }
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException(L.Ui("Entre no Claude Code neste PC para ver os limites."));
            if (expires > 0 && DateTimeOffset.FromUnixTimeMilliseconds(expires) < DateTimeOffset.UtcNow)
                throw new InvalidOperationException(L.Ui("O login do Claude Code expirou. Abra o Claude Code para renovar."));

            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            using var req = new HttpRequestMessage(HttpMethod.Get, ClaudeUsageUrl);
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            req.Headers.TryAddWithoutValidation("anthropic-beta", "oauth-2025-04-20");
            req.Headers.UserAgent.ParseAdd("DroidDeck");
            using var resp = await client.SendAsync(req);
            if (resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new InvalidOperationException(L.Ui("O login do Claude Code expirou. Abra o Claude Code para renovar."));
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(L.Ui("A Anthropic não respondeu ({code}).", ("code", (int)resp.StatusCode)));
            return ParseClaudePlan(await resp.Content.ReadAsStringAsync());
        }

        /// <summary>Resposta do /api/oauth/usage: five_hour e seven_day com utilization e resets_at.</summary>
        public static Plan ParseClaudePlan(string json)
        {
            using var doc = JsonDocument.Parse(json);
            Window? Read(string name)
            {
                if (!doc.RootElement.TryGetProperty(name, out var w) || w.ValueKind != JsonValueKind.Object) return null;
                if (!w.TryGetProperty("utilization", out var u) || u.ValueKind != JsonValueKind.Number) return null;
                DateTimeOffset? resets = w.TryGetProperty("resets_at", out var r) && r.ValueKind == JsonValueKind.String
                    && DateTimeOffset.TryParse(r.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var at)
                    ? at : null;
                return new Window(u.GetDouble(), resets);
            }
            return new Plan(Read("five_hour"), Read("seven_day"));
        }

        // ---- Tokens ----

        public Task<Tokens> TokensAsync(string provider) => Task.Run(() =>
        {
            var entries = provider == Codex
                ? _codexLogs.Entries(Path.Combine(CodexDir, "sessions"))
                : _claudeLogs.Entries(Path.Combine(ClaudeDir, "projects"));
            return Sum(entries, DateTimeOffset.Now);
        });

        /// <summary>Hoje (desde a meia-noite local) e os últimos 7 dias, contando hoje.</summary>
        public static Tokens Sum(IEnumerable<Entry> entries, DateTimeOffset now)
        {
            var today = new DateTimeOffset(now.Date, now.Offset);
            var week = today.AddDays(-6);
            long t = 0, w = 0;
            foreach (var e in entries)
            {
                if (e.At >= week && e.At <= now.AddMinutes(5)) w += e.Tokens;
                if (e.At >= today && e.At <= now.AddMinutes(5)) t += e.Tokens;
            }
            return new Tokens(t, w);
        }

        /// <summary>
        /// Uma linha do Claude Code com "usage". A mesma resposta aparece em várias linhas (uma por
        /// bloco), então o id da mensagem evita contar duas vezes. Conta entrada, saída e cache.
        /// </summary>
        public static Entry? ParseClaudeLine(string line)
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            if (!root.TryGetProperty("message", out var msg) || msg.ValueKind != JsonValueKind.Object) return null;
            if (!msg.TryGetProperty("usage", out var u) || u.ValueKind != JsonValueKind.Object) return null;
            if (!root.TryGetProperty("timestamp", out var ts) || !DateTimeOffset.TryParse(ts.GetString(),
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var at)) return null;
            long Num(string k) => u.TryGetProperty(k, out var v) && v.TryGetInt64(out var n) ? n : 0;
            var tokens = Num("input_tokens") + Num("output_tokens")
                + Num("cache_creation_input_tokens") + Num("cache_read_input_tokens");
            var id = msg.TryGetProperty("id", out var i) ? i.GetString() : null;
            if (root.TryGetProperty("requestId", out var rq)) id += "|" + rq.GetString();
            return new Entry(at, tokens, id);
        }

        /// <summary>
        /// Evento token_count do Codex: last_token_usage é o gasto daquela chamada; rate_limits traz a
        /// janela primária (5 h) e a secundária (semana), com resets_at (segundos Unix) ou
        /// resets_in_seconds nas versões antigas.
        /// </summary>
        public static Entry? ParseCodexLine(string line)
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            if (!root.TryGetProperty("payload", out var p) || p.ValueKind != JsonValueKind.Object) return null;
            if (!p.TryGetProperty("type", out var type) || type.GetString() != "token_count") return null;
            if (!root.TryGetProperty("timestamp", out var ts) || !DateTimeOffset.TryParse(ts.GetString(),
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var at)) return null;

            long tokens = 0;
            if (p.TryGetProperty("info", out var info) && info.ValueKind == JsonValueKind.Object
                && info.TryGetProperty("last_token_usage", out var last) && last.ValueKind == JsonValueKind.Object)
            {
                if (last.TryGetProperty("total_tokens", out var tt) && tt.TryGetInt64(out var n)) tokens = n;
                else
                {
                    long Num(string k) => last.TryGetProperty(k, out var v) && v.TryGetInt64(out var x) ? x : 0;
                    tokens = Num("input_tokens") + Num("output_tokens");
                }
            }

            Plan? plan = null;
            if (p.TryGetProperty("rate_limits", out var rl) && rl.ValueKind == JsonValueKind.Object)
            {
                Window? Read(string name)
                {
                    if (!rl.TryGetProperty(name, out var w) || w.ValueKind != JsonValueKind.Object) return null;
                    if (!w.TryGetProperty("used_percent", out var u) || u.ValueKind != JsonValueKind.Number) return null;
                    DateTimeOffset? resets = null;
                    if (w.TryGetProperty("resets_at", out var r) && r.TryGetInt64(out var unix))
                        resets = DateTimeOffset.FromUnixTimeSeconds(unix);
                    else if (w.TryGetProperty("resets_in_seconds", out var s) && s.TryGetInt64(out var secs))
                        resets = at.AddSeconds(secs);
                    return new Window(u.GetDouble(), resets);
                }
                var primary = Read("primary");
                var secondary = Read("secondary");
                if (primary != null || secondary != null) plan = new Plan(primary, secondary);
            }
            return new Entry(at, tokens, null, plan);
        }

        /// <summary>
        /// Lê os .jsonl recentes de uma pasta aos poucos: guarda até onde leu cada arquivo e só
        /// processa o que foi acrescentado. Linhas sem o marcador nem são interpretadas.
        /// </summary>
        public sealed class LogScanner
        {
            private readonly Func<string, Entry?> _parse;
            private readonly string _marker;
            private readonly object _gate = new();
            private readonly Dictionary<string, FileState> _files = new(StringComparer.OrdinalIgnoreCase);

            private sealed class FileState
            {
                public long Offset;
                public readonly List<Entry> Entries = new();
                public readonly HashSet<string> Ids = new();
            }

            public LogScanner(Func<string, Entry?> parse, string marker)
            {
                _parse = parse;
                _marker = marker;
            }

            public List<Entry> Entries(string dir)
            {
                lock (_gate)
                {
                    Scan(dir);
                    var cutoff = DateTimeOffset.UtcNow - Keep;
                    return _files.Values.SelectMany(f => f.Entries).Where(e => e.At >= cutoff).ToList();
                }
            }

            public Plan? LatestPlan(string dir)
            {
                lock (_gate)
                {
                    Scan(dir);
                    return _files.Values.SelectMany(f => f.Entries)
                        .Where(e => e.Plan != null)
                        .OrderByDescending(e => e.At)
                        .Select(e => e.Plan)
                        .FirstOrDefault();
                }
            }

            private void Scan(string dir)
            {
                if (!Directory.Exists(dir)) return;
                var since = DateTime.UtcNow - Keep;
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var path in Directory.EnumerateFiles(dir, "*.jsonl", SearchOption.AllDirectories))
                {
                    FileInfo fi;
                    try { fi = new FileInfo(path); }
                    catch { continue; }
                    if (fi.LastWriteTimeUtc < since) continue;
                    seen.Add(path);
                    if (!_files.TryGetValue(path, out var st)) _files[path] = st = new FileState();
                    if (fi.Length < st.Offset)
                    {
                        // Arquivo reescrito: começa de novo.
                        st.Offset = 0;
                        st.Entries.Clear();
                        st.Ids.Clear();
                    }
                    if (fi.Length == st.Offset) continue;
                    try { ReadFrom(path, st); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
                foreach (var gone in _files.Keys.Where(k => !seen.Contains(k)).ToList())
                    _files.Remove(gone);
            }

            private void ReadFrom(string path, FileState st)
            {
                // O Claude Code e o Codex continuam escrevendo enquanto lemos; um registro passa
                // de centenas de MB, então vai em pedaços e só linhas completas contam.
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                fs.Seek(st.Offset, SeekOrigin.Begin);
                var marker = Encoding.UTF8.GetBytes(_marker);
                var chunk = new byte[1 << 20];
                var line = new MemoryStream();
                int n;
                while ((n = fs.Read(chunk, 0, chunk.Length)) > 0)
                {
                    var start = 0;
                    for (var i = 0; i < n; i++)
                    {
                        if (chunk[i] != (byte)'\n') continue;
                        line.Write(chunk, start, i - start);
                        st.Offset += line.Length + 1;
                        Handle(line, marker, st);
                        line.SetLength(0);
                        start = i + 1;
                    }
                    line.Write(chunk, start, n - start);
                }
            }

            private void Handle(MemoryStream line, byte[] marker, FileState st)
            {
                var bytes = new ReadOnlySpan<byte>(line.GetBuffer(), 0, (int)line.Length);
                if (bytes.IndexOf(marker) < 0) return;
                Entry? e;
                try { e = _parse(Encoding.UTF8.GetString(bytes)); }
                catch (JsonException) { return; }
                catch (FormatException) { return; }
                catch (InvalidOperationException) { return; }
                if (e == null) return;
                if (e.Id != null && !st.Ids.Add(e.Id)) return;
                st.Entries.Add(e);
            }
        }
    }
}
