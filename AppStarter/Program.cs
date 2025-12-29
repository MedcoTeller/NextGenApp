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
                
                var UI = await StartCustomerUIBrowser();
                await UI.Start();
                await UI.Navigate("https://google.com/");
                //Application.Run();
            }
            catch (Exception ex)
            {
                utils.LogWarning($"Application Exception: {ex.Message}");
            }
        }

        private static async Task<EdgeBrowser> StartCustomerUIBrowser()
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
            });
            UiThread.SetApartmentState(ApartmentState.STA);
            UiThread.Start();

            await Task.Delay(400);// Wait for the form to initialize
            //UIBrowser?.Start();

            //UIBrowser?.Navigate("https://google.com/");
            return UIBrowser;
        }
    }
}