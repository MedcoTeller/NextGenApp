using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simulators.Xfs4IoT
{
    public enum MessageType
    {
        Command,
        Completion,
        Event
    }

    public class Xfs4Header
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; set; } = MessageType.Command;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? RequestId { get; set; }   // optional for unsolicited events

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }
    }

    public class Xfs4Message
    {
        [JsonPropertyName("header")]
        public Xfs4Header Header { get; set; } = new();

        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonElement? Payload { get; set; }

        // --- Constructors ---

        /// <summary>
        /// Construct from JSON string
        /// </summary>
        public Xfs4Message(string json)
        {
            var parsed = JsonSerializer.Deserialize<Xfs4Message>(json);
            if (parsed == null)
                throw new ArgumentException("Invalid JSON for Xfs4Message");

            Header = parsed.Header;
            Payload = parsed.Payload;
        }

        /// <summary>
        /// Construct a new message manually
        /// </summary>
        public Xfs4Message(
            MessageType type,
            string name,
            int? requestId = null,
            string? status = null,
            object? payload = null)
        {
            Header = new Xfs4Header
            {
                Type = type,
                Name = name,
                RequestId = requestId,
                Status = status
            };

            if (payload != null)
                Payload = JsonSerializer.SerializeToElement(payload);
        }

        // Empty default ctor for serialization
        public Xfs4Message() { }

        // --- Methods ---
        public string ToJson(bool indented = false)
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
        }

        public static Xfs4Message FromJson(string json)
        {
            return JsonSerializer.Deserialize<Xfs4Message>(json)!;
        }
    }
}
