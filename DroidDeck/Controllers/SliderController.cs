using System;
using System.Diagnostics;
using DroidDeck.Services;
using Microsoft.AspNetCore.Mvc;
using NAudio.CoreAudioApi;

namespace DroidDeck.Controllers
{
    /// <summary>
    /// Botao controle deslizante: volume do som do PC, do microfone ou de um app, arrastando
    /// o dedo. Usa sempre o dispositivo padrao do Windows (o id muda quando se troca de fone).
    ///
    /// target: "output" (padrao), "input" ou "app" (com app = nome do processo, sem .exe).
    /// </summary>
    [ApiController]
    [Route("api/slider")]
    public class SliderController : ControllerBase
    {
        private readonly MixerService _mixer;

        public SliderController(MixerService mixer) => _mixer = mixer;

        public class SliderRequest
        {
            public string? Target { get; set; }
            public string? App { get; set; }
            public int Volume { get; set; }
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? target, [FromQuery] string? app)
        {
            var state = Apply(target, app, null);
            return state == null ? NotFound() : Ok(state);
        }

        [HttpPut]
        public IActionResult Set([FromBody] SliderRequest request)
        {
            var state = Apply(request.Target, request.App, Math.Clamp(request.Volume, 0, 100));
            return state == null ? NotFound() : Ok(state);
        }

        /// <summary>Le (volume null) ou muda o volume. Null se o alvo nao existe agora.</summary>
        private object? Apply(string? target, string? app, int? volume)
        {
            if (string.Equals(target, "app", StringComparison.OrdinalIgnoreCase))
                return AppVolume(app ?? "", volume);

            var flow = string.Equals(target, "input", StringComparison.OrdinalIgnoreCase) ? DataFlow.Capture : DataFlow.Render;
            var id = _mixer.GetDefaultDeviceId(flow);
            if (id == null) return null;
            var state = _mixer.SetDeviceAudio(id, volume: volume);
            return state == null ? null : new { volume = state.Volume, mute = state.Mute, label = state.Title };
        }

        /// <summary>Volume das sessoes de audio de um app no dispositivo de saida padrao.</summary>
        private static object? AppVolume(string app, int? volume)
        {
            app = app.Trim();
            if (app.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) app = app[..^4];
            if (app.Length == 0) return null;

            using var en = new MMDeviceEnumerator();
            using var device = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;
            int? current = null;
            var mute = false;
            for (var i = 0; i < sessions.Count; i++)
            {
                using var session = sessions[i];
                if (session.IsSystemSoundsSession) continue;
                string name;
                try
                {
                    using var p = Process.GetProcessById((int)session.GetProcessID);
                    name = p.ProcessName;
                }
                catch
                {
                    continue;
                }
                if (!name.Equals(app, StringComparison.OrdinalIgnoreCase)) continue;
                // Um app pode ter varias sessoes (navegador com abas): mexe em todas.
                if (volume.HasValue) session.SimpleAudioVolume.Volume = volume.Value / 100f;
                current ??= (int)Math.Round(session.SimpleAudioVolume.Volume * 100);
                mute |= session.SimpleAudioVolume.Mute;
            }
            return current == null ? null : new { volume = current, mute, label = app };
        }
    }
}
