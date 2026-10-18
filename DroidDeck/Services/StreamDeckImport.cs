using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DroidDeck.Models;

namespace DroidDeck.Services
{
    /// <summary>
    /// Importa um perfil exportado pelo app da Elgato (.streamDeckProfile, um zip).
    ///
    /// Formatos conhecidos:
    ///   1.0 (Stream Deck 4/5): &lt;id&gt;.sdProfile/manifest.json com "Actions" {"col,row": ...};
    ///       pastas em Profiles/&lt;id&gt;.sdProfile; imagens em "col,row/CustomImages/stateN.png".
    ///   2.0 (Stream Deck 6+):  &lt;id&gt;.sdProfile/manifest.json com "Pages"; cada pagina (e cada
    ///       pasta) em Profiles/&lt;pasta&gt;/manifest.json com "Controllers"[{"Type":"Keypad",
    ///       "Actions": {...}}]; imagens em Images/ dentro da pagina.
    /// Cada pagina vira um perfil do DroidDeck; pastas viram perfis abertos por "Abrir perfil".
    /// O que nao tem equivalente fica com o titulo e a imagem, sem acao.
    /// </summary>
    public static class StreamDeckImport
    {
        public const long MaxSize = 50 * 1024 * 1024;
        private const int IconSize = 144;

        public class Result
        {
            public List<DeckProfile> Profiles { get; } = new();
            public int Buttons { get; set; }
            /// <summary>Nomes das acoes sem equivalente (sem repeticao).</summary>
            public SortedSet<string> Unsupported { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private class Page
        {
            public string Dir = "";          // pasta da pagina dentro do zip, com "/" no fim
            public string Key = "";          // nome da pasta, sem ".sdProfile", em maiusculas
            public JsonObject Actions = new();
            public string? Name;
            public DeckProfile Profile = new();
            public bool IsChild;
        }

        public static Result Read(Stream stream, int rows, int columns)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            var entries = zip.Entries
                .Where(e => e.Length > 0)
                .GroupBy(e => e.FullName.Replace('\\', '/'), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var pages = new List<Page>();
            string? profileName = null;
            List<string>? pageOrder = null;

            foreach (var (path, entry) in entries)
            {
                if (!path.EndsWith("manifest.json", StringComparison.OrdinalIgnoreCase)) continue;
                JsonObject? json;
                try
                {
                    using var s = entry.Open();
                    json = JsonNode.Parse(s) as JsonObject;
                }
                catch (JsonException)
                {
                    continue;
                }
                if (json == null) continue;

                var dir = path[..^"manifest.json".Length];
                var actions = json["Actions"] as JsonObject
                    ?? (json["Controllers"] as JsonArray)?.OfType<JsonObject>()
                        .FirstOrDefault(c => (string?)c["Type"] is null or "Keypad")?["Actions"] as JsonObject;

                if (json["Pages"] is JsonObject pagesInfo)
                {
                    // Manifesto do perfil (2.0): nome e ordem das paginas.
                    profileName ??= (string?)json["Name"];
                    pageOrder = (pagesInfo["Pages"] as JsonArray)?.Select(p => Normalize((string?)p ?? "")).ToList();
                    if (actions == null) continue;
                }
                if (actions == null) continue;

                var key = Normalize(dir.TrimEnd('/').Split('/').Last());
                pages.Add(new Page
                {
                    Dir = dir,
                    Key = key,
                    Actions = actions,
                    Name = (string?)json["Name"] is { Length: > 0 } n ? n : null,
                });
                // 1.0: o manifesto da raiz tem o nome do perfil e as acoes juntos.
                if (dir.Count(c => c == '/') == 1) profileName ??= (string?)json["Name"];
            }

            if (pages.Count == 0)
                throw new InvalidDataException("O arquivo não parece um perfil do Stream Deck.");

            profileName = string.IsNullOrWhiteSpace(profileName) ? "Stream Deck" : profileName.Trim();

            // Ids novos: importar duas vezes nao sobrescreve nada.
            foreach (var page in pages) page.Profile.Id = Guid.NewGuid().ToString();

            // Pastas apontam para outras paginas pelo UUID.
            var byKey = new Dictionary<string, Page>(StringComparer.OrdinalIgnoreCase);
            foreach (var page in pages) byKey.TryAdd(page.Key, page);

            var result = new Result();
            var ordered = pages
                .OrderBy(p => pageOrder?.FindIndex(o => Matches(o, p.Key)) is int i and >= 0 ? i : int.MaxValue)
                .ThenBy(p => p.Dir.Count(c => c == '/'))
                .ThenBy(p => p.Dir, StringComparer.Ordinal)
                .ToList();

            foreach (var page in ordered)
            {
                foreach (var (_, node) in page.Actions)
                {
                    if (node is not JsonObject a || !IsFolder(a)) continue;
                    var target = FindPage(byKey, (string?)a["Settings"]?["ProfileUUID"]);
                    if (target != null && target != page) target.IsChild = true;
                }
            }

            var topIndex = 0;
            foreach (var page in ordered)
            {
                var profile = page.Profile;
                profile.Rows = rows;
                profile.Columns = columns;
                profile.Buttons = BuildButtons(page, entries, byKey, rows, columns, result);
                result.Buttons += profile.Buttons.Count;
                if (!page.IsChild)
                {
                    topIndex++;
                    profile.Name = topIndex == 1 ? profileName : $"{profileName} ({topIndex})";
                }
                result.Profiles.Add(profile);
            }

            // Pastas: nome do botao que as abre.
            foreach (var page in ordered.Where(p => p.IsChild))
            {
                var opener = result.Profiles.SelectMany(p => p.Buttons)
                    .FirstOrDefault(b => b.Action?.Type == "open_profile" &&
                                         b.Action.Parameters.GetValueOrDefault("profileId") == page.Profile.Id);
                var label = !string.IsNullOrWhiteSpace(opener?.Label) ? opener!.Label!.Replace('\n', ' ') : page.Name ?? "Pasta";
                page.Profile.Name = $"{profileName} — {label}";
            }

            return result;
        }

        private static List<DeckButton> BuildButtons(Page page, Dictionary<string, ZipArchiveEntry> entries,
            Dictionary<string, Page> byKey, int rows, int columns, Result result)
        {
            var buttons = new List<(int row, int col, DeckButton button)>();
            foreach (var (coord, node) in page.Actions)
            {
                if (node is not JsonObject a) continue;
                var parts = coord.Split(',');
                if (parts.Length != 2 || !int.TryParse(parts[0], out var col) || !int.TryParse(parts[1], out var row)) continue;

                var states = (a["States"] as JsonArray)?.OfType<JsonObject>().ToList() ?? new();
                var button = new DeckButton
                {
                    Label = Title(states.ElementAtOrDefault(0)),
                    IconBase64 = Image(entries, page, coord, states.ElementAtOrDefault(0), 0),
                    BackgroundColor = "#1F1F23",
                };
                button.Action = MapAction(a, byKey, result);
                if (button.Action == null)
                {
                    var uuid = (string?)a["UUID"] ?? "";
                    // Sem imagem nem titulo, o botao vazio nao diz nada: usa o nome da acao.
                    if (string.IsNullOrEmpty(button.Label) && button.IconBase64 == null)
                        button.Label = (string?)a["Name"];
                    if (!uuid.StartsWith("com.elgato.streamdeck.page.", StringComparison.OrdinalIgnoreCase))
                        result.Unsupported.Add((string?)a["Name"] is { Length: > 0 } name ? name : uuid);
                }
                else if (button.Action.Type == "open_profile" && button.IconBase64 == null)
                {
                    button.IconName = "folder";
                }
                else if (button.Action.Type == "back" && button.IconBase64 == null)
                {
                    button.IconName = "arrow_back";
                }

                // Segundo estado (mudo/desmudo, gravando...): rotulo e imagem do estado ativo.
                if (states.Count > 1)
                {
                    var activeLabel = Title(states[1]);
                    if (!string.IsNullOrEmpty(activeLabel) && activeLabel != button.Label) button.ActiveLabel = activeLabel;
                    button.ActiveIconBase64 = Image(entries, page, coord, states[1], 1);
                    // Acao sem estado proprio (atalho alternado): o DroidDeck guarda o estado.
                    if (button.Action?.Type is "hotkey" or "launch_app") button.Toggle = true;
                }
                buttons.Add((row, col, button));
            }

            // Perfil de um Stream Deck maior que a grade do celular: reorganiza em ordem de
            // leitura para nenhum botao ficar fora da tela.
            var fits = buttons.All(b => b.row < rows && b.col < columns);
            var ordered = buttons.OrderBy(b => b.row).ThenBy(b => b.col).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                var (row, col, button) = ordered[i];
                button.Row = fits ? row : i / columns;
                button.Column = fits ? col : i % columns;
            }
            return ordered.Select(b => b.button).Where(b => b.Row < rows).ToList();
        }

        // ---- Acoes ----

        private static bool IsFolder(JsonObject a) =>
            ((string?)a["UUID"] ?? "").Equals("com.elgato.streamdeck.profile.openchild", StringComparison.OrdinalIgnoreCase);

        private static DeckAction? MapAction(JsonObject a, Dictionary<string, Page> byKey, Result result)
        {
            var uuid = ((string?)a["UUID"] ?? "").ToLowerInvariant();
            var settings = a["Settings"] as JsonObject ?? new JsonObject();
            string S(string key) => settings[key] is JsonValue v && v.TryGetValue<string>(out var s) ? s : settings[key]?.ToString() ?? "";

            switch (uuid)
            {
                case "com.elgato.streamdeck.system.hotkey":
                case "com.elgato.streamdeck.system.hotkeyswitch":
                    return Hotkeys(settings["Hotkeys"] as JsonArray) is { Length: > 0 } keys ? Act("hotkey", ("keys", keys)) : null;

                case "com.elgato.streamdeck.system.text":
                    var text = EscapeText(S("pastedText"));
                    if (settings["isSendingEnter"]?.GetValue<bool>() == true) text += "{ENTER}";
                    return text.Length > 0 ? Act("hotkey", ("keys", text)) : null;

                case "com.elgato.streamdeck.system.open":
                case "com.elgato.streamdeck.system.website":
                    var path = S("path").Trim().Trim('"');
                    return path.Length > 0 ? Act("launch_app", ("path", path)) : null;

                case "com.elgato.streamdeck.system.multimedia":
                    return S("actionIdx") switch
                    {
                        "0" => Act("media", ("command", "playpause")),
                        "1" => Act("media", ("command", "next")),
                        "2" => Act("media", ("command", "previous")),
                        "3" => Act("mixer", ("operation", "volumeUp"), ("deviceId", "default")),
                        "4" => Act("mixer", ("operation", "volumeDown"), ("deviceId", "default")),
                        "5" => Act("mixer", ("operation", "toggleMute"), ("deviceId", "default")),
                        _ => Act("media", ("command", "playpause")),
                    };

                case "com.elgato.streamdeck.profile.openchild":
                    var target = FindPage(byKey, S("ProfileUUID"));
                    return target != null ? Act("open_profile", ("profileId", target.Profile.Id)) : null;

                case "com.elgato.streamdeck.profile.backtoparent":
                    return Act("back");

                case "com.elgato.streamdeck.multiactions.routine":
                case "com.elgato.streamdeck.multiactions.routine2":
                    return MultiAction(settings, byKey, result);

                // Plugin oficial do Discord
                case "com.elgato.discord.mute":
                    return Act("discord", ("operation", "toggleMute"));
                case "com.elgato.discord.deafen":
                    return Act("discord", ("operation", "toggleDeafen"));

                // Plugin oficial do OBS
                case "com.elgato.obsstudio.record":
                    return Act("obs", ("operation", "toggleRecord"));
                case "com.elgato.obsstudio.stream":
                    return Act("obs", ("operation", "toggleStream"));
                case "com.elgato.obsstudio.replaybuffer":
                    return Act("obs", ("operation", "toggleReplayBuffer"));
                case "com.elgato.obsstudio.virtualcamera":
                case "com.elgato.obsstudio.virtualcam":
                    return Act("obs", ("operation", "toggleVirtualCam"));
                case "com.elgato.obsstudio.scene":
                    var scene = S("sceneId") is { Length: > 0 } id ? id : S("scene");
                    return scene.Length > 0 ? Act("obs", ("operation", "setScene"), ("scene", scene)) : null;
            }
            return null;
        }

        private static DeckAction? MultiAction(JsonObject settings, Dictionary<string, Page> byKey, Result result)
        {
            var routine = settings["Routine"] as JsonArray ?? settings["Actions"] as JsonArray;
            if (routine == null) return null;
            var steps = new List<MultiStep>();
            foreach (var node in routine.OfType<JsonObject>())
            {
                var uuid = ((string?)node["UUID"] ?? "").ToLowerInvariant();
                if (uuid == "com.elgato.streamdeck.system.delay")
                {
                    var ms = int.TryParse(node["Settings"]?["delayTime"]?.ToString(), out var d) ? d : 0;
                    if (steps.Count > 0) steps[^1].DelayMs += ms;
                    continue;
                }
                var step = MapAction(node, byKey, result);
                if (step == null)
                {
                    result.Unsupported.Add((string?)node["Name"] is { Length: > 0 } name ? name : uuid);
                    continue;
                }
                steps.Add(new MultiStep { Type = step.Type, Parameters = step.Parameters });
            }
            if (steps.Count == 0) return null;
            return Act("multi", ("steps", JsonSerializer.Serialize(steps,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));
        }

        private static DeckAction Act(string type, params (string key, string value)[] parameters)
        {
            var action = new DeckAction { Type = type };
            foreach (var (key, value) in parameters) action.Parameters[key] = value;
            return action;
        }

        // ---- Teclas ----

        /// <summary>Hotkeys da Elgato (virtual-key do Windows + modificadores) na sintaxe do SendKeys.</summary>
        public static string Hotkeys(JsonArray? hotkeys)
        {
            if (hotkeys == null) return "";
            var sb = new StringBuilder();
            foreach (var h in hotkeys.OfType<JsonObject>())
            {
                var vk = h["VKeyCode"]?.GetValue<int>() ?? -1;
                if (vk <= 0) continue;
                var key = KeyFromVk(vk);
                if (key == null) continue;
                if (h["KeyCtrl"]?.GetValue<bool>() == true) sb.Append('^');
                if (h["KeyShift"]?.GetValue<bool>() == true) sb.Append('+');
                if (h["KeyOption"]?.GetValue<bool>() == true) sb.Append('%');
                if (h["KeyCmd"]?.GetValue<bool>() == true) sb.Append('#');
                sb.Append(key);
            }
            return sb.ToString();
        }

        private static readonly Dictionary<int, string> VkNames = new()
        {
            [0x08] = "{BS}", [0x09] = "{TAB}", [0x0D] = "{ENTER}", [0x13] = "{BREAK}", [0x14] = "{CAPSLOCK}",
            [0x1B] = "{ESC}", [0x20] = "{SPACE}", [0x21] = "{PGUP}", [0x22] = "{PGDN}", [0x23] = "{END}",
            [0x24] = "{HOME}", [0x25] = "{LEFT}", [0x26] = "{UP}", [0x27] = "{RIGHT}", [0x28] = "{DOWN}",
            [0x2C] = "{PRTSC}", [0x2D] = "{INS}", [0x2E] = "{DEL}", [0x5B] = "{LWIN}", [0x5D] = "{APPS}",
            [0x6A] = "{MULTIPLY}", [0x6B] = "{ADD}", [0x6D] = "{SUBTRACT}", [0x6E] = "{DECIMAL}", [0x6F] = "{DIVIDE}",
            [0x90] = "{NUMLOCK}", [0x91] = "{SCROLLLOCK}",
            [0xAD] = "{VOLUME_MUTE}", [0xAE] = "{VOLUME_DOWN}", [0xAF] = "{VOLUME_UP}",
            [0xB0] = "{MEDIA_NEXT}", [0xB1] = "{MEDIA_PREV}", [0xB2] = "{MEDIA_STOP}", [0xB3] = "{MEDIA_PLAY_PAUSE}",
            // Pontuacao no layout americano (o da Elgato grava a tecla, nao o caractere).
            [0xBA] = ";", [0xBB] = "=", [0xBC] = ",", [0xBD] = "-", [0xBE] = ".", [0xBF] = "/", [0xC0] = "`",
            [0xDB] = "[", [0xDC] = "\\", [0xDD] = "]", [0xDE] = "'",
        };

        private static string? KeyFromVk(int vk)
        {
            if (vk is >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A) return char.ToLowerInvariant((char)vk).ToString();
            if (vk is >= 0x60 and <= 0x69) return "{NUMPAD" + (vk - 0x60) + "}";
            if (vk is >= 0x70 and <= 0x87) return "{F" + (vk - 0x6F) + "}";
            return VkNames.GetValueOrDefault(vk);
        }

        /// <summary>Texto para digitar, com os caracteres especiais do SendKeys entre chaves.</summary>
        public static string EscapeText(string text)
        {
            var sb = new StringBuilder();
            foreach (var c in text.Replace("\r\n", "\n"))
            {
                if (c == '\n') sb.Append("{ENTER}");
                else if ("+^%#~(){}[]".Contains(c)) sb.Append('{').Append(c).Append('}');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        // ---- Paginas e imagens ----

        private static string Normalize(string name)
        {
            if (name.EndsWith(".sdProfile", StringComparison.OrdinalIgnoreCase)) name = name[..^".sdProfile".Length];
            return name.ToUpperInvariant();
        }

        private static Page? FindPage(Dictionary<string, Page> byKey, string? uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid)) return null;
            var key = Normalize(uuid);
            if (byKey.TryGetValue(key, out var page)) return page;
            return byKey.Values.FirstOrDefault(p => Matches(key, p.Key));
        }

        /// <summary>
        /// A pasta de uma pagina 2.0 nao tem o UUID escrito como na referencia: e o UUID em
        /// base32. Compara pelas formas conhecidas.
        /// </summary>
        private static bool Matches(string uuid, string folder)
        {
            if (string.Equals(uuid, folder, StringComparison.OrdinalIgnoreCase)) return true;
            if (!Guid.TryParse(uuid, out var guid)) return false;
            var bytes = Convert.FromHexString(guid.ToString("N"));
            foreach (var alphabet in new[] { "0123456789ABCDEFGHIJKLMNOPQRSTUV", "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567" })
            {
                var encoded = Base32(bytes, alphabet);
                if (folder.StartsWith(encoded, StringComparison.OrdinalIgnoreCase) ||
                    encoded.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && folder.Length >= 24)
                    return true;
            }
            return false;
        }

        private static string Base32(byte[] data, string alphabet)
        {
            var sb = new StringBuilder();
            int buffer = 0, bits = 0;
            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bits += 8;
                while (bits >= 5)
                {
                    sb.Append(alphabet[(buffer >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }
            if (bits > 0) sb.Append(alphabet[(buffer << (5 - bits)) & 31]);
            return sb.ToString();
        }

        private static string? Title(JsonObject? state)
        {
            if (state == null) return null;
            if (state["ShowTitle"] is JsonValue show && show.TryGetValue<bool>(out var visible) && !visible) return null;
            var title = ((string?)state["Title"])?.Trim();
            return string.IsNullOrEmpty(title) ? null : title;
        }

        private static string? Image(Dictionary<string, ZipArchiveEntry> entries, Page page, string coord, JsonObject? state, int index)
        {
            var candidates = new List<string>();
            if ((string?)state?["Image"] is { Length: > 0 } image && !image.StartsWith("data:"))
                candidates.Add(page.Dir + image.TrimStart('/'));
            candidates.Add($"{page.Dir}{coord}/CustomImages/state{index}.png");
            candidates.Add($"{page.Dir}{coord}/CustomImages/state{index}.jpg");
            candidates.Add($"{page.Dir}{coord}/CustomImages/state{index}.gif");

            foreach (var path in candidates)
            {
                if (!entries.TryGetValue(path, out var entry)) continue;
                try
                {
                    using var s = entry.Open();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    return Resize(ms.ToArray());
                }
                catch
                {
                    // Imagem corrompida ou formato que o GDI+ nao le (SVG): segue sem ela.
                }
            }
            return null;
        }

        /// <summary>Reduz para 144 px em PNG (as imagens da Elgato chegam a 288 px).</summary>
        private static string Resize(byte[] bytes)
        {
            using var input = new MemoryStream(bytes);
            using var source = System.Drawing.Image.FromStream(input);
            // GIF animado vai como esta: reduzir perderia a animacao.
            if (source.RawFormat.Equals(ImageFormat.Gif) && bytes.Length <= 2 * 1024 * 1024)
                return Convert.ToBase64String(bytes);
            if (source.Width <= IconSize && source.Height <= IconSize && source.RawFormat.Equals(ImageFormat.Png))
                return Convert.ToBase64String(bytes);
            using var bitmap = new Bitmap(IconSize, IconSize);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, IconSize, IconSize);
            }
            using var output = new MemoryStream();
            bitmap.Save(output, ImageFormat.Png);
            return Convert.ToBase64String(output.ToArray());
        }
    }
}
