using Devices;
using GlobalShared;
//using Simulators.CardReader;

namespace TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var utils = new Utils("TestApp");

            var cr = new CardReader("CardReader","CardReader", "ws://localhost:1132");
            //CardReaderSimulator cr = new();
            Console.ReadLine();
        }
    }
}
