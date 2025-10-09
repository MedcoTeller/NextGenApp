using GlobalShared;
using Simulators.Xfs4IoT;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;

namespace Simulators
{
    /// <summary>
    /// Provides a base implementation for a device simulator that manages HTTP and WebSocket connections, command
    /// handling, and device state for XFS4IoT services.
    /// </summary>
    /// <remarks>BaseSimulator is designed to be extended for specific device types, allowing derived classes
    /// to customize status and capability reporting by overriding relevant methods. It manages client connections,
    /// message broadcasting, and command dispatching, and exposes events for client and message activity. The class is
    /// not thread-safe for all operations; care should be taken when accessing shared state from derived classes.
    /// Dispose the simulator to ensure all resources are released and background tasks are stopped.</remarks>
    public class BaseSimulator : IDisposable
    {
        private readonly string _url;
        private HttpListener? _listener;
        private CancellationTokenSource? _workerCts;
        private readonly ConcurrentDictionary<string, Func<WebSocket, Xfs4Message, CancellationToken, Task>> _CommandHandlers = new();
        private readonly Channel<(Xfs4Message msg, WebSocket conn, CancellationToken tkn)> _queue;
        private Task? _workerTask;

        protected CancellationTokenSource? _cts;
        protected readonly Utils _logger;
        protected readonly List<WebSocket> _allClients = new();
        protected Xfs4Message CurrentCommand;
        protected TransactionStateEnum TransactionState = TransactionStateEnum.inactive;
        protected string TransactionId = string.Empty;
        protected string CommandNonce = string.Empty;
        protected string SecureOperation = string.Empty;
        protected string SecureOperationUniquueId = string.Empty;
        protected (bool, WebSocket) CancelRequested;
        protected (int[]?, WebSocket) CancelCoomandIds;

        public string ServiceName { get; protected set; }
        public int Port { get; protected set; }
        public string HostName { get; protected set; }
        public string DeviceName { get; protected set; }
        public string Url => _url;

        // Shared device state / properties
        public bool IsOnline { get; protected set; } = false;
        public DeviceStatusEnum? DeviceStatus { get; protected set; } = DeviceStatusEnum.noDevice;
        public DevicePositionStatusEnum? DevicePositionStatus { get; private set; } = null;// DevicePositionStatusEnum.notInPosition;
        public int PowerSaveRecoveryTime { get; private set; } = 10;
        public AntiFraudModuleStatusEnum? AntiFraudModuleStatus { get; private set; } = null;// AntiFraudModuleStatusEnum.ok;
        public ExchangeStatusEnum? ExchangeStatus { get; private set; } = null;// ExchangeStatusEnum.active;
        public EndToEndSecurityStatusEnum? EndToEndSecurityStatus { get; private set; } = null;
        public int RemainingCapacityStatus { get; private set; } = 0;
        public DateTime LastHeartbeat { get; protected set; } = DateTime.MinValue;

        // Capabilities: e.g. common capabilities
        protected Dictionary<string, object> CommonCapabilities { get; } = new Dictionary<string, object>();
        public string ServiceVersion { get; private set; } = "1.0.0";
        public string ModelName { get; private set; }
        public bool PowerSaveControlCp { get; private set; } = false;
        public bool HasAntiFraudModuleCp { get; private set; } = false;
        public RequiredEndToEndSecurityEnum? RequiredEndToEndSecurityCp { get; private set; } = RequiredEndToEndSecurityEnum.always;
        public bool HardwareSecurityElementCp { get; private set; } = false;
        public ResponseSecurityEnabledEnum? ResponseSecurityEnabledCp { get; private set; } = ResponseSecurityEnabledEnum.always;

        /// <summary>
        /// Gets or sets the timeout period, in seconds, for command nonces before they expire.
        /// If this device supports end-to-end security and can return a command nonce with the 
        /// command Common.GetCommandNonce, and the device automatically clears the command nonce 
        /// after a fixed length of time, this property will report the number of seconds between returning the command nonce and clearing it.
        /// The value is given in seconds but it should not be assumed that the timeout will be 
        /// accurate to the nearest second.The nonce may also become invalid before the timeout, for example because of a power failure.
        /// The device may impose a timeout to reduce the chance of an attacker re-using a nonce 
        /// value or a token.This timeout will be long enough to support normal operations such as 
        /// dispense and present including creating the required token on the host and passing it to 
        /// the device.For example, a command nonce might time out after one hour (that is, 3600 seconds).
        /// In all other cases, commandNonceTimeout will have a value of zero.Any command nonce will never 
        /// timeout.It may still become invalid, for example because of a power failure or when explicitly 
        /// cleared using the Common.ClearCommandNonce command.
        /// </summary>
        /// <remarks>A command nonce is used to prevent replay attacks or duplicate command execution.
        /// Adjust this value to control how long a nonce remains valid after issuance. Setting a lower value increases
        /// security but may require clients to complete operations more quickly.</remarks>
        public int CommandNonceTimeout { get; set; } = 3600;

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
            _queue = Channel.CreateUnbounded<(Xfs4Message msg, WebSocket conn, CancellationToken tkn)>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            DeviceName = deviceName;
            ServiceName = serviceName;
            ModelName = $"NextGen{DeviceName}SimulatorModel";
            HostName = new Uri(url).Host;
            RegisterCommonCommandHandlers();
            RegisterDeviceCommandHandlers();
        }

        /// <summary>
        /// Registers handlers for common command types supported by the system.
        /// </summary>
        /// <remarks>This method associates predefined command names with their corresponding handler
        /// methods. It should be called during initialization to ensure that all standard commands are properly
        /// handled. Calling this method multiple times may result in duplicate registrations, depending on the
        /// implementation of the underlying registration mechanism.</remarks>
        private void RegisterCommonCommandHandlers()
        {
            RegisterCommandHandler("Common.Status", HandleCommonStatusAsync);
            RegisterCommandHandler("Common.Capabilities", HandleCommonCapabilitiesAsync);
            RegisterCommandHandler("Common.GetTransactionState", HandleGetTransactionState);
            RegisterCommandHandler("Common.SetTransactionState", HandleSetTransactionState);
            RegisterCommandHandler("Common.GetCommandNonce", HandleGetCommandNonce);
            RegisterCommandHandler("Common.ClearCommandNonce", HandleClearCommandNonce);
            RegisterCommandHandler("Common.StartSecureOperation", HandleStartSecureOperation);
            RegisterCommandHandler("Common.Cancel", HandleCancel);
            RegisterCommandHandler("Common.GetInterfaceInfo", HandleGetInterfaceInfo);
            RegisterCommandHandler("Common.SetVersions", HandleSetVersions);
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
                Task.Run(() => AcceptLoop(_cts.Token));

                _workerTask = Task.Run(() => WorkerLoopAsync(_workerCts.Token));
                IsOnline = true;
                DeviceStatus = DeviceStatusEnum.online;
                LastHeartbeat = DateTime.UtcNow;
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

            IsOnline = false;
            DeviceStatus = DeviceStatusEnum.noDevice;
        }

        /// <summary>
        /// Registers a command handler for a specific command name.
        /// </summary>
        /// <param name="command">The command name to handle.</param>
        /// <param name="handler">The handler function to execute for the command.</param>
        public void RegisterCommandHandler(string command, Func<WebSocket, Xfs4Message, CancellationToken, Task> handler)
        {
            _CommandHandlers[command] = handler;
        }

        /// <summary>
        /// Sends the specified message to all connected clients whose WebSocket connection is open.
        /// </summary>
        /// <remarks>This method blocks until all messages have been sent. If a client is not connected or
        /// its WebSocket state is not open, the message will not be sent to that client.</remarks>
        /// <param name="message">The message to broadcast to all active clients. Cannot be null.</param>
        public void BroadcastMessage(Xfs4Message message)
        {
            var tasks = new List<Task>();
            foreach (var client in _allClients)
            {
                if (client.State == WebSocketState.Open)
                {
                    tasks.Add(SendAsync(client, message, CancellationToken.None));
                }
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
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
                        _allClients.Add(wsContext.WebSocket);
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

        public virtual void RegisterDeviceCommandHandlers()
        {

        }

        protected virtual void DeviceGetInterfaceInfo(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        protected virtual void DeviceSetVersions(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            throw new NotImplementedException();
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
                            var ack = new Xfs4Message(MessageType.Acknowledge, req.Header.Name, req.Header.RequestId);
                            await SendAsync(socket, ack, _cts.Token);
                            var cmdTkn = new CancellationTokenSource();
                            if (!_queue.Writer.TryWrite((req, socket, cmdTkn.Token)))
                                throw new Exception($"Unable to queue command {req}");
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
                    var (msg, conn, cmdTkn) = item;
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
                        CurrentCommand = msg;
                        var h = handler(conn, msg, cmdTkn);
                        //await Task.WhenAny(h, Task.Delay(-1, cmdTkn));
                        //cmdTkn.ThrowIfCancellationRequested();
                        _ = h;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"excwption threwn for {msg.Header.Name}: {ex.Message}");
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

        protected virtual object GetDeviceCommonStatusPart()
        {
            var common = new
            {
                device = DeviceStatus.ToString(),
                devicePosition = DevicePositionStatus?.ToString(),
                powerSaveRecoveryTime = PowerSaveRecoveryTime,
                antiFraudModule = AntiFraudModuleStatus?.ToString(),
                exchange = ExchangeStatus?.ToString(),
                endToEndSecurity = EndToEndSecurityStatus?.ToString(), // Enum value as string, case-sensitive, null if not supported
                persistentDataStore = new { remaining = RemainingCapacityStatus }
            };
            return common;
        }

        /// <summary>
        /// Handler for Common.Status command — returns shared (common) status plus device-specific status.
        /// Derived classes should override GetDeviceStatusPart to add their own part.
        /// </summary>
        protected virtual object GetDeviceStatusPart()
        {
            throw new NotImplementedException($"GetDeviceStatusPart not implemented for {DeviceName}");
        }

        /// <summary>
        /// Handler for Common.Capabilities command — returns shared capabilities + device-specific capabilities.
        /// Derived classes should override GetDeviceCapabilitiesPart to add their own.
        /// </summary>
        protected virtual object GetDeviceCapabilitiesPart()
        {
            throw new NotImplementedException($"GetDeviceCapabilitiesPart not implemented for {DeviceName}");
        }

        /// <summary>
        /// Array of commands which require an E2E token to authorize. These commands will fail if called without a valid token.
        ///The commands that can be listed here depend on the XFS4IoT standard, but it's possible that the standard will change 
        ///over time, so for maximum compatibility an application should check this property before calling a command.
        ///Note that this only includes commands that require a token.Commands that take a nonce and return a token will not be 
        ///listed here.Those commands can be called without a nonce and will continue to operate in a compatible way.
        /// </summary>
        protected virtual IEnumerable<string> SupportedEndToEndSecurityCommands => Array.Empty<string>();

        private async Task HandleCommonStatusAsync(WebSocket socket, Xfs4Message msg, CancellationToken cmdtkn)
        {
            var common = GetDeviceCommonStatusPart();

            var devicePart = GetDeviceStatusPart();

            var payload = new Dictionary<string, object>
            {
                ["common"] = common
            };
            if (devicePart != null)
                payload[DeviceName.ToLower()] = devicePart;

            var resp = new Xfs4Message(MessageType.Completion, "Common.Status", msg.Header.RequestId, payload, status: "success");
            await SendAsync(socket, resp, _cts!.Token);
        }


        private async Task HandleCommonCapabilitiesAsync(WebSocket socket, Xfs4Message msg, CancellationToken cmdtkn)
        {
            var common = new
            {
                serviceVersion = ServiceVersion,
                deviceInformation = new[] {
                    new {
                        modelName = ModelName,
                        serialNumber = "1.0.10.25",
                        revisionNumber = "1.0",
                        modelDescription = "NextGen Simulator",
                        firmware = new[] {
                            new {
                                firmwareName = "NextGen Simulator Firmware",
                                firmwareVersion = "1.0.0",
                                hardwareRevision = "1.0"
                            }
                        },
                        software = new[] {
                            new {
                                softwareName = DeviceName + "SW",
                                softwareVersion = "1.0.0"
                            }
                        }
                    }
                },
                powerSaveControl = PowerSaveControlCp,
                antiFraudModule = HasAntiFraudModuleCp,
                endToEndSecurity = new
                {
                    required = RequiredEndToEndSecurityCp?.ToString(),
                    hardwareSecurityElement = HardwareSecurityElementCp,
                    responseSecurityEnabled = ResponseSecurityEnabledCp?.ToString(),
                    commands = SupportedEndToEndSecurityCommands.ToArray(),
                    commandNonceTimeout = CommandNonceTimeout
                },
                persistentDataStore = new { capacity = 0 }
            };

            var devicePart = GetDeviceCapabilitiesPart();

            var payload = new Dictionary<string, object>
            {
                ["common"] = common
            };
            if (devicePart != null)
                payload[DeviceName.ToLower()] = devicePart;

            var resp = new Xfs4Message(MessageType.Completion, "Common.Capabilities", msg.Header.RequestId, payload, status: "success");
            await SendAsync(socket, resp, _cts!.Token);
        }

        private async Task HandleGetTransactionState(WebSocket socket, Xfs4Message message, CancellationToken cmdtkn)
        {
            try
            {
                TransactionState = message.Payload?.GetType().GetProperty("state") != null ?
                    Enum.Parse<TransactionStateEnum>(message.Payload?.GetType().GetProperty("state")!.GetValue(message.Payload)?.ToString()!) :
                    TransactionStateEnum.inactive;
                TransactionId = message.Payload?.GetType().GetProperty("transactionId") != null ?
                    message.Payload?.GetType().GetProperty("transactionId")!.GetValue(message.Payload)?.ToString()! :
                    string.Empty;
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleSetTransactionState(WebSocket socket, Xfs4Message message, CancellationToken cmdtkn)
        {
            try
            {
                var completion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = message.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = message.Header.RequestId
                    },
                    Payload = new
                    {
                        state = TransactionState,
                        transactionId = TransactionId
                    }
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleClearCommandNonce(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            try
            {
                CommandNonce = string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleGetCommandNonce(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            try
            {
                CommandNonce = Guid.NewGuid().ToString("N").ToUpperInvariant();
                var completion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = message.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = message.Header.RequestId
                    },
                    Payload = new
                    {
                        commandNonce = CommandNonce
                    }
                };
                await SendAsync(socket, completion, token);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task HandleStartSecureOperation(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            try
            {
                SecureOperationUniquueId = message.Payload?.GetType().GetProperty("uniqueId") != null ?
                    message.Payload?.GetType().GetProperty("uniqueId")!.GetValue(message.Payload)?.ToString()! :
                    string.Empty;
                SecureOperation = message.Payload?.GetType().GetProperty("operation") != null ?
                    message.Payload?.GetType().GetProperty("operation")!.GetValue(message.Payload)?.ToString()! :
                    string.Empty;

                var completion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = message.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = message.Header.RequestId
                    },
                    Payload = new { }
                };
                await SendAsync(socket, completion, token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleSetVersions(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            DeviceSetVersions(socket, message, token);
        }

        private async Task HandleGetInterfaceInfo(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            DeviceGetInterfaceInfo(socket, message, token);
        }

        private async Task HandleCancel(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            CancelRequested = (true, socket);
            CancelCoomandIds = (message.GetPayloadValue<int[]>("requestIds"), socket);

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
