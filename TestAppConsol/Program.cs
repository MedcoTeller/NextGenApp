using Devices;
using GlobalShared;
using System.Threading.Tasks;

namespace TestAppConsol
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //var utils = new Utils("TestApp");
            await TestServiceDiscoveryAsync();

            var cr = new CardReader("CardReader", "CardReader", "ws://localhost:1234");
            await cr.StartAsync();
            await cr.ReadCard(true, true, false, false, 240000);

            //TestJsonMessage();

            Console.WriteLine($"END");
            Console.ReadLine();
        }

        private static void TestJsonMessage()
        {
            string jsn = @"{""header"":{""type"":""command"",""name"":""Common.Status"",""version"":""1.0"",""requestId"":12345,""timeout"":1000},""payload"":{""vendorName"":""<Name of hardware/software vendor>"",""services"":[{""serviceURI"":""wss://machinename:port/xfs4iot/v1.0/<servicename1>""},{""serviceURI"":""wss://machinename:port/xfs4iot/v1.0/<servicename2>""}]}}";
            var msg = Command.FromJson(jsn);
            var services = msg.GetPayloadValue<object[]>("services");
            //utils.LogInfo($"Found {services?.Length ?? 0} services.");
            Console.WriteLine($"Found {services?.Length ?? 0} services.");
            Console.WriteLine($"vendor: {msg.GetPayloadValue<string>("vendorName")}.");
            for (int i = 0; i < services?.Length; i++)
            {
                Console.WriteLine((msg.GetPayloadValue<string>($"services[{i}].serviceURI") ?? string.Empty));
            }
        }

        private static void TestCardReader()
        {
            var cr = new CardReader("CardReader", "CardReader", "ws://localhost:1234");
            _ = cr.StartAsync();
        }

        private static async Task TestServiceDiscoveryAsync()
        {
            var disc = new ServiceDiscovery();
            await disc.GetPublishers();
            await disc.GetServices();
        }
    }
}
