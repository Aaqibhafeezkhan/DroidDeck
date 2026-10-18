using System.Collections.Generic;
using System.Threading.Tasks;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc;

namespace DroidDeck.Controllers
{
    /// <summary>Automacoes: acontecimentos no PC que apertam um botao (ver TriggerService).</summary>
    [ApiController]
    [Route("api/triggers")]
    public class TriggersController : ControllerBase
    {
        private readonly TriggerService _triggers;

        public TriggersController(TriggerService triggers) => _triggers = triggers;

        [HttpGet]
        public IActionResult Get() => Ok(_triggers.GetAll());

        /// <summary>Troca a lista inteira (o app edita e manda tudo de volta).</summary>
        [HttpPut]
        public IActionResult Save([FromBody] List<TriggerService.Trigger> triggers)
        {
            _triggers.SaveAll(triggers ?? new());
            return Ok(_triggers.GetAll());
        }

        /// <summary>Testa um gatilho agora (sem esperar o acontecimento).</summary>
        [HttpPost("{id}/run")]
        public async Task<IActionResult> Run(string id)
        {
            var trigger = _triggers.GetAll().Find(t => t.Id == id);
            if (trigger == null) return NotFound();
            return await _triggers.RunAsync(trigger) ? Ok() : NotFound();
        }
    }
}
