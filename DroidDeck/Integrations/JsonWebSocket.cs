using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace DroidDeck.Integrations
{
    /// <summary>
    /// Conexao WebSocket que troca JSON com pedido e resposta casados por um id (Streamer.bot,
    /// VTube Studio). Quem usa le as primeiras mensagens com <see cref="ReceiveAsync"/> (aperto
    /// de mao, autenticacao) e depois deixa <see cref="RunAsync"/> distribuindo as respostas.
    /// </summary>
    public sealed class JsonWebSocket : IDisposable
    {
        private readonly ClientWebSocket _socket = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonNode>> _pending = new();
        private readonly Func<JsonNode, string?> _idOf;

        /// <param name="idOf">Le o id de uma resposta (null = mensagem sem pedido, ex.: evento).</param>
        public JsonWebSocket(Func<JsonNode, string?> idOf) => _idOf = idOf;

        public bool IsOpen => _socket.State == WebSocketState.Open;

        public Task ConnectAsync(Uri uri, CancellationToken ct) => _socket.ConnectAsync(uri, ct);

        public async Task SendAsync(JsonNode message, CancellationToken ct)
        {
            var bytes = Encoding.UTF8.GetBytes(message.ToJsonString());
            await _sendLock.WaitAsync(ct);
            try { await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct); }
            finally { _sendLock.Release(); }
        }

        /// <summary>Proxima mensagem (null = conexao fechada). So antes de <see cref="RunAsync"/>.</summary>
        public async Task<JsonNode?> ReceiveAsync(CancellationToken ct)
        {
            var buffer = new byte[16 * 1024];
            using var ms = new MemoryStream();
            while (true)
            {
                var r = await _socket.ReceiveAsync(buffer, ct);
                if (r.MessageType == WebSocketMessageType.Close) return null;
                ms.Write(buffer, 0, r.Count);
                if (r.EndOfMessage) break;
            }
            return JsonNode.Parse(ms.ToArray());
        }

        /// <summary>
        /// Manda o pedido e espera a resposta com o mesmo id. <paramref name="timeout"/> longo
        /// serve para pedidos que esperam o usuario (o VTube Studio pergunta se permite o plugin).
        /// </summary>
        public async Task<JsonNode> RequestAsync(string id, JsonNode message, TimeSpan timeout, CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[id] = tcs;
            try
            {
                await SendAsync(message, ct);
                var done = await Task.WhenAny(tcs.Task, Task.Delay(timeout, ct));
                if (done != tcs.Task)
                {
                    ct.ThrowIfCancellationRequested();
                    throw new TimeoutException("Sem resposta.");
                }
                return await tcs.Task;
            }
            finally
            {
                _pending.TryRemove(id, out _);
            }
        }

        /// <summary>Le ate a conexao cair, entregando as respostas aos pedidos pendentes.</summary>
        public async Task RunAsync(Action<JsonNode>? onOther, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsOpen)
                {
                    var msg = await ReceiveAsync(ct);
                    if (msg == null) break;
                    if (_idOf(msg) is { } id && _pending.TryRemove(id, out var tcs)) tcs.TrySetResult(msg);
                    else onOther?.Invoke(msg);
                }
            }
            finally
            {
                foreach (var p in _pending.Values) p.TrySetCanceled();
                _pending.Clear();
            }
        }

        public void Abort()
        {
            try { _socket.Abort(); } catch { }
        }

        public void Dispose()
        {
            Abort();
            _socket.Dispose();
            _sendLock.Dispose();
        }
    }
}
