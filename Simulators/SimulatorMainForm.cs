using GlobalShared;
using Simulators.CardReader;
using Simulators.Xfs4IoT;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simulators
{
    public partial class SimulatorMainForm : Form
    {
        public SimulatorMainForm()
        {
            InitializeComponent();
        }

        //Devices:
        CardReaderSimulator cardReaderSimulator = new CardReaderSimulator("ws://localhost:1234", "CardReader", "CardReader1");
        ServicePublisher publisher = new ServicePublisher(
            vendorName: "ACME ATM Hardware GmbH",
            machineName: "localhost",
            services: new[] { "cardreader1", "cashdispenser1" },
            useTls: false
        );

        private void SimulatorMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                cardReaderSimulator.Start();
                cardReaderSimulator.OnStatusChange += CardReaderSimulator_StatusChange;

                var publisher = new ServicePublisher(
                    vendorName: "ACME ATM Hardware GmbH",
                    machineName: "localhost",
                    services: new[] { "cardreader1", "cashdispenser1" },
                    useTls: false
                );
                publisher.AddServiceUri(cardReaderSimulator.Url);
                _ = publisher.StartAsync();

                UpdateStatus();

                Utils utils = new Utils("SimulatorForm");
                utils.LogInfo(ToJson());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        public string ToJson(bool indented = true)
        {
            var opts = new JsonSerializerOptions { WriteIndented = indented, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            opts.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Serialize(cardReaderSimulator, opts);
        }

        #region Card Reader

        private void CardReaderSimulator_StatusChange(string status)
        {
            this.SafeInvoke(() =>
            {
                switch (status)
                {
                    case "ReaderEnabled":
                        InsertCardBtn.Enabled = true;
                        TakeCardBtn.Enabled = false;
                        break;

                    case "CardInserted":
                        InsertCardBtn.Enabled = false;
                        TakeCardBtn.Enabled = false;
                        break;

                    case "TakeCard":
                        InsertCardBtn.Enabled = false;
                        TakeCardBtn.Enabled = true;
                        break;
                }
                UpdateStatus();
            });
        }

        private void UpdateStatus()
        {
            CardStatusLbl.Text = cardReaderSimulator.DeviceStatus.ToString();
            CardReaderStatusLbl.Text = cardReaderSimulator.MediaStatus.ToString();
        }

        private void InsertCardBtn_Click(object sender, EventArgs e)
        {
            cardReaderSimulator.CardInserted = true;
            InsertCardBtn.Enabled = false;
        }

        private void TakeCardBtn_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}

public static class ControlExtensions
{
    public static void SafeInvoke(this Control control, Action action)
    {
        if (control.InvokeRequired)
            control.Invoke(action);
        else
            action();
    }
}
