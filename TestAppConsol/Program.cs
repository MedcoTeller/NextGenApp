using Devices;
using Devices.Common;
using GlobalShared;
using Simulators;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI;

namespace TestAppConsol
{
    internal class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Browser form = null;
            var t = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                form = new Browser();
                Application.Run(form);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            //Task T = Task.Run(() =>
            //{
            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    form = new Browser();
            //    Application.Run(form);
            //});

            //Console.WriteLine("Browser started");
            //var utils = new Utils("TestApp");
            await TestServiceDiscoveryAsync();
            Console.WriteLine("service discovery done");

            var cr = new CardReader("CardReader", "CardReader", "ws://localhost:1234");
            cr.PropertyValueChanged += (s, e) =>
            {
                Console.WriteLine($"Property {e.PropertyName} changed to {e.NewValue}");
            };

            //Console.WriteLine("Starting cardreader service");
            //await cr.StartAsync();

            //Console.WriteLine("Starting read card");
            //await cr.ReadCard(true, true, false, false, 240000);

            ////TestJsonMessage();
            form?.Invoke(new Action(() =>
            {
                form.Text = "test";
                form.Navigate("https://www.bing.com/");
            }));

            form?.Navigate("https://chatgpt.com/");
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
            Console.WriteLine("Before GetPublishers()");
            await disc.GetPublishers();
            Console.WriteLine("After GetPublishers()"); // <- add this

            Console.WriteLine("Before GetServices()");
            await disc.GetServices();                    // maybe this is blocking
            Console.WriteLine("After GetServices()");
        }
    }
}
