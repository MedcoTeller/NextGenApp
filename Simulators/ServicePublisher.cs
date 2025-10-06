using GlobalShared;
using Simulators.Xfs4IoT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// ServicePublisher handles XFS4IoT service discovery per the XFS4IoT spec.
    /// It exposes a WebSocket endpoint like:
    ///   ws(s)://machinename:port/xfs4iot/v1.0
    /// and responds to ServicePublisher.GetServices with a completion message listing all available services.
    /// </summary>
    public class ServicePublisher
    {
        private readonly Utils _logger;
        private readonly HttpListener _listener;
        private readonly List<string> _serviceUris = new();
        private readonly bool _useTls;
        private readonly int _port;
        private CancellationTokenSource _cts;

        public string VendorName { get; }
        public string MachineName { get; }

        /// <summary>
        /// Create a new service publisher.
        /// </summary>
        /// <param name="vendorName">Vendor name for the payload.</param>
        /// <param name="machineName">DNS or IP used in URIs.</param>
        /// <param name="services">List of services to publish (e.g. cardreader1, cashdispenser1)</param>
        /// <param name="useTls">True for wss/https, false for ws/http.</param>
        public ServicePublisher(string vendorName, string machineName, IEnumerable<string> services, bool useTls = false)
        {
            VendorName = vendorName;
            MachineName = machineName;
            _useTls = useTls;
            _logger = new Utils(nameof(ServicePublisher));

            // Determine port automatically
            _port = FindAvailablePort(useTls);
            foreach (var s in services)
            {
                string uri = $"{(_useTls ? "wss" : "ws")}://{machineName}:{_port}/xfs4iot/v1.0/{s}";
                _serviceUris.Add(uri);
            }

            _listener = new HttpListener();
            string prefix = $"{(_useTls ? "https" : "http")}://+:{_port}/xfs4iot/v1.0/";
            _listener.Prefixes.Add(prefix);
        }

        /// <summary>
        /// Starts listening for WebSocket clients.
        /// </summary>
        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            _logger.LogInfo($"ServicePublisher started on port {_port} ({(_useTls ? "secure" : "insecure")})");

            while (!_cts.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = HandleWebSocketAsync(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        /// <summary>
        /// Stops the service publisher.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _listener.Stop();
            _logger.LogInfo("ServicePublisher stopped.");
        }

        /// <summary>
        /// Handles an incoming WebSocket connection.
        /// </summary>
        private async Task HandleWebSocketAsync(HttpListenerContext context)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            var socket = wsContext.WebSocket;

            _logger.LogInfo("Client connected to ServicePublisher.");

            var buffer = new byte[4096];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogDebug($"Received message: {json}");

                try
                {
                    var msg = JsonSerializer.Deserialize<Xfs4Message>(json);
                    if (msg?.Header.Type == MessageType.Command && msg.Header.Name == "ServicePublisher.GetServices")
                    {
                        await SendGetServicesResponseAsync(socket, msg.Header.RequestId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInfo($"Error handling message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sends the completion message for GetServices with all known services.
        /// </summary>
        private async Task SendGetServicesResponseAsync(System.Net.WebSockets.WebSocket socket, int? requestId)
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
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            _logger.LogInfo("Sent ServicePublisher.GetServices completion.");
        }

        /// <summary>
        /// Finds the first available port from the standard XFS4IoT range.
        /// </summary>
        private static int FindAvailablePort(bool useTls)
        {
            int[] portSequence = useTls
                ? new[] { 443, 5846, 5847, 5848, 5849, 5850, 5851, 5852, 5853, 5854, 5855, 5856 }
                : new[] { 80, 5846, 5847, 5848, 5849, 5850, 5851, 5852, 5853, 5854, 5855, 5856 };

            foreach (int port in portSequence)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    listener.Stop();
                    return port;
                }
                catch
                {
                    continue;
                }
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

