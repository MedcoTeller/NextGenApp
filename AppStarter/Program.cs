using GlobalShared;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI;

namespace AppStarter
{
    internal static class Program
    {

        static Utils utils = new Utils("AppStarter");

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
                utils.LogWarning($"Application Exception: {ex.Message}");
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
                utils.LogWarning($"Unhandled UI Exception: {e.Exception}");
            };

            EdgeBrowser UIBrowser = null;
            var UiThread = new Thread(() =>
            {
                UIBrowser = new EdgeBrowser();
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