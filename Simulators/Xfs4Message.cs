using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Simulators.Xfs4IoT
{
    /// <summary>
    /// XFS4IoT message types used in the header.type field.
    /// </summary>
    public enum MessageType
    {
        Command,
        Acknowledge,
        Event,
        Completion
    }

    /// <summary>
    /// XFS4IoT message header - matches spec fields used in simulator.
    /// requestId is nullable because unsolicited events do not include it.
    /// </summary>
    public class Xfs4Header
    {
        [JsonPropertyName("requestId")]
        public int? RequestId { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageType Type { get; set; } = MessageType.Command;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }
    }

    /// <summary>
    /// Single envelope for all XFS4IoT messages (command, event, completion).
    /// Constructors:
    /// - Xfs4Message(string json) : parse from incoming JSON.
    /// - Xfs4Message(MessageType, name, requestId?, payload?, status?) : create new message.
    /// </summary>
    public class Xfs4Message
    {
        [JsonPropertyName("header")]
        public Xfs4Header Header { get; set; } = new();

        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; set; }

        /// <summary>
        /// Default ctor for serializer.
        /// </summary>
        public Xfs4Message() { }

        /// <summary>
        /// Parse a JSON string into an Xfs4Message.
        /// Throws if JSON is invalid.
        /// </summary>
        public Xfs4Message(string json)
        {
            var parsed = JsonSerializer.Deserialize<Xfs4Message>(json);
            if (parsed == null)
                throw new ArgumentException("Invalid XFS4IoT JSON message");

            Header = parsed.Header;
            Payload = parsed.Payload;
        }

        /// <summary>
        /// Create a new message programmatically.
        /// </summary>
        /// <param name="type">Message type (Command/Event/Completion)</param>
        /// <param name="name">Command or event name</param>
        /// <param name="requestId">Nullable requestId: required for commands/completions, null for unsolicited events</param>
        /// <param name="payload">Optional payload object (will be serialized)</param>
        /// <param name="status">Optional status (used for completion)</param>
        public Xfs4Message(MessageType type, string name, int? requestId = null, object? payload = null, string? status = null)
        {
            Header = new Xfs4Header
            {
                Type = type,
                Name = name,
                RequestId = requestId,
                Status = status
            };
            Payload = payload;
        }

        public T? GetPayloadValue<T>(string propertyName)
        {
            if (Payload == null)
                return default;

            try
            {
                // Convert object payload to JsonNode for querying
                var node = JsonSerializer.SerializeToNode(Payload);
                if (node is JsonObject obj && obj.TryGetPropertyValue(propertyName, out var valueNode))
                {
                    return valueNode.Deserialize<T>();
                }
            }
            catch
            {
                // ignored
            }

            return default;
            //usage:
            //string? vendor = msg.GetPayloadValue<string>("vendorName");

            //""payload"": {
            //    ""vendorName"": ""ACME Hardware GmbH"",
            //    ""services"": [
            //      { ""serviceURI"": ""wss://ATM1:123/xfs4iot/v1.0/CardReader"" },
            //      { ""serviceURI"": ""wss://ATM1:123/xfs4iot/v1.0/CashDispenser"" }
            //    ]
            //  }
            //}
        }

        /// <summary>
        /// Extracts all nested values from an array of objects, e.g. all 'serviceURI' values from 'services'
        /// </summary>
        public List<T>? GetNestedArrayValues<T>(string arrayName, string nestedProperty)
        {
            var result = new List<T>();

            if (Payload == null)
                return result;

            try
            {
                var node = JsonSerializer.SerializeToNode(Payload);
                if (node is JsonObject obj &&
                    obj.TryGetPropertyValue(arrayName, out var arrayNode) &&
                    arrayNode is JsonArray array)
                {
                    foreach (var element in array)
                    {
                        if (element is JsonObject elementObj &&
                            elementObj.TryGetPropertyValue(nestedProperty, out var nestedNode))
                        {
                            var value = nestedNode.Deserialize<T>();
                            if (value != null)
                                result.Add(value);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
            return result;
            //usage:
            //List<string>? uris = msg.GetNestedArrayValues<string>("services", "serviceURI");
        }

        /// <summary>
        /// Serialize to JSON string.
        /// </summary>
        public string ToJson(bool indented = false)
        {
            var opts = new JsonSerializerOptions { WriteIndented = indented, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            opts.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Serialize(this, opts);
        }

        override public string ToString() => ToJson(true);
    }
}
