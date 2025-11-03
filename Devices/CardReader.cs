using Devices.Events;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using WatsonWebsocket;
using static System.Net.Mime.MediaTypeNames;

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
        protected override void UpdateDeviceStatus(object? payload)
        {
            if (payload == null)
                return;
        }

        protected override void UpdateDeviceCapabilities(object? payload)
        {
            if (payload == null)
                return;

        }

        protected override void DeviceSpecialEventHandling(DeviceEvent evt)
        {
            switch(evt.Header.Name)
            {
                case CardReaderCommands.CardReader_Move:
                    
                    break;
                case CardReaderCommands.CardReader_ReadRawData:

                    break;
                case CardReaderEvents.CardReader_MediaRemovedEvent_Unsolic:
                    
                    break;
                case CardReaderEvents.CardReader_MediaInsertedEvent:

                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track1"></param>
        /// <param name="trak2"></param>
        /// <param name="Track3"></param>
        /// <param name="chip"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task ReadCard(bool track1, bool trak2, bool Track3, bool chip, int timeout = 0)
        {
            var cmd = new Command(CardReaderCommands.CardReader_ReadRawData, timeout);
            cmd.Payload = new {
                track1 = track1,
                track2 = trak2,
                track3 = Track3,
                chip = chip
            };
            await SendCommand(cmd);
        }

        /// <summary>
        /// This command is only applicable to motorized and latched dip card readers.
        /// If after a successful completion event the card is at the exit position, the card will be accessible to the user.
        /// A CardReader.MediaRemovedEvent is generated to inform the application when the card is taken.
        /// Motorized card readers
        /// Motorized card readers can physically move cards from or to the transport or exit positions or a storage unit. 
        /// The default operation is to move a card in the transport position to the exit position.
        /// If the card is being moved from the exit position to the exit position, these are valid behaviors:
        /// The card does not move as the card reader can detect the card is already in the correct position.
        /// The card is moved back into the card reader then moved back to the exit to ensure the card is in the correct position.
        /// Latched dip card 
        /// Latched dips card readers can logically move cards from the transport position to the exit position 
        /// by unlatching the card reader.That is, the card will not physically move but will be accessible to the user.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task EjectCard(int timeout = 0)
        {
            var cmd = new Command(CardReaderCommands.CardReader_Move, timeout);
            cmd.Payload = new {
                from = "exit",
                to = "transport"
            };
            await SendCommand(cmd);
        }


    }
}
