using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// Sink for sending messages back to the client (e.g. via WebSocket).
    /// Handlers use this to emit events and completions.
    /// </summary>
    public interface IMessageSink
    {
        Task SendMessageAsync(Xfs4Message message);
    }

    /// <summary>
    /// Delegate for command handlers.
    /// Handlers can emit zero or more events and must emit one completion.
    /// </summary>
    /// <param name="command">The incoming command message.</param>
    /// <param name="sink">The sink to send responses/events to.</param>
    public delegate Task CommandHandler(Xfs4Message command, IMessageSink sink);

    /// <summary>
    /// Generic dispatcher for XFS4IoT commands.
    /// Routes commands to registered handlers.
    /// </summary>
    public class CommandDispatcher
    {
        private readonly ConcurrentDictionary<string, CommandHandler> _handlers = new();

        /// <summary>
        /// Register a handler for a specific command name (e.g. "CardReader.ReadRawData").
        /// </summary>
        public void Register(string commandName, CommandHandler handler)
        {
            if (!_handlers.TryAdd(commandName, handler))
            {
                throw new InvalidOperationException($"Handler already registered for {commandName}");
            }
        }

        /// <summary>
        /// Dispatches an incoming command and lets the handler push responses/events
        /// to the given sink.
        /// </summary>
        public async Task DispatchAsync(Xfs4Message command, IMessageSink sink)
        {
            if (command.Header.Type != MessageType.Command)
                throw new InvalidOperationException("Dispatcher only accepts command messages.");

            var commandName = command.Header.Name;
            var requestId = command.Header.RequestId;

            if (string.IsNullOrWhiteSpace(commandName) || requestId == null || requestId < 0)
            {
                await sink.SendMessageAsync(new Xfs4Message(
                    MessageType.Completion,
                    commandName ?? "Unknown",
                    requestId,
                    status: "invalidRequestID",
                    payload: new { error = "Invalid or missing requestId." }
                ));
                return;
            }

            if (_handlers.TryGetValue(commandName, out var handler))
            {
                try
                {
                    await handler(command, sink);
                }
                catch (Exception ex)
                {
                    await sink.SendMessageAsync(new Xfs4Message(
                        MessageType.Completion,
                        commandName,
                        requestId,
                        status: "internalError",
                        payload: new { error = ex.Message }
                    ));
                }
            }
            else
            {
                await sink.SendMessageAsync(new Xfs4Message(
                    MessageType.Completion,
                    commandName,
                    requestId,
                    status: "unsupportedCommand",
                    payload: new { error = $"No handler for {commandName}" }
                ));
            }
        }
    }
}
