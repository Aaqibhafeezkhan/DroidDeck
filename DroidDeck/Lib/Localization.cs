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
    /// O idioma e por requisicao (AsyncLocal), marcado pelo middleware UseLanguage. Fora de uma
    /// requisicao (servicos em segundo plano, SignalR) vale portugues.
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
            var text = Ui(pt);
            foreach (var (key, value) in args)
                text = text.Replace("{" + key + "}", value?.ToString() ?? "");
            return text;
        }

        /// <summary>Traduz e depois troca os {chave} pelos valores.</summary>
        public static string T(string pt, params (string key, object? value)[] args)
        {
            var text = T(pt);
            foreach (var (key, value) in args)
                text = text.Replace("{" + key + "}", value?.ToString() ?? "");
            return text;
        }

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
