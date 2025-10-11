using Simulators.Xfs4IoT;
using System.Net.WebSockets;

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

        public override void MessageReceived(Xfs4Message req, Guid socket)
        {

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
            },"cardReader");
        }

        protected async Task CardReadProcessAsync(Guid socket, Xfs4Message req, CancellationToken cmdtkn)
        {
            await Task.Delay(1500); // simulate hardware delay
            var cancel = Task.Run(() => {
                while (!cmdtkn.IsCancellationRequested)
                {
                    cmdtkn.ThrowIfCancellationRequested();
                    Task.Delay(100).Wait();
                }
            });

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
            //return Task.CompletedTask;
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

                //***************** Event Messages*****************
                //CardReader.InsertCardEvent
                //CardReader.MediaInsertedEvent
                //CardReader.InvalidMediaEvent
                //CardReader.TrackDetectedEvent
                _logger.LogInfo("Simulating card read...");
                var readTask = CardReadProcessAsync(socket, req, cmdtkn);
                var cancelTask = Task.Delay(Timeout.Infinite, cmdtkn);

                var finished = await Task.WhenAny(readTask, cancelTask);

                if (finished == cancelTask)
                    throw new OperationCanceledException(cmdtkn);

                await readTask; // completes normally
                _logger.LogInfo("Card read completed.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CardReader] ReadRawData canceled by client.");
                //await SendAsync(socket, msg, "CardReader.ReadRawData", "canceled",
                    //new { message = "Command canceled by user." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CardReader] ReadRawData error: {ex.Message}");
                //await SendAsync(socket, msg, "CardReader.ReadRawData", "internalError",
                    //new { error = ex.Message });
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
}
