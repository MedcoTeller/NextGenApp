using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UI
{
    public partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string StartupUrl { get; set; }

        private async void Browser_Load(object sender, EventArgs e)
        {
            //await EdgeBrowser.EnsureCoreWebView2Async();
            //EdgeBrowser.CoreWebView2.Navigate("https://www.google.com");
        }

        public void Start()
        {
            Invoke(new Action(async () =>
            {
                await EdgeBrowser.EnsureCoreWebView2Async();
                //EdgeBrowser.CoreWebView2.Navigate("https://www.google.com");
            }
            ));
        }


        public void Navigate(string url)
        {
            Invoke(new Action(() =>
            {
                if (EdgeBrowser.CoreWebView2 != null)
                {
                    EdgeBrowser.CoreWebView2.Navigate(url);
                }
                else
                    MessageBox.Show("CoreWebView2 is not initialized yet.");
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

            try
            {
                // Subscribe (one-time)
                EdgeBrowser.CoreWebView2.WebMessageReceived += Handler;

                var completed = await Task.WhenAny(tcs.Task, timeoutTask);

                // Check timeout
                if (completed == timeoutTask)
                {
                    return (false, null); // Timeout
                }

                return await tcs.Task;
            }
            finally
            {
                // Cleanup subscription
                EdgeBrowser.CoreWebView2.WebMessageReceived -= Handler;
            }
        }

        public void ExecuteScript(string script)
        {
            Invoke(new Action(async () =>
            {
                if (EdgeBrowser.CoreWebView2 != null)
                {
                    await EdgeBrowser.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                    MessageBox.Show("CoreWebView2 is not initialized yet.");
            }
            ));
        }

        public void CallFunctionAsync(string func, List<object> parameters) {
            Invoke(new Action(async () =>
            {
                if (EdgeBrowser.CoreWebView2 != null)
                {
                    await EdgeBrowser.CoreWebView2.ExecuteScriptAsync($"window.{func}('{parameters}');");
                }
                else
                    MessageBox.Show("CoreWebView2 is not initialized yet.");
            }
            ));
        }
    }
}
