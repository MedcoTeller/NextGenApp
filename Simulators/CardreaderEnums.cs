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
        unknown,    // -The media state cannot be determined with the device in its current state (e.g. the value of device is noDevice, powerOff, offline or hardwareError.
        present,    // -Media is present in the device, not in the entering position and not jammed. On the latched dip device, this indicates that the card is present in the device and the card is unlatched.
        notPresent, // -Media is not present in the device and not at the entering position.
        jammed,     // -Media is jammed in the device; operator intervention is required.
        entering,   // -Media is at the entry/exit slot of a motorized device.
        latched,    // -Media is present and latched in a latched dip card unit. This means the card can be used for chip card dialog.
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
}
