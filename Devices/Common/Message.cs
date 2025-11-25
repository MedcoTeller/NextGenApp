using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devices.Common
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
        [JsonPropertyName("header")]
        public Header Header { get; set; } = new();

        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; set; }

        /// <summary>
        /// The original JSON string of the message (if parsed from JSON).
        /// </summary>
        [JsonIgnore]
        public string? JsonString { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message() { }

        /// <summary>
        /// Initializes a new message programmatically.
        /// </summary>
        public Message(MessageType type, string name, object? payload = null)
        {
            Header.Type = type;
            Header.Name = name;
            Payload = payload;
        }

        /// <summary>
        /// Parses a raw JSON string into a <see cref="Message"/>.
        /// </summary>
        public static Message FromJson(string json)
        {
            var msg = JsonSerializer.Deserialize<Message>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new ArgumentException("Invalid JSON message.");

            msg.JsonString = json;
            return msg;
        }

        /// <summary>
        /// Serializes this message to JSON.
        /// </summary>
        public string ToJson(bool indented = false)
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = indented,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Serialize(this, opts);
        }

        public override string ToString() => ToJson(true);

        /// <summary>
        /// Gets a value from the payload by a JSON path (e.g. "services[0].serviceURI").
        /// Returns default(T) if not found or type mismatch.
        /// </summary>
        public T? GetPayloadValue<T>(string path)
        {
            if (Payload == null)
                return default;

            JsonElement root;

            // If payload is already JsonElement
            if (Payload is JsonElement je)
                root = je;
            else
            {
                // Serialize the object to JSON then parse back to JsonElement
                var json = JsonSerializer.Serialize(Payload);
                root = JsonDocument.Parse(json).RootElement;
            }

            JsonElement current = root;
            string[] parts = path.Split('.');

            try
            {
                foreach (var part in parts)
                {
                    // Handle array syntax: e.g. services[0]
                    var (propName, index) = ParsePathPart(part);

                    if (!current.TryGetProperty(propName, out JsonElement prop))
                        return default;

                    current = prop;

                    if (index.HasValue)
                    {
                        if (current.ValueKind != JsonValueKind.Array)
                            return default;

                        var arr = current.EnumerateArray().ToList();
                        if (index.Value < 0 || index.Value >= arr.Count)
                            return default;

                        current = arr[index.Value];
                    }
                }

                return current.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }

        public static T? GetPayloadValue<T>(object? payload, string path)
        {
            if (payload == null)
                return default;
            JsonElement root;
            // If payload is already JsonElement
            if (payload is JsonElement je)
                root = je;
            else
            {
                // Serialize the object to JSON then parse back to JsonElement
                var json = JsonSerializer.Serialize(payload);
                root = JsonDocument.Parse(json).RootElement;
            }
            JsonElement current = root;
            string[] parts = path.Split('.');
            try
            {
                foreach (var part in parts)
                {
                    // Handle array syntax: e.g. services[0]
                    var (propName, index) = ParsePathPart(part);
                    if (!current.TryGetProperty(propName, out JsonElement prop))
                        return default;
                    current = prop;
                    if (index.HasValue)
                    {
                        if (current.ValueKind != JsonValueKind.Array)
                            return default;
                        var arr = current.EnumerateArray().ToList();
                        if (index.Value < 0 || index.Value >= arr.Count)
                            return default;
                        current = arr[index.Value];
                    }
                }
                return current.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }
        /// <summary>
        /// Parses a path part like "services[0]" into ("services", 0).
        /// </summary>
        private static (string propertyName, int? index) ParsePathPart(string part)
        {
            var openBracket = part.IndexOf('[');
            if (openBracket == -1)
                return (part, null);

            var closeBracket = part.IndexOf(']', openBracket + 1);
            if (closeBracket == -1)
                return (part, null);

            var prop = part[..openBracket];
            var indexStr = part[(openBracket + 1)..closeBracket];
            if (int.TryParse(indexStr, out int idx))
                return (prop, idx);

            return (prop, null);
        }

        /// <summary>
        /// Gets a full object (e.g. a JSON object or array) from the payload at the specified path.
        /// Example: GetPayloadObject<CardReaderStatus>("cardReader").
        /// Returns null if not found or cannot be converted.
        /// </summary>
        public T? GetPayloadObject<T>(string path)
        {
            if (Payload == null)
                return default;

            JsonElement root;

            // Handle JsonElement payloads
            if (Payload is JsonElement je)
                root = je;
            else
            {
                // Serialize to JSON, then parse back to JsonElement
                var json = JsonSerializer.Serialize(Payload);
                root = JsonDocument.Parse(json).RootElement;
            }

            JsonElement current = root;
            string[] parts = path.Split('.');

            try
            {
                foreach (var part in parts)
                {
                    var (propName, index) = ParsePathPart(part);

                    if (!current.TryGetProperty(propName, out JsonElement prop))
                        return default;

                    current = prop;

                    if (index.HasValue)
                    {
                        if (current.ValueKind != JsonValueKind.Array)
                            return default;

                        var arr = current.EnumerateArray().ToList();
                        if (index.Value < 0 || index.Value >= arr.Count)
                            return default;

                        current = arr[index.Value];
                    }
                }

                // Deserialize full object at current path
                return current.Deserialize<T>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Gets a full object as JsonElement (useful for dynamic inspection).
        /// </summary>
        public JsonElement? GetPayloadObject(string path)
        {
            if (Payload == null)
                return null;

            JsonElement root;

            if (Payload is JsonElement je)
                root = je;
            else
            {
                var json = JsonSerializer.Serialize(Payload);
                root = JsonDocument.Parse(json).RootElement;
            }

            JsonElement current = root;
            string[] parts = path.Split('.');

            try
            {
                foreach (var part in parts)
                {
                    var (propName, index) = ParsePathPart(part);

                    if (!current.TryGetProperty(propName, out JsonElement prop))
                        return null;

                    current = prop;

                    if (index.HasValue)
                    {
                        if (current.ValueKind != JsonValueKind.Array)
                            return null;

                        var arr = current.EnumerateArray().ToList();
                        if (index.Value < 0 || index.Value >= arr.Count)
                            return null;

                        current = arr[index.Value];
                    }
                }

                return current;
            }
            catch
            {
                return null;
            }
        }

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
        [JsonPropertyName("version")]
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
        /// Attempts to retrieve the value of the specified property from a JSON element and parse it as a Boolean
        /// value.
        /// </summary>
        /// <param name="element">The JSON element containing the property to retrieve.</param>
        /// <param name="propertyName">The name of the property whose value is to be parsed as a Boolean.</param>
        /// <param name="ignoreCase">Specifies whether to perform a case-insensitive comparison when searching for the property name. The default
        /// is <see langword="true"/>.</param>
        /// <returns>A Boolean value parsed from the property value if successful; otherwise, <see langword="null"/> if the
        /// property is not found or cannot be parsed as a Boolean.</returns>
        public static bool? GetBool(this JsonElement element, string propertyName, bool ignoreCase = true)
        {
            var str = element.GetString(propertyName, ignoreCase);
            if (bool.TryParse(str, out bool result))
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
