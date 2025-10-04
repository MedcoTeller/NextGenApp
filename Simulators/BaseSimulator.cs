using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// Base class for all device simulators. Handles generic XFS4IoT message dispatching.
    /// </summary>
    public abstract class BaseSimulator
    {
        private readonly CommandDispatcher _dispatcher = new CommandDispatcher();

        /// <summary>
        /// Registers a command handler for this simulator.
        /// </summary>
        protected void RegisterCommand(string commandName, CommandHandler handler)
        {
            _dispatcher.Register(commandName, handler);
        }

        /// <summary>
        /// Processes an incoming raw JSON message from a client.
        /// </summary>
        public async Task ProcessMessageAsync(string json, IMessageSink sink)
        {
            Xfs4Message? msg;
            try
            {
                msg = JsonSerializer.Deserialize<Xfs4Message>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Simulator] Failed to parse incoming JSON: {ex.Message}");
                return;
            }

            if (msg == null)
                return;

            await _dispatcher.DispatchAsync(msg, sink);
        }

        /// <summary>
        /// Helper to send a completion message for a request.
        /// </summary>
        protected async Task SendCompletionAsync(IMessageSink sink, Xfs4Message request, string completionCode, object? payload = null)
        {
            var completion = new Xfs4Message
            {
                Header = new Xfs4Header
                {
                    RequestId = request.Header.RequestId,
                    Type = MessageType.Completion,
                    Name = request.Header.Name,
                    Status = completionCode
                },
                Payload = (JsonElement?)payload
            };

            //string json = JsonSerializer.Serialize(completion);
            await sink.SendMessageAsync(completion);
        }

        /// <summary>
        /// Helper to send an event related to a request.
        /// </summary>
        protected async Task SendEventAsync(IMessageSink sink, Xfs4Message request, object? payload = null)
        {
            var ev = new Xfs4Message
            {
                Header = new Xfs4Header
                {
                    RequestId = request.Header.RequestId,
                    Type = MessageType.Event,
                    Name = request.Header.Name + ".Event" // convention: extend with .Event
                },
                Payload = (JsonElement?)payload
            };

            //string json = JsonSerializer.Serialize(ev);
            await sink.SendMessageAsync(ev);
        }
    }
}
