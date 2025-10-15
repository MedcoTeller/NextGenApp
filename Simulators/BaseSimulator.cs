using GlobalShared;
using Simulators.Xfs4IoT;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WatsonWebsocket;

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
        private WatsonWsServer? _wsServer;
        private CancellationTokenSource? _workerCts;
        private readonly ConcurrentDictionary<string, Func<Guid, Xfs4Message, CancellationToken, Task>> _CommandHandlers = new();
        // bounded channel to provide backpressure and avoid unbounded memory growth
        private readonly Channel<(Xfs4Message msg, Guid clientId, CancellationToken tkn)> _queue;
        private Task? _workerTask;
        // per-client active command cancellation tokens (so we can cancel running commands for a client if client disconnects)
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeCommandTokens = new();
        // limit concurrent outbound send concurrency and avoid overwhelming Watson threads
        private readonly SemaphoreSlim _sendSemaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);

        protected readonly string ConfigFolder = @"C:\ProgramData\NextGen\Simulators\Device Configurations";
        protected CancellationTokenSource? _cts;
        protected readonly Utils _logger;
        protected TransactionStateEnum TransactionState = TransactionStateEnum.inactive;
        protected string TransactionId = string.Empty;
        protected string CommandNonce = string.Empty;
        protected string SecureOperation = string.Empty;
        protected string SecureOperationUniquueId = string.Empty;
        protected (bool, Guid) CancelRequested;
        protected (int[]?, Guid) CancelCoomandIds;
        private int? lastRequestId;

        public string ServiceName { get; protected set; }
        public int Port { get; protected set; }
        public string HostName { get; protected set; }
        public string DeviceName { get; protected set; }
        public string Url => _url;

        // Shared device state / properties
        public bool IsOnline { get; protected set; } = false;
        public DeviceStatusEnum? DeviceStatus { get; protected set; } = DeviceStatusEnum.noDevice;
        public DevicePositionStatusEnum? DevicePositionStatus { get; private set; } = null;
        public int PowerSaveRecoveryTime { get; private set; } = 10;
        public AntiFraudModuleStatusEnum? AntiFraudModuleStatus { get; private set; } = null;
        public ExchangeStatusEnum? ExchangeStatus { get; private set; } = null;
        public EndToEndSecurityStatusEnum? EndToEndSecurityStatus { get; private set; } = null;
        public int RemainingCapacityStatus { get; private set; } = 0;
        public DateTime LastHeartbeat { get; protected set; } = DateTime.MinValue;

        // Capabilities
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
        /// </summary>
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
        /// Event triggered when a device status has changed.
        /// </summary>
        public Action<string>? OnStatusChange;

        /// <summary>
        /// Constructs a new BaseSimulator with the specified URL, device name, and service name.
        /// </summary>
        /// <param name="url">The base URL for the simulator.</param>
        /// <param name="deviceName">The name of the device being simulated.</param>
        /// <param name="serviceName">The name of the service being simulated.</param>
        public BaseSimulator(string url, string deviceName, string serviceName, bool secureConnextion = false)
        {
            _url = $"{url}/xfs4iot/v1.0/{serviceName}/";
            _logger = new Utils($"{serviceName}"); // use provided service name for logger
            // create a bounded channel with some reasonable capacity to provide backpressure under burst conditions.
            // tune capacity as needed (1000 is a typical starting point for device simulators).
            var options = new BoundedChannelOptions(1000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(Xfs4Message msg, Guid clientId, CancellationToken tkn)>(options);
            DeviceName = deviceName;
            ServiceName = serviceName;
            ModelName = $"NextGen{DeviceName}SimulatorModel";
            HostName = new Uri(url).Host;

            // parse host & port from the URL
            var uri = new Uri(_url);
            Port = uri.IsDefaultPort ? 80 : uri.Port; ;

            // Initialize Watson server
            _wsServer = new WatsonWsServer(uri.Host, Port, secureConnextion);

            // attach handlers
            _wsServer.ClientConnected += WsServer_ClientConnected;
            _wsServer.ClientDisconnected += WsServer_ClientDisconnected;
            _wsServer.MessageReceived += WsServer_MessageReceived;

            RegisterCommonCommandHandlers();
            RegisterDeviceCommandHandlers();
            _= RefreshConfig();
        }

        /// <summary>
        /// Registers handlers for common command types supported by the system.
        /// </summary>
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
        /// Starts the simulator, initializing the Watson WebSocket server and worker tasks.
        /// </summary>
        public bool Start()
        {
            try
            {
                _cts = new CancellationTokenSource();
                _workerCts = new CancellationTokenSource();

                // start server
                _wsServer?.Start();

                _logger.LogInfo($"Service {ServiceName} listening on: {_url}");

                // start worker loop that will process queued messages
                _workerTask = Task.Run(() => WorkerLoopAsync(_workerCts.Token), CancellationToken.None);

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
        /// Stops the simulator asynchronously, cancelling worker and server.
        /// </summary>
        public async Task StopAsync()
        {
            // signal cancellation to worker and any in-flight command tokens
            try
            {
                _workerCts?.Cancel();
                _cts?.Cancel();

                // cancel all per-client active command tokens
                foreach (var kv in _activeCommandTokens)
                {
                    try { kv.Value.Cancel(); } catch { /* swallow */ }
                }

                // detach events before stopping to avoid callbacks during shutdown
                if (_wsServer != null)
                {
                    try
                    {
                        _wsServer.ClientConnected -= WsServer_ClientConnected;
                        _wsServer.ClientDisconnected -= WsServer_ClientDisconnected;
                        _wsServer.MessageReceived -= WsServer_MessageReceived;
                    }
                    catch { /* ignore */ }
                }

                // stop web socket server
                try
                {
                    _wsServer?.Stop();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while stopping Watson server: {ex.Message}");
                }

                // wait for worker task to finish up to a short timeout
                if (_workerTask != null)
                {
                    var finished = await Task.WhenAny(_workerTask, Task.Delay(TimeSpan.FromSeconds(5)));
                    if (finished != _workerTask)
                    {
                        _logger.LogInfo("Worker did not stop within timeout; continuing shutdown.");
                    }
                }
            }
            finally
            {
                IsOnline = false;
                DeviceStatus = DeviceStatusEnum.noDevice;
                _wsServer = null;
            }
        }

        /// <summary>
        /// Registers a command handler for a specific command name.
        /// </summary>
        public void RegisterCommandHandler(string command, Func<Guid, Xfs4Message, CancellationToken, Task> handler)
        {
            _CommandHandlers[command] = handler;
        }


        protected async Task StatusChanged()
        {
            var common = new
            {
                device = DeviceStatus.ToString(),
                devicePosition = DevicePositionStatus?.ToString(),
                powerSaveRecoveryTime = PowerSaveRecoveryTime,
                antiFraudModule = AntiFraudModuleStatus?.ToString(),
                exchange = ExchangeStatus?.ToString(),
                endToEndSecurity = EndToEndSecurityStatus?.ToString(),
                persistentDataStore = new { remaining = RemainingCapacityStatus }
            };

            var payload = new Dictionary<string, object>
            {
                ["common"] = common
            };

            var (devicePart, name) = GetDeviceStatusPart();
            if (devicePart != null)
                payload[name] = devicePart;

            var resp = new Xfs4Message(MessageType.Event, "Common.Capabilities", 0, payload);
            await BroadcastMessageAsync(resp, CancellationToken.None);
        }

        /// <summary>
        /// List of all supported command by the service.
        /// </summary>
        /// <returns></returns>
        public List<string> SupportedCommands()
        {
            return _CommandHandlers.Keys.ToList();
        }

        /// <summary>
        /// Sends the specified message to all connected clients whose connection is active.
        /// </summary>
        /// <remarks>
        /// This method preserves the original synchronous signature. It will attempt to send asynchronously to all
        /// clients and will block until those sends complete. If you prefer non-blocking behavior, use
        /// BroadcastMessageAsync instead.
        /// </remarks>
        public async Task BroadcastMessage(Xfs4Message message)
        {
            // Keep method signature for compatibility but delegate to async helper and wait with timeout.
            // Blocking here is intentional to preserve original behaviour; consider calling BroadcastMessageAsync in new code.
            try
            {
                var t = BroadcastMessageAsync(message, CancellationToken.None);
                t.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"BroadcastMessage error: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously broadcast a message to all connected clients.
        /// </summary>
        public async Task BroadcastMessageAsync(Xfs4Message message, CancellationToken token)
        {
            if (_wsServer == null) return;

            List<Task> tasks = new List<Task>();
            foreach (var client in _wsServer.ListClients())
            {
                tasks.Add(SafeSendToClientAsync(client.Guid, message, token));
            }

            // run sends concurrently but observe cancellation
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"BroadcastMessageAsync: some sends failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Watson ClientConnected event handler.
        /// </summary>
        private void WsServer_ClientConnected(object? sender, ConnectionEventArgs args)
        {
            try
            {
                OnClientConnected?.Invoke();
                ClientConnected();
                _logger.LogInfo($"Client connected: {args.Client}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ClientConnected handler error: {ex.Message}");
            }
        }

        /// <summary>
        /// Watson ClientDisconnected event handler.
        /// </summary>
        private void WsServer_ClientDisconnected(object? sender, DisconnectionEventArgs args)
        {
            try
            {
                if (_activeCommandTokens.TryRemove(args.Client.Guid, out var cts))
                {
                    try { cts.Cancel(); } catch { /* ignore */ }
                    try { cts.Dispose(); } catch { /* ignore */ }
                }

                OnClientDisconnected?.Invoke();
                _logger.LogInfo($"Client disconnected: {args.Client}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ClientDisconnected handler error: {ex.Message}");
            }
        }

        /// <summary>
        /// Watson MessageReceived event handler.
        /// </summary>
        private void WsServer_MessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            // MessageReceived is called by Watson thread; keep handler fast and non-blocking.
            _ = Task.Run(async () =>
            {
                try
                {

                    // decode message bytes -> string
                    string message;
                    try
                    {
                        message = Encoding.UTF8.GetString(args.Data);
                        _logger.LogInfo($"Message received: \n{message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to decode message from {args.Client}: {ex.Message}");
                        return;
                    }

                    // Try to deserialize safely. If message is malformed, respond with a completion error
                    Xfs4Message? req = null;
                    try
                    {
                        req = JsonSerializer.Deserialize<Xfs4Message>(message, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            AllowTrailingCommas = true
                        });
                    }
                    catch (JsonException jex)
                    {
                        _logger.LogError($"JSON parse error from {args.Client}: {jex.Message}");
                        // send a completion with invalidRequest error shape (best-effort)
                        var errMsg = new Xfs4Message(MessageType.Acknowledge, req?.Header?.Name, req?.Header?.RequestId, payload: new { error = "Invalid JSON" }, status: CommandStatusEnum.invalidMessage);
                        await SafeSendToClientAsync(args.Client.Guid, errMsg, CancellationToken.None).ConfigureAwait(false);
                        return;
                    }

                    if (req == null)
                    {
                        _logger.LogError($"Deserialized message is null from {args.Client}.");
                        return;
                    }

                    try
                    {
                        // invoke public event and virtual hook
                        OnMessageReceived?.Invoke(req);

                        // map legacy MessageReceived(req, WebSocket) signature to the new signature using clientId
                        MessageReceived(req, args.Client.Guid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"MessageReceived hook threw: {ex.Message}");
                    }

                    if (req?.Header.RequestId <= lastRequestId)
                    {
                        var errMsg = new Xfs4Message(MessageType.Acknowledge, req?.Header?.Name, req?.Header?.RequestId, payload: new { error = "Invalid JSON" }, status: CommandStatusEnum.invalidRequestID);
                        await SafeSendToClientAsync(args.Client.Guid, errMsg, CancellationToken.None).ConfigureAwait(false);
                        return;
                    }
                    else
                        lastRequestId = req?.Header.RequestId;

                    // send ack (best-effort)
                    try
                    {
                        var ack = new Xfs4Message(MessageType.Acknowledge, req.Header.Name, req.Header.RequestId);
                        await SafeSendToClientAsync(args.Client.Guid, ack, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to send ack to {args.Client}: {ex.Message}");
                        return;
                    }

                    // prepare a cancellation token source that is tied to the client's lifetime
                    var perCmdCts = new CancellationTokenSource();
                    _activeCommandTokens.AddOrUpdate(args.Client.Guid, perCmdCts, (_, old) =>
                    {
                        try { old.Cancel(); old.Dispose(); } catch { }
                        return perCmdCts;
                    });

                    // queue the command (waits if the bounded channel is full)
                    var writeOk = await _queue.Writer.WaitToWriteAsync().ConfigureAwait(false);
                    if (!writeOk)
                    {
                        _logger.LogError($"Queue is closed/unwritable; dropping message from {args.Client}.");
                        return;
                    }

                    await _queue.Writer.WriteAsync((req, args.Client.Guid, perCmdCts.Token));
                    // note: WriteAsync completes once the item is accepted without blocking the Watson thread beyond this point.
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Invalid message received: {ex.Message}");
                }
            }).ConfigureAwait(false);
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
        public virtual void MessageReceived(Xfs4Message req, Guid clientId)
        {
        }

        public virtual void RegisterDeviceCommandHandlers()
        {
        }

        public async virtual Task RefreshConfig()
        {            
        }

        protected virtual void DeviceGetInterfaceInfo(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected virtual void DeviceSetVersions(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Worker loop that processes messages from the queue and dispatches them to registered command handlers.
        /// </summary>
        private async Task WorkerLoopAsync(CancellationToken token)
        {
            var reader = _queue.Reader;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Wait for an item to be available (observes cancellation)
                    if (!await reader.WaitToReadAsync(token).ConfigureAwait(false))
                    {
                        break;
                    }

                    while (reader.TryRead(out var item))
                    {
                        var (msg, clientId, cmdTkn) = item;

                        try
                        {
                            if (!_CommandHandlers.TryGetValue(msg.Header.Name, out var handler))
                            {
                                _logger.LogError($"No handler found for {msg.Header.Name} - {ServiceName}");
                                var err = new Xfs4Message(MessageType.Completion, msg.Header.Name, msg.Header.RequestId,
                                    payload: new { error = "Unsupported command" }, status: CommandStatusEnum.invalidMessage);
                                await SafeSendToClientAsync(clientId, err, token).ConfigureAwait(false);
                                continue;
                            }

                            // call handler and observe its cancellation token
                            // capture task to observe exceptions separately so worker loop continues
                            var handlerTask = Task.Run(async () =>
                            {
                                try
                                {
                                    await handler(clientId, msg, cmdTkn).ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                    _logger.LogInfo($"Handler for {msg.Header.Name} was canceled for client {clientId}");

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Exception thrown in handler for {msg.Header.Name}: {ex.Message}");
                                    var err = new Xfs4Message(MessageType.Completion, msg.Header.Name, msg.Header.RequestId,
                                        payload: new { error = ex.Message }, status: CommandStatusEnum.invalidMessage);
                                    try { await SafeSendToClientAsync(clientId, err, CancellationToken.None).ConfigureAwait(false); } catch { }
                                }
                                finally
                                {
                                    // remove per-client active token if it matches this handler's token
                                    _activeCommandTokens.TryRemove(clientId, out _);
                                }
                            });

                            // don't await here to allow concurrent handling of commands where appropriate.
                            // but observe exceptions by continuing to next loop; we log/catch inside handlerTask.
                            _ = handlerTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"exception thrown for {msg.Header.Name}: {ex.Message}");
                            var err = new Xfs4Message(MessageType.Completion, msg.Header.Name, msg.Header.RequestId,
                                payload: new { error = ex.Message }, status: CommandStatusEnum.invalidMessage);
                            try { await SafeSendToClientAsync(clientId, err, CancellationToken.None).ConfigureAwait(false); } catch { }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"WorkerLoopAsync error: {ex.Message}");
                    // short delay to avoid hot-looping on repeated errors
                    await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
                }
            }

            _logger.LogInfo($"{ServiceName}-Worker stopped.");
        }

        /// <summary>
        /// Sends a message to the specified client asynchronously.
        /// </summary>
        public async Task SendAsync(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            // public send wrapper that uses SafeSendToClientAsync internally
            await SafeSendToClientAsync(clientId, message, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Utility that attempts to send to a client safely and will remove a client if it is no longer reachable.
        /// </summary>
        private async Task SafeSendToClientAsync(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            if (_wsServer == null) throw new InvalidOperationException($"{ServiceName} Server not started.");

            // throttle concurrent sends
            await _sendSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var json = message.ToJson();
                _logger.LogInfo($"Sending: {json}");

                try
                {
                    await _wsServer.SendAsync(clientId, json).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{ServiceName} Send to client {clientId} failed: {ex.Message}. Removing client and cancelling operations.");
                    // best-effort cleanup for a dead/unresponsive client

                    //lock (_allClients) { _allClients.RemoveAll(g => g == clientId); }
                    if (_activeCommandTokens.TryRemove(clientId, out var cts))
                    {
                        try { cts.Cancel(); cts.Dispose(); } catch { }
                    }
                }
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }

        protected virtual (object, string) GetDeviceStatusPart()
        {
            throw new NotImplementedException($"GetDeviceStatusPart not implemented for {DeviceName}");
        }

        protected virtual (object, string) GetDeviceCapabilitiesPart()
        {
            throw new NotImplementedException($"GetDeviceCapabilitiesPart not implemented for {DeviceName}");
        }

        protected virtual IEnumerable<string> SupportedEndToEndSecurityCommands => Array.Empty<string>();

        protected virtual object GetDeviceCommonStatusPart()
        {
            var common = new
            {
                device = DeviceStatus.ToString(),
                devicePosition = DevicePositionStatus?.ToString(),
                powerSaveRecoveryTime = PowerSaveRecoveryTime,
                antiFraudModule = AntiFraudModuleStatus?.ToString(),
                exchange = ExchangeStatus?.ToString(),
                endToEndSecurity = EndToEndSecurityStatus?.ToString(),
                persistentDataStore = new { remaining = RemainingCapacityStatus }
            };
            return common;
        }

        private async Task HandleCommonStatusAsync(Guid clientId, Xfs4Message msg, CancellationToken cmdtkn)
        {
            var common = GetDeviceCommonStatusPart();
            var (devicePart, name) = GetDeviceStatusPart();

            var payload = new Dictionary<string, object>
            {
                ["common"] = common
            };
            if (devicePart != null)
                payload[name] = devicePart;

            var resp = new Xfs4Message(MessageType.Completion, "Common.Status", msg.Header.RequestId, payload);
            await SafeSendToClientAsync(clientId, resp, _cts!.Token).ConfigureAwait(false);
        }

        private async Task HandleCommonCapabilitiesAsync(Guid clientId, Xfs4Message msg, CancellationToken cmdtkn)
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

            var (devicePart, name) = GetDeviceCapabilitiesPart();

            var payload = new Dictionary<string, object>
            {
                ["common"] = common
            };
            if (devicePart != null)
                payload[name] = devicePart;

            var resp = new Xfs4Message(MessageType.Completion, "Common.Capabilities", msg.Header.RequestId, payload);
            await SafeSendToClientAsync(clientId, resp, _cts!.Token).ConfigureAwait(false);
        }

        private Task HandleGetTransactionState(Guid clientId, Xfs4Message message, CancellationToken cmdtkn)
        {
            try
            {
                TransactionState = message.Payload?.GetType().GetProperty("state") != null ?
                    Enum.Parse<TransactionStateEnum>(message.Payload?.GetType().GetProperty("state")!.GetValue(message.Payload)?.ToString()!) :
                    TransactionStateEnum.inactive;
                TransactionId = message.Payload?.GetType().GetProperty("transactionId") != null ?
                    message.Payload?.GetType().GetProperty("transactionId")!.GetValue(message.Payload)?.ToString()! :
                    string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
            return Task.CompletedTask;
        }

        private Task HandleSetTransactionState(Guid clientId, Xfs4Message message, CancellationToken cmdtkn)
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
                return SendAsync(clientId, completion, CancellationToken.None);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Task HandleClearCommandNonce(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            CommandNonce = string.Empty;
            return Task.CompletedTask;
        }

        private async Task HandleGetCommandNonce(Guid clientId, Xfs4Message message, CancellationToken token)
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
                Payload = new { commandNonce = CommandNonce }
            };
            await SafeSendToClientAsync(clientId, completion, token).ConfigureAwait(false);
        }

        private async Task HandleStartSecureOperation(Guid clientId, Xfs4Message message, CancellationToken token)
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
            await SafeSendToClientAsync(clientId, completion, token).ConfigureAwait(false);
        }

        private Task HandleSetVersions(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            DeviceSetVersions(clientId, message, token);
            return Task.CompletedTask;
        }

        private Task HandleGetInterfaceInfo(Guid clientId, Xfs4Message message, CancellationToken token)
        {
            DeviceGetInterfaceInfo(clientId, message, token);
            return Task.CompletedTask;
        }

        private async Task HandleCancel(Guid clientId, Xfs4Message msg, CancellationToken cmdtkn)
        {
            try
            {
                // Try to get the client's current active command token
                if (_activeCommandTokens.TryGetValue(clientId, out var cts))
                {
                    _logger.LogInfo($"Cancel requested by client {clientId} for ongoing command.");

                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error while cancelling client {clientId}: {ex.Message}");
                    }

                    // Replace with a fresh CTS for subsequent commands
                    var newCts = new CancellationTokenSource();
                    _activeCommandTokens[clientId] = newCts;

                    // Send completion acknowledgment back to client
                    var completion = new Xfs4Message
                    (
                        MessageType.Completion,
                        "Common.Cancel",
                        msg.Header.RequestId,
                        payload: new { message = "Command cancelled successfully." }
                    );

                    await SafeSendToClientAsync(clientId, completion, CancellationToken.None);
                }
                else
                {
                    // No active command for this client
                    var completion = new Xfs4Message
                    (
                        MessageType.Completion,
                        "Common.Cancel",
                        msg.Header.RequestId,
                        payload: new { message = "No active command to cancel." }
                    );

                    await SafeSendToClientAsync(clientId, completion, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"HandleCancel error: {ex.Message}");

                var err = new Xfs4Message
                (
                    MessageType.Completion,
                    "Common.Cancel",
                    msg.Header.RequestId,
                    payload: new { error = ex.Message },
                    status: CommandStatusEnum.invalidMessage
                );

                await SafeSendToClientAsync(clientId, err, CancellationToken.None);
            }
        }


        /// <summary>
        /// Disposes the simulator, stopping all tasks and closing the WebSocket server.
        /// </summary>
        public void Dispose()
        {
            // prefer async stop but implement IDisposable for convenience
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dispose encountered an error: {ex.Message}");
            }
            finally
            {
                // cleanup local resources
                try { _sendSemaphore.Dispose(); } catch { }
                foreach (var kv in _activeCommandTokens) { try { kv.Value.Dispose(); } catch { } }
                _activeCommandTokens.Clear();
            }
        }
    }
}
