namespace LogViewer
{
    partial class LogsViewer
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markSelectedToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Label labelSearch;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contextMarkToolStripMenuItem;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dataGridView1 = new DataGridView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            contextMarkToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openLogFileToolStripMenuItem = new ToolStripMenuItem();
            saveFiltredToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            actionsToolStripMenuItem = new ToolStripMenuItem();
            markSelectedToolStripMenuItem = new ToolStripMenuItem();
            viewDetailsToolStripMenuItem = new ToolStripMenuItem();
            filtersToolStripMenuItem = new ToolStripMenuItem();
            messageToolStripMenuItem = new ToolStripMenuItem();
            dateTimeToolStripMenuItem = new ToolStripMenuItem();
            levelToolStripMenuItem = new ToolStripMenuItem();
            componentToolStripMenuItem = new ToolStripMenuItem();
            typeToolStripMenuItem = new ToolStripMenuItem();
            applicationToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            clearFilterToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            updatesToolStripMenuItem = new ToolStripMenuItem();
            aboutUsToolStripMenuItem = new ToolStripMenuItem();
            contactUsToolStripMenuItem = new ToolStripMenuItem();
            textBoxSearch = new TextBox();
            labelSearch = new Label();
            saveFileDialog1 = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.BackgroundColor = SystemColors.Control;
            dataGridView1.ColumnHeadersHeight = 46;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.GridColor = SystemColors.Control;
            dataGridView1.Location = new Point(0, 78);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 82;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1664, 791);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.MouseDown += dataGridView1_MouseDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(32, 32);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { contextMarkToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(222, 42);
            // 
            // contextMarkToolStripMenuItem
            // 
            contextMarkToolStripMenuItem.Name = "contextMarkToolStripMenuItem";
            contextMarkToolStripMenuItem.Size = new Size(221, 38);
            contextMarkToolStripMenuItem.Text = "Toggle Mark";
            contextMarkToolStripMenuItem.Click += contextMarkToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(32, 32);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 872);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1664, 22);
            statusStrip1.TabIndex = 4;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(0, 12);
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, actionsToolStripMenuItem, filtersToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1664, 42);
            menuStrip1.TabIndex = 5;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openLogFileToolStripMenuItem, saveFiltredToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(71, 38);
            fileToolStripMenuItem.Text = "File";
            // 
            // openLogFileToolStripMenuItem
            // 
            openLogFileToolStripMenuItem.Name = "openLogFileToolStripMenuItem";
            openLogFileToolStripMenuItem.Size = new Size(296, 44);
            openLogFileToolStripMenuItem.Text = "Open Log File";
            openLogFileToolStripMenuItem.Click += openLogFileToolStripMenuItem_Click;
            // 
            // saveFiltredToolStripMenuItem
            // 
            saveFiltredToolStripMenuItem.Name = "saveFiltredToolStripMenuItem";
            saveFiltredToolStripMenuItem.Size = new Size(296, 44);
            saveFiltredToolStripMenuItem.Text = "Save Filtred";
            saveFiltredToolStripMenuItem.Click += SaveFiltredToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(296, 44);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // actionsToolStripMenuItem
            // 
            actionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { markSelectedToolStripMenuItem, viewDetailsToolStripMenuItem });
            actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            actionsToolStripMenuItem.Size = new Size(112, 38);
            actionsToolStripMenuItem.Text = "Actions";
            // 
            // markSelectedToolStripMenuItem
            // 
            markSelectedToolStripMenuItem.Name = "markSelectedToolStripMenuItem";
            markSelectedToolStripMenuItem.Size = new Size(280, 44);
            markSelectedToolStripMenuItem.Text = "Toggle Mark";
            markSelectedToolStripMenuItem.Click += markSelectedToolStripMenuItem_Click;
            // 
            // viewDetailsToolStripMenuItem
            // 
            viewDetailsToolStripMenuItem.Name = "viewDetailsToolStripMenuItem";
            viewDetailsToolStripMenuItem.Size = new Size(280, 44);
            viewDetailsToolStripMenuItem.Text = "View Details";
            viewDetailsToolStripMenuItem.Click += viewDetailsToolStripMenuItem_Click;
            // 
            // filtersToolStripMenuItem
            // 
            filtersToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { messageToolStripMenuItem, dateTimeToolStripMenuItem, levelToolStripMenuItem, componentToolStripMenuItem, typeToolStripMenuItem, applicationToolStripMenuItem, toolStripSeparator1, clearFilterToolStripMenuItem });
            filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            filtersToolStripMenuItem.Size = new Size(97, 38);
            filtersToolStripMenuItem.Text = "Filters";
            // 
            // messageToolStripMenuItem
            // 
            messageToolStripMenuItem.Name = "messageToolStripMenuItem";
            messageToolStripMenuItem.Size = new Size(359, 44);
            messageToolStripMenuItem.Text = "Message";
            messageToolStripMenuItem.Click += messageToolStripMenuItem_Click_1;
            // 
            // dateTimeToolStripMenuItem
            // 
            dateTimeToolStripMenuItem.Name = "dateTimeToolStripMenuItem";
            dateTimeToolStripMenuItem.Size = new Size(359, 44);
            dateTimeToolStripMenuItem.Text = "Date Time";
            dateTimeToolStripMenuItem.Click += dateTimeToolStripMenuItem_Click;
            // 
            // levelToolStripMenuItem
            // 
            levelToolStripMenuItem.Name = "levelToolStripMenuItem";
            levelToolStripMenuItem.Size = new Size(359, 44);
            levelToolStripMenuItem.Text = "Level";
            levelToolStripMenuItem.Click += levelToolStripMenuItem_Click_1;
            // 
            // componentToolStripMenuItem
            // 
            componentToolStripMenuItem.Name = "componentToolStripMenuItem";
            componentToolStripMenuItem.Size = new Size(359, 44);
            componentToolStripMenuItem.Text = "Component";
            componentToolStripMenuItem.Click += componentToolStripMenuItem_Click;
            // 
            // typeToolStripMenuItem
            // 
            typeToolStripMenuItem.Name = "typeToolStripMenuItem";
            typeToolStripMenuItem.Size = new Size(359, 44);
            typeToolStripMenuItem.Text = "Type";
            typeToolStripMenuItem.Click += typeToolStripMenuItem_Click;
            // 
            // applicationToolStripMenuItem
            // 
            applicationToolStripMenuItem.Name = "applicationToolStripMenuItem";
            applicationToolStripMenuItem.Size = new Size(359, 44);
            applicationToolStripMenuItem.Text = "Application";
            applicationToolStripMenuItem.Click += applicationToolStripMenuItem_Click_1;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(356, 6);
            // 
            // clearFilterToolStripMenuItem
            // 
            clearFilterToolStripMenuItem.Name = "clearFilterToolStripMenuItem";
            clearFilterToolStripMenuItem.Size = new Size(359, 44);
            clearFilterToolStripMenuItem.Text = "Clear Filter";
            clearFilterToolStripMenuItem.Click += clearFilterToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { updatesToolStripMenuItem, aboutUsToolStripMenuItem, contactUsToolStripMenuItem });
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(99, 38);
            aboutToolStripMenuItem.Text = "About";
            // 
            // updatesToolStripMenuItem
            // 
            updatesToolStripMenuItem.Name = "updatesToolStripMenuItem";
            updatesToolStripMenuItem.Size = new Size(260, 44);
            updatesToolStripMenuItem.Text = "Updates";
            // 
            // aboutUsToolStripMenuItem
            // 
            aboutUsToolStripMenuItem.Name = "aboutUsToolStripMenuItem";
            aboutUsToolStripMenuItem.Size = new Size(260, 44);
            aboutUsToolStripMenuItem.Text = "About us";
            // 
            // contactUsToolStripMenuItem
            // 
            contactUsToolStripMenuItem.Name = "contactUsToolStripMenuItem";
            contactUsToolStripMenuItem.Size = new Size(260, 44);
            contactUsToolStripMenuItem.Text = "Contact us";
            // 
            // textBoxSearch
            // 
            textBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSearch.Location = new Point(101, 40);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(1563, 39);
            textBoxSearch.TabIndex = 3;
            textBoxSearch.TextChanged += textBoxSearch_TextChanged;
            // 
            // labelSearch
            // 
            labelSearch.AutoSize = true;
            labelSearch.Location = new Point(10, 43);
            labelSearch.Name = "labelSearch";
            labelSearch.Size = new Size(90, 32);
            labelSearch.TabIndex = 2;
            labelSearch.Text = "Search:";
            // 
            // MainForm
            // 
            ClientSize = new Size(1664, 894);
            Controls.Add(dataGridView1);
            Controls.Add(labelSearch);
            Controls.Add(textBoxSearch);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Binary Log Viewer";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        private ToolStripMenuItem viewDetailsToolStripMenuItem;
        private ToolStripMenuItem saveFiltredToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem filtersToolStripMenuItem;
        private ToolStripMenuItem dateTimeToolStripMenuItem;
        private ToolStripMenuItem levelToolStripMenuItem;
        private ToolStripMenuItem componentToolStripMenuItem;
        private ToolStripMenuItem typeToolStripMenuItem;
        private ToolStripMenuItem applicationToolStripMenuItem;
        private ToolStripMenuItem messageToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem clearFilterToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem updatesToolStripMenuItem;
        private ToolStripMenuItem aboutUsToolStripMenuItem;
        private ToolStripMenuItem contactUsToolStripMenuItem;
    }
}
