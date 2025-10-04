
namespace Simulators.Services
{
    public class CashDispenserSimulator : IDeviceService
    {
        public string DeviceType => "CashDispenser";
        public int Port { get; private set; }

        public CashDispenserSimulator(int port)
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
