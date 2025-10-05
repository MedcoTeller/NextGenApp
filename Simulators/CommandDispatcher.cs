using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GlobalShared;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// Sink interface for handlers to send messages (events/completion).
    /// Implemented by a transport wrapper that sends JSON to a connected client.
    /// </summary>
    public interface IMessageSink
    {
        /// <summary>
        /// Send an Xfs4Message to the connected client.
        /// </summary>
        Task SendAsync(Xfs4Message message);
    }

    /// <summary>
    /// Command handler delegate. Handlers receive the parsed command and an IMessageSink
    /// to emit events/completion messages. Handlers should NOT block the thread.
    /// </summary>
    /// <param name="command">Incoming command message (Header.Type == Command)</param>
    /// <param name="sink">Sink to send events/completion through</param>
    public delegate Task CommandHandler(Xfs4Message command, IMessageSink sink);

    /// <summary>
    /// Generic command dispatcher. Maps command name -> handler (string match).
    /// It does minimal validation (requestId presence & non-negative) and invokes the handler.
    /// The handler is expected to send events and the final completion using the provided sink.
    /// </summary>
    public class CommandDispatcher
    {
        private readonly ConcurrentDictionary<string, CommandHandler> _handlers = new();
        private readonly Utils _logger = new Utils(nameof(CommandDispatcher));

        /// <summary>
        /// Register a handler for a command name. Overwrites any existing registration.
        /// </summary>
        public void Register(string commandName, CommandHandler handler)
        {
            _handlers[commandName] = handler;
            _logger.LogDebug($"Registered handler for {commandName}");
        }

        /// <summary>
        /// Dispatch an incoming command message to its registered handler.
        /// If the command is invalid or no handler exists, a completion message is sent via the sink.
        /// </summary>
        public async Task DispatchAsync(Xfs4Message command, IMessageSink sink)
        {
            // Validate incoming is a command
            if (command?.Header == null)
            {
                _logger.LogError("Received null or malformed command");
                return;
            }

            if (command.Header.Type != MessageType.Command)
            {
                _logger.LogWarning("DispatchAsync called with non-command message");
                return;
            }

            var name = command.Header.Name;
            var requestId = command.Header.RequestId;

            // Spec: requestId must be present and non-negative for commands
            if (requestId == null || requestId < 0)
            {
                _logger.LogWarning("Invalid or missing requestId in command");
                var invalidResp = new Xfs4Message(MessageType.Completion, name ?? "Unknown", requestId, payload: new { error = "Invalid or missing requestId" }, status: "invalidRequestID");
                await sink.SendAsync(invalidResp);
                return;
            }

            if (!_handlers.TryGetValue(name, out var handler))
            {
                _logger.LogWarning($"No handler for {name}");
                var unsupported = new Xfs4Message(MessageType.Completion, name, requestId, payload: new { error = $"No handler for {name}" }, status: "unsupportedCommand");
                await sink.SendAsync(unsupported);
                return;
            }

            try
            {
                // Call device-specific handler which will use sink to send events/completion
                await handler(command, sink);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Handler for {name} threw: {ex.Message}");
                var err = new Xfs4Message(MessageType.Completion, name, requestId, payload: new { error = ex.Message }, status: "internalError");
                await sink.SendAsync(err);
            }
        }
    }
}
