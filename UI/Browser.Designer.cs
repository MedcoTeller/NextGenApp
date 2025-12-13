namespace UI
{
    partial class EdgeBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            EdgeWebView2Browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)EdgeWebView2Browser).BeginInit();
            SuspendLayout();
            // 
            // EdgeBrowser
            // 
            EdgeWebView2Browser.AllowExternalDrop = false;
            EdgeWebView2Browser.CreationProperties = null;
            EdgeWebView2Browser.DefaultBackgroundColor = Color.White;
            EdgeWebView2Browser.Dock = DockStyle.Fill;
            EdgeWebView2Browser.Location = new Point(0, 0);
            EdgeWebView2Browser.Name = "EdgeBrowser";
            EdgeWebView2Browser.Size = new Size(1406, 781);
            EdgeWebView2Browser.TabIndex = 0;
            EdgeWebView2Browser.ZoomFactor = 1D;
            // 
            // Browser
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1406, 781);
            ControlBox = false;
            Controls.Add(EdgeWebView2Browser);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MinimizeBox = false;
            Name = "Browser";
            Text = "Browser";
            WindowState = FormWindowState.Maximized;
            Load += Browser_Load;
            ((System.ComponentModel.ISupportInitialize)EdgeWebView2Browser).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 EdgeWebView2Browser;
    }
}