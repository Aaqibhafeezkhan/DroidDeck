using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DroidDeck.Hubs;
using DroidDeck.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// MQTT generico: o DroidDeck fica conectado ao broker configurado (Mosquitto, o do Home
    /// Assistant, Node-RED...) e os botoes publicam mensagens. So publica; nao assina nada.
    /// </summary>
    public class MqttIntegration : AccountIntegration
    {
        public const string Event = "ReceiveMqttState";

        private IMqttClient? _client;
        private bool _connected;
        private string? _error;
        private CancellationTokenSource? _reconnect;

        public MqttIntegration(ILogger<MqttIntegration> logger, IHubContext<DeckHub> hub, StreamDeckConfigService config)
            : base(logger, hub, config) { }

        public override string Name => "mqtt";
        public override string Title => "MQTT";
        public override string StateEvent => Event;

        private string? Server => Get("server");

        public override IntegrationStatus GetStatus() => new()
        {
            Connected = _connected,
            Configured = Server != null,
            Account = _connected ? Server : null,
            Message = _connected ? null : _error,
            Fields = new() { ["server"] = Server ?? "", ["username"] = Get("username") ?? "" },
        };

        public override object GetState() => new { connected = _connected };

        /// <summary>Endereco do broker: "host", "host:porta", "mqtt://host:1883" ou "mqtts://host:8883" (TLS).</summary>
        public record BrokerAddress(string Host, int Port, bool Tls)
        {
            public override string ToString() => $"{(Tls ? "mqtts" : "mqtt")}://{Host}:{Port}";
        }

        public static BrokerAddress ParseServer(string? value)
        {
            var s = (value ?? "").Trim();
            var tls = false;
            if (s.StartsWith("mqtts://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("ssl://", StringComparison.OrdinalIgnoreCase))
                tls = true;
            var scheme = s.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0) s = s[(scheme + 3)..];
            s = s.TrimEnd('/');
            if (s.Length == 0 || !Uri.TryCreate("tcp://" + s, UriKind.Absolute, out var uri) || uri.PathAndQuery != "/")
                throw new InvalidOperationException("Endereço do broker inválido. Ex.: mqtt://192.168.0.10:1883");
            var port = uri.Port > 0 ? uri.Port : tls ? 8883 : 1883;
            return new BrokerAddress(uri.Host, port, tls);
        }

        public override async Task<IntegrationStatus> ConfigureAsync(Dictionary<string, string> fields)
        {
            var server = ParseServer(fields.GetValueOrDefault("server"));
            var username = (fields.GetValueOrDefault("username") ?? "").Trim();
            var password = fields.GetValueOrDefault("password") ?? "";
            Set(("server", server.ToString()), ("username", username),
                // Senha vazia mantem a guardada, a nao ser que o usuario tambem tenha sido apagado.
                ("password", password.Length > 0 ? password : username.Length > 0 ? Get("password") : null));

            Restart();
            for (var i = 0; i < 40 && !_connected && _error == null; i++) await Task.Delay(250);
            return GetStatus();
        }

        public override Task LogoutAsync()
        {
            Clear();
            Restart();
            return Task.CompletedTask;
        }

        private void Restart()
        {
            _error = null;
            _reconnect?.Cancel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(5);
            while (!stoppingToken.IsCancellationRequested)
            {
                _reconnect = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                if (Server == null)
                {
                    await Wait(TimeSpan.FromDays(1), _reconnect.Token);
                    continue;
                }
                try
                {
                    await RunAsync(_reconnect.Token);
                    delay = TimeSpan.FromSeconds(5);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    if (!_reconnect.IsCancellationRequested)
                    {
                        _error = ex is MQTTnet.Adapter.MqttConnectingFailedException { ResultCode: MqttClientConnectResultCode.BadUserNameOrPassword or MqttClientConnectResultCode.NotAuthorized }
                            ? "O broker MQTT recusou o usuário ou a senha."
                            : "Não foi possível conectar ao broker MQTT. Confira o endereço.";
                        Logger.LogInformation("MQTT: {Error}", ex.Message);
                    }
                }
                if (_connected)
                {
                    _connected = false;
                    await BroadcastAsync();
                }
                if (!_reconnect.IsCancellationRequested)
                {
                    await Wait(delay, _reconnect.Token);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            var server = ParseServer(Server);
            using var client = new MqttFactory().CreateMqttClient();
            var lost = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            client.DisconnectedAsync += _ =>
            {
                lost.TrySetResult();
                return Task.CompletedTask;
            };

            var builder = new MqttClientOptionsBuilder()
                .WithClientId("droiddeck-" + Environment.MachineName.ToLowerInvariant() + "-" + Guid.NewGuid().ToString("N")[..6])
                .WithTcpServer(server.Host, server.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithCleanSession();
            if (Get("username") is { } user) builder = builder.WithCredentials(user, Get("password") ?? "");
            if (server.Tls) builder = builder.WithTlsOptions(o => o.UseTls());

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            await client.ConnectAsync(builder.Build(), timeout.Token);
            _client = client;
            _connected = true;
            _error = null;
            Logger.LogInformation("MQTT: conectado a {Server}", server);
            await BroadcastAsync();

            try
            {
                await Task.WhenAny(lost.Task, Task.Delay(Timeout.Infinite, ct));
            }
            finally
            {
                _client = null;
                if (client.IsConnected)
                {
                    try { await client.DisconnectAsync(); } catch { }
                }
            }
        }

        /// <summary>Publica uma mensagem. Lanca se o broker nao estiver conectado.</summary>
        public async Task PublishAsync(string topic, string payload, int qos, bool retain)
        {
            var client = _client;
            if (!_connected || client == null || !client.IsConnected) throw new InvalidOperationException("MQTT desconectado.");
            var message = BuildMessage(topic, payload, qos, retain);
            var result = await client.PublishAsync(message);
            if (!result.IsSuccess) throw new InvalidOperationException(result.ReasonString ?? "O broker MQTT recusou a mensagem.");
        }

        public static MqttApplicationMessage BuildMessage(string topic, string payload, int qos, bool retain)
        {
            var t = topic.Trim();
            // Curingas so valem para assinar, nunca para publicar.
            if (t.Length == 0 || t.Contains('#') || t.Contains('+'))
                throw new InvalidOperationException("Tópico MQTT inválido.");
            return new MqttApplicationMessageBuilder()
                .WithTopic(t)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)Math.Clamp(qos, 0, 2))
                .WithRetainFlag(retain)
                .Build();
        }
    }
}
