
namespace Devices
{
    public class Command : Message
    {
        public Command(string name, int? timeout=null) : base(MessageType.Command, name)
        {
            Header.Timeout = timeout;
        }
    }


}
