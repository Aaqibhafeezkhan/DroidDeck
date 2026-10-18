using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DroidDeck.Auth;
using DroidDeck.Models;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DroidDeck.Tests
{
    public class ProfileShareTests
    {
        private static DeckProfile Sample() => new()
        {
            Id = "orig",
            Name = "Live",
            IsDefault = true,
            Buttons =
            {
                new DeckButton { Id = "b1", Row = 0, Column = 0, Label = "Cena", IconName = "videocam", IconBase64 = new string('A', 5000),
                    ColumnSpan = 2, Action = new DeckAction { Type = "obs", Parameters = { ["operation"] = "setScene", ["scene"] = "Jogo" } } },
                new DeckButton { Id = "b2", Row = 1, Column = 0, Label = "Copiar",
                    Action = new DeckAction { Type = "hotkey", Parameters = { ["keys"] = "^c" } } },
            },
        };

        [Fact]
        public void IdaEVolta_SemImagensEComIdsNovos()
        {
            var data = ProfileShare.Encode(Sample());
            Assert.True(data.Length < 600, $"texto grande demais: {data.Length}");
            Assert.DoesNotContain('+', data);
            Assert.DoesNotContain('/', data);

            var p = ProfileShare.Decode(data);
            Assert.NotEqual("orig", p.Id);
            Assert.False(p.IsDefault);
            Assert.Equal("Live", p.Name);
            Assert.Equal(2, p.Buttons.Count);
            Assert.Null(p.Buttons[0].IconBase64);
            Assert.Equal("videocam", p.Buttons[0].IconName);
            Assert.Equal(2, p.Buttons[0].ColumnSpan);
            Assert.Equal("Jogo", p.Buttons[0].Action!.Parameters["scene"]);
            Assert.NotEqual("b1", p.Buttons[0].Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("isso nao e base64!")]
        [InlineData("aGVsbG8")]
        public void TextoInvalido_Erro(string data) =>
            Assert.Throws<InvalidDataException>(() => ProfileShare.Decode(data));
    }

    public class ProfileShareEndpointTests : IClassFixture<WebApplicationFactory<DroidDeck.Program>>
    {
        private readonly WebApplicationFactory<DroidDeck.Program> _factory;

        public ProfileShareEndpointTests(WebApplicationFactory<DroidDeck.Program> factory) => _factory = factory;

        [Fact]
        public async Task ImportarDeFora_TiraOAtalho()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-API-KEY", ApiKeyProvider.GetKey());
            var profile = new DeckProfile
            {
                Name = "Compartilhado",
                Buttons =
                {
                    new DeckButton { Id = "x", Label = "Copiar", Action = new DeckAction { Type = "hotkey", Parameters = { ["keys"] = "^c" } } },
                    new DeckButton { Id = "y", Row = 1, Label = "Play", Action = new DeckAction { Type = "media", Parameters = { ["command"] = "playpause" } } },
                },
            };
            var resp = await client.PostAsJsonAsync("/api/share/import", new { data = ProfileShare.Encode(profile) });
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetProperty("stripped").GetInt32());

            var saved = await client.GetFromJsonAsync<DeckProfile>("/api/streamdeck/profiles/" + body.GetProperty("profileId").GetString());
            Assert.Null(saved!.Buttons.Find(b => b.Label == "Copiar")!.Action);
            Assert.Equal("media", saved.Buttons.Find(b => b.Label == "Play")!.Action!.Type);

            var bad = await client.PostAsJsonAsync("/api/share/import", new { data = "lixo" });
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        }
    }
}
