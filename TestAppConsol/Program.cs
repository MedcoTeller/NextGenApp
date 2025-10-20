using Devices;
using GlobalShared;
using System.Formats.Tar;

namespace TestAppConsol
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var utils = new Utils("TestApp");

            var cr = new CardReader("CardReader", "ws://localhost:1132");
            //CardReaderSimulator cr = new();
            Console.ReadLine();
        }
    }
}
