using GlobalShared;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace UI
{
    public partial class EdgeBrowser : Form
    {
        public EdgeBrowser()
        {
            InitializeComponent();
        }

        private readonly Utils utils = new Utils("EdgeBrowser");

        /// <summary>
        /// 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string StartupUrl { get; set; }

        //public int MyProperty { get; set; }

        private async void Browser_Load(object sender, EventArgs e)
        {
            //await EdgeBrowser.EnsureCoreWebView2Async();
            //EdgeBrowser.CoreWebView2.Navigate("https://www.google.com");
        }

        public void Start()
        {
            Invoke(new Action(async () =>
            {
                await EdgeWebView2Browser.EnsureCoreWebView2Async();
                //EdgeBrowser.CoreWebView2.Navigate("https://www.google.com");
            }
            ));
        }

        public void Navigate(string url)
        {
            Invoke(new Action(() =>
            {
                if (EdgeWebView2Browser.CoreWebView2 != null)
                {
                    EdgeWebView2Browser.CoreWebView2.Navigate(url);
                }
                else
                    utils.LogWarning("CoreWebView2 is not initialized yet.");
            }
            ));
                //throw new NotImplementedException();
        }

        public async Task<(bool success, string? value)> GetInputAsync(int timeoutMs)
        {
            var tcs = new TaskCompletionSource<(bool, string?)>();
            void Handler(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
            {
                var msg = e.TryGetWebMessageAsString();
                // Complete with success
                tcs.TrySetResult((true, msg));
            }
            // Timeout
            var timeoutTask = Task.Delay(timeoutMs);
            (bool, string?) result = (false, null);

            Invoke(new Action(async () =>
            {
                try
                {
                    // Subscribe (one-time)
                    EdgeWebView2Browser.CoreWebView2.WebMessageReceived += Handler;

                    var completed = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Check timeout
                    if (completed == timeoutTask)
                    {
                        result = (false, null); // Timeout
                    }

                    result = await tcs.Task;
                }
                finally
                {
                    // Cleanup subscription
                    EdgeWebView2Browser.CoreWebView2.WebMessageReceived -= Handler;
                }
                /*
                 JavaScript example to send inpput to C#:
                 function userClicked(value) {
                    chrome.webview.postMessage(value);
                 }
                 */
            }
            ));
            return result;
        }

        public void ExecuteScript(string script)
        {
            Invoke(new Action(async () =>
            {
                if (EdgeWebView2Browser.CoreWebView2 != null)
                {
                    await EdgeWebView2Browser.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                    utils.LogWarning("CoreWebView2 is not initialized yet.");
            }
            ));
        }

        public async Task CallFunctionAsync(string func, List<object> parameters) {
            Invoke(new Action(async () =>
            {
                if (EdgeWebView2Browser.CoreWebView2 != null)
                {
                    await EdgeWebView2Browser.CoreWebView2.ExecuteScriptAsync($"window.{func}('{parameters}');");
                }
                else
                    utils.LogWarning("CoreWebView2 is not initialized yet.");
            }
            ));
        }

        public void SendDataToWebView(object data)
        {
            Invoke(new Action(async () =>
            {
                if (EdgeWebView2Browser.CoreWebView2 != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    EdgeWebView2Browser.CoreWebView2.PostWebMessageAsJson(json);
                }
                else
                    utils.LogWarning("CoreWebView2 is not initialized yet.");
            }
            ));
        }

        public async Task CallFunctionAsync(string func, object obj)
        {
            Invoke(new Action(async () =>
            {
                var json = JsonSerializer.Serialize(obj);
                await EdgeWebView2Browser.CoreWebView2.ExecuteScriptAsync(
                    $"window.{func}({json});"
                );

                /*\
                 * Javascript example to receive config from C#:
                 window.loadConfig = function(cfg) {
                    console.log("Config from C#:", cfg);
                 };             
                 /*/
            }
            ));
        }

        public async Task PreloadObjectAsync(object data)
        {
            Invoke(new Action(async () =>
            {
                var json = JsonSerializer.Serialize(data);
            var script = $"window.__startupData = {json};";
            await EdgeWebView2Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

            // JavaScript example to access preloaded data:
            //const session = window.__startupData;
            }
            ));
        }

        public async Task NavigateWithData(string url, object data)
        {
            await PreloadObjectAsync(data);
            Navigate(url);
        }

    }


}
