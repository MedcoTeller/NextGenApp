using GlobalShared;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WatsonWebsocket;

namespace Simulators.Xfs4IoT
{
    public class ServicePublisher
    {
        private readonly Utils _logger;
        private readonly WatsonWsServer _server;
        private readonly List<string> _serviceUris = new();
        private readonly bool _useTls;
        private readonly int _port;

        public string VendorName { get; }
        public string MachineName { get; }

        public ServicePublisher(string vendorName, string machineName, IEnumerable<string> services, bool useTls = false)
        {
            VendorName = vendorName;
            MachineName = machineName;
            _useTls = useTls;
            _logger = new Utils(nameof(ServicePublisher));
            _port = FindAvailablePort(useTls);

            // Initialize Watson WebSocket server
            var url = new Uri($"{(_useTls ? "wss" : "ws")}://{machineName}:{_port}/xfs4iot/v1.0");
            string host = url.Host;
            _server = new WatsonWsServer(host, _port, _useTls);
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
            _server.MessageReceived += OnMessageReceived;
        }

        public async Task StartAsync()
        {
            _ = _server.StartAsync();
            _logger.LogInfo($"ServicePublisher started on port {_port} ({(_useTls ? "secure" : "insecure")})");
        }

        public async Task StopAsync()
        {
            _server.Stop();
            _logger.LogInfo("ServicePublisher stopped.");
        }

        private void OnClientConnected(object? sender, ConnectionEventArgs args)
        {
            _logger.LogInfo($"Client connected: {args.Client}");
        }

        private void OnClientDisconnected(object? sender, DisconnectionEventArgs args)
        {
            _logger.LogInfo($"Client disconnected: {args.Client}");
        }

        private async void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            string json = Encoding.UTF8.GetString(args.Data);
            _logger.LogDebug($"Received message from {args.Client}: \nJson: {json}");

            try
            {
                var msg = JsonSerializer.Deserialize<Xfs4Message>(json);
                if (msg?.Header.Type == MessageType.Command &&
                    msg.Header.Name == "ServicePublisher.GetServices")
                {
                    var ack = new Xfs4Message(MessageType.Acknowledge, msg.Header.Name, msg.Header.RequestId);
                    _logger.LogDebug($"Received message from {args.Client}: \nJson: {ack}");
                    await _server.SendAsync(args.Client.Guid, ack.ToJson()); ;
                    await SendGetServicesResponseAsync(args.Client.Guid, msg.Header.RequestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInfo($"Error handling message: {ex.Message}");
            }
        }

        public void AddServiceUri(string uri)
        {
            _serviceUris.Add(uri);
        }

        private async Task SendGetServicesResponseAsync(Guid clientId, int? requestId)
        {
            var payload = new
            {
                vendorName = VendorName,
                services = _serviceUris.Select(uri => new { serviceURI = uri }).ToList()
            };

            var completion = new Xfs4Message(
                requestId: requestId,
                type: MessageType.Completion,
                name: "ServicePublisher.GetServices",
                payload: payload
            );

            string json = completion.ToJson();
            await _server.SendAsync(clientId, json);

            _logger.LogInfo($"Sent ServicePublisher.GetServices \ncompletion: {completion} \nto: {clientId}");
        }

        private static int FindAvailablePort(bool useTls)
        {
            int[] portSequence = useTls
                ? new[] { 5846, 5847, 5848, 5849, 5850, 5851, 443 }
                : new[] { 5846, 5847, 5848, 5849, 5850, 5851, 80 };

            foreach (int port in portSequence)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    listener.Stop();
                    return port;
                }
                catch { continue; }
            }
            throw new Exception("No available port found in XFS4IoT port sequence.");
        }
    }
}


//Usage example:
//var publisher = new ServicePublisher(
//    vendorName: "ACME ATM Hardware GmbH",
//    machineName: "ATM1",
//    services: new[] { "cardreader1", "cashdispenser1" },
//    useTls: false
//);

//await publisher.StartAsync();

