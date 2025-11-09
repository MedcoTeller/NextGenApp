using Devices.Events;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
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

        //Status:
        private MediaStatus? _media = MediaStatus.unknown;
        public MediaStatus? Media
        {
            get => _media;
            private set => SetProperty(ref _media, value);
        }

        private string? _security;
        public string? Security
        {
            get => _security;
            private set => SetProperty(ref _security, value);
        }

        private ChipPowerStatus? _chipPower;
        public ChipPowerStatus? ChipPower
        {
            get => _chipPower;
            private set => SetProperty(ref _chipPower, value);
        }

        private ChipModuleStatus? _chipModule;
        public ChipModuleStatus? ChipModule
        {
            get => _chipModule;
            private set => SetProperty(ref _chipModule, value);
        }

        private string? _magWriteModule;
        public string? MagWriteModule
        {
            get => _magWriteModule;
            private set => SetProperty(ref _magWriteModule, value);
        }

        private string? _frontImageModule;
        public string? FrontImageModule
        {
            get => _frontImageModule;
            private set => SetProperty(ref _frontImageModule, value);
        }

        private string? _backImageModule;
        public string? BackImageModule
        {
            get => _backImageModule;
            private set => SetProperty(ref _backImageModule, value);
        }

        //Capabilities:


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
            switch (evt.Header.Name)
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
            cmd.Payload = new
            {
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
            cmd.Payload = new
            {
                from = "exit",
                to = "transport"
            };
            await SendCommand(cmd);
        }

        public enum MediaStatus
        {
            unknown,    // - The media state cannot be determined with the device in its current state (e.g.the value of device is noDevice, powerOff, offline or hardwareError.
            present,    // - Media is present in the device, not in the entering position and not jammed.On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
            notPresent, // - Media is not present in the device and not at the entering position.
            jammed,     // - Media is jammed in the device; operator intervention is required.
            entering,   // - Media is at the entry/exit slot of a motorized device.
            latched,    // - Media is present and latched in a latched dip card unit.This means the card can be used for chip card dialog.
        }

        public enum ChipPowerStatus
        {
            unknown,    // - The media state cannot be determined with the device in its current state (e.g.the value of device is noDevice, powerOff, offline or hardwareError.
            present,    // - Media is present in the device, not in the entering position and not jammed.On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
            notPresent, // - Media is not present in the device and not at the entering position.
            jammed,     // - Media is jammed in the device; operator intervention is required.
            entering,   // - Media is at the entry/exit slot of a motorized device.
            latched,    // - Media is present and latched in a latched dip card unit.This means the card can be used for chip card dialog.
        }

        public enum ChipModuleStatus
        {
            ok, // - The chip card module is in a good state.
            inoperable, // - The chip card module is inoperable.
            unknown, // - The state of the chip card module cannot be determined.
        }


        public enum GeneralStatus
        {
            ok, // - The chip card module is in a good state.
            inoperable, // - The chip card module is inoperable.
            unknown, // - The state of the chip card module cannot be determined.
        }
    }
}
