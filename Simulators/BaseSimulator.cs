using GlobalShared;
using Simulators.Xfs4IoT;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Simulators
{
    public class BaseSimulator: IDisposable
    {
        private readonly string _url;
        private HttpListener? _listener;
        private CancellationTokenSource? _workerCts;
        private readonly ConcurrentDictionary<string, Func<WebSocket,Xfs4Message, Task>> _CommandHandlers = new();
        private readonly Channel<(Xfs4Message msg, WebSocket conn)> _queue;
        private Task? _workerTask;

        protected CancellationTokenSource? _cts;
        protected readonly Utils _logger;

        /// <summary>
        /// Event triggered when a message is received from a client.
        /// </summary>
        public event Action<Xfs4Message>? OnMessageReceived;

        /// <summary>
        /// Event triggered when a client connects.
        /// </summary>
        public event Action? OnClientConnected;

        /// <summary>
        /// Event triggered when a client disconnects.
        /// </summary>
        public event Action? OnClientDisconnected;
        public string ServiceName { get; protected set; }
        public int Port { get; protected set; }
        public string HostName { get; protected set; }
        public string DeviceName  { get; protected set; }
        public string Url => _url;

        /// <summary>
        /// Constructs a new BaseSimulator with the specified URL and device name.
        /// </summary>
        /// <param name="url">The base URL for the simulator.</param>
        /// <param name="deviceName">The name of the device being simulated.</param>
        public BaseSimulator(string url, string deviceName)
        {
            _url = url;
            _logger = new Utils($"{ServiceName}");
            _queue = Channel.CreateUnbounded<(Xfs4Message msg, WebSocket conn)>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            DeviceName = deviceName;
        }

        /// <summary>
        /// Constructs a new BaseSimulator with the specified URL, device name, and service name.
        /// </summary>
        /// <param name="url">The base URL for the simulator.</param>
        /// <param name="deviceName">The name of the device being simulated.</param>
        /// <param name="serviceName">The name of the service being simulated.</param>
        public BaseSimulator(string url, string deviceName, string serviceName)
        {
            _url = url;
            _logger = new Utils($"{ServiceName}");
            _queue = Channel.CreateUnbounded<(Xfs4Message msg, WebSocket conn)>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            DeviceName = deviceName;
            ServiceName = serviceName;
        }

        /// <summary>
        /// Starts the simulator, initializing the HTTP listener and worker tasks.
        /// </summary>
        /// <returns>True if started successfully, false otherwise.</returns>
        public bool Start()
        {
            try
            {
                _cts = new CancellationTokenSource();
                _workerCts = new CancellationTokenSource();
                _listener = new HttpListener();
                string prefix = $"{_url.TrimEnd('/')}/xfs4iot/v1.0/{ServiceName.ToLowerInvariant()}/";
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _logger.LogInfo($"Listening on {prefix}");
                ``
                Task.Run(() => AcceptLoop(_cts.Token));

                _workerTask = Task.Run(() => WorkerLoopAsync(_workerCts.Token));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while starting {DeviceName}: {ex.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stops the simulator asynchronously, cancelling worker and listener tasks.
        /// </summary>
        public async Task StopAsync()
        {
            _workerCts?.Cancel();
            _cts?.Cancel();
            if (_workerTask != null) await _workerTask;
        }

        /// <summary>
        /// Registers a command handler for a specific command name.
        /// </summary>
        /// <param name="command">The command name to handle.</param>
        /// <param name="handler">The handler function to execute for the command.</param>
        public void RegisterCommandHandler(string command, Func<WebSocket, Xfs4Message, Task> handler)
        {
            _CommandHandlers[command] = handler;
        }

        /// <summary>
        /// Accepts incoming HTTP connections and upgrades them to WebSocket if requested.
        /// </summary>
        /// <param name="token">Cancellation token to stop the loop.</param>
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
                        ClientConnected();
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

        /// <summary>
        /// Called when a client connects. Can be overridden in derived classes for custom behavior.
        /// </summary>
        public virtual void ClientConnected()
        {

        }

        /// <summary>
        /// Called when a message is received from a client. Can be overridden for custom processing.
        /// </summary>
        /// <param name="req">The received Xfs4Message.</param>
        /// <param name="socket">The WebSocket connection.</param>
        public virtual void MessageReceived(Xfs4Message req, WebSocket socket)
        {

        }

        /// <summary>
        /// Handles communication with a connected WebSocket client, receiving and processing messages.
        /// </summary>
        /// <param name="socket">The WebSocket client connection.</param>
        /// <param name="token">Cancellation token to stop the loop.</param>
        private async Task HandleClient(WebSocket socket, CancellationToken token)
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
                        try
                        {
                            var req = JsonSerializer.Deserialize<Xfs4Message>(message);
                            if (req == null)
                                throw new Exception("Deserialized message is null.");
                            OnMessageReceived?.Invoke(req);
                            MessageReceived(req, socket);
                            _queue.Writer.TryWrite((req, socket));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Invalid message received: {ex.Message}");
                        }
                    }
                }
            }
            catch
            {
                OnClientDisconnected?.Invoke();
            }
        }

        /// <summary>
        /// Worker loop that processes messages from the queue and dispatches them to registered command handlers.
        /// </summary>
        /// <param name="token">Cancellation token to stop the loop.</param>
        private async Task WorkerLoopAsync(CancellationToken token)
        {
            //Logger.LogInfo("Worker started.");
            var reader = _queue.Reader;

            while (await reader.WaitToReadAsync(token))
            {
                while (reader.TryRead(out var item))
                {
                    var (msg, conn) = item;
                    try
                    {
                        if (!_CommandHandlers.TryGetValue(msg.Header.Name, out var handler))
                        {
                            _logger.LogError($"No handler found for {msg.Header.Name} - {ServiceName}");
                            var err = new Xfs4Message(MessageType.Completion, msg.Header.Name, msg.Header.RequestId,
                                payload: new { error = "Unsupported command" }, status: "invalidCommand");
                            await SendAsync(conn, err, token);
                            continue;
                        }
                        await handler(conn, msg);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Handler threw for {msg.Header.Name}: {ex.Message}");
                        // ensure an error completion is sent
                        var err = new Xfs4Message(MessageType.Completion, msg.Header.Name, msg.Header.RequestId, payload: new { error = ex.Message }, status: "internalError");
                        await SendAsync(conn, err, token);
                    }
                    finally
                    {

                    }
                }
            }
            _logger.LogInfo($"{ServiceName}-Worker stopped.");
        }

        /// <summary>
        /// Sends a message to the specified WebSocket client asynchronously.
        /// </summary>
        /// <param name="socket">The WebSocket client connection.</param>
        /// <param name="message">The Xfs4Message to send.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        public async Task SendAsync(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            var bytes = Encoding.UTF8.GetBytes(message.ToJson());
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
        }


        /// <summary>
        /// Disposes the simulator, stopping all tasks and closing the HTTP listener.
        /// </summary>
        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            _listener?.Close();
        }
    }
}
