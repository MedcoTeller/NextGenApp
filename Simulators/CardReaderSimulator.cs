using Simulators.Xfs4IoT;
using System.Formats.Tar;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simulators.CardReader
{
    public class CardReaderSimulator : BaseSimulator
    {

        public const string InsertCardEvent = "CardReader.InsertCardEvent";
        public const string MediaInsertedEvent = "CardReader.MediaInsertedEvent";
        public const string InvalidMediaEvent = "CardReader.InvalidMediaEvent";
        public const string TrackDetectedEvent = "CardReader.TrackDetectedEvent";
        public const string EMVClessReadStatusEvent = "CardReader.EMVClessReadStatusEvent";
        public const string MediaRemovedEvent = "CardReader.MediaRemovedEvent";
        public const string CardActionEvent = "CardReader.CardActionEvent";

        public CardReaderSimulator()
        {

        }

        public CardReaderSimulator(string url, string deviceName, string serviceName) : base(url, "CardReder", serviceName)
        {
            MediaStatus = MediaStatusEnum.unknown;
            DeviceStatus = DeviceStatusEnum.noDevice;
        }

        // Device status properties (get only, set private)
        [JsonInclude] public MediaStatusEnum MediaStatus { get; private set; } = MediaStatusEnum.notPresent;
        [JsonInclude] public SecurityStatusEnum? SecurityStatus { get; private set; } = SecurityStatusEnum.notReady;
        [JsonInclude] public ChipPowerStatusEnum? ChipPowerStatus { get; private set; } = ChipPowerStatusEnum.noCard;
        [JsonInclude] public ChipModuleStatusEnum? ChipModuleStatus { get; private set; } = ChipModuleStatusEnum.ok;
        [JsonInclude] public MagWriteModuleStatusEnum? MagWriteModuleStatus { get; private set; } = MagWriteModuleStatusEnum.unknown;
        [JsonInclude] public FrontImageModuleStatusEnum? FrontImageModuleStatus { get; private set; } = FrontImageModuleStatusEnum.unknown;
        [JsonInclude] public string BackImageModuleStatus { get; private set; } = "unknown";


        // Add these properties to CardReaderSimulator (set protected, get public)
        [JsonInclude] public bool Track1Cp { get; protected set; } = false;
        [JsonInclude] public bool Track2Cp { get; protected set; } = false;
        [JsonInclude] public bool Track3Cp { get; protected set; } = false;
        [JsonInclude] public bool WatermarkCp { get; protected set; } = false;
        [JsonInclude] public bool FrontImageCp { get; protected set; } = false;
        [JsonInclude] public bool BackImageCp { get; protected set; } = false;
        [JsonInclude] public bool Track1JISCp { get; protected set; } = false;
        [JsonInclude] public bool Track3JISCp { get; protected set; } = false;
        [JsonInclude] public bool DdiCp { get; protected set; } = false;
        [JsonInclude] public bool ChipT0Cp { get; protected set; } = false;
        [JsonInclude] public bool ChipT1Cp { get; protected set; } = false;
        [JsonInclude] public bool ChipProtocolNotRequiredCp { get; protected set; } = false;
        [JsonInclude] public bool ChipTypeAPart3Cp { get; protected set; } = false;
        [JsonInclude] public bool ChipTypeAPart4Cp { get; protected set; } = false;
        [JsonInclude] public bool ChipTypeBCp { get; protected set; } = false;
        [JsonInclude] public bool ChipTypeNFCCp { get; protected set; } = false;
        [JsonInclude] public SecurityTypeEnum? SecurityTypeCp { get; protected set; } = SecurityTypeEnum.mm;
        [JsonInclude] public string PowerOnOptionCp { get; protected set; } = "exit";
        [JsonInclude] public string PowerOffOptionCp { get; protected set; } = "exit";
        [JsonInclude] public bool FluxSensorProgrammableCp { get; protected set; } = false;
        [JsonInclude] public bool ReadWriteAccessFromExitCp { get; protected set; } = false;
        [JsonInclude] public bool LocoCp { get; protected set; } = false;
        [JsonInclude] public bool HicoCp { get; protected set; } = false;
        [JsonInclude] public bool AutoCp { get; protected set; } = false;
        [JsonInclude] public bool ColdCp { get; protected set; } = false;
        [JsonInclude] public bool WarmCp { get; protected set; } = false;
        [JsonInclude] public bool OffCp { get; protected set; } = false;
        [JsonInclude] public bool Siemens4442Cp { get; protected set; } = false;
        [JsonInclude] public bool Gpm896Cp { get; protected set; } = false;
        [JsonInclude] public bool ExitCp { get; protected set; } = false;
        [JsonInclude] public bool TransportCp { get; protected set; } = false;
        [JsonInclude] public bool CardTakenSensorCp { get; protected set; } = false;
        [JsonInclude] public CardReaderTypeEnum? CardReaderTypeCp { get; private set; } = CardReaderTypeEnum.motor;

        public List<CardData> ConfiguredCards { get; set; } = new() { new CardData() { Track1 = "test tck1", ChipData = "Chip data test", Track2 = "test trk2" } };
        // Simulated properties to control card insertion and removal
        public bool CardInserted { get; set; }
        public bool CardTaken { get; set; }

        public List<string> BreakPoint = new();

        public int CurrentCardIndex { get; set; } = 0;

        public override void ClientConnected()
        {

        }

        public override void MessageReceived(Xfs4Message req, Guid socket)
        {

        }

        public override async Task RefreshConfig()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var json = File.ReadAllText(@$"C:\ProgramData\NextGen\Simulators\Device Configurations\Cardreader.json");
            var obj = JsonSerializer.Deserialize<CardReaderSimulator>(json, jsonOptions);
            if (obj != null)
            {
                this.SecureConnection = obj.SecureConnection;
                this.Track1Cp = obj.Track1Cp;
                this.Track2Cp = obj.Track2Cp;
                this.Track3Cp = obj.Track3Cp;
                this.WatermarkCp = obj.WatermarkCp;
                this.FrontImageCp = obj.FrontImageCp;
                this.BackImageCp = obj.BackImageCp;
                this.Track1JISCp = obj.Track1JISCp;
                this.Track3JISCp = obj.Track3JISCp;
                this.DdiCp = obj.DdiCp;
                this.ChipT0Cp = obj.ChipT0Cp;
                this.ChipT1Cp = obj.ChipT1Cp;
                this.ChipProtocolNotRequiredCp = obj.ChipProtocolNotRequiredCp;
                this.ChipTypeAPart3Cp = obj.ChipTypeAPart3Cp;
                this.ChipTypeAPart4Cp = obj.ChipTypeAPart4Cp;
                this.ChipTypeBCp = obj.ChipTypeBCp;
                this.ChipTypeNFCCp = obj.ChipTypeNFCCp;
                this.SecurityTypeCp = obj.SecurityTypeCp;
                this.PowerOnOptionCp = obj.PowerOnOptionCp;
                this.PowerOffOptionCp = obj.PowerOffOptionCp;
                this.FluxSensorProgrammableCp = obj.FluxSensorProgrammableCp;
                this.ReadWriteAccessFromExitCp = obj.ReadWriteAccessFromExitCp;
                this.LocoCp = obj.LocoCp;
                this.HicoCp = obj.HicoCp;
                this.AutoCp = obj.AutoCp;
                this.ColdCp = obj.ColdCp;
                this.WarmCp = obj.WarmCp;
                this.OffCp = obj.OffCp;
                this.Siemens4442Cp = obj.Siemens4442Cp;
                this.Gpm896Cp = obj.Gpm896Cp;
                this.ExitCp = obj.ExitCp;
                this.TransportCp = obj.TransportCp;
                this.CardTakenSensorCp = obj.CardTakenSensorCp;
                this.CardReaderTypeCp = obj.CardReaderTypeCp;
                this.ConfiguredCards = obj.ConfiguredCards;
                this.CurrentCardIndex = obj.CurrentCardIndex;
                this.Port = obj.Port;
                this.HostName = obj.HostName;
                this.ModelName = obj.ModelName;
                this.DeviceStatus = obj.DeviceStatus;
                this.MediaStatus = obj.MediaStatus;
                this.SecurityStatus = obj.SecurityStatus;
                this.ChipPowerStatus = obj.ChipPowerStatus;
                this.ChipModuleStatus = obj.ChipModuleStatus;
                this.MagWriteModuleStatus = obj.MagWriteModuleStatus;
                this.FrontImageModuleStatus = obj.FrontImageModuleStatus;
                this.BackImageModuleStatus = obj.BackImageModuleStatus;
                this.CardInserted = obj.CardInserted;
                this.CardTaken = obj.CardTaken;
                this.BreakPoint = obj.BreakPoint;
                this.ServiceName = obj.ServiceName;
                this.DeviceName = obj.DeviceName;
                this.AntiFraudModuleStatus = obj.AntiFraudModuleStatus;
                this.ExchangeStatus = obj.ExchangeStatus;
                this.EndToEndSecurityStatus = obj.EndToEndSecurityStatus;
                this.PowerSaveRecoveryTime = obj.PowerSaveRecoveryTime;


            }
        }

        public override void RegisterDeviceCommandHandlers()
        {
            RegisterCommandHandler("CardReader.ReadRawData", ReadCard);
            RegisterCommandHandler("CardReader.Reset", Reset);
            RegisterCommandHandler("CardReader.QueryIFMIdentifier", QueryIFMIdentifier);
            RegisterCommandHandler("CardReader.EMVClessQueryApplications", EMVClessQueryApplications);
            RegisterCommandHandler("CardReader.WriteRawData", WriteRawData);
            RegisterCommandHandler("CardReader.Move", Move);
            RegisterCommandHandler("CardReader.SetKey", SetKey);
            RegisterCommandHandler("CardReader.ChipIO", ChipIO);
            RegisterCommandHandler("CardReader.ChipPower", ChipPower);
            RegisterCommandHandler("CardReader.EMVClessConfigure", EMVClessConfigure);
            RegisterCommandHandler("CardReader.EMVClessPerformTransaction", EMVClessPerformTransaction);
            RegisterCommandHandler("CardReader.EMVClessIssuerUpdate", EMVClessIssuerUpdate);
        }

        private async Task EMVClessIssuerUpdate(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "data": "O2gAUACFyEARAJAC"
            //}

            //***************** Completion Message *****************
            //Payload, Version 3.0
            //{ 
            //  "errorCode": "noMedia",
            //  "chip": {
            //    "txOutcome": "multipleCards",
            //    "dataRead": "O2gAUACFyEARAJAC",
            //    "clessOutcome": {
            //      "cvm": "onlinePIN",
            //      "alternateInterface": "magneticStripe",
            //      "receipt": false,
            //      "uiOutcome": {
            //        "messageId": 0,
            //        "status": "notReady",
            //        "holdTime": 0,
            //        "valueDetails": {
            //          "qualifier": "amount",
            //          "value": "000000012345",
            //          "currencyCode": 826
            //        },
            //        "languagePreferenceData": "en"
            //      },
            //      "uiRestart": See chip/clessOutcome/uiOutcome properties
            //      "fieldOffHoldTime": 0,
            //      "cardRemovalTimeout": 0,
            //      "discretionaryData": "O2gAUACFyEARAJAC"
            //    }
            //  }
            //}

            //***************** Event Messages *****************
            //CardReader.EMVClessReadStatusEvent

            throw new NotImplementedException();
        }

        private async Task EMVClessPerformTransaction(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "data": "O2gAUACFyEARAJAC"
            //}

            //***************** Completion Message *****************
            //Payload, Version 3.0
            //{ 
            //  "errorCode": "noMedia",
            //  "chip": {
            //    "txOutcome": "multipleCards",
            //    "cardholderAction": "none",
            //    "dataRead": "O2gAUACFyEARAJAC",
            //    "clessOutcome": {
            //      "cvm": "onlinePIN",
            //      "alternateInterface": "magneticStripe",
            //      "receipt": false,
            //      "uiOutcome": {
            //        "messageId": 0,
            //        "status": "notReady",
            //        "holdTime": 0,
            //        "valueDetails": {
            //          "qualifier": "amount",
            //          "value": "000000012345",
            //          "currencyCode": 826
            //        },
            //        "languagePreferenceData": "en"
            //      },
            //      "uiRestart": See chip/clessOutcome/uiOutcome properties
            //      "fieldOffHoldTime": 0,
            //      "cardRemovalTimeout": 0,
            //      "discretionaryData": "O2gAUACFyEARAJAC"
            //    }
            //  },
            //  "track1": See chip properties
            //  "track2": See chip properties
            //  "track3": See chip properties
            //}

            //***************** Event Messages *****************
            //CardReader.EMVClessReadStatusEvent

            throw new NotImplementedException();
        }

        private async Task EMVClessConfigure(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0, Required
            //{ 
            //  "terminalData": "O2gAUACFyEARAJAC",
            //  "aidData": [{
            //    "aid": "O2gAUACFyEARAJAC",
            //    "partialSelection": false,
            //    "transactionType": 0,
            //    "kernelIdentifier": "O2gAUACFyEARAJAC",
            //    "configData": "O2gAUACFyEARAJAC"
            //  }],
            //  "keyData": [{
            //    "rid": "O2gAUACFyEARAJAC",
            //    "caPublicKey": {
            //      "index": 0,
            //      "algorithmIndicator": 0,
            //      "exponent": "O2gAUACFyEARAJAC",
            //      "modulus": "O2gAUACFyEARAJAC",
            //      "checksum": "O2gAUACFyEARAJAC"
            //    }
            //  }]
            //}
            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "invalidTerminalData"
            //}

            //***************** Event Messages *****************
            //none

            throw new NotImplementedException();
        }

        private async Task ChipPower(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "chipPower": "cold"
            //}

            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "chipPowerNotSupported",
            //  "chipData": "O2gAUACFyEARAJAC"
            //}

            //***************** Event Messages *****************
            //none
            throw new NotImplementedException();
        }

        private async Task ChipIO(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "chipProtocol": "chipT1",
            //  "chipData": "O2gAUACFyEARAJAC"
            //}

            //***************** Completion Message *****************
            //Payload, Version 3.0
            //{ 
            //  "errorCode": "mediaJam"
            //  "chipProtocol": "chipT1",
            //  "chipData": "O2gAUACFyEARAJAC"
            //}
            //}

            //***************** Event Messages *****************
            //none
            throw new NotImplementedException();
        }

        private async Task SetKey(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "keyValue": "O2gAUACFyEARAJAC"
            //}

            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "mediaJam"
            //}

            //***************** Event Messages *****************
            //none
            throw new NotImplementedException();
        }

        private async Task Move(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "from": "unit1",
            //  "to": "exit"
            //}

            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "mediaJam"
            //}

            //***************** Event Messages *****************
            //none
            throw new NotImplementedException();
        }

        private async Task WriteRawData(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "track1": {
            //    "data": "O2gAUACFyEARAJAC",
            //    "writeMethod": "loco"
            //  },
            //  "track2": See track1 properties
            //  "track3": See track1 properties
            //  "track1Front": See track1 properties
            //  "track1JIS": See track1 properties
            //  "track3JIS": See track1 properties
            //  "additionalProperties": See track1 properties
            //}

            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "mediaJam"
            //}

            //***************** Event Messages *****************
            //CardReader.InsertCardEvent
            //CardReader.MediaInsertedEvent
            //CardReader.InvalidMediaEvent
            throw new NotImplementedException();
        }

        private async Task EMVClessQueryApplications(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Completion Message *****************
            //Payload, Version 3.0
            //{
            //  "appData": [{
            //     "aid": "O2gAUACFyEARAJAC",
            //    "kernelIdentifier": "O2gAUACFyEARAJAC"
            //  }]
            //}
            throw new NotImplementedException();
        }

        private async Task QueryIFMIdentifier(Guid guid, Xfs4Message message, CancellationToken token)
        {
            //*************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "ifmIdentifiers": {
            //    "emv": "Example IFM Identifier",
            //    "europay": See ifmIdentifiers/emv
            //  }
            //}
            //emv - The Level 1 Type Approval IFM identifier assigned by EMVCo.
            //europay - The Level 1 Type Approval IFM identifier assigned by Europay.
            //visa - The Level 1 Type Approval IFM identifier assigned by VISA.
            //giecb - The IFM identifier assigned by GIE Cartes Bancaires.
            throw new NotImplementedException();
        }

        protected override void DeviceGetInterfaceInfo(Guid socket, Xfs4Message message, CancellationToken token)
        {

        }

        protected override void DeviceSetVersions(Guid socket, Xfs4Message message, CancellationToken token)
        {

        }

        // Replace the anonymous object in GetDeviceCapabilitiesPart with the properties
        protected override (object, string) GetDeviceCapabilitiesPart()
        {
            return (new
            {
                type = CardReaderTypeCp?.ToString(),
                readTracks = new
                {
                    track1 = Track1Cp,
                    track2 = Track2Cp,
                    track3 = Track3Cp,
                    watermark = WatermarkCp,
                    frontImage = FrontImageCp,
                    backImage = BackImageCp,
                    track1JIS = Track1JISCp,
                    track3JIS = Track3JISCp,
                    ddi = DdiCp
                },
                writeTracks = new
                {
                    track1 = Track1Cp,
                    track2 = Track2Cp,
                    track3 = Track3Cp,
                    watermark = WatermarkCp,
                    frontImage = FrontImageCp,
                    track1JIS = Track1JISCp,
                    track3JIS = Track3JISCp,
                },
                chipProtocols = new
                {
                    chipT0 = ChipT0Cp,
                    chipT1 = ChipT1Cp,
                    chipProtocolNotRequired = ChipProtocolNotRequiredCp,
                    chipTypeAPart3 = ChipTypeAPart3Cp,
                    chipTypeAPart4 = ChipTypeAPart4Cp,
                    chipTypeB = ChipTypeBCp,
                    chipTypeNFC = ChipTypeNFCCp
                },
                securityType = SecurityTypeCp?.ToString(),
                powerOnOption = PowerOnOptionCp,
                powerOffOption = PowerOffOptionCp,
                fluxSensorProgrammable = FluxSensorProgrammableCp,
                readWriteAccessFromExit = ReadWriteAccessFromExitCp,
                writeMode = new
                {
                    loco = LocoCp,
                    hico = HicoCp,
                    auto = AutoCp
                },
                chipPower = new
                {
                    cold = ColdCp,
                    warm = WarmCp,
                    off = OffCp
                },
                memoryChipProtocols = new
                {
                    siemens4442 = Siemens4442Cp,
                    gpm896 = Gpm896Cp
                },
                positions = new
                {
                    exit = ExitCp,
                    transport = TransportCp
                },
                cardTakenSensor = CardTakenSensorCp
            }, "cardReader");
        }

        protected override (object, string) GetDeviceStatusPart()
        {
            return (new
            {
                media = MediaStatus.ToString(), // Enum value as string, case-sensitive
                security = SecurityStatus?.ToString(),
                chipPower = ChipPowerStatus?.ToString(),
                chipModule = ChipModuleStatus?.ToString(),
                magWriteModule = MagWriteModuleStatus?.ToString(),
                frontImageModule = FrontImageModuleStatus?.ToString(),
                backImageModule = BackImageModuleStatus
            }, "cardReader");
        }

        private Task CancelationCheckAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        Task.Delay(100).Wait();
                    }
                    catch (Exception)
                    {
                        _logger.LogError("Operation canceled.");
                    }
                }
            });
        }

        protected async Task EnableCardReaderAndWaitForCard(Guid socket, Xfs4Message req, CancellationToken cmdtkn)
        {
            //wait for card to be inserted
            _logger.LogInfo("Waiting for card to be inserted...");
            var insertEvent = new Xfs4Message
            {
                Header = new Xfs4Header
                {
                    Name = InsertCardEvent,
                    Type = MessageType.Event,
                    RequestId = req.Header.RequestId
                },
                Payload = new { }
            };
            await SendAsync(socket, insertEvent, cmdtkn);
            OnStatusChange?.Invoke("ReaderEnabled");

            while (!CardInserted)
            {
                await Task.Delay(100);
            }
            MediaStatus = MediaStatusEnum.entering;
            OnStatusChange?.Invoke("CardInserted");
            await Task.Delay(500);
            MediaStatus = MediaStatusEnum.present;
            OnStatusChange?.Invoke("CardInserted");
            _logger.LogInfo("Card Inserted.");
        }

        private async Task<CardData> ProcessCardData()
        {
            //will need to get simulated card data from configuration
            CardData cardData = new CardData
            {
                Track1 = "B1234567890123456^CARD/USER^25121010000000000000?",
                Track2 = "1234567890123456=25121010000000000000?",
                Track3 = "12345678901234567890=25121010000000000000?",
                ChipData = "O2gAUACFyEARAJAC"
            };

            return cardData;
            //throw new NotImplementedException();
        }

        private async Task ReadCard(Guid socket, Xfs4Message req, CancellationToken cmdtkn)
        {
            try
            {
                //*************** Command Message *****************
                //Payload, Version 2.0
                //{ 
                //  "track1": false,
                //  "track2": false,
                //  "track3": false,
                //  "chip": false,
                //  "security": false,
                //  "fluxInactive": false,
                //  "watermark": false,
                //  "memoryChip": false,
                //  "track1Front": false,
                //  "frontImage": false,
                //  "backImage": false,
                //  "track1JIS": false,
                //  "track3JIS": false,
                //  "ddi": false
                //}
                _logger.LogInfo("Card read...");
                var timeout = req.Header.Timeout;
                if (timeout is null || timeout.Value < 0)
                {
                    //reject command

                }

                var readTask = EnableCardReaderAndWaitForCard(socket, req, cmdtkn);
                var cancelReadTask = Task.Delay(Timeout.Infinite, cmdtkn);
                var timeoutTsk = Task.Delay(timeout.Value == 0 ? Timeout.Infinite : timeout.Value);

                var finished = await Task.WhenAny(readTask, cancelReadTask, timeoutTsk);

                switch (finished)
                {
                    case var t when t == timeoutTsk:
                        _logger.LogWarning("[CardReader] ReadRawData timed out.");
                        //send timeout completion
                        var timeoutCompletion = new Xfs4Message
                        {
                            Header = new Xfs4Header
                            {
                                Name = req.Header.Name,
                                Type = MessageType.Completion,
                                RequestId = req.Header.RequestId,
                                CompletionCode = CompletionCodeEnum.timeOut,
                                ErrorDiscription = "ReadCard command timed out."
                            },
                            Payload = new
                            {
                                errorCode = "timeOut"
                            }
                        };
                        await SendAsync(socket, timeoutCompletion, _cts.Token);
                        return;
                    case var t when t == cancelReadTask:
                        //send cancel completion
                        _logger.LogInfo("[CardReader] ReadRawData canceled.");
                        var canceleCompletion = new Xfs4Message
                        {
                            Header = new Xfs4Header
                            {
                                Name = req.Header.Name,
                                Type = MessageType.Completion,
                                RequestId = req.Header.RequestId,
                                CompletionCode = CompletionCodeEnum.canceled,
                                ErrorDiscription = "Command canceled"
                            },
                            Payload = new
                            {
                                errorCode = "Command canceled"
                            }
                        };
                        await SendAsync(socket, canceleCompletion, CancellationToken.None);
                        return;
                }

                await readTask; // completes normally
                var insertedEvent = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = MediaInsertedEvent,
                        Type = MessageType.Event
                    },
                    Payload = new { }
                };
                await SendAsync(socket, insertedEvent, _cts.Token);

                var crdData = await ProcessCardData();

                bool invalidCard = false; //determine from card data
                if (crdData == null)
                {
                    invalidCard = true;
                }

                //***************** Completion Message *****************
                //Payload, Version 3.0
                //{ 
                //  "errorCode": "mediaJam",
                //  "track1": {
                //    "status": "dataMissing",
                //    "data": "O2gAUACFyEARAJAC"
                //  },
                //  "track2": See track1 properties
                //  "track3": See track1 properties
                //  "chip": [{
                //    "status": See track1/status,
                //    "data": See track1/data
                //  }],
                //  "security": {
                //    "status": See track1/status,
                //    "data": "readLevel1"
                //  },
                //  "watermark": See track1 properties
                //  "memoryChip": {
                //    "status": See track1/status,
                //    "protocol": "chipT0",
                //    "data": "O2gAUACFyEARAJAC"
                //  },
                //  "track1Front": See track1 properties
                //  "frontImage": "O2gAUACFyEARAJAC",
                //  "backImage": "O2gAUACFyEARAJAC",
                //  "track1JIS": See track1 properties
                //  "track3JIS": See track1 properties
                //  "ddi": See track1 properties
                //}

                /*
                 errorCode:
                    mediaJam - The card is jammed. Operator intervention is required.
                    shutterFail - The open of the shutter failed due to manipulation or hardware error. Operator intervention is required.
                    noMedia - The card was removed before completion of the read action (the event CardReader.MediaInsertedEvent has been generated). For motor driven devices, the read is disabled; i.e. another command has to be issued to enable the reader for card entry.
                    invalidMedia - No track or chip found; card may have been inserted or pulled through the wrong way.
                    cardTooShort - The card that was inserted is too short. When this error occurs the card remains at the exit slot.
                    cardTooLong - The card that was inserted is too long. When this error occurs the card remains at the exit slot.
                    securityFail - The security module failed reading the card's security and no other data source was requested.
                    cardCollision - There was an unresolved collision of two or more contactless card signals.                 
                 */

                if (invalidCard)
                {
                    var invalidEvent = new Xfs4Message
                    {
                        Header = new Xfs4Header
                        {
                            Name = InvalidMediaEvent,
                            Type = MessageType.Event
                        },
                        Payload = new { }
                    };
                    await SendAsync(socket, invalidEvent, _cts.Token);

                    _logger.LogWarning("Invalid card inserted.");
                    //send completion with invalid media error
                    var completionError = new Xfs4Message
                    {
                        Header = new Xfs4Header
                        {
                            Name = "CardReader.ReadCard",
                            Type = MessageType.Completion,
                            RequestId = req.Header.RequestId
                        },
                        Payload = new
                        {
                            errorCode = CardReaderErrorCode.invalidMedia
                        }
                    };

                    await SendAsync(socket, completionError, _cts.Token);
                    _logger.LogInfo("Card read completed. >> InvalidCard");
                    return;
                }

                _logger.LogInfo("Check detected traks.");
                var tracksDetectedEvent = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = TrackDetectedEvent,
                        Type = MessageType.Event
                    },
                    Payload = new
                    {
                        track1 = crdData.Track1 != null,
                        track2 = crdData.Track2 != null,
                        track3 = crdData.Track3 != null,
                        watermark = false,
                        chip = crdData.ChipData != null
                    }
                };
                await SendAsync(socket, tracksDetectedEvent, _cts.Token);


                var completion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = req.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = req.Header.RequestId
                    },
                    Payload = new
                    {
                        track1 = crdData.Track1 == null ? null : new
                        {
                            data = crdData.Track1
                        },
                        track2 = crdData.Track2 == null ? null : new
                        {
                            data = crdData.Track2
                        },
                        track3 = crdData.Track3 == null ? null : new
                        {
                            data = crdData.Track3
                        },
                        chip = crdData.ChipData == null ? null : new
                        {
                            data = crdData.ChipData
                        }
                    }
                };
                await SendAsync(socket, completion, _cts.Token);

                //***************** Event Messages*****************
                //CardReader.InsertCardEvent
                //CardReader.MediaInsertedEvent
                //CardReader.InvalidMediaEvent
                //CardReader.TrackDetectedEvent
                //CardReader.EMVClessReadStatusEvent

                //**************** Unsolicited Messages ************
                //CardReader.MediaRemovedEvent
                //CardReader.CardActionEvent
                //CardReader.MediaDetectedEvent

                _logger.LogInfo("Card read completed.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CardReader] ReadRawData canceled by client.");
                var timeoutCompletion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = req.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = req.Header.RequestId,
                        CompletionCode = CompletionCodeEnum.timeOut,
                        ErrorDiscription = "ReadCard command timed out."
                    },
                    Payload = new
                    {
                        errorCode = "timeOut"
                    }
                };
                await SendAsync(socket, timeoutCompletion, _cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CardReader] ReadRawData error: {ex.Message} - {ex.StackTrace}");
                var errorCompletion = new Xfs4Message
                {
                    Header = new Xfs4Header
                    {
                        Name = req.Header.Name,
                        Type = MessageType.Completion,
                        RequestId = req.Header.RequestId,
                        CompletionCode = CompletionCodeEnum.internalError,
                        ErrorDiscription = $"{ex.Message}"
                    },
                    Payload = new
                    {
                        errorCode = "timeOut"
                    }
                };
                await SendAsync(socket, errorCompletion, _cts.Token);
            }
        }

        private async Task Reset(Guid socket, Xfs4Message req, CancellationToken cmdtkn)
        {
            //*************** Command Message *****************
            //Payload, Version 3.0
            //{ 
            //  "to": "unit1",
            //  "storageId": "exit"
            //}

            //***************** Completion Message *****************
            //Payload, Version 2.0
            //{ 
            //  "errorCode": "mediaJam"
            //}

            //***************** Event Messages *****************
            //none
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

    public class CardData
    {
        public string? Track1 { get; set; } = null;
        public string? Track2 { get; set; } = null;
        public string? Track3 { get; set; } = null;
        public string? ChipData { get; set; } = null;
    }
}
