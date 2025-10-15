using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics.Metrics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Simulators
{
    /// <summary>
    /// Specifies the operational status of a device or service, indicating its availability, error state, or other
    /// relevant condition.
    /// </summary>
    /// <remarks>Use this enumeration to determine the current state of a device or service, such as whether
    /// it is online, offline, experiencing errors, or in a transitional state. The values can be used to guide
    /// application behavior, such as enabling or disabling functionality, handling errors, or responding to potential
    /// fraud conditions. Some states, such as 'starting', are temporary and should be monitored for changes to a stable
    /// state. Application logic may be required for certain statuses, such as 'potentialFraud', to decide whether to
    /// take further action.</remarks>
    public enum DeviceStatusEnum
    {
        online,         // - The device or service is online. This is returned when the device is present and operational or the service is not associated with a device, e.g., a vendor mode / vendor application service.
        offline,        // - The device is offline (e.g., the operator has taken the device offline by turning a switch or breaking an interlock).
        powerOff,       // - The device is powered off or physically not connected.
        noDevice,       // - The device is not intended to be there, e.g. this type of self-service machine does not contain such a device or it is internally not configured.
        hardwareError,  // - The device is inoperable due to a hardware error.
        userError,      // -The device is present but a person is preventing proper device operation.
        deviceBusy,     // -The device is busy and unable to process a command at this time.
        fraudAttempt,   // -The device is present but is inoperable because it has detected a fraud attempt.
        potentialFraud, // -The device has detected a potential fraud attempt and is capable of remaining in service. In this case the application should make the decision as to whether to take the device offline.
        starting,       // -The device is starting and performing whatever initialization is necessary. This can be reported after the connection is made but before the device is ready to accept commands. This must only be a temporary state, the service must report a different state as soon as possible. If an error causes initialization to fail then the state should change to hardwareError.
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DevicePositionStatusEnum
    {
        inPosition,     // The device is in its normal operating position, or is fixed in place and cannot be moved.
        notInPosition,  // The device has been removed from its normal operating position.
        unknown         // Due to a hardware error or other condition, the position of the device cannot be determined.
    }

    /// <summary>
    /// Specifies the operational status of the anti-fraud module, indicating whether it is functioning correctly,
    /// inoperable, has detected a foreign device, or its state is unknown.
    /// </summary>
    /// <remarks>Use this enumeration to determine the current state of the anti-fraud module and to respond
    /// appropriately to changes in its status. The values represent distinct states that may affect security or device
    /// operation.</remarks>
    public enum AntiFraudModuleStatusEnum
    {
        ok,             // Anti-fraud module is in a good state and no foreign device is detected.
        inoperable,     // Anti-fraud module is inoperable.
        deviceDetected, // Anti-fraud module detected the presence of a foreign device.
        unknown         // The state of the anti-fraud module cannot be determined.
    }

    /// <summary>
    /// Specifies the operational status of an exchange on the service.
    /// </summary>
    /// <remarks>Use this enumeration to determine whether commands can be sent to the exchange. When the
    /// status is set to 'inactive', operations that interact with the device may be rejected.</remarks>
    public enum ExchangeStatusEnum
    {
        active,   // Exchange is active on this service. Commands which interact with the device may be rejected with an error code as appropriate.
        inactive  // Exchange is not active on this service.
    }

    /// <summary>
    /// Specifies the status of end to end security support on this device.
    /// This property is null in Common.Status if E2E security is not supported by this hardware and any command can be called without a token.
    /// </summary>
    /// <remarks>
    /// Also see Common.CapabilityProperties.endToEndSecurity.
    /// </remarks>
    public enum EndToEndSecurityStatusEnum
    {
        notEnforced,   // E2E security is supported by this hardware but it is not currently enforced, for example because required keys aren't loaded. It's currently possible to perform E2E commands without a token.
        notConfigured, // E2E security is supported but not correctly configured, for example because required keys aren't loaded. Any attempt to perform any command protected by E2E security will fail.
        enforced
    }


    /// <summary>
    /// Specifies the level of support for end-to-end security.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine whether E2E security is always enforced or only if configured.
    /// </remarks>
    public enum RequiredEndToEndSecurityEnum
    {
        ifConfigured, // The device is capable of supporting E2E security, but it will not be enforced if not configured, for example because the required keys are not loaded.
        always        // E2E security is supported and enforced at all times. Failure to supply the required security details will lead to errors. If E2E security is not correctly configured, for example because the required keys are not loaded, all secured commands will fail with an error.
    }

    /// <summary>
    /// Specifies if this device will return a security token as part of the response data to commands that support end-to-end security.
    /// This property is null in Common.Status if the device is incapable of returning a response token, otherwise one of the following values:
    /// </summary>
    /// <remarks>
    /// Use this enumeration to determine whether a security token is always included in responses or only if E2E security is correctly configured.
    /// </remarks>
    public enum ResponseSecurityEnabledEnum
    {
        ifConfigured, // The device is capable of supporting E2E security if correctly configured. If E2E security has not been correctly configured, for example because the required keys are not loaded, commands will complete without a security token.
        always        // A security token will be included with command responses. If E2E security is not correctly configured, for example because the required keys are not loaded, the command will complete with an error.
    }

    /// <summary>
    /// This command allows the application to specify the transaction state, which the service can then 
    /// utilize in order to optimize performance. After receiving this command, this service can perform 
    /// the necessary processing to start or end the customer transaction. 
    /// This command should be called for every service that could be used in a customer transaction. 
    /// The transaction state applies to every session.
    /// </summary>
    public enum TransactionStateEnum
    {
        active,     //- A customer transaction is in progress.
        inactive    //- No customer transaction is in progress.
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CommandStatusEnum
    {
        invalidMessage,     // - The JSON in the message is invalid and can't be parsed.
        invalidRequestID,   // - The request ID on the command is invalid. This could be because the value is not an integer, has a zero value, or because a command with the same request ID from the same client is already queued or is executing.
        tooManyRequests,    // - The service has currently received and queued more requests than it can process.
    }

    public enum CompletionCodeEnum
    {
        commandErrorCode,       // - Check the errorCode property for the command specific error code.
        canceled,               // - Canceled using the Common.Cancel command.
        timeOut,                //- Timed out after the client specified timeout.
        deviceNotReady,         // - The device is not ready or timed out.
        hardwareError,          // - An error occurred on the device.
        internalError,          // - An internal inconsistency or other unexpected error occurred.
        invalidCommand,         // - The command is not supported by the service.
        invalidRequestID,       // - The requestId is invalid.
        unsupportedCommand,     // - The command is valid for the interface but is not supported by the service or device.
        invalidData,            // - The command message contains invalid data.
        userError,              // - The user is preventing proper operation of the device.
        unsupportedData,        // - The command message contains data that is valid for the interface command but is not supported by the service or device.
        fraudAttempt,           // - The user is attempting a fraudulent act on the device.
        sequenceError,          // - The command request is not valid at this time or in the device's current state.
        authorizationRequired,  // - The command request cannot be performed because it requires authorization.
        noCommandNonce,         // - The value of the nonce stored in the hardware was cleared, for example by a power failure.
        invalidToken,           // - The security token is invalid.
        invalidTokenNonce,      // - The value of the nonce in the security token does not match the stored value.
        invalidTokenHMAC,       // - The value of the HMAC in the security token is incorrect.
        invalidTokenFormat,     // - The token format version value is not recognized, or the token format is somehow invalid.
        invalidTokenKeyNoValue, // - The key used for the HMAC for a token has not been loaded and the token cannot be validated.
        notEnoughSpace,         // - There is not enough space on the storage.
    }


}