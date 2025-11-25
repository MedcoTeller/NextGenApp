using System;
using System.Collections.Generic;
using System.Text;

namespace Devices.Common
{
    public class InternalException : Exception
    {
        public InternalException(string message) : base(message) { }
        public InternalException(string message, Exception inner) : base(message, inner) { }
        public InternalException(ExceptionType type, string message) : base(message)
        {
            Type = type;
        }

        public bool Reboot { get; set; }
        public ExceptionType Type { get; set; } = ExceptionType.None;
        public enum ExceptionType
        {
            None,
            NoServicesFoundEx,
            NoAcknolgedReceivedEx,
            CommandTimedOutEx,
            WrongCommandTypeEx,
            InvalidAcknowlege,
            InvalidEvent,
            AssurtionFailure
        }
    }

    public class NoServicesFoundEx : InternalException
    {
        public NoServicesFoundEx(string message) : base(message) { Reboot = true; }
        public NoServicesFoundEx(string message, Exception inner) : base(message, inner) { Reboot = true; }
    }

    public class NoAcknolgedReceivedEx : InternalException
    {
        public NoAcknolgedReceivedEx(string message) : base(message) { Reboot = true; }
        public NoAcknolgedReceivedEx(string message, Exception inner) : base(message, inner) { Reboot = true; }
    }

    public class CommandTimedOutEx : InternalException
    {
        public CommandTimedOutEx(string message) : base(message) { }
        public CommandTimedOutEx(string message, Exception inner) : base(message, inner) { }
    }

    public class WrongCommandTypeEx : InternalException
    {
        public WrongCommandTypeEx(string message) : base(message) { Reboot = true; }
        public WrongCommandTypeEx(string message, Exception inner) : base(message, inner) { Reboot = true; }
    }

    //#################### Common constants ####################
    //Commands:
    public class CommonCommands
    {
        public const string Common_Cancel = "Common.Cancel";
        public const string Common_Status = "Common.Status";
        public const string Common_Capabilities = "Common.Capabilities";
        public const string Common_GetInterfaceInfo = "Common.GetInterfaceInfo";
        public const string Common_SetVersions = "Common.SetVersions";
        public const string Common_SetTransactionState = "Common.SetTransactionState";
        public const string Common_GetTransactionState = "Common.GetTransactionState";
        public const string Common_GetCommandNonce = "Common.GetCommandNonce";
        public const string Common_ClearCommandNonce = "Common.ClearCommandNonce";
        public const string Common_StartSecureOperation = "Common.StartSecureOperation";
        public const string Common_SGetInterfaceInfo = "Common.GetInterfaceInfo";
    }

    //Events:
    public class CommonEvents
    {
        //unsolic
        public const string Common_StatusChangedEvent_Unsolic = "Common.StatusChangedEvent";
        public const string Common_ErrorEvent_Unsolic = "Common.ErrorEvent";
        public const string Common_NonceClearedEvent_Unsolic = "Common.NonceClearedEvent";
    }

    //#################### Card Reader constants ####################
    //commands:
    public class CardReaderCommands
    {
        public const string CardReader_QueryIFMIdentifier = "CardReader.QueryIFMIdentifier";
        public const string CardReader_EMVClessQueryApplications = "CardReader.EMVClessQueryApplications";
        public const string CardReader_ReadRawData = "CardReader.ReadRawData";
        public const string CardReader_WriteRawData = "CardReader.WriteRawData";
        public const string CardReader_Move = "CardReader.Move";
        public const string CardReader_SetKey = "CardReader.SetKey";
        public const string CardReader_ChioIO = "CardReader.ChioIO";
        public const string CardReader_ChipPower = "CardReader.ChipPower";
        public const string CardReader_EMVClessConfigure = "CardReader.EMVClessConfigure";
        public const string CardReader_EMVClessPerformTransaction = "CardReader.EMVClessPerformTransaction";
        public const string CardReader_EMVClessIssuerUpdate = "CardReader.EMVClessIssuerUpdate";
    }

    //Events
    public class CardReaderEvents
    {
        public const string CardReader_InsertCardEvent = "CardReader.InsertCardEvent";
        public const string CardReader_MediaInsertedEvent = "CardReader.MediaInsertedEvent";
        public const string CardReader_InvalidMediaEvent = "CardReader.InvalidMediaEvent";
        public const string CardReader_TrackDetectedEvent = "CardReader.TrackDetectedEvent";
        public const string CardReader_EMVClessReadStatusEvent = "CardReader.EMVClessReadStatusEvent";
        //unsolic
        public const string CardReader_MediaRemovedEvent_Unsolic = "CardReader.MediaRemovedEvent";
        public const string CardReader_CardActionEvent_Unsolic = "CardReader.CardActionEvent";
        public const string CardReader_MediaDetectedEvent_Unsolic = "CardReader.MediaDetectedEvent";
    }

    public static class XFS4IoT
    {

        public static string[] Interfaces = { "cardReader", "cashAcceptor", "cashDispenser", "cashManagement", "keyManagement", "keyboard", "textTerminal", "printer", "barcodeReader", "biometric", "camera", "lights", "auxiliaries", "storage", "vendorMode", "vendorApplication" };
        public static Dictionary<string, Device> InterfaceDevice = new Dictionary<string, Device>(15);
    }

    public enum InterfaceName
    {
        Common,
        CardReader,
        CashAcceptor,
        CashDispenser,
        CashManagement,
        PinPad,
        Crypto,
        KeyManagement,
        Keyboard,
        TextTerminal,
        Printer,
        SensorsAndIndicators,
        CardEmbosser,
        BarcodeReader,
    }
    //#################### Dispenser constants ######################
    //}
}
