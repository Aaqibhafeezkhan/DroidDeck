using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using DroidDeck.Models;

namespace DroidDeck.Services
{
    /// <summary>
    /// Perfil num texto curto para link e QR: JSON comprimido (deflate) em base64url.
    /// O link do site (me.gf2.in/DroidDeck/share/#...) mostra o perfil e leva ao configurador
    /// do PC com ?import=..., que chama o POST /api/share/import.
    ///
    /// Imagens proprias ficam de fora (o texto ficaria grande demais para um QR); o icone
    /// embutido, o rotulo, as cores e as acoes vao. Para levar tudo, o backup .droiddeck.
    /// </summary>
    public static class ProfileShare
    {
        public const string SiteUrl = "https://me.gf2.in/DroidDeck/share/";
        public const int MaxLength = 64 * 1024;

        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        public class Shared
        {
            public int V { get; set; } = 1;
            public DeckProfile Profile { get; set; } = new();
        }

        public static string Encode(DeckProfile profile)
        {
            var copy = JsonSerializer.Deserialize<DeckProfile>(JsonSerializer.Serialize(profile, Json), Json)!;
            copy.IsDefault = false;
            copy.AutoSwitchApps = null;
            foreach (var b in copy.Buttons)
            {
                b.IconBase64 = null;
                b.ActiveIconBase64 = null;
            }
            var json = JsonSerializer.SerializeToUtf8Bytes(new Shared { Profile = copy }, Json);
            using var ms = new MemoryStream();
            using (var deflate = new DeflateStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
                deflate.Write(json);
            return Convert.ToBase64String(ms.ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        /// <summary>Perfil do texto, com ids novos (importar duas vezes nao sobrescreve).</summary>
        public static DeckProfile Decode(string data)
        {
            if (string.IsNullOrWhiteSpace(data) || data.Length > MaxLength)
                throw new InvalidDataException("Link de perfil inválido.");
            try
            {
                var b64 = data.Trim().Replace('-', '+').Replace('_', '/');
                b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
                using var input = new MemoryStream(Convert.FromBase64String(b64));
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var limited = new MemoryStream();
                var buffer = new byte[8192];
                int read;
                while ((read = deflate.Read(buffer, 0, buffer.Length)) > 0)
                {
                    limited.Write(buffer, 0, read);
                    if (limited.Length > 4 * 1024 * 1024) throw new InvalidDataException("Link de perfil inválido.");
                }
                var shared = JsonSerializer.Deserialize<Shared>(limited.ToArray(), Json);
                if (shared?.Profile == null) throw new InvalidDataException("Link de perfil inválido.");
                var p = shared.Profile;
                p.Id = Guid.NewGuid().ToString();
                p.IsDefault = false;
                p.AutoSwitchApps = null;
                p.Name = string.IsNullOrWhiteSpace(p.Name) ? "Perfil compartilhado" : p.Name.Trim();
                p.Buttons ??= new List<DeckButton>();
                foreach (var b in p.Buttons) b.Id = Guid.NewGuid().ToString();
                return p;
            }
            catch (Exception ex) when (ex is FormatException or JsonException or InvalidDataException or IOException)
            {
                throw new InvalidDataException("Link de perfil inválido.", ex);
            }
        }
    }
}
