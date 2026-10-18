using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DroidDeck.Models;
using DroidDeck.Services;
using Xunit;

namespace DroidDeck.Tests
{
    /// <summary>
    /// Arquivos montados aqui seguindo os formatos do app da Elgato (sem um .streamDeckProfile
    /// real no repositorio): 1.0 com pastas aninhadas e 2.0 com paginas em base32.
    /// </summary>
    public class StreamDeckImportTests
    {
        private static MemoryStream Zip(params (string path, string content)[] files)
        {
            var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (path, content) in files)
                {
                    using var w = new StreamWriter(zip.CreateEntry(path).Open(), Encoding.UTF8);
                    w.Write(content);
                }
            }
            ms.Position = 0;
            return ms;
        }

        private static string Hotkey(int vk, bool ctrl = false, bool shift = false) => $$$"""
            {"Name":"Hotkey","UUID":"com.elgato.streamdeck.system.hotkey","State":0,
             "States":[{"Title":"Copiar"}],
             "Settings":{"Hotkeys":[{"KeyCmd":false,"KeyCtrl":{{{ctrl.ToString().ToLower()}}},"KeyOption":false,"KeyShift":{{{shift.ToString().ToLower()}}},"VKeyCode":{{{vk}}}},
                                    {"KeyCmd":false,"KeyCtrl":false,"KeyOption":false,"KeyShift":false,"VKeyCode":-1}]}}
            """;

        [Fact]
        public void Formato1_PastasAtalhosEMidia()
        {
            var child = "B2C3D4E5-0000-0000-0000-000000000002";
            var root = $$$"""
                {"Name":"Live","Version":"1.0","Actions":{
                  "0,0": {{{Hotkey(0x43, ctrl: true)}}},
                  "1,0": {"Name":"Multimedia","UUID":"com.elgato.streamdeck.system.multimedia","States":[{"Title":""}],"Settings":{"actionIdx":1}},
                  "2,0": {"Name":"Create Folder","UUID":"com.elgato.streamdeck.profile.openchild","States":[{"Title":"Cenas"}],"Settings":{"ProfileUUID":"{{{child}}}"}},
                  "3,0": {"Name":"Weird Plugin","UUID":"com.someone.plugin.thing","States":[{"Title":""}],"Settings":{}},
                  "0,1": {"Name":"Mute","UUID":"com.elgato.discord.mute","States":[{"Title":"Mic"},{"Title":"Mudo"}],"Settings":{}}
                }}
                """;
            var folder = """
                {"Name":"","Version":"1.0","Actions":{
                  "0,0": {"Name":"Parent Folder","UUID":"com.elgato.streamdeck.profile.backtoparent","States":[{}],"Settings":{}},
                  "1,0": {"Name":"Text","UUID":"com.elgato.streamdeck.system.text","States":[{"Title":"gg"}],"Settings":{"pastedText":"gg (wp)","isSendingEnter":true}}
                }}
                """;
            using var zip = Zip(
                ("A1B2C3D4-0000-0000-0000-000000000001.sdProfile/manifest.json", root),
                ($"A1B2C3D4-0000-0000-0000-000000000001.sdProfile/Profiles/{child}.sdProfile/manifest.json", folder));

            var result = StreamDeckImport.Read(zip, 4, 8);

            Assert.Equal(2, result.Profiles.Count);
            var main = result.Profiles[0];
            Assert.Equal("Live", main.Name);
            Assert.Equal(4, main.Rows);
            Assert.Equal(8, main.Columns);

            var copy = main.Buttons.Single(b => b.Row == 0 && b.Column == 0);
            Assert.Equal("Copiar", copy.Label);
            Assert.Equal("hotkey", copy.Action!.Type);
            Assert.Equal("^c", copy.Action.Parameters["keys"]);

            Assert.Equal("next", main.Buttons.Single(b => b.Column == 1 && b.Row == 0).Action!.Parameters["command"]);

            var folderButton = main.Buttons.Single(b => b.Column == 2);
            Assert.Equal("open_profile", folderButton.Action!.Type);
            Assert.Equal(result.Profiles[1].Id, folderButton.Action.Parameters["profileId"]);
            Assert.Equal("Live — Cenas", result.Profiles[1].Name);

            var unknown = main.Buttons.Single(b => b.Column == 3);
            Assert.Null(unknown.Action);
            Assert.Equal("Weird Plugin", unknown.Label);
            Assert.Contains("Weird Plugin", (IEnumerable<string>)result.Unsupported);

            var mute = main.Buttons.Single(b => b.Row == 1);
            Assert.Equal("discord", mute.Action!.Type);
            Assert.Equal("Mudo", mute.ActiveLabel);
            Assert.False(mute.Toggle); // o Discord tem estado proprio

            var sub = result.Profiles[1];
            Assert.Equal("back", sub.Buttons.Single(b => b.Column == 0).Action!.Type);
            Assert.Equal("gg {(}wp{)}{ENTER}", sub.Buttons.Single(b => b.Column == 1).Action!.Parameters["keys"]);
        }

        [Fact]
        public void Formato2_PaginasComPastasEmBase32()
        {
            var page1 = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301");
            var page2 = Guid.Parse("8c1d2f6a-1b2c-4d3e-9f40-112233445566");
            var folder = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
            string Folder(Guid g)
            {
                // Mesmo esquema do app da Elgato: os 16 bytes em base32 (0-9A-V) + "Z".
                var bytes = Convert.FromHexString(g.ToString("N"));
                const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
                var sb = new StringBuilder();
                int buffer = 0, bits = 0;
                foreach (var b in bytes)
                {
                    buffer = (buffer << 8) | b; bits += 8;
                    while (bits >= 5) { sb.Append(alphabet[(buffer >> (bits - 5)) & 31]); bits -= 5; }
                }
                if (bits > 0) sb.Append(alphabet[(buffer << (5 - bits)) & 31]);
                return sb + "Z";
            }

            var profile = $$$"""
                {"Name":"Trabalho","Version":"2.0","Pages":{"Current":"{{{page2}}}","Default":"{{{page1}}}","Pages":["{{{page1}}}","{{{page2}}}"]}}
                """;
            string Page(string actions) => """{"Controllers":[{"Type":"Keypad","Actions":{""" + actions + "}}]}";
            using var zip = Zip(
                ("X.sdProfile/manifest.json", profile),
                // Pagina 2 vem antes no zip; a ordem certa vem do manifesto do perfil.
                ($"X.sdProfile/Profiles/{Folder(page2)}/manifest.json", Page($$$"""
                    "0,0": {"Name":"Website","UUID":"com.elgato.streamdeck.system.website","States":[{"Title":"Site"}],"Settings":{"path":"https://example.com"}}
                    """)),
                ($"X.sdProfile/Profiles/{Folder(page1)}/manifest.json", Page($$$"""
                    "0,0": {"Name":"Folder","UUID":"com.elgato.streamdeck.profile.openchild","States":[{"Title":"Apps"}],"Settings":{"ProfileUUID":"{{{folder}}}"}},
                    "1,0": {"Name":"Record","UUID":"com.elgato.obsstudio.record","States":[{"Title":"REC"},{"Title":"Gravando"}],"Settings":{}}
                    """)),
                ($"X.sdProfile/Profiles/{Folder(folder)}/manifest.json", Page("""
                    "0,0": {"Name":"Open","UUID":"com.elgato.streamdeck.system.open","States":[{"Title":"Bloco"}],"Settings":{"path":"\"C:\\Windows\\notepad.exe\""}}
                    """)));

            var result = StreamDeckImport.Read(zip, 3, 5);

            Assert.Equal(new[] { "Trabalho", "Trabalho (2)", "Trabalho — Apps" }, result.Profiles.Select(p => p.Name));
            var first = result.Profiles[0];
            Assert.Equal(result.Profiles[2].Id, first.Buttons.Single(b => b.Column == 0).Action!.Parameters["profileId"]);
            Assert.Equal("toggleRecord", first.Buttons.Single(b => b.Column == 1).Action!.Parameters["operation"]);
            Assert.Equal("https://example.com", result.Profiles[1].Buttons[0].Action!.Parameters["path"]);
            Assert.Equal(@"C:\Windows\notepad.exe", result.Profiles[2].Buttons[0].Action!.Parameters["path"]);
        }

        [Fact]
        public void PerfilMaiorQueAGrade_Reorganiza()
        {
            var actions = string.Join(",", Enumerable.Range(0, 8).Select(c =>
                $$$"""
                "{{{c}}},3": {"Name":"Website","UUID":"com.elgato.streamdeck.system.website","States":[{"Title":"B{{{c}}}"}],"Settings":{"path":"https://x/{{{c}}}"}}
                """));
            using var zip = Zip(("P.sdProfile/manifest.json", """{"Name":"XL","Actions":{""" + actions + "}}"));

            var buttons = StreamDeckImport.Read(zip, 2, 5).Profiles[0].Buttons;

            Assert.Equal(8, buttons.Count);
            Assert.All(buttons, b => Assert.True(b.Row < 2 && b.Column < 5));
            Assert.Equal("B5", buttons.Single(b => b.Row == 1 && b.Column == 0).Label);
        }

        [Fact]
        public void ArquivoQueNaoEPerfil_Erro()
        {
            using var zip = Zip(("leia-me.txt", "oi"));
            Assert.Throws<InvalidDataException>(() => StreamDeckImport.Read(zip, 3, 5));
        }

        [Fact]
        public void Atalhos_ModificadoresETeclasNomeadas()
        {
            var keys = JsonNode.Parse("""
                [{"KeyCmd":true,"KeyCtrl":false,"KeyOption":false,"KeyShift":true,"VKeyCode":83},
                 {"KeyCmd":false,"KeyCtrl":false,"KeyOption":true,"KeyShift":false,"VKeyCode":115},
                 {"VKeyCode":-1}]
                """) as JsonArray;
            Assert.Equal("+#s%{F4}", StreamDeckImport.Hotkeys(keys));
        }
    }
}
