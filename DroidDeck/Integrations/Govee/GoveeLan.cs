using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DroidDeck.Integrations
{
    /// <summary>Um aparelho Govee que respondeu a busca na rede.</summary>
    public record GoveeDevice(string Ip, string? Sku, string? Id);

    /// <summary>
    /// API LAN da Govee (UDP): a busca vai por multicast para 239.255.255.250:4001, os comandos
    /// para o IP do aparelho na porta 4003 e as respostas chegam na porta 4002 deste PC. Precisa
    /// do "Controle LAN" ligado no app Govee Home, em cada aparelho.
    /// </summary>
    public sealed class GoveeLan : IDisposable
    {
        public const int ScanPort = 4001, ReplyPort = 4002, CommandPort = 4003;
        public static readonly IPAddress MulticastGroup = IPAddress.Parse("239.255.255.250");

        private readonly ILogger<GoveeLan> _logger;
        private readonly object _lock = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonObject>> _status = new();
        private readonly ConcurrentDictionary<string, GoveeDevice> _found = new();
        private UdpClient? _listener;

        public GoveeLan(ILogger<GoveeLan> logger) => _logger = logger;

        // ---- Mensagens ----

        public static JsonObject Message(string cmd, JsonObject data) =>
            new() { ["msg"] = new JsonObject { ["cmd"] = cmd, ["data"] = data } };

        public static JsonObject Scan() => Message("scan", new JsonObject { ["account_topic"] = "reserve" });
        public static JsonObject Status() => Message("devStatus", new JsonObject());
        public static JsonObject Turn(bool on) => Message("turn", new JsonObject { ["value"] = on ? 1 : 0 });
        public static JsonObject Brightness(int percent) => Message("brightness", new JsonObject { ["value"] = Math.Clamp(percent, 1, 100) });

        /// <summary>Cor RGB (temperatura zerada) ou temperatura em Kelvin (cor zerada).</summary>
        public static JsonObject Color(int r, int g, int b, int kelvin = 0) => Message("colorwc", new JsonObject
        {
            ["color"] = new JsonObject { ["r"] = Math.Clamp(r, 0, 255), ["g"] = Math.Clamp(g, 0, 255), ["b"] = Math.Clamp(b, 0, 255) },
            ["colorTemInKelvin"] = kelvin == 0 ? 0 : Math.Clamp(kelvin, 2000, 9000),
        });

        /// <summary>"#FF8000" ou "ff8000" para (255, 128, 0).</summary>
        public static (int r, int g, int b) ParseHex(string hex)
        {
            var h = hex.Trim().TrimStart('#');
            if (h.Length == 3) h = string.Concat(h.Select(c => $"{c}{c}"));
            if (h.Length != 6 || !int.TryParse(h, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
                throw new InvalidOperationException("Cor inválida. Use o formato #RRGGBB.");
            return ((v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
        }

        /// <summary>Resposta da busca: {"msg":{"cmd":"scan","data":{"ip":..,"device":..,"sku":..}}}.</summary>
        public static GoveeDevice? ParseScan(JsonNode? node)
        {
            var msg = node?["msg"];
            if (msg?["cmd"]?.GetValue<string>() != "scan") return null;
            var data = msg["data"];
            var ip = data?["ip"]?.GetValue<string>();
            if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out _)) return null;
            return new GoveeDevice(ip, data?["sku"]?.GetValue<string>(), data?["device"]?.GetValue<string>());
        }

        /// <summary>Resposta do devStatus: aceso ou nao.</summary>
        public static bool? ParseOn(JsonNode? node)
        {
            var data = node?["msg"]?["cmd"]?.GetValue<string>() == "devStatus" ? node["msg"]?["data"] : null;
            return data?["onOff"] is JsonValue v && v.TryGetValue<int>(out var on) ? on == 1 : null;
        }

        /// <summary>Resposta do devStatus: brilho (1-100), ou null.</summary>
        public static int? ParseBrightness(JsonNode? node)
        {
            var data = node?["msg"]?["cmd"]?.GetValue<string>() == "devStatus" ? node["msg"]?["data"] : null;
            return data?["brightness"] is JsonValue v && v.TryGetValue<int>(out var level) ? level : null;
        }

        // ---- Rede ----

        /// <summary>IP do aparelho como o usuario digitou, conferido.</summary>
        public static IPAddress ParseIp(string ip) =>
            IPAddress.TryParse(ip.Trim(), out var address) && address.AddressFamily == AddressFamily.InterNetwork
                ? address
                : throw new InvalidOperationException("IP do aparelho Govee inválido.");

        public async Task SendAsync(string ip, JsonObject message)
        {
            var address = ParseIp(ip);
            var bytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            // Pela porta de respostas: o firewall do Windows deixa a resposta voltar.
            var listener = EnsureListener();
            await listener.SendAsync(bytes, bytes.Length, new IPEndPoint(address, CommandPort));
        }

        /// <summary>Brilho atual (1-100). Null = nao respondeu.</summary>
        public async Task<int?> BrightnessAsync(string ip, TimeSpan timeout) =>
            ParseBrightness(await StatusAsync(ip, timeout));

        /// <summary>Pergunta se o aparelho esta aceso. Null = nao respondeu.</summary>
        public async Task<bool?> IsOnAsync(string ip, TimeSpan timeout) =>
            ParseOn(await StatusAsync(ip, timeout));

        private async Task<JsonObject?> StatusAsync(string ip, TimeSpan timeout)
        {
            var key = ParseIp(ip).ToString();
            var tcs = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            _status[key] = tcs;
            try
            {
                await SendAsync(ip, Status());
                var done = await Task.WhenAny(tcs.Task, Task.Delay(timeout));
                return done == tcs.Task ? await tcs.Task : null;
            }
            finally
            {
                _status.TryRemove(key, out _);
            }
        }

        /// <summary>Busca os aparelhos com Controle LAN, em todas as placas de rede ligadas.</summary>
        public async Task<List<GoveeDevice>> ScanAsync(TimeSpan wait)
        {
            EnsureListener();
            _found.Clear();
            var bytes = Encoding.UTF8.GetBytes(Scan().ToJsonString());
            foreach (var local in LocalAddresses())
            {
                try
                {
                    using var sender = new UdpClient(new IPEndPoint(local, 0));
                    sender.JoinMulticastGroup(MulticastGroup, local);
                    await sender.SendAsync(bytes, bytes.Length, new IPEndPoint(MulticastGroup, ScanPort));
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Govee: busca por {Ip} falhou ({Error})", local, ex.Message);
                }
            }
            await Task.Delay(wait);
            return _found.Values.OrderBy(d => d.Ip, StringComparer.Ordinal).ToList();
        }

        private static IEnumerable<IPAddress> LocalAddresses() =>
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Select(a => a.Address)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                .Distinct();

        private UdpClient EnsureListener()
        {
            lock (_lock)
            {
                if (_listener != null) return _listener;
                var udp = new UdpClient(AddressFamily.InterNetwork);
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                try { udp.Client.Bind(new IPEndPoint(IPAddress.Any, ReplyPort)); }
                catch (SocketException)
                {
                    udp.Dispose();
                    throw new InvalidOperationException("A porta UDP 4002 está em uso por outro programa (precisa dela para falar com a Govee).");
                }
                _listener = udp;
                _ = Task.Run(() => ListenAsync(udp));
                return udp;
            }
        }

        private async Task ListenAsync(UdpClient udp)
        {
            while (true)
            {
                UdpReceiveResult r;
                try { r = await udp.ReceiveAsync(); }
                catch (ObjectDisposedException) { return; }
                catch (SocketException) { continue; }
                JsonNode? node;
                try { node = JsonNode.Parse(r.Buffer); }
                catch { continue; }

                if (ParseScan(node) is { } device) _found[device.Ip] = device;
                else if (node?["msg"]?["cmd"]?.GetValue<string>() == "devStatus" &&
                         _status.TryGetValue(r.RemoteEndPoint.Address.ToString(), out var tcs) && node is JsonObject obj)
                    tcs.TrySetResult(obj);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _listener?.Dispose();
                _listener = null;
            }
        }
    }
}
