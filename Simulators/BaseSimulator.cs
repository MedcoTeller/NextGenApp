using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using GlobalShared;
using Simulators.Xfs4IoT.Network;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// Generic base class for device simulators.
    /// - Owns a CommandDispatcher
    /// - Hosts a WebSocketHost that accepts connections on a port
    /// - For each connection creates an IMessageSink wrapper and wires incoming messages into Dispatcher
    /// </summary>
    public abstract class BaseSimulator
    {
        protected readonly Utils Logger;
        protected readonly CommandDispatcher Dispatcher;
        private readonly WebSocketHost _host;
        private readonly int _port;

        /// <summary>
        /// Construct base simulator.
        /// </summary>
        /// <param name="deviceName">Name used in logger</param>
        /// <param name="port">Port to listen on for WebSocket clients</param>
        protected BaseSimulator(string deviceName, int port)
        {
            Logger = new Utils(deviceName);
            Dispatcher = new CommandDispatcher();
            _port = port;

            // Provide a callback to handle new connections
            _host = new WebSocketHost(port, OnNewConnectionAsync);

            Logger.LogInfo($"{deviceName} simulator created on port {port}");
        }

        /// <summary>
        /// Start the simulator's WebSocket host (non-blocking).
        /// </summary>
        public Task StartAsync()
        {
            Logger.LogInfo("Starting simulator host...");
            return _host.StartAsync();
        }

        /// <summary>
        /// Stop the simulator host.
        /// </summary>
        public Task StopAsync()
        {
            Logger.LogInfo("Stopping simulator host...");
            return _host.StopAsync();
        }

        /// <summary>
        /// Called by WebSocketHost when a new connection arrives.
        /// Wires message receive events to parse incoming JSON and dispatch commands.
        /// </summary>
        private async Task OnNewConnectionAsync(BaseWebSocketConnection connection)
        {
            Logger.LogInfo("New client connection established.");

            // Create a sink that wraps this connection so handlers can send messages easily
            var sink = new ConnectionMessageSink(connection, Logger);

            // When connection provides text messages, parse and forward to dispatcher
            connection.TextMessageReceived += async (json) =>
            {
                try
                {
                    var msg = new Xfs4Message(json);
                    Logger.LogDebug($"Incoming {msg.Header.Type} : {msg.Header.Name} (requestId={msg.Header.RequestId})");

                    if (msg.Header.Type == MessageType.Command)
                    {
                        // Dispatch the command to registered handler(s)
                        await Dispatcher.DispatchAsync(msg, sink);
                    }
                    else
                    {
                        // For now ignore unsolicited completions/acks from client
                        Logger.LogDebug("Ignoring non-command message from client");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to parse/process incoming message: {ex.Message}");
                    // Optionally send back an error completion if appropriate
                }
            };

            connection.Disconnected += () =>
            {
                Logger.LogInfo("Client disconnected.");
            };

            // Note: receive loop already started by WebSocketHost before calling this callback.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Small adapter implementing IMessageSink by sending Xfs4Message JSON via BaseWebSocketConnection.
        /// This allows dispatcher handlers to send messages without dealing with sockets directly.
        /// </summary>
        private class ConnectionMessageSink : IMessageSink
        {
            private readonly BaseWebSocketConnection _connection;
            private readonly Utils _logger;

            public ConnectionMessageSink(BaseWebSocketConnection connection, Utils logger)
            {
                _connection = connection;
                _logger = logger;
            }

            public async Task SendAsync(Xfs4Message message)
            {
                var json = message.ToJson();
                await _connection.SendAsync(json);
                _logger.LogDebug($"Sent message via sink: {message.Header.Type} {message.Header.Name} requestId={message.Header.RequestId}");
            }
        }
    }
}
