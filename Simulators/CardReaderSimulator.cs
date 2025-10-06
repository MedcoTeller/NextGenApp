using Simulators.Xfs4IoT;

namespace Simulators.Devices
{
    public class CardReaderSimulator : BaseSimulator
    {
        public CardReaderSimulator(string url, string deviceName = "Cardreader")
            : base(url, deviceName)
        {
            RegisterCommandHandler("CardReader.ReadCard", ReadCard);
            RegisterCommandHandler("CardReader.Resetrd", Reset);
            //more commands handling
        }

        public override void ClientConnected()
        {

        }

        public override void MessageReceived(Xfs4Message req, System.Net.WebSockets.WebSocket socket)
        {

        }

        private async Task ReadCard(System.Net.WebSockets.WebSocket socket, Xfs4Message req)
        {
            _logger.LogInfo("Simulating card read...");

            await Task.Delay(1500); // simulate hardware delay

            var completion = new Xfs4Message
            {
                Header = new Xfs4Header
                {
                    Name = "CardReader.ReadCard",
                    Type = MessageType.Completion,
                    RequestId = req.Header.RequestId
                },
                Payload = new
                {
                    Status = "Success",
                    CardData = "1234-5678-9012-3456"
                }
            };

            await SendAsync(socket, completion, _cts.Token);
            _logger.LogInfo("Card read completed.");
        }


        private async Task Reset(System.Net.WebSockets.WebSocket socket, Xfs4Message req)
        {
            var response = new Xfs4Message
            {
                Header = new Xfs4Header
                {
                    Name = "CardReader.Reset",
                    Type = MessageType.Completion,
                    RequestId = req.Header.RequestId
                },
                Payload = new
                {
                    Status = "Success"
                }
            };
            await SendAsync(socket, response, _cts.Token);
        }
    }
}
