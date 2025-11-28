namespace UI
{
    partial class Browser
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
            EdgeBrowser = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)EdgeBrowser).BeginInit();
            SuspendLayout();
            // 
            // EdgeBrowser
            // 
            EdgeBrowser.AllowExternalDrop = false;
            EdgeBrowser.CreationProperties = null;
            EdgeBrowser.DefaultBackgroundColor = Color.White;
            EdgeBrowser.Dock = DockStyle.Fill;
            EdgeBrowser.Location = new Point(0, 0);
            EdgeBrowser.Name = "EdgeBrowser";
            EdgeBrowser.Size = new Size(1406, 781);
            EdgeBrowser.TabIndex = 0;
            EdgeBrowser.ZoomFactor = 1D;
            // 
            // Browser
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1406, 781);
            ControlBox = false;
            Controls.Add(EdgeBrowser);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MinimizeBox = false;
            Name = "Browser";
            Text = "Browser";
            WindowState = FormWindowState.Maximized;
            Load += Browser_Load;
            ((System.ComponentModel.ISupportInitialize)EdgeBrowser).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 EdgeBrowser;
    }
}