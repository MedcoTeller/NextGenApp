using Devices.Events;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using WatsonWebsocket;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private MediaStatusEnum? _mediaStatus = MediaStatusEnum.unknown;
        public MediaStatusEnum? MediaStatus
        {
            get => _mediaStatus;
            private set => SetProperty(ref _mediaStatus, value);
        }

        private CardReaderSecurityStatusEnum? _securityStatus = null;
        public CardReaderSecurityStatusEnum? SecurityStatus
        {
            get => _securityStatus;
            private set => SetProperty(ref _securityStatus, value);
        }

        private ChipPowerStatusEnum? _chipPowerStatus = null;
        public ChipPowerStatusEnum? ChipPowerStatus
        {
            get => _chipPowerStatus;
            private set => SetProperty(ref _chipPowerStatus, value);
        }

        private ChipModuleStatusEnum? _chipModule;
        public ChipModuleStatusEnum? ChipModule
        {
            get => _chipModule;
            private set => SetProperty(ref _chipModule, value);
        }

        private MagWriteModuleStatusEnum? _magWriteModuleStatus = null;
        public MagWriteModuleStatusEnum? MagWriteModuleStatus
        {
            get => _magWriteModuleStatus;
            private set => SetProperty(ref _magWriteModuleStatus, value);
        }

        private FrontImageModuleStatusEnum? _frontImageModuleStatus = null;
        public FrontImageModuleStatusEnum? FrontImageModuleStatus
        {
            get => _frontImageModuleStatus;
            private set => SetProperty(ref _frontImageModuleStatus, value);
        }

        private BackImageModuleStatusEnum? _backImageModuleStatus = null;
        public BackImageModuleStatusEnum? BackImageModuleStatus
        {
            get => _backImageModuleStatus;
            private set => SetProperty(ref _backImageModuleStatus, value);
        }

        //Capabilities:
        public CardReaderTypeEnum? CardReaderType { get; private set; } = null;

        public bool CanReadTrac1Cp { get; private set; } = false;
        public bool CanReadTrac2Cp { get; private set; } = false;
        public bool CanReadTrac3Cp { get; private set; } = false;
        public bool CanReadWaterMarkCp { get; private set; } = false;
        public bool CanReadFrontTrac1 { get; private set; } = false;
        public bool CanReadFrontImageCp { get; private set; } = false;
        public bool CanReadBackImageCp { get; private set; } = false;
        public bool CanReadTrac1JISCp { get; private set; } = false;
        public bool CanReadTrac3JISCp { get; private set; } = false;
        public bool CanWriteTrac1Cp { get; private set; } = false;
        public bool CanWriteTrac2Cp { get; private set; } = false;
        public bool CanWriteTrac3Cp { get; private set; } = false;
        public bool CanWriteWaterMarkCp { get; private set; } = false;
        public bool CanWriteFrontTrac1Cp { get; private set; } = false;
        public bool CanWriteFrontImageCp { get; private set; } = false;
        public bool CanWriteBackImageCp { get; private set; } = false;
        public bool CanWriteTrac1JISCp { get; private set; } = false;
        public bool CanWriteTrac3JISCp { get; private set; } = false;
        public bool ChipProtocolsChipT0Cp { get; private set; } = false;
        public bool ChipProtocolsChipT1Cp { get; private set; } = false;
        public bool ChipProtocolNotRequiredCp { get; private set; } = false;
        public bool ChipProtocolsChipTypeAPart3Cp { get; private set; } = false;
        public bool ChipProtocolsChipTypeAPart4Cp { get; private set; } = false;
        public bool ChipProtocolsChipTypeBCp { get; private set; } = false;
        public bool ChipProtocolsChipTypeNFCCp { get; private set; } = false;
        public CardreaderSecurityCpEnum? CardreaderSecurityCp { get; private set; } = null;
        public PowerOptionCpEnum? PowerOnOptionCp { get; private set; } = null;
        public PowerOptionCpEnum? PowerOffOptionCp { get; private set; } = null;
        public bool FluxSensorProgrammableCp { get; private set; } = false;
        public bool ReadWriteAccessFromExitCp { get; private set; } = false;
        public bool WriteModeLocoCp { get; private set; } = false;
        public bool WriteModeHicoCp { get; private set; } = false;
        public bool WriteModeAutoCp { get; private set; } = false;
        public bool ChipPowerColdCp { get; private set; } = false;
        public bool ChipPowerWarmCp { get; private set; } = false;
        public bool ChipPowerOffCp { get; private set; } = false;
        public bool MemoryChipProtocolsSiemens4442Cp { get; private set; } = false;
        public bool MemoryChipProtocolsgpm896Cp { get; private set; } = false;
        public bool PositionsExitCp { get; private set; } = false;
        public bool PositionsTransportCp { get; private set; } = false;
        public bool cardTakenSensorCp { get; private set; } = false;

        //overrides
        protected override void UpdateDeviceSpecificStatus(object? payload)
        {
            if (payload == null)
                return;

        }

        protected override void UpdateDeviceSpecificCapabilities(object? payload)
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

        public enum MediaStatusEnum
        {
            unknown,    // - The media state cannot be determined with the device in its current state (e.g.the value of device is noDevice, powerOff, offline or hardwareError.
            present,    // - Media is present in the device, not in the entering position and not jammed.On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
            notPresent, // - Media is not present in the device and not at the entering position.
            jammed,     // - Media is jammed in the device; operator intervention is required.
            entering,   // - Media is at the entry/exit slot of a motorized device.
            latched,    // - Media is present and latched in a latched dip card unit.This means the card can be used for chip card dialog.
        }

        public enum ChipPowerStatusEnum
        {
            unknown,    // - The media state cannot be determined with the device in its current state (e.g.the value of device is noDevice, powerOff, offline or hardwareError.
            present,    // - Media is present in the device, not in the entering position and not jammed.On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
            notPresent, // - Media is not present in the device and not at the entering position.
            jammed,     // - Media is jammed in the device; operator intervention is required.
            entering,   // - Media is at the entry/exit slot of a motorized device.
            latched,    // - Media is present and latched in a latched dip card unit.This means the card can be used for chip card dialog.
        }

        public enum ChipModuleStatusEnum
        {
            ok,         // - The chip card module is in a good state.
            inoperable, // - The chip card module is inoperable.
            unknown,    // - The state of the chip card module cannot be determined.
        }

        public enum CardReaderSecurityStatusEnum
        {
            notReady,   // - The security module is not ready to process cards or is inoperable.
            open,       // - The security module is open and ready to process cards.
        }

        public enum BackImageModuleStatusEnum
        {
            ok,         //- The back card module is in a good state.
            inoperable, // - The back card module is inoperable.
            unknown,    // - The state of the back card module cannot be determined.
        }

        public enum FrontImageModuleStatusEnum
        {
            ok,         //- The front card module is in a good state.
            inoperable, // - The front card module is inoperable.
            unknown,    // - The state of the front card module cannot be determined.
        }

        public enum MagWriteModuleStatusEnum
        {
            ok,         //- The magnetic card writing module is in a good state.
            inoperable, // - The magnetic card writing module is inoperable.
            unknown,    // - The state of the magnetic card writing module cannot be determined.
        }

        public enum CardReaderTypeEnum
        {
            motor,// - The ID card unit is a motor driven card unit.
            swipe,// - The ID card unit is a swipe (pull-through) card unit.
            dip,// - The ID card unit is a dip card unit. This dip type is not capable of latching cards entered.
            latchedDip,// - The ID card unit is a latched dip card unit.This device type is used when a dip card unit device supports chip communication. The latch ensures the consumer cannot remove the card during chip communication. Any card entered will automatically latch when a request to initiate a chip dialog is made (via the CardReader.ReadRawData command). The CardReader.Move command is used to unlatch the card.
            contactless,// - The ID card unit is a contactless card unit, i.e.no insertion of the card is required.
            intelligentContactless,// - The ID card unit is an intelligent contactless card unit, i.e.no insertion of the card is required and the card unit has built-in EMV or smart card application functionality that adheres to the EMVCo Contactless Specifications[Ref.cardreader - 3] or individual payment system's specifications. The ID card unit is capable of performing both magnetic stripe emulation and EMV-like transactions.
            permanent,// - The ID card unit is dedicated to a permanently housed chip card (no user interaction is available with this type of card).
        }

        public enum CardreaderSecurityCpEnum
        {
            mm,     // - The security module is a MMBox.
            cim86,  // - The security module is a CIM86
        }

        public enum PowerOptionCpEnum
        {
            exit,// - The card will be moved to the exit position.
            retain,// - The card will be moved to a retain storage unit.
            exitThenRetain,// - The card will be moved to the exit position for a finite time, then if not taken, the card will be moved to a retain storage unit. The time for which the card remains at the exit position is vendor dependent.
            transport,// - The card will be moved to the transport position.
        }


    }
}
