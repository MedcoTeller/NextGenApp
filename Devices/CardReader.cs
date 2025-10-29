using WatsonWebsocket;

namespace Devices
{
    public class CardReader : Device
    {
        public CardReader(string name, string id, string uri) : base(name, id, uri)
        {

        }

        public CardReader(string name, string id, WatsonWsClient client) : base(name, id, client)
        {
            
        }

        public async Task ReadCard(bool track1, bool trak2, bool Track3, bool chip, int timeout = 0)
        {
            var cmd = new Command("CardReader.ReadRawData", timeout);
            cmd.Payload = new {
                track1 = track1,
                track2 = trak2,
                track3 = Track3,
                chip = chip
            };
            await SendCommand(cmd);
        }
    }
}
