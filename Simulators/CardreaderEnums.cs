namespace Simulators.CardReader
{
    /// <summary>
    /// Specifies the transport/exit position media state. This property will be null 
    /// if the capability to report media position is not supported by the device 
    /// (e.g., a typical swipe reader or contactless chip card reader), otherwise one of the following.
    /// </summary>
    /// <remarks>Use this enumeration to determine the current state of media in the device, including whether
    /// media is present, absent, jammed, entering, or latched. The values correspond to typical states encountered in
    /// devices that handle physical media, and can be used to guide application logic for user prompts, error handling,
    /// or device operations.</remarks>
    public enum MediaStatusEnum
    {
        /// <summary>
        /// The media state cannot be determined with the device in its current state (e.g. the value of device is noDevice, powerOff, offline or hardwareError.
        /// </summary>
        unknown,    
        /// <summary>
        /// Media is present in the device, not in the entering position and not jammed. On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
        /// </summary>
        present,    // -
        /// <summary>
        /// Media is not present in the device and not at the entering position.
        /// </summary>
        notPresent, // -
        /// <summary>
        /// Media is jammed in the device; operator intervention is required.
        /// </summary>
        jammed,     // -
        /// <summary>
        /// Media is at the entry/exit slot of a motorized device.
        /// </summary>
        entering,   // -
        /// <summary>
        /// Media is present and latched in a latched dip card unit. This means the card can be used for chip card dialog.
        /// </summary>
        latched,    // -
    }

    /// <summary>
    /// Specifies the operational status of the security module for card processing.
    /// </summary>
    /// <remarks>Use this enumeration to determine whether the security module is ready to process cards or is
    /// currently unavailable. The status can be used to control access to card processing operations and to handle
    /// error conditions appropriately.</remarks>
    public enum SecurityStatusEnum
    {
        notReady,   // The security module is not ready to process cards or is inoperable.
        open        // The security module is open and ready to process cards.
    }

    /// <summary>
    /// Specifies the state of the chip controlled by this service.
    /// This property will be null if the capability to report the state of the chip is not supported by the ID card unit device
    /// and will apply to contactless chip card readers, otherwise one of the following values.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the current state of the chip in the device, including whether
    /// the chip is present, powered, busy, or in error. The values correspond to typical states encountered in
    /// devices that handle chip cards, and can be used to guide application logic for user prompts, error handling,
    /// or device operations.
    /// </remarks>
    public enum ChipPowerStatusEnum
    {
        unknown,        // The state of the chip cannot be determined with the device in its current state.
        online,         // The chip is present, powered on and online (i.e. operational, not busy processing a request and not in an error state).
        busy,           // The chip is present, powered on, and busy (unable to process a command at this time).
        poweredOff,     // The chip is present, but powered off (i.e. not contacted).
        noDevice,       // A card is currently present in the device, but has no chip.
        hardwareError,  // The chip is present, but inoperable due to a hardware error that prevents it from being used (e.g., MUTE, if there is an unresponsive card in the reader).
        noCard          // There is no card in the device.
    }

    /// <summary>
    /// Specifies the state of the chip card module reader.
    /// This property will be null if reporting the chip card module status is not supported, otherwise one of the following values.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the current state of the chip card module, including whether
    /// the module is operational, inoperable, or unknown. The values correspond to typical states encountered in
    /// devices that handle chip cards, and can be used to guide application logic for user prompts, error handling,
    /// or device operations.
    /// </remarks>
    public enum ChipModuleStatusEnum
    {
        ok,         // The chip card module is in a good state.
        inoperable, // The chip card module is inoperable.
        unknown     // The state of the chip card module cannot be determined.
    }

    /// <summary>
    /// Specifies the state of the magnetic card writer.
    /// This property will be null if reporting the magnetic card writing module status is not supported, otherwise one of the following values.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the current state of the magnetic card writing module, including whether
    /// the module is operational, inoperable, or unknown. The values correspond to typical states encountered in
    /// devices that handle magnetic card writing, and can be used to guide application logic for user prompts, error handling,
    /// or device operations.
    /// </remarks>
    public enum MagWriteModuleStatusEnum
    {
        ok,         // The magnetic card writing module is in a good state.
        inoperable, // The magnetic card writing module is inoperable.
        unknown     // The state of the magnetic card writing module cannot be determined.
    }

    /// <summary>
    /// Specifies the state of the front image reader.
    /// This property will be null if reporting the front image reading module status is not supported, otherwise one of the following values.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the current state of the front image reading module, including whether
    /// the module is operational, inoperable, or unknown. The values correspond to typical states encountered in
    /// devices that handle front image reading, and can be used to guide application logic for user prompts, error handling,
    /// or device operations.
    /// </remarks>
    public enum FrontImageModuleStatusEnum
    {
        ok,         // The front image reading module is in a good state.
        inoperable, // The front image reading module is inoperable.
        unknown     // The state of the front image reading module cannot be determined.
    }

    /// <summary>
    /// Specifies the type of the ID card unit.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the mechanical and functional type of the card reader device.
    /// </remarks>
    public enum CardReaderTypeEnum
    {
        motor,                // The ID card unit is a motor driven card unit.
        swipe,                // The ID card unit is a swipe (pull-through) card unit.
        dip,                  // The ID card unit is a dip card unit. This dip type is not capable of latching cards entered.
        latchedDip,           // The ID card unit is a latched dip card unit. Used when a dip card unit supports chip communication and latching.
        contactless,          // The ID card unit is a contactless card unit, i.e. no insertion required.
        intelligentContactless, // The ID card unit is an intelligent contactless card unit with built-in EMV/smart card application functionality.
        permanent             // The ID card unit is dedicated to a permanently housed chip card (no user interaction).
    }

    /// <summary>
    /// Specifies the type of security module.
    /// This property will be null if the device has no security module.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine the type of security module present in the device.
    /// </remarks>
    public enum SecurityTypeEnum
    {
        mm,     // The security module is a MMBox.
        cim86   // The security module is a CIM86.
    }

    public enum CardReaderErrorCode
    {
        /// <summary>
        /// The card is jammed. Operator intervention is required.
        /// </summary>
        mediaJam,

        /// <summary>
        /// The open of the shutter failed due to manipulation or hardware error. Operator intervention is required.
        /// </summary>
        shutterFail,

        /// <summary>
        /// The card was removed before completion of the write action (the event CardReader.MediaInsertedEvent has been generated). 
        /// For motor driven devices, the write is disabled, i.e., another command has to be issued to enable the reader for card entry.
        /// </summary>
        noMedia,

        /// <summary>
        /// No track found; card may have been inserted or pulled through the wrong way.
        /// </summary>
        invalidMedia,

        /// <summary>
        /// The writeMethod value is inconsistent with device capabilities.
        /// </summary>
        writeMethod,

        /// <summary>
        /// The card that was inserted is too short. When this error occurs the card remains at the exit slot.
        /// </summary>
        cardTooShort,

        /// <summary>
        /// The card that was inserted is too long. When this error occurs the card remains at the exit slot.
        /// </summary>
        cardTooLong,

        /// <summary>
        /// The retain bin is full; no more cards can be retained. The current card is still in the device.
        /// </summary>
        retainBinFull,

        /// <summary>
        /// The specified action is not supported by the hardware device.
        /// </summary>
        chipPowerNotSupported,

        /// <summary>
        /// The ATR has not been obtained (only applies to user chips).
        /// </summary>
        atrNotObtained,

        /// <summary>
        /// An error occurred while communicating with the chip.
        /// </summary>
        invalidData,

        securityFail,// - The security module failed reading the card's security and no other data source was requested.

        cardCollision,// - There was an unresolved collision of two or more contactless card signals.

        occupied,// - A card already occupies the requested to position.

        full,// - The to position is full. The card is still in the device.

        mediaRetained,// - The card has been retained during attempts to move it to the exit position. The device is clear and can be used.

        invalidKey,// - The key does not fit to the security module.

        invalidTerminalData,// - Input data terminalData was invalid. Contactless chip card reader could not be configured successfully.

        invalidAidData,// - Input data aidData was invalid. Contactless chip card reader could not be configured successfully.
        
        invalidKeyData,// - Input data keyData was invalid. Contactless chip card reader could not be configured successfully.

        readerNotConfigured,//This command was issued before calling CardReader.EMVClessConfigure command.
       
        transactionNotInitiated, // - This command was issued before calling the CardReader.EMVClessPerformTransaction command.
    }

    public enum ChipClessOutcomeAlternateInterface
    {
        /// <summary>
        /// txOutcome is tryAnotherInterface and the contact chip interface should be used to complete a transaction.
        /// </summary>
        contact,

        /// <summary>
        /// txOutcome is tryAnotherInterface and the magnetic stripe interface should be used to complete a transaction.
        /// </summary>
        magneticStripe
    }

    public enum ChipClessOutcomeUiValueQualifier
    {
        /// <summary>
        /// value is an Amount.
        /// </summary>
        amount,

        /// <summary>
        /// value is a Balance.
        /// </summary>
        balance
    }

    public enum ChipClessOutcomeCvm
    {
        /// <summary>
        /// Online PIN should be entered by the card holder.
        /// </summary>
        onlinePIN,

        /// <summary>
        /// A confirmation code entry has been successfully done on a mobile device.
        /// </summary>
        confirmationCodeVerified,

        /// <summary>
        /// Application should obtain card holder signature.
        /// </summary>
        sign,

        /// <summary>
        /// No CVM is required for this transaction.
        /// </summary>
        noCVM,

        /// <summary>
        /// There is no CVM preference, but application can follow the payment system's rules to process the transaction.
        /// </summary>
        noCVMPreference
    }

    public enum ChipClessOutcomeUiStatus
    {
        /// <summary>
        /// Contactless card reader is not able to communicate with a card. 
        /// This status occurs towards the end of a contactless transaction or if the reader is not powered on.
        /// </summary>
        notReady,

        /// <summary>
        /// Contactless card reader is powered on, but the reader field is not yet active for communication with a card.
        /// </summary>
        idle,

        /// <summary>
        /// Contactless card reader is powered on and attempting to initiate communication with a card.
        /// </summary>
        readyToRead,

        /// <summary>
        /// Contactless card reader is in the process of reading the card.
        /// </summary>
        processing,

        /// <summary>
        /// Contactless card reader was able to read a card successfully.
        /// </summary>
        cardReadOk,

        /// <summary>
        /// Contactless card reader was not able to process the card successfully.
        /// </summary>
        processingError
    }

    public enum ChipTxOutcome
    {
        /// <summary>
        /// Transaction could not be completed as more than one contactless card was tapped.
        /// </summary>
        multipleCards,

        /// <summary>
        /// Transaction was approved offline.
        /// </summary>
        approve,

        /// <summary>
        /// Transaction was declined offline.
        /// </summary>
        decline,

        /// <summary>
        /// Transaction was requested for online authorization.
        /// </summary>
        onlineRequest,

        /// <summary>
        /// Transaction requested online authorization and will be completed after a re-tap of the card. 
        /// Transaction should be completed by issuing the CardReader.EMVClessIssuerUpdate command.
        /// </summary>
        onlineRequestCompletionRequired,

        /// <summary>
        /// Transaction could not be completed due to a card read error. 
        /// The contactless card could be tapped again to re-attempt the transaction.
        /// </summary>
        tryAgain,

        /// <summary>
        /// Transaction could not be completed over the contactless interface. 
        /// Another interface may be suitable for this transaction (for example contact).
        /// </summary>
        tryAnotherInterface,

        /// <summary>
        /// Transaction cannot be completed on the contactless card due to an irrecoverable error.
        /// </summary>
        endApplication,

        /// <summary>
        /// Transaction was not completed because of a requirement to allow entry of confirmation code on a mobile device. 
        /// Transaction should be completed by issuing the CardReader.EMVClessPerformTransaction after a card removal and a re-tap of the card.
        /// </summary>
        confirmationRequired
    }

    public enum ChipCardholderAction
    {
        /// <summary>
        /// Transaction was completed. No further action is required.
        /// </summary>
        none,

        /// <summary>
        /// The contactless card should be re-tapped to complete the transaction. 
        /// This value can be returned when txOutcome is onlineRequest, onlineRequestCompletionRequired or confirmationRequired.
        /// </summary>
        retap,

        /// <summary>
        /// The contactless card should not be removed from the field until the transaction is completed.
        /// </summary>
        holdCard
    }

    public enum TrackStatus
    {
        /// <summary>
        /// The track/chip/memory chip is blank.
        /// </summary>
        dataMissing,

        /// <summary>
        /// The data contained on the track/chip/memory chip is invalid. 
        /// This will typically be returned when data reports badReadLevel or dataInvalid.
        /// </summary>
        dataInvalid,

        /// <summary>
        /// The data contained on the track/chip/memory chip is too long.
        /// </summary>
        dataTooLong,

        /// <summary>
        /// The data contained on the track/chip/memory chip is too short.
        /// </summary>
        dataTooShort,

        /// <summary>
        /// The data source to read from is not supported by the service.
        /// </summary>
        dataSourceNotSupported,

        /// <summary>
        /// The data source to read from is missing on the card, or is unable to be read due to a hardware problem, 
        /// or the module has not been initialized. For example, this will be returned on a request to read a Memory Card 
        /// and the customer has entered a magnetic card without associated memory chip. This will also be reported when 
        /// data reports noData, notInitialized or hardwareError. This will also be reported when the image reader could 
        /// not create a BMP file due to the state of the image reader or due to a failure.
        /// </summary>
        dataSourceMissing
    }

    public enum MemoryChipProtocol
    {
        /// <summary>
        /// The card reader has used the T=0 protocol.
        /// </summary>
        chipT0,

        /// <summary>
        /// The card reader has used the T=1 protocol.
        /// </summary>
        chipT1,

        /// <summary>
        /// The card reader has used the ISO 14443 (Part3) Type A contactless chip card protocol.
        /// </summary>
        chipTypeAPart3,

        /// <summary>
        /// The card reader has used the ISO 14443 (Part4) Type A contactless chip card protocol.
        /// </summary>
        chipTypeAPart4,

        /// <summary>
        /// The card reader has used the ISO 14443 Type B contactless chip card protocol.
        /// </summary>
        chipTypeB,

        /// <summary>
        /// The card reader has used the ISO 18092 (106/212/424kbps) contactless chip card protocol.
        /// </summary>
        chipTypeNFC
    }

    public enum WriteMethod
    {
        /// <summary>
        /// Write using low coercivity.
        /// </summary>
        loco,

        /// <summary>
        /// Write using high coercivity.
        /// </summary>
        hico
    }

    public enum ChipProtocol
    {
        /// <summary>
        /// Use the T=0 protocol to communicate with the chip.
        /// </summary>
        chipT0,

        /// <summary>
        /// Use the T=1 protocol to communicate with the chip.
        /// </summary>
        chipT1,

        /// <summary>
        /// The service will automatically determine the protocol used to communicate with the chip.
        /// </summary>
        chipProtocolNotRequired,

        /// <summary>
        /// Use the ISO 14443 (Part3) Type A contactless chip card protocol to communicate with the chip.
        /// </summary>
        chipTypeAPart3,

        /// <summary>
        /// Use the ISO 14443 (Part4) Type A contactless chip card protocol to communicate with the chip.
        /// </summary>
        chipTypeAPart4,

        /// <summary>
        /// Use the ISO 14443 Type B contactless chip card protocol to communicate with the chip.
        /// </summary>
        chipTypeB,

        /// <summary>
        /// Use the ISO 18092 (106/212/424kbps) contactless chip card protocol to communicate with the chip.
        /// </summary>
        chipTypeNFC
    }

    public enum CardRestToDestination
    {
        /// <summary>
        /// Move the card to the exit position. If the card is already at the exit, it may be moved to ensure it is in the correct position to be taken.
        /// </summary>
        exit,

        /// <summary>
        /// Move the card to a retain storage unit.
        /// </summary>
        retain,

        /// <summary>
        /// Keep the card in its current position. If the card is in the transport, it may be moved in the transport to verify it is not jammed.
        /// </summary>
        currentPosition,

        /// <summary>
        /// The service will select the position to which the card will be moved based on device capabilities, retain storage units available and service specific configuration.
        /// </summary>
        auto
    }

    public enum ChipPower
    {
        /// <summary>
        /// The chip is powered on and reset.
        /// </summary>
        cold,

        /// <summary>
        /// The chip is reset.
        /// </summary>
        warm,

        /// <summary>
        /// The chip is powered off.
        /// </summary>
        off
    }

}

