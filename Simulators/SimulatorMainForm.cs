using Simulators.CardReader;
using Simulators.Xfs4IoT;

namespace Simulators
{
    public partial class SimulatorMainForm : Form
    {
        public SimulatorMainForm()
        {
            InitializeComponent();
        }

        private void SimulatorMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                CardReaderSimulator cardReaderSimulator = new CardReaderSimulator("ws://localhost:1234", "CardReader", "CardReader1");
                cardReaderSimulator.Start();


                var publisher = new ServicePublisher(
                    vendorName: "ACME ATM Hardware GmbH",
                    machineName: "localhost",
                    services: new[] { "cardreader1", "cashdispenser1" },
                    useTls: false
                );

                publisher.AddServiceUri(cardReaderSimulator.Url);

                _ =  publisher.StartAsync();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
    }
}
