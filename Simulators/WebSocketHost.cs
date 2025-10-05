using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GlobalShared;
using Simulators.Xfs4IoT.Network;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// Hosts an HttpListener and accepts incoming WebSocket connections.
    /// For each accepted socket it builds a BaseWebSocketConnection and calls the provided
    /// connection handler to wire it to a simulator.
    /// </summary>
    public class WebSocketHost
    {
        private readonly int _port;
        private readonly Func<BaseWebSocketConnection, Task> _onNewConnectionAsync;
        private readonly Utils _logger;
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">Port to listen on (http prefix used: http://localhost:port/)</param>
        /// <param name="onNewConnectionAsync">Callback invoked for each new connection</param>
        public WebSocketHost(int port, Func<BaseWebSocketConnection, Task> onNewConnectionAsync)
        {
            _port = port;
            _onNewConnectionAsync = onNewConnectionAsync ?? throw new ArgumentNullException(nameof(onNewConnectionAsync));
            _logger = new Utils($"WebSocketHost:{port}");
        }

        /// <summary>
        /// Start listening for connections. Non-blocking - starts accept loop in background.
        /// </summary>
        public Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();

            _logger.LogInfo($"WebSocketHost listening on port {_port}");

            // Run accept loop in background
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop listening and cancel accept loop.
        /// </summary>
        public Task StopAsync()
        {
            _cts?.Cancel();
            try { _listener?.Stop(); } catch { /* ignore */ }
            _logger.LogInfo("WebSocketHost stopped.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Accept loop: accepts connections, upgrades to WebSocket and hands to caller.
        /// </summary>
        private async Task AcceptLoopAsync(CancellationToken token)
        {
            if (_listener == null) return;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        continue;
                    }

                    var wsContext = await context.AcceptWebSocketAsync(null);
                    var systemSocket = wsContext.WebSocket;

                    var conn = new BaseWebSocketConnection(systemSocket, $"Conn:{_port}");
                    _logger.LogInfo("Accepted new WebSocket client");

                    // Kick off the receive loop and let caller wire events
                    _ = conn.StartReceiveLoopAsync(token);

                    // Notify the simulator (or whoever provided the callback)
                    await _onNewConnectionAsync(conn);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    // Listener was stopped - exit cleanly
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"AcceptLoop exception: {ex.Message}");
                    await Task.Delay(200); // small backoff
                }
            }
        }
    }
}
