using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer
{
    public partial class LogDetailsForm : Form
    {
        public LogDetailsForm(LogEntryView entry)
        {
            this.Text = "Log Entry Details";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10),
                Text = FormatEntry(entry)
            };

            this.Controls.Add(textBox);
        }

        private string FormatEntry(LogEntryView entry)
        {
            return
$@"ID:        {entry.Id}
Date:      {entry.Timestamp:yyyy-MM-dd}
Time:      {entry.Timestamp:HH:mm:ss.fff}
Level:     {entry.Level}
App:       {entry.Application}
Instance:  {entry.Instance}

Message:
{entry.Message}";
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // LogDetailsForm
            // 
            ClientSize = new Size(704, 484);
            Name = "LogDetailsForm";
            ResumeLayout(false);

        }
    }

}
