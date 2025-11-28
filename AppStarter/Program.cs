using System.Threading.Tasks;
using System.Windows.Forms;
using UI;

namespace AppStarter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            try
            {
                await StartCustomerUIBrowser();
                //Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application Exception: {ex.Message}");
            }

        }

        private static async Task StartCustomerUIBrowser()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show($"Unhandled UI Exception: {e.Exception}");
            };

            Browser UIBrowser = null;
            var UiThread = new Thread(() =>
            {
                UIBrowser = new Browser();
                Application.Run(UIBrowser);
                //UIBrowser.ShowDialog();
            });
            UiThread.SetApartmentState(ApartmentState.STA);
            UiThread.Start();

            await Task.Delay(500);// Wait for the form to initialize
            UIBrowser?.Start();
            await Task.Delay(500);
            UIBrowser?.Navigate("https://chatgpt.com/");
        }
    }
}