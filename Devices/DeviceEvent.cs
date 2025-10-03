
namespace Devices.Events
{
    public class DeviceEvent:Message
    {
        public DeviceEvent(string name) : base(MessageType.Event, name)
        {

        }

        public string Data { get; internal set; }
        public DateTime Timestamp { get; internal set; }
    }
}
