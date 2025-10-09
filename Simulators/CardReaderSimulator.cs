using Simulators.Xfs4IoT;
using System.Net.WebSockets;

namespace Simulators.CardReader
{    
    public class CardReaderSimulator : BaseSimulator
    {
        public CardReaderSimulator(string url, string deviceName, string serviceName) : base(url, "CardReder", serviceName)
        {
        }

        // Device status properties (get only, set private)
        public MediaStatusEnum MediaStatus { get; private set; } = MediaStatusEnum.unknown;
        public SecurityStatusEnum? SecurityStatus { get; private set; } = SecurityStatusEnum.notReady;
        public ChipPowerStatusEnum? ChipPowerStatus { get; private set; } = ChipPowerStatusEnum.unknown;
        public ChipModuleStatusEnum? ChipModuleStatus { get; private set; } = ChipModuleStatusEnum.ok;
        public MagWriteModuleStatusEnum? MagWriteModuleStatus { get; private set; } = MagWriteModuleStatusEnum.ok;
        public FrontImageModuleStatusEnum? FrontImageModuleStatus { get; private set; } = FrontImageModuleStatusEnum.ok;
        public string BackImageModuleStatus { get; private set; } = "ok";


        // Add these properties to CardReaderSimulator (set protected, get public)
        public bool Track1 { get; protected set; } = false;
        public bool Track2 { get; protected set; } = false;
        public bool Track3 { get; protected set; } = false;
        public bool Watermark { get; protected set; } = false;
        public bool FrontImage { get; protected set; } = false;
        public bool BackImage { get; protected set; } = false;
        public bool Track1JIS { get; protected set; } = false;
        public bool Track3JIS { get; protected set; } = false;
        public bool Ddi { get; protected set; } = false;
        public bool ChipT0 { get; protected set; } = false;
        public bool ChipT1 { get; protected set; } = false;
        public bool ChipProtocolNotRequired { get; protected set; } = false;
        public bool ChipTypeAPart3 { get; protected set; } = false;
        public bool ChipTypeAPart4 { get; protected set; } = false;
        public bool ChipTypeB { get; protected set; } = false;
        public bool ChipTypeNFC { get; protected set; } = false;
        public SecurityTypeEnum? SecurityType { get; protected set; } = SecurityTypeEnum.mm;
        public string PowerOnOption { get; protected set; } = "exit";
        public string PowerOffOption { get; protected set; } = "exit";
        public bool FluxSensorProgrammable { get; protected set; } = false;
        public bool ReadWriteAccessFromExit { get; protected set; } = false;
        public bool Loco { get; protected set; } = false;
        public bool Hico { get; protected set; } = false;
        public bool Auto { get; protected set; } = false;
        public bool Cold { get; protected set; } = false;
        public bool Warm { get; protected set; } = false;
        public bool Off { get; protected set; } = false;
        public bool Siemens4442 { get; protected set; } = false;
        public bool Gpm896 { get; protected set; } = false;
        public bool Exit { get; protected set; } = false;
        public bool Transport { get; protected set; } = false;
        public bool CardTakenSensor { get; protected set; } = false;
        public CardReaderTypeEnum? CardReaderTypeCp { get; private set; } = CardReaderTypeEnum.motor;

        public override void ClientConnected()
        {

        }

        public override void MessageReceived(Xfs4Message req, WebSocket socket)
        {

        }

        public override void RegisterDeviceCommandHandlers()
        {
            RegisterCommandHandler("CardReader.ReadCard", ReadCard);
            RegisterCommandHandler("CardReader.Reset", Reset);
        }
        protected override void DeviceGetInterfaceInfo(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            
        }

        protected override void DeviceSetVersions(WebSocket socket, Xfs4Message message, CancellationToken token)
        {
            
        }

        // Replace the anonymous object in GetDeviceCapabilitiesPart with the properties
        protected override object GetDeviceCapabilitiesPart()
        {
            return new
            {
                type = CardReaderTypeCp?.ToString(),
                readTracks = new
                {
                    track1 = Track1,
                    track2 = Track2,
                    track3 = Track3,
                    watermark = Watermark,
                    frontImage = FrontImage,
                    backImage = BackImage,
                    track1JIS = Track1JIS,
                    track3JIS = Track3JIS,
                    ddi = Ddi
                },
                writeTracks = new
                {
                    track1 = Track1,
                    track2 = Track2,
                    track3 = Track3,
                    watermark = Watermark,
                    frontImage = FrontImage,
                    track1JIS = Track1JIS,
                    track3JIS = Track3JIS,
                },
                chipProtocols = new
                {
                    chipT0 = ChipT0,
                    chipT1 = ChipT1,
                    chipProtocolNotRequired = ChipProtocolNotRequired,
                    chipTypeAPart3 = ChipTypeAPart3,
                    chipTypeAPart4 = ChipTypeAPart4,
                    chipTypeB = ChipTypeB,
                    chipTypeNFC = ChipTypeNFC
                },
                securityType = SecurityType?.ToString(),
                powerOnOption = PowerOnOption,
                powerOffOption = PowerOffOption,
                fluxSensorProgrammable = FluxSensorProgrammable,
                readWriteAccessFromExit = ReadWriteAccessFromExit,
                writeMode = new
                {
                    loco = Loco,
                    hico = Hico,
                    auto = Auto
                },
                chipPower = new
                {
                    cold = Cold,
                    warm = Warm,
                    off = Off
                },
                memoryChipProtocols = new
                {
                    siemens4442 = Siemens4442,
                    gpm896 = Gpm896
                },
                positions = new
                {
                    exit = Exit,
                    transport = Transport
                },
                cardTakenSensor = CardTakenSensor
            };
        }

        protected override object GetDeviceStatusPart()
        {
            return new
            {
                media = MediaStatus.ToString(), // Enum value as string, case-sensitive
                security = SecurityStatus?.ToString(),
                chipPower = ChipPowerStatus?.ToString(),
                chipModule = ChipModuleStatus?.ToString(),
                magWriteModule = MagWriteModuleStatus?.ToString(),
                frontImageModule = FrontImageModuleStatus?.ToString(),
                backImageModule = BackImageModuleStatus
            };
        }



        private async Task ReadCard(WebSocket socket, Xfs4Message req, CancellationToken cmdtkn)
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


        private async Task Reset(System.Net.WebSockets.WebSocket socket, Xfs4Message req, CancellationToken cmdtkn)
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
