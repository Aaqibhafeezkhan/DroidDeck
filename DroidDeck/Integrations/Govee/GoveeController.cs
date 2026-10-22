using System;
using System.Linq;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Integrations;
using DroidDeck.Lib;
using Microsoft.AspNetCore.Mvc;

namespace DroidDeck.Controllers
{
    /// <summary>Busca dos aparelhos Govee na rede (lista do campo "Aparelho" do editor).</summary>
    [Route("api/govee")]
    [ApiController]
    public class GoveeController : ControllerBase
    {
        private readonly GoveeLan _lan;

        public GoveeController(GoveeLan lan) => _lan = lan;

        [HttpGet("devices")]
        public async Task<ActionResult> Devices()
        {
            try
            {
                var devices = await _lan.ScanAsync(TimeSpan.FromSeconds(2));
                return Ok(devices.Select(d => new FieldOption(d.Ip, d.Sku != null ? $"{d.Sku} ({d.Ip})" : d.Ip)));
            }
            catch (Exception ex) { return StatusCode(502, new { error = L.Message(ex) }); }
        }
    }
}
