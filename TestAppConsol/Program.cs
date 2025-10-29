using Devices;
using GlobalShared;

namespace TestAppConsol
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //var utils = new Utils("TestApp");
            var disc = new ServiceDiscovery();
            disc.GetPublishers();
            disc.GetServices();
            var cr = new CardReader("CardReader", "CardReader", "ws://localhost:1234");
            _ = cr.StartAsync();

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


    }
}
