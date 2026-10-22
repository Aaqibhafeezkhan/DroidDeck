using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Traducao dos textos que o servidor manda para o app. O texto-fonte continua em portugues
    /// no codigo; L.T("...") devolve a versao em ingles (Localization.en.cs) ou em espanhol
    /// (Localization.es.cs) conforme o Accept-Language da requisicao. Espanhol sem traducao cai
    /// no ingles; sem traducao nenhuma fica o portugues -- nunca quebra.
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
        private static readonly AsyncLocal<string?> _language = new();

        /// <summary>Idioma da requisicao: "pt", "en" ou "es".</summary>
        public static string Language
        {
            get => _language.Value ?? "pt";
            set => _language.Value = value;
        }

        public static bool English
        {
            get => Language == "en";
            set => Language = value ? "en" : "pt";
        }

        /// <summary>A requisicao pediu outro idioma que nao o portugues (o schema e traduzido).</summary>
        public static bool Translating => Language != "pt";

        public static string T(string pt) => TFor(Language, pt);

        private static string TFor(string language, string pt) => language switch
        {
            "en" => En.TryGetValue(pt, out var en) ? en : pt,
            "es" => Es.TryGetValue(pt, out var es) ? es : En.TryGetValue(pt, out var en) ? en : pt,
            _ => pt,
        };

        /// <summary>
        /// Texto da bandeja e das janelas do PC: segue o idioma do Windows (nao ha requisicao).
        /// Portugues e o padrao; Windows em ingles ou espanhol ve a traducao.
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

        private static System.Globalization.CultureInfo CultureOf(string language) =>
            System.Globalization.CultureInfo.GetCultureInfo(language switch
            {
                "en" => "en-US",
                "es" => "es-ES",
                _ => "pt-BR",
            });

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
            public string Plain => Fill(Pt, "pt", _args);
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
            language switch { "en" => En.ContainsKey(pt), "es" => Es.ContainsKey(pt), _ => true };

        /// <summary>Todos os textos com traducao para o ingles.</summary>
        public static IReadOnlyCollection<string> TranslatedKeys => En.Keys;

        /// <summary>Versao que aceita null (campos opcionais do schema).</summary>
        public static string? TOrNull(string? pt) => pt == null ? null : T(pt);

        /// <summary>
        /// Idioma pela primeira preferencia do Accept-Language: "en..." vira ingles, "es..."
        /// espanhol, o resto portugues.
        /// </summary>
        public static string LanguageOf(string? acceptLanguage)
        {
            if (string.IsNullOrWhiteSpace(acceptLanguage)) return "pt";
            var first = acceptLanguage.Split(',')[0].Split(';')[0].Trim();
            if (first.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en";
            if (first.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return "es";
            return "pt";
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
