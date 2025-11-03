using Devices.Events;
using GlobalShared;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WatsonWebsocket;
using Websocket.Client;

namespace Devices
{
    public class Device : IDisposable
    {
        private readonly WatsonWsClient _client;
        private readonly ConcurrentQueue<DeviceEvent> _events = new();
        private readonly SemaphoreSlim _eventSignal = new(0);
        private int _requestId = 0;
        private bool _disposed;
        private bool _capabilitiesFetched = false;

        protected Utils utils;

        public string Name { get; }
        public string Uri { get; }
        public string Id { get; }

        public List<string> SupportedCommands { get; } = new();
        public List<string> SecureCommands { get; } = new();
        public bool IsConnected => _client?.Connected ?? false;
        public bool IsSecureConnection { get; } = false;

        public int CommandNonceTimeout { get; set; } = 3600;
        public bool SecureConnection { get; set; }

        public event Action<Message>? OnEvent = null;
        public List<Message> SessionEvents = new();
        public List<Message> TransactionEvents = new();

        [JsonInclude] public DeviceStatusEnum? DeviceStatus { get; protected set; } = DeviceStatusEnum.noDevice;
        [JsonInclude] public DevicePositionStatusEnum? DevicePositionStatus { get; protected set; }
        [JsonInclude] public int PowerSaveRecoveryTime { get; protected set; }
        [JsonInclude] public AntiFraudModuleStatusEnum? AntiFraudModuleStatus { get; protected set; }
        [JsonInclude] public ExchangeStatusEnum? ExchangeStatus { get; protected set; }
        [JsonInclude] public EndToEndSecurityStatusEnum? EndToEndSecurityStatus { get; protected set; }
        [JsonInclude] public int RemainingCapacityStatus { get; protected set; }

        // Capabilities
        [JsonInclude] public string ServiceVersion { get; protected set; }
        [JsonInclude] public string ModelName { get; protected set; }
        [JsonInclude] public bool PowerSaveControlCp { get; protected set; }
        [JsonInclude] public bool HasAntiFraudModuleCp { get; protected set; }
        [JsonInclude] public RequiredEndToEndSecurityEnum? RequiredEndToEndSecurityCp { get; protected set; }
        [JsonInclude] public bool HardwareSecurityElementCp { get; protected set; }
        [JsonInclude] public ResponseSecurityEnabledEnum? ResponseSecurityEnabledCp { get; protected set; }

        public Device(string name, string id, string uri, bool isSecure = false)
        {
            Name = name;
            Id = id;
            Uri = uri;
            utils = new(GetType().ToString());

            var adress = new Uri(uri);
            IsSecureConnection = isSecure;
            _client = new WatsonWsClient(adress.Host, adress.Port, isSecure);

            _client.ServerConnected += (s, e) =>
            {
                utils.LogInfo($"[{Name}] Connected to {Uri}");
            };

            _client.ServerDisconnected += (s, e) =>
            {
                utils.LogError($"[{Name}] Disconnected from {Uri}");
            };

            _client.MessageReceived += (s, e) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(e.Data);
                    if (string.IsNullOrEmpty(message))
                    {
                        utils.LogError($"[{Name}] Empty message received from server");
                        return;
                    }

                    OnMessage(message);
                }
                catch (Exception ex)
                {
                    utils.LogError($"[{Name}] Exception in message handler: {ex.Message}");
                }
            };
        }

        public Device(string name, string id, WatsonWsClient websocketClient) 
        { 
            Name = name; Id = id; 
            utils = new(GetType().ToString());
            _client?.MessageReceived += (s, e) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(e.Data);
                    if (string.IsNullOrEmpty(message))
                    {
                        utils.LogError($"[{Name}] Empty message received from server");
                        return;
                    }

                    OnMessage(message);
                }
                catch (Exception ex)
                {
                    utils.LogError($"[{Name}] Exception in message handler: {ex.Message}");
                }
            };
        }

        public async Task<bool> StartAsync()
        {
            try
            {
                utils.LogInfo($"Starting device {Name}/{Id}");
                await _client.StartAsync();

                if (!_capabilitiesFetched)
                {
                    await GetStatus();
                    await GetCapabilitiesAsync();
                }

                utils.LogInfo($"Device {Name}/{Id} started successfully");
                return true;
            }
            catch (Exception ex)
            {
                utils.LogError($"Exception while starting device {Name}/{Id}: {ex.Message}");
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_disposed) return;
            utils.LogInfo($"Stopping device {Name}/{Id}");
            await _client.StopAsync();
            DeviceStatus = DeviceStatusEnum.noDevice;
        }

        public void NewSession()
        {
            SessionEvents.Clear();
            TransactionEvents.Clear();
        }
        public void EndSession()
        {
            

        }

        public void NewTransaction()
        {
            TransactionEvents.Clear();
        }
        public void EndTransaction()
        {

        }

        protected void SaveEvent(Message ev)
        {
            SessionEvents.Add(ev);
            TransactionEvents.Add(ev);
        }

        protected async Task SendCommand(Command cmd)
        {
            cmd.Header.RequestId = Interlocked.Increment(ref _requestId);
            string json = cmd.ToJson();

            utils.LogDebug($"[{Name}] Sending command: {json}");
            await _client.SendAsync(json);

            if (!ConfirmAcknowledge(cmd))
            {
                utils.LogError($"[{Name}] Failed to receive acknowledge for command {cmd.Header.Name}");
                throw new InvalidOperationException($"Acknowledge not received for {cmd.Header.Name}");
            }
        }

        protected virtual void OnMessage(string json)
        {
            utils.LogDebug($"[{Name}] Received: {json}");

            var msg = Message.FromJson(json);
            var evt = new DeviceEvent
            {
                Header = msg.Header,
                Payload = msg.Payload
            };

            OnEvent?.Invoke(msg);
            DeviceSpecialEventHandling(evt);

            _events.Enqueue(evt);
            _eventSignal.Release();

            utils.LogDebug($"[{Name}] Event queued (count={_events.Count})");
        }

        protected virtual void DeviceSpecialEventHandling(DeviceEvent evt)
        {
            
        }

        public DeviceEvent? GetEvent(int timeoutMs)
        {
            if (_disposed) return null;
            utils.LogDebug($"[{Name}] Waiting for event ({timeoutMs}ms)");

            if (_eventSignal.Wait(timeoutMs))
            {
                if (_events.TryDequeue(out var evnt))
                {
                    utils.LogDebug($"[{Name}] Dequeued event: {evnt.Header.Name}");
                    return evnt;
                }
            }

            utils.LogDebug($"[{Name}] No event within timeout");
            return null;
        }

        private bool ConfirmAcknowledge(Command cmd)
        {
            var ack = GetEvent(5000);
            if (ack == null)
            {
                utils.LogError($"[{Name}] No acknowledge received for {cmd.Header.Name}");
                return false;
            }

            if (ack.Header.Type != MessageType.Acknowledge ||
                ack.Header.RequestId != cmd.Header.RequestId)
            {
                utils.LogError($"[{Name}] Invalid acknowledge for {cmd.Header.Name}");
                return false;
            }

            return true;
        }

        public async Task Cancel(int[] ids = null)
        {
            var cmd = new Command(CommonCommands.Common_Cancel)
            {
                Payload = ids != null ? new { requestIds = ids } : null
            };

            await SendCommand(cmd);
            _ = GetEvent(500);
        }

        public async Task GetCapabilitiesAsync()
        {
            var cmd = new Command(CommonCommands.Common_Capabilities);
            await SendCommand(cmd);

            var res = GetEvent(60000);
            if (res?.Payload == null)
            {
                utils.LogError($"[{Name}] No capabilities response");
                return;
            }

            SetCommonCapabilities(res.Payload);
            UpdateDeviceCapabilities(res.Payload);
        }

        public async Task GetStatus()
        {
            utils.LogInfo($"[{Name}] Requesting status");
            var cmd = new Command(CommonCommands.Common_Status);
            await SendCommand(cmd);

            var res = GetEvent(60000);
            if (res?.Payload == null)
            {
                utils.LogError($"[{Name}] No status response received");
                return;
            }

            SetCommonStatus(res.Payload);
            UpdateDeviceStatus(res.Payload);
        }

        public void SetCommonCapabilities(object? payload)
        {
            try
            {
                ServiceVersion = Message.GetPayloadValue<string>(payload, "common.serviceVersion");
                ModelName = Message.GetPayloadValue<string>(payload, "common.deviceInformation.modelName");
                PowerSaveControlCp = Message.GetPayloadValue<bool>(payload, "common.powerSaveControl");
                HardwareSecurityElementCp = Message.GetPayloadValue<bool>(payload, "common.endToEndSecurity.hardwareSecurityElement");
                HasAntiFraudModuleCp = Message.GetPayloadValue<bool>(payload, "common.antiFraudModule");

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.endToEndSecurity.required"), out RequiredEndToEndSecurityEnum r))
                    RequiredEndToEndSecurityCp = r;

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.endToEndSecurity.responseSecurityEnabled"), out ResponseSecurityEnabledEnum re))
                    ResponseSecurityEnabledCp = re;

                var commands = Message.GetPayloadValue<object[]>(payload, "common.endToEndSecurity.commands");
                if (commands != null)
                {
                    foreach (var c in commands)
                    {
                        var name = c?.ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                            SecureCommands.Add(name);
                    }
                }

                CommandNonceTimeout = Message.GetPayloadValue<int>(payload, "common.endToEndSecurity.commandNonceTimeout");

                _capabilitiesFetched = true;
                utils.LogInfo($"[{Name}] Capabilities set successfully");
            }
            catch (Exception ex)
            {
                utils.LogError($"[{Name}] Failed to parse capabilities: {ex.Message}");
            }
        }

        public void SetCommonStatus(object? payload)
        {
            try
            {
                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.device"), out DeviceStatusEnum ds))
                    DeviceStatus = ds;

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.devicePosition"), out DevicePositionStatusEnum dp))
                    DevicePositionStatus = dp;

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.antiFraudModule"), out AntiFraudModuleStatusEnum af))
                    AntiFraudModuleStatus = af;

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.exchange"), out ExchangeStatusEnum exs))
                    ExchangeStatus = exs;

                if (Enum.TryParse(Message.GetPayloadValue<string>(payload, "common.endToEndSecurity"), out EndToEndSecurityStatusEnum es))
                    EndToEndSecurityStatus = es;

                PowerSaveRecoveryTime = Message.GetPayloadValue<int>(payload, "common.powerSaveRecoveryTime");
                RemainingCapacityStatus = Message.GetPayloadValue<int>(payload, "common.persistentDataStore.remaining");

                utils.LogInfo($"[{Name}] Status updated successfully");
            }
            catch (Exception ex)
            {
                utils.LogError($"[{Name}] Failed to parse status: {ex.Message}");
            }
        }

        protected virtual void UpdateDeviceStatus(object? payload) { }
        protected virtual void UpdateDeviceCapabilities(object? payload) { }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _client.Dispose();
            _eventSignal.Dispose();
            utils.LogInfo($"[{Name}] Device disposed");
        }
    }

    /// <summary>
    /// Specifies the operational status of a device or service, indicating its availability, error state, or other
    /// relevant condition.
    /// </summary>
    /// <remarks>Use this enumeration to determine the current state of a device or service, such as whether
    /// it is online, offline, experiencing errors, or in a transitional state. The values can be used to guide
    /// application behavior, such as enabling or disabling functionality, handling errors, or responding to potential
    /// fraud conditions. Some states, such as 'starting', are temporary and should be monitored for changes to a stable
    /// state. Application logic may be required for certain statuses, such as 'potentialFraud', to decide whether to
    /// take further action.</remarks>
    public enum DeviceStatusEnum
    {
        online,         // - The device or service is online. This is returned when the device is present and operational or the service is not associated with a device, e.g., a vendor mode / vendor application service.
        offline,        // - The device is offline (e.g., the operator has taken the device offline by turning a switch or breaking an interlock).
        powerOff,       // - The device is powered off or physically not connected.
        noDevice,       // - The device is not intended to be there, e.g. this type of self-service machine does not contain such a device or it is internally not configured.
        hardwareError,  // - The device is inoperable due to a hardware error.
        userError,      // -The device is present but a person is preventing proper device operation.
        deviceBusy,     // -The device is busy and unable to process a command at this time.
        fraudAttempt,   // -The device is present but is inoperable because it has detected a fraud attempt.
        potentialFraud, // -The device has detected a potential fraud attempt and is capable of remaining in service. In this case the application should make the decision as to whether to take the device offline.
        starting,       // -The device is starting and performing whatever initialization is necessary. This can be reported after the connection is made but before the device is ready to accept commands. This must only be a temporary state, the service must report a different state as soon as possible. If an error causes initialization to fail then the state should change to hardwareError.
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DevicePositionStatusEnum
    {
        inPosition,     // The device is in its normal operating position, or is fixed in place and cannot be moved.
        notInPosition,  // The device has been removed from its normal operating position.
        unknown         // Due to a hardware error or other condition, the position of the device cannot be determined.
    }

    /// <summary>
    /// Specifies the operational status of the anti-fraud module, indicating whether it is functioning correctly,
    /// inoperable, has detected a foreign device, or its state is unknown.
    /// </summary>
    /// <remarks>Use this enumeration to determine the current state of the anti-fraud module and to respond
    /// appropriately to changes in its status. The values represent distinct states that may affect security or device
    /// operation.</remarks>
    public enum AntiFraudModuleStatusEnum
    {
        ok,             // Anti-fraud module is in a good state and no foreign device is detected.
        inoperable,     // Anti-fraud module is inoperable.
        deviceDetected, // Anti-fraud module detected the presence of a foreign device.
        unknown         // The state of the anti-fraud module cannot be determined.
    }

    /// <summary>
    /// Specifies the operational status of an exchange on the service.
    /// </summary>
    /// <remarks>Use this enumeration to determine whether commands can be sent to the exchange. When the
    /// status is set to 'inactive', operations that interact with the device may be rejected.</remarks>
    public enum ExchangeStatusEnum
    {
        active,   // Exchange is active on this service. Commands which interact with the device may be rejected with an error code as appropriate.
        inactive  // Exchange is not active on this service.
    }

    /// <summary>
    /// Specifies the status of end to end security support on this device.
    /// This property is null in Common.Status if E2E security is not supported by this hardware and any command can be called without a token.
    /// </summary>
    /// <remarks>
    /// Also see Common.CapabilityProperties.endToEndSecurity.
    /// </remarks>
    public enum EndToEndSecurityStatusEnum
    {
        notEnforced,   // E2E security is supported by this hardware but it is not currently enforced, for example because required keys aren't loaded. It's currently possible to perform E2E commands without a token.
        notConfigured, // E2E security is supported but not correctly configured, for example because required keys aren't loaded. Any attempt to perform any command protected by E2E security will fail.
        enforced
    }


    /// <summary>
    /// Specifies the level of support for end-to-end security.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine whether E2E security is always enforced or only if configured.
    /// </remarks>
    public enum RequiredEndToEndSecurityEnum
    {
        ifConfigured, // The device is capable of supporting E2E security, but it will not be enforced if not configured, for example because the required keys are not loaded.
        always        // E2E security is supported and enforced at all times. Failure to supply the required security details will lead to errors. If E2E security is not correctly configured, for example because the required keys are not loaded, all secured commands will fail with an error.
    }

    /// <summary>
    /// Specifies if this device will return a security token as part of the response data to commands that support end-to-end security.
    /// This property is null in Common.Status if the device is incapable of returning a response token, otherwise one of the following values:
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine whether a security token is always included in responses or only if E2E security is correctly configured.
    /// </remarks>
    public enum ResponseSecurityEnabledEnum
    {
        ifConfigured, // The device is capable of supporting E2E security if correctly configured. If E2E security has not been correctly configured, for example because the required keys are not loaded, commands will complete without a security token.
        always        // A security token will be included with command responses. If E2E security is not correctly configured, for example because the required keys are not loaded, the command will complete with an error.
    }

    /// <summary>
    /// This command allows the application to specify the transaction state, which the service can then 
    /// utilize in order to optimize performance. After receiving this command, this service can perform 
    /// the necessary processing to start or end the customer transaction. 
    /// This command should be called for every service that could be used in a customer transaction. 
    /// The transaction state applies to every session.
    /// </summary>
    public enum TransactionStateEnum
    {
        active,     //- A customer transaction is in progress.
        inactive    //- No customer transaction is in progress.
    }
}
