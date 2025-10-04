using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simulators.WebSocket
{
    public class WebSocketServer
    {
        private readonly int _port;
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;

        public event Action<string>? OnMessageReceived;
        public event Action? OnClientConnected;
        public event Action? OnClientDisconnected;

        public WebSocketServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_port}/");
            _listener.Start();

            Task.Run(() => AcceptLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener?.IsListening == true)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();

                    if (ctx.Request.IsWebSocketRequest)
                    {
                        var wsContext = await ctx.AcceptWebSocketAsync(null);
                        OnClientConnected?.Invoke();
                        _ = HandleClient(wsContext.WebSocket, token);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                        ctx.Response.Close();
                    }
                }
                catch (Exception)
                {
                    if (!token.IsCancellationRequested)
                        throw;
                }
            }
        }

        private async Task HandleClient(System.Net.WebSockets.WebSocket socket, CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", token);
                        OnClientDisconnected?.Invoke();
                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMessageReceived?.Invoke(message);

                        // Echo back (placeholder, will later send proper XFS4IoT responses)
                        await Send(socket, $"{{\"ack\":\"{DateTime.UtcNow:o}\"}}", token);
                    }
                }
            }
            catch
            {
                OnClientDisconnected?.Invoke();
            }
        }

        public async Task Send(System.Net.WebSockets.WebSocket socket, string message, CancellationToken token)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
        }
    }
}
