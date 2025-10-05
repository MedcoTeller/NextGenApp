using System.Threading.Tasks;
using GlobalShared;

namespace Simulators.Xfs4IoT.Devices
{
    /// <summary>
    /// Example CardReader simulator. Demonstrates:
    /// - registering handlers
    /// - sending events then final completion
    /// </summary>
    public class CardReaderSimulator : BaseSimulator
    {
        private bool _cardInserted;

        public CardReaderSimulator(int port) : base("CardReader", port)
        {
            // Register the command handlers for card reader commands.
            // Handlers use the CommandHandler signature: Task(Xfs4Message, IMessageSink)
            Dispatcher.Register("CardReader.ReadRawData", HandleReadRawDataAsync);
            Dispatcher.Register("CardReader.EjectCard", HandleEjectCardAsync);
            Dispatcher.Register("CardReader.Reset", HandleResetAsync);
        }

        /// <summary>
        /// Simulate reading raw data from the inserted card.
        /// Emits a MediaInserted event if necessary then a completion with card data.
        /// </summary>
        private async Task HandleReadRawDataAsync(Xfs4Message command, IMessageSink sink)
        {
            Logger.LogInfo("HandleReadRawDataAsync invoked");

            // If card not present, emit an event to indicate media inserted
            if (!_cardInserted)
            {
                _cardInserted = true;
                var ev = new Xfs4Message(MessageType.Event, "CardReader.MediaInserted", command.Header.RequestId, payload: new { info = "card inserted (simulated)" });
                await sink.SendAsync(ev);
            }

            // Simulate a delay for reading
            await Task.Delay(500);

            // Optionally send progress events (example)
            var progress = new Xfs4Message(MessageType.Event, "CardReader.ReadProgress", command.Header.RequestId, payload: new { percent = 50 });
            await sink.SendAsync(progress);

            await Task.Delay(500);

            // Final completion with data
            var payload = new
            {
                track1 = "B1234567890123456^DOE/JOHN^25121010000000000000",
                track2 = "1234567890123456=25121010000000000000",
                chipData = "9F2608AABBCCDDEEFF1122"
            };

            var completion = new Xfs4Message(MessageType.Completion, "CardReader.ReadRawData", command.Header.RequestId, payload: payload, status: "success");
            await sink.SendAsync(completion);

            Logger.LogInfo("HandleReadRawDataAsync completed");
        }

        /// <summary>
        /// Simulate ejecting the card: present, then remove, then completion.
        /// </summary>
        private async Task HandleEjectCardAsync(Xfs4Message command, IMessageSink sink)
        {
            Logger.LogInfo("HandleEjectCardAsync invoked");

            if (!_cardInserted)
            {
                // No media present -> immediate completion with code
                var noMedia = new Xfs4Message(MessageType.Completion, "CardReader.EjectCard", command.Header.RequestId, payload: null, status: "noMediaPresent");
                await sink.SendAsync(noMedia);
                return;
            }

            // Media presented event
            var presented = new Xfs4Message(MessageType.Event, "CardReader.MediaPresented", command.Header.RequestId, payload: new { message = "Card is being presented" });
            await sink.SendAsync(presented);

            // Simulate user taking card
            await Task.Delay(800);

            _cardInserted = false;

            // Media removed event
            var removed = new Xfs4Message(MessageType.Event, "CardReader.MediaRemoved", command.Header.RequestId, payload: new { message = "Card removed" });
            await sink.SendAsync(removed);

            // Final completion
            var completion = new Xfs4Message(MessageType.Completion, "CardReader.EjectCard", command.Header.RequestId, payload: null, status: "success");
            await sink.SendAsync(completion);

            Logger.LogInfo("HandleEjectCardAsync completed");
        }

        /// <summary>
        /// Reset the simulated device.
        /// </summary>
        private async Task HandleResetAsync(Xfs4Message command, IMessageSink sink)
        {
            Logger.LogInfo("HandleResetAsync invoked");
            _cardInserted = false;
            await Task.Delay(300);

            var completion = new Xfs4Message(MessageType.Completion, "CardReader.Reset", command.Header.RequestId, payload: new { message = "reset done" }, status: "success");
            await sink.SendAsync(completion);
            Logger.LogInfo("HandleResetAsync completed");
        }
    }
}
