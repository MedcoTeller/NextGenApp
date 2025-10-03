
namespace Devices.Commands
{
    public class Command : Message
    {
        public Command(string name, int timeot=0) : base(MessageType.Command, name)
        {
            Header.Timeout = timeot;
        }
    }


}
