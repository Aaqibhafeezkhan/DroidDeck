using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Auth;
using DroidDeck.Lib;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DroidDeck.Tests
{
    public class LocalizationTests
    {
        private static T InEnglish<T>(System.Func<T> f)
        {
            L.English = true;
            try { return f(); }
            finally { L.English = false; }
        }

        [Fact]
        public void SemIngles_DevolveOPortugues() =>
            Assert.Equal("Abrir programa", L.T("Abrir programa"));

        [Fact]
        public void ComIngles_DevolveATraducao() =>
            Assert.Equal("Open program", InEnglish(() => L.T("Abrir programa")));

        [Fact]
        public void SemTraducao_FicaOPortugues() =>
            Assert.Equal("Texto que ninguém traduziu", InEnglish(() => L.T("Texto que ninguém traduziu")));

        [Fact]
        public void Placeholders_SaoTrocadosDepoisDeTraduzir()
        {
            Assert.Equal(" Button: \"Live\".", InEnglish(() => L.T(" Botão: \"{label}\".", ("label", "Live"))));
            Assert.Equal(" Botão: \"Live\".", L.T(" Botão: \"{label}\".", ("label", "Live")));
        }

        [Fact]
        public void ComEspanhol_DevolveATraducao()
        {
            L.Language = "es";
            try { Assert.Equal("Atajo de teclado", L.T("Atalho de teclado")); }
            finally { L.Language = "pt"; }
        }

        private static Dictionary<string, string> Dictionary(string name) =>
            (Dictionary<string, string>)typeof(L)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        [Fact]
        public void Espanhol_TemTodaChaveDoIngles_ComOsMesmosPlaceholders()
        {
            var en = Dictionary("En");
            var es = Dictionary("Es");
            Assert.Empty(en.Keys.Except(es.Keys));
            Assert.Empty(es.Keys.Except(en.Keys));
            var placeholder = new Regex(@"\{(\w+)\}");
            foreach (var (pt, text) in es)
            {
                var expected = placeholder.Matches(pt).Select(m => m.Value).OrderBy(x => x);
                Assert.Equal(expected, placeholder.Matches(text).Select(m => m.Value).OrderBy(x => x));
            }
        }

        [Theory]
        [InlineData("es", "es")]
        [InlineData("es-MX,en;q=0.8", "es")]
        [InlineData("en-US,es;q=0.8", "en")]
        [InlineData("pt-BR,es;q=0.9", "pt")]
        [InlineData(null, "pt")]
        public void AcceptLanguage_EscolheOIdioma(string? header, string language) =>
            Assert.Equal(language, L.LanguageOf(header));

        [Theory]
        [InlineData("en", true)]
        [InlineData("en-US,pt;q=0.8", true)]
        [InlineData("EN-gb", true)]
        [InlineData("pt-BR", false)]
        [InlineData("pt-BR,en;q=0.9", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void AcceptLanguage_UsaAPrimeiraPreferencia(string? header, bool english) =>
            Assert.Equal(english, L.IsEnglish(header));
    }

    public class LocalizedSchemaEndpointTests : IClassFixture<WebApplicationFactory<DroidDeck.Program>>
    {
        // Nomes proprios: iguais nos dois idiomas.
        private static readonly HashSet<string> SameInEnglish = new()
        {
            "Discord", "OBS Studio", "Soundboard", "Windows", "Home Assistant", "Spotify", "Twitch", "YouTube", "Streaming",
        };

        private readonly WebApplicationFactory<DroidDeck.Program> _factory;

        public LocalizedSchemaEndpointTests(WebApplicationFactory<DroidDeck.Program> factory) => _factory = factory;

        private async Task<List<ActionSchema>> Schema(string language)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-API-KEY", ApiKeyProvider.GetKey());
            client.DefaultRequestHeaders.Add("Accept-Language", language);
            return (await client.GetFromJsonAsync<List<ActionSchema>>("/api/actions/schema"))!;
        }

        [Fact]
        public async Task SchemaEmIngles_TraduzTitulosCategoriasEDescricoes()
        {
            var pt = await Schema("pt-BR");
            var en = (await Schema("en")).ToDictionary(s => s.Type);

            foreach (var s in pt)
            {
                var t = en[s.Type];
                if (!SameInEnglish.Contains(s.Title)) Assert.NotEqual(s.Title, t.Title);
                if (!SameInEnglish.Contains(s.Category)) Assert.NotEqual(s.Category, t.Category);
                if (s.Description != null) Assert.NotEqual(s.Description, t.Description);
                // Valores e chaves nunca mudam.
                Assert.Equal(s.Fields.Select(f => f.Key), t.Fields.Select(f => f.Key));
                Assert.Equal(s.Fields.Select(f => f.Default), t.Fields.Select(f => f.Default));
            }
        }

        [Fact]
        public async Task SchemaEmPortugues_NaoMuda()
        {
            var pt = await Schema("pt-BR");
            Assert.Contains(pt, s => s.Title == "Abrir programa");
        }
    }
}
