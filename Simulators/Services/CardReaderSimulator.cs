
namespace Simulators.Services
{
    public class CardReaderSimulator : IDeviceService
    {
        public string DeviceType => "CardReader";
        public int Port { get; private set; }

        public CardReaderSimulator(int port)
        {
            Port = port;
        }

        public void Start()
        {
            // TODO: WebSocket server startup + UI updates
        }

        public void Stop()
        {
            // TODO: shutdown
        }
    }
}
