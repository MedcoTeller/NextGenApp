using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devices
{
    /// <summary>
    /// Represents the type of message exchanged between devices.
    /// </summary>
    public enum MessageType
    {
        Command,
        Acknowledge,
        Event,
        Completion,
        Unsolicited
    }

    /// <summary>
    /// Represents a device message, including header and payload.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class with the specified type and name.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="name">The name of the message.</param>
        public Message(MessageType type, string name)//, object? payload = null )
        {
            Header.Type = type;
            Header.Name = name;
            //Payload = payload;
        }

        /// <summary>
        /// Gets or sets the header of the message.
        /// </summary>
        [JsonPropertyName("header")]
        public Header Header { get; set; } = new();

        /// <summary>
        /// Gets or sets the payload of the message as a <see cref="JsonElement"/>.
        /// </summary>
        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonElement? Payload { get; set; }

        /// <summary>
        /// Gets or sets the original JSON string of the message.
        /// </summary>
        public string JsonString { get; set; }
    }

    /// <summary>
    /// Represents the header information for a device message.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the message (e.g., "Common.Status").
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Gets or sets the version of the message format.
        /// </summary>
        [JsonPropertyName("name")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets the request ID, if applicable.
        /// </summary>
        [JsonPropertyName("requestId")]
        public int? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the timeout value, if applicable.
        /// </summary>
        [JsonPropertyName("timeout")]
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the status of the message, if applicable.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the completion code, if applicable.
        /// </summary>
        [JsonPropertyName("completionCode")]
        public string? CompletionCode { get; set; }

        /// <summary>
        /// Gets or sets the error description, if applicable.
        /// </summary>
        [JsonPropertyName("errorDescription")]
        public string? ErrorDescription { get; set; }
    }

    /// <summary>
    /// Provides methods for parsing device messages from JSON.
    /// </summary>
    public static class Xfs4DynamicParser
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Parses a JSON string into a <see cref="Message"/> object.
        /// </summary>
        /// <param name="json">The JSON string representing the message.</param>
        /// <returns>The parsed <see cref="Message"/> object.</returns>
        public static Message Parse(string json)
        {
            var msg = JsonSerializer.Deserialize<Message>(json, Options)!;
            msg?.JsonString = json;
            return msg;
        }
    }

    /// <summary>
    /// Provides extension methods for working with <see cref="JsonElement"/>.
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Gets the string value of a property from a <see cref="JsonElement"/> by property name.
        /// </summary>
        /// <param name="element">The JSON element.</param>
        /// <param name="propertyName">The property name to search for.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the property name.</param>
        /// <returns>The string value of the property, or null if not found.</returns>
        public static string? GetString(this JsonElement element, string propertyName, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
                }
                return null;
            }

            if (element.TryGetProperty(propertyName, out var value))
                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();

            return null;
        }

        /// <summary>
        /// Gets the integer value of a property from a <see cref="JsonElement"/> by property name.
        /// </summary>
        /// <param name="element">The JSON element.</param>
        /// <param name="propertyName">The property name to search for.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the property name.</param>
        /// <returns>The integer value of the property, or null if not found or not an integer.</returns>
        public static int? GetInt(this JsonElement element, string propertyName, bool ignoreCase = true)
        {
            var str = element.GetString(propertyName, ignoreCase);
            if (int.TryParse(str, out int result))
                return result;
            return null;
        }

        /// <summary>
        /// Gets a property as a <see cref="JsonElement"/> from a <see cref="JsonElement"/> by property name.
        /// </summary>
        /// <param name="element">The JSON element.</param>
        /// <param name="propertyName">The property name to search for.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the property name.</param>
        /// <returns>The <see cref="JsonElement"/> of the property, or null if not found.</returns>
        public static JsonElement? GetPropertyElement(this JsonElement element, string propertyName, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        return prop.Value;
                }
                return null;
            }

            if (element.TryGetProperty(propertyName, out var value))
                return value;

            return null;
        }
    }
}
