using Simulators.CardReader;

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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
    }
}
