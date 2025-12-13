using GlobalShared;
using System.Text.Json;
using UI;

namespace AppCommon
{
    public class UserUI
    {
        private readonly static Utils utils = new Utils("UserUI");
        public static EdgeBrowser edgeBrowser;

        public UserUI(EdgeBrowser browser)
        {
            //edgeBrowser = browser;
        }

        public static void NavigateToUrl(string url)
        {
            utils.LogInfo($"Navigating to URL: {url}");
            edgeBrowser.Navigate(url);
        }

        public static async Task NavigateWithData(string url, object data)
        {
            utils.LogInfo($"Navigating to URL: {url} with data.");
            await edgeBrowser.NavigateWithData(url, data);
        }

        public static async Task<(bool success, string? value)> GetInputAsync(int timeoutMs)
        {
            utils.LogInfo($"Getting input with timeout: {timeoutMs} ms");
            return await edgeBrowser.GetInputAsync(timeoutMs);
        }

        public static async Task CallFunctionWithDataAsync(string func, object data)
        {
            utils.LogInfo($"Calling function: {func} with data.");
            await edgeBrowser.CallFunctionAsync(func, data);
        }
    }
}
