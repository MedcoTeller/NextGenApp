using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalShared;

namespace Simulators.Xfs4IoT.Network
{
    /// <summary>
    /// Lightweight wrapper around System.Net.WebSockets.WebSocket for a single client connection.
    /// Exposes events for incoming text messages and a SendAsync helper.
    /// </summary>
    public class BaseWebSocketConnection
    {
        private readonly System.Net.WebSockets.WebSocket _socket;
        private readonly Utils _logger;
        private readonly byte[] _buffer = new byte[8192];

        /// <summary>
        /// Fired when a complete text message is received.
        /// </summary>
        public event Func<string, Task>? TextMessageReceived;

        /// <summary>
        /// Fired when the connection is closed or an unrecoverable error occurs.
        /// </summary>
        public event Action? Disconnected;

        public BaseWebSocketConnection(System.Net.WebSockets.WebSocket socket, string loggerName)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _logger = new Utils(loggerName);
        }

        /// <summary>
        /// Start an async loop to receive messages and raise TextMessageReceived events.
        /// Returns a task that completes when the receive loop ends.
        /// </summary>
        public async Task StartReceiveLoopAsync(CancellationToken token = default)
        {
            try
            {
                while (_socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInfo("Client requested close. Closing socket.");
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", token);
                        Disconnected?.Invoke();
                        break;
                    }

                    // Only handle text payloads for XFS4IoT
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msg = Encoding.UTF8.GetString(_buffer, 0, result.Count);
                        _logger.LogDebug($"Received: {msg}");
                        if (TextMessageReceived != null)
                            await TextMessageReceived.Invoke(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Receive loop error: {ex.Message}");
                Disconnected?.Invoke();
            }
        }

        /// <summary>
        /// Send a text message to the client (serializes pre-built JSON).
        /// </summary>
        public async Task SendAsync(string json, CancellationToken token = default)
        {
            if (_socket.State != WebSocketState.Open) return;

            var bytes = Encoding.UTF8.GetBytes(json);
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
            _logger.LogDebug($"Sent: {json}");
        }

        /// <summary>
        /// Close the socket gracefully.
        /// </summary>
        public async Task CloseAsync(string reason = "by server")
        {
            if (_socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
                Disconnected?.Invoke();
            }
        }
    }
}
