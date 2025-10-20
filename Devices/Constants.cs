using System;
using System.Collections.Generic;
using System.Text;

namespace Devices
{
    public class Constants
    {
        //#################### Common constants ####################
        //Commands:
        public class CommonCommands
        {
            public const string Common_CancelCommand = "Common.Cancel";
            public const string Common_Status = "Common.Status";
            public const string Common_Capabilities = "Common.Capabilities";
            public const string Common_GetInterfaceInfo = "Common.GetInterfaceInfo";
            public const string Common_SetVersions = "Common.SetVersions";
            public const string Common_SetTransactionState = "Common.SetTransactionState";
            public const string Common_GetTransactionState = "Common.GetTransactionState";
            public const string Common_GetCommandNonce = "Common.GetCommandNonce";
            public const string Common_ClearCommandNonce = "Common.ClearCommandNonce";
            public const string Common_StartSecureOperation = "Common.StartSecureOperation";
        }

        //Events:
        public class CommonEvents
        {
            //unsolic
            public const string Common_StatusChangedEvent = "Common.StatusChangedEvent";
            public const string Common_ErrorEvent = "Common.ErrorEvent";
            public const string Common_NonceClearedEvent = "Common.NonceClearedEvent";
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
            public const string CardReader_MediaRemovedEvent = "CardReader.MediaRemovedEvent";
            public const string CardReader_CardActionEvent = "CardReader.CardActionEvent";
            public const string  CardReader_MediaDetectedEvent = "CardReader.MediaDetectedEvent";
        }


        //#################### Dispenser constants ######################
    }
}
