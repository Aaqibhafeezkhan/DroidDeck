using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Traducao dos textos que o servidor manda para o app. O texto-fonte continua em portugues
    /// no codigo e serve de chave; as traducoes vem de app/assets/i18n/<idioma>.json (os mesmos
    /// arquivos do app, embutidos no executavel). Arquivo novo na pasta = idioma novo, sem mexer
    /// em codigo. Texto sem traducao no idioma pedido sai em ingles, o idioma padrao.
    ///
    /// O idioma e por requisicao (AsyncLocal), marcado pelo middleware UseLanguage. No SignalR,
    /// cada conexao entra no grupo do idioma dela e SendLocalizedAsync monta a mensagem uma vez
    /// por idioma. O que o servidor calcula em segundo plano guarda um L.Text (portugues + valores)
    /// e so traduz na hora de enviar. A bandeja e as janelas do PC usam L.Ui (idioma do Windows).
    ///
    /// Os testes (LocalizationCoverageTests) leem o codigo-fonte e falham se um texto passado a
    /// L.T/L.Ui/L.Text nao tiver traducao, ou se sobrar traducao de texto que nao existe mais.
    /// </summary>
    public static partial class L
    {
        /// <summary>Idioma em que o codigo esta escrito: as chaves ja sao o texto dele.</summary>
        public const string Source = "pt";

        /// <summary>Idioma padrao: pedido sem idioma conhecido e texto sem traducao ficam nele.</summary>
        public const string Fallback = "en";

        private sealed record Catalog(Dictionary<string, string> Strings, CultureInfo Culture);

        private static readonly Dictionary<string, Catalog> Catalogs = LoadCatalogs();

        /// <summary>Codigos dos idiomas com arquivo, em ordem alfabetica.</summary>
        public static IReadOnlyList<string> Languages { get; } =
            Catalogs.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        private static Dictionary<string, Catalog> LoadCatalogs()
        {
            var catalogs = new Dictionary<string, Catalog>(StringComparer.OrdinalIgnoreCase);
            var assembly = typeof(L).Assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith("i18n.", StringComparison.Ordinal) || !name.EndsWith(".json", StringComparison.Ordinal)) continue;
                var code = name["i18n.".Length..^".json".Length];
                using var stream = assembly.GetManifestResourceStream(name)!;
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream) ?? new();
                var strings = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var (key, value) in json)
                    if (!key.StartsWith('@') && value.ValueKind == JsonValueKind.String) strings[key] = value.GetString()!;
                catalogs[code] = new Catalog(strings, CultureFor(json, code));
            }
            // Sem arquivo nenhum (nao deveria acontecer): ao menos o portugues do codigo.
            if (!catalogs.ContainsKey(Source)) catalogs[Source] = new Catalog(new(), CultureInfo.GetCultureInfo("pt-BR"));
            if (!catalogs.ContainsKey(Fallback)) catalogs[Fallback] = new Catalog(new(), CultureInfo.GetCultureInfo("en-US"));
            return catalogs;
        }

        private static CultureInfo CultureFor(Dictionary<string, JsonElement> json, string code)
        {
            try
            {
                return CultureInfo.GetCultureInfo(
                    json.TryGetValue("@locale", out var l) && l.ValueKind == JsonValueKind.String ? l.GetString()! : code);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.InvariantCulture;
            }
        }

        private static readonly AsyncLocal<string?> _language = new();

        /// <summary>Idioma da requisicao: o codigo de um dos arquivos ("en", "es"...).</summary>
        public static string Language
        {
            get => _language.Value ?? Fallback;
            set => _language.Value = value;
        }

        public static bool English
        {
            get => Language == "en";
            set => Language = value ? "en" : Source;
        }

        /// <summary>A requisicao pediu outro idioma que nao o do codigo (o schema e traduzido).</summary>
        public static bool Translating => Language != Source;

        public static string T(string pt) => TFor(Language, pt);

        private static string TFor(string language, string pt)
        {
            if (language == Source) return pt;
            if (Catalogs.TryGetValue(language, out var c) && c.Strings.TryGetValue(pt, out var text)) return text;
            return Catalogs[Fallback].Strings.TryGetValue(pt, out var en) ? en : pt;
        }

        /// <summary>
        /// Texto da bandeja e das janelas do PC: segue o idioma do Windows (nao ha requisicao).
        /// Windows num idioma sem arquivo ve o ingles.
        /// </summary>
        public static string Ui(string pt) =>
            TFor(LanguageOf(System.Globalization.CultureInfo.CurrentUICulture.Name), pt);

        public static string Ui(string pt, params (string key, object? value)[] args)
        {
            var language = LanguageOf(System.Globalization.CultureInfo.CurrentUICulture.Name);
            return Fill(TFor(language, pt), language, args);
        }

        /// <summary>Traduz e depois troca os {chave} pelos valores.</summary>
        public static string T(string pt, params (string key, object? value)[] args) =>
            Fill(T(pt), Language, args);

        /// <summary>
        /// Troca os {chave}. Numero e data saem no formato do idioma (1.5 vira "1,5" em
        /// portugues e espanhol).
        /// </summary>
        private static string Fill(string text, string language, (string key, object? value)[] args)
        {
            var culture = CultureOf(language);
            foreach (var (key, value) in args)
                text = text.Replace("{" + key + "}", value switch
                {
                    null => "",
                    IFormattable f => f.ToString(null, culture),
                    _ => value.ToString() ?? "",
                });
            return text;
        }

        private static CultureInfo CultureOf(string language) =>
            Catalogs.TryGetValue(language, out var c) ? c.Culture : Catalogs[Fallback].Culture;

        /// <summary>
        /// Mensagem que so vira texto na hora de ir para alguem: guarda o portugues e os valores,
        /// e cada cliente recebe no idioma dele. Usada no que o servidor calcula em segundo plano
        /// (botoes de informacao) e vai pelo SignalR.
        /// </summary>
        public sealed class Text
        {
            public string Pt { get; }
            private readonly (string key, object? value)[] _args;

            public Text(string pt, params (string key, object? value)[] args)
            {
                Pt = pt;
                _args = args;
            }

            /// <summary>No idioma da requisicao (ou do grupo do SignalR) atual.</summary>
            public override string ToString() => In(Language);

            public string In(string language) => Fill(TFor(language, Pt), language, _args);

            /// <summary>Portugues com os valores: serve para comparar e para o log.</summary>
            public string Plain => Fill(Pt, Source, _args);
        }

        /// <summary>Texto de um erro para o cliente: traduz a mensagem, com os valores se houver.</summary>
        public static string Message(Exception ex) =>
            ex is LocalizedException le ? le.Text.ToString() : T(ex.Message);
    }

    /// <summary>
    /// Erro com mensagem traduzivel. Message fica em portugues (log); quem mostra ao usuario
    /// usa L.Message(ex), que traduz inclusive quando a mensagem tem valores.
    /// </summary>
    public class LocalizedException : InvalidOperationException
    {
        public L.Text Text { get; }

        public LocalizedException(L.Text text) : base(text.Plain) => Text = text;

        public LocalizedException(string pt, params (string key, object? value)[] args) : this(new L.Text(pt, args)) { }
    }

    public static partial class L
    {

        /// <summary>Ha traducao para este texto em portugues? (Os testes de cobertura usam.)</summary>
        public static bool Has(string pt, string language) =>
            language == Source || (Catalogs.TryGetValue(language, out var c) && c.Strings.ContainsKey(pt));

        /// <summary>Textos traduzidos num idioma (chave em portugues -> texto). Os testes usam.</summary>
        public static IReadOnlyDictionary<string, string> StringsOf(string language) =>
            Catalogs.TryGetValue(language, out var c) ? c.Strings : new Dictionary<string, string>();

        /// <summary>Versao que aceita null (campos opcionais do schema).</summary>
        public static string? TOrNull(string? pt) => pt == null ? null : T(pt);

        /// <summary>
        /// Idioma pelo Accept-Language: a primeira preferencia que tem arquivo ("pt-BR" vale
        /// "pt"; "fr,es;q=0.8" vale "es" se nao houver frances). Nenhuma: ingles.
        /// </summary>
        public static string LanguageOf(string? acceptLanguage)
        {
            if (string.IsNullOrWhiteSpace(acceptLanguage)) return Fallback;
            foreach (var part in acceptLanguage.Split(','))
            {
                var code = part.Split(';')[0].Trim().Split('-', '_')[0].ToLowerInvariant();
                if (code.Length > 0 && Catalogs.ContainsKey(code)) return code;
            }
            return Fallback;
        }

        /// <summary>Primeira preferencia do Accept-Language comeca com "en"?</summary>
        public static bool IsEnglish(string? acceptLanguage) => LanguageOf(acceptLanguage) == "en";

        /// <summary>Marca o idioma da requisicao para L.T.</summary>
        public static IApplicationBuilder UseLanguage(this IApplicationBuilder app) =>
            app.Use(async (HttpContext ctx, Func<Task> next) =>
            {
                Language = LanguageOf(ctx.Request.Headers.AcceptLanguage.ToString());
                await next();
            });
    }
}
