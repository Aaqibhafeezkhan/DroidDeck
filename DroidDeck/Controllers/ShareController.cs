using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DroidDeck.Hubs;
using DroidDeck.Lib;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DroidDeck.Controllers
{
    /// <summary>Compartilhar um perfil por link ou QR, e importar de um link (ver ProfileShare).</summary>
    [ApiController]
    [Route("api/share")]
    public class ShareController : ControllerBase
    {
        private readonly StreamDeckConfigService _config;
        private readonly IHubContext<DeckHub> _hub;

        public ShareController(StreamDeckConfigService config, IHubContext<DeckHub> hub)
        {
            _config = config;
            _hub = hub;
        }

        [HttpGet("{profileId}")]
        public IActionResult Get(string profileId)
        {
            var profile = _config.GetProfile(profileId);
            if (profile == null) return NotFound();
            var data = ProfileShare.Encode(profile);
            return Ok(new
            {
                data,
                url = ProfileShare.SiteUrl + "#" + data,
                // QR comporta ~2,9 KB; acima disso so o link.
                qr = data.Length + ProfileShare.SiteUrl.Length < 2800,
                images = profile.Buttons.Any(b => b.IconBase64 != null || b.ActiveIconBase64 != null),
            });
        }

        [HttpGet("{profileId}/qr")]
        public IActionResult Qr(string profileId)
        {
            var profile = _config.GetProfile(profileId);
            if (profile == null) return NotFound();
            var url = ProfileShare.SiteUrl + "#" + ProfileShare.Encode(profile);
            return File(PairingInfo.BuildQrPng(url, 6), "image/png");
        }

        public class ImportRequest
        {
            public string? Data { get; set; }
        }

        /// <summary>
        /// Importa o perfil de um link como perfil novo. De fora do PC, botoes que executam
        /// codigo entram sem a acao (mesma regra do POST profiles).
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] ImportRequest request)
        {
            Models.DeckProfile profile;
            try { profile = ProfileShare.Decode(request.Data ?? ""); }
            catch (InvalidDataException ex) { return BadRequest(new { error = L.T(ex.Message) }); }

            var stripped = 0;
            if (!ActionPolicy.IsLocal(HttpContext.Connection))
            {
                foreach (var b in profile.Buttons)
                {
                    foreach (var slot in Models.DeckButton.ActionSlots)
                    {
                        if (!ActionPolicy.IsPrivileged(b.ActionFor(slot))) continue;
                        switch (slot)
                        {
                            case "longPress": b.LongPressAction = null; break;
                            case "doubleTap": b.DoubleTapAction = null; break;
                            case "active": b.ActiveAction = null; break;
                            default: b.Action = null; break;
                        }
                        stripped++;
                    }
                    if (ActionPolicy.IsPrivilegedInfo(b))
                    {
                        b.DynamicType = null;
                        b.DynamicParameters = null;
                        stripped++;
                    }
                }
            }

            _config.SaveProfile(profile);
            await _hub.Clients.All.SendAsync("ReceiveDeckUpdate", new { profileId = profile.Id });
            return Ok(new { profileId = profile.Id, name = profile.Name, buttons = profile.Buttons.Count, stripped });
        }
    }
}
