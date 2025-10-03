using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace LogViewer
{
    public partial class LogsViewer : Form
    {
        private string currentLogFile = string.Empty;
        private long lastReadPosition = 0;


        // Master list holds all logs
        private List<LogEntryView> allEntries = new List<LogEntryView>();

        // Filtered/bound list for UI
        private BindingList<LogEntryView> visibleEntries = new BindingList<LogEntryView>();

        private HashSet<Guid> markedIds = new HashSet<Guid>();

        private System.Windows.Forms.Timer? refreshTimer;
        private int SelectedRowIndex = 0;
        private LogFilters filters = new LogFilters();


        /// <summary>
        /// 
        /// </summary>
        public LogsViewer()
        {
            InitializeComponent();
            SetupGridColumns();

            dataGridView1.DataSource = visibleEntries;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;

            SetupTimer();

            if (!File.Exists(@"C:\ProgramData\NextGen\Logs\ATMAppLog.bin"))
                return;
            currentLogFile = $@"C:\ProgramData\NextGen\Logs\ATMAppLog.bin";
            lastReadPosition = 0;
            allEntries.Clear();
            visibleEntries.Clear();
            LoadNewLogs();
        }

        private void SetupTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += (s, e) => LoadNewLogs();
            refreshTimer.Start();
        }

        private void SetupGridColumns()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Timestamp", HeaderText = "Timestamp", Width = 150 });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Level", HeaderText = "Level", Width = 60 });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Application", HeaderText = "Application", Width = 100 });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Instance", HeaderText = "Instance", Width = 100 });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Message", HeaderText = "Message", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            var chkCol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Marked",
                HeaderText = "Marked",
                Width = 50
            };
            dataGridView1.Columns.Add(chkCol);

            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void LoadNewLogs()
        {
            if (string.IsNullOrWhiteSpace(currentLogFile) || !File.Exists(currentLogFile))
                return;

            using var fs = new FileStream(currentLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(lastReadPosition, SeekOrigin.Begin);
            using var br = new BinaryReader(fs);

            bool newDataAdded = false;

            while (fs.Position + 249 <= fs.Length)
            {
                var id = new Guid(br.ReadBytes(16));
                var timestamp = new DateTime(br.ReadInt64(), DateTimeKind.Local);
                var level = (LogLevel)br.ReadByte();
                var app = Encoding.UTF8.GetString(br.ReadBytes(32)).TrimEnd();
                var instance = Encoding.UTF8.GetString(br.ReadBytes(32)).TrimEnd();
                var message = Encoding.UTF8.GetString(br.ReadBytes(160)).TrimEnd();

                if (allEntries.Any(e => e.Id == id))
                    continue; // skip duplicates if any

                var entry = new LogEntryView
                {
                    Id = id,
                    Timestamp = timestamp,
                    Level = level,
                    Application = app,
                    Instance = instance,
                    Message = message,
                    Marked = markedIds.Contains(id)
                };

                allEntries.Add(entry);
                newDataAdded = true;
            }

            lastReadPosition = fs.Position;

            if (newDataAdded)
                ApplyFilter();
        }

        private void ApplyFilter()
        {
            // Save scroll & selection
            int firstDisplayedRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex >= 0
                ? dataGridView1.FirstDisplayedScrollingRowIndex : 0;

            DataGridViewRow? selectedRow = dataGridView1.CurrentRow;
            Guid? selectedId = selectedRow?.DataBoundItem is LogEntryView selectedEntry
                ? selectedEntry.Id
                : null;

            string searchTerm = textBoxSearch.Text.Trim().ToLower();

            var filtered = allEntries.Where(e =>
                e.Marked || (e.Message.ToLower().Contains(searchTerm))).ToList();

            visibleEntries.RaiseListChangedEvents = false;
            visibleEntries.Clear();
            foreach (var e in filtered)
                visibleEntries.Add(e);
            visibleEntries.RaiseListChangedEvents = true;
            visibleEntries.ResetBindings();

            // Restore scroll
            if (firstDisplayedRowIndex < dataGridView1.RowCount)
            {
                try
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex;
                }
                catch { }
            }

            // Restore selection
            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.DataBoundItem is LogEntryView entry && entry.Id == selectedId.Value)
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }

            toolStripStatusLabel1.Text = $"Loaded: {allEntries.Count}, Visible: {visibleEntries.Count}, Marked: {markedIds.Count}";
        }

        private void ApplyFilters()
        {
            int firstDisplayedRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex >= 0
                ? dataGridView1.FirstDisplayedScrollingRowIndex : 0;

            var selectedId = dataGridView1.CurrentRow?.DataBoundItem is LogEntryView selectedEntry
                ? selectedEntry.Id
                : (Guid?)null;

            var filtered = allEntries.Where(e => e.Marked || filters.IsMatch(e)).ToList();

            visibleEntries.RaiseListChangedEvents = false;
            visibleEntries.Clear();
            foreach (var e in filtered)
                visibleEntries.Add(e);
            visibleEntries.RaiseListChangedEvents = true;
            visibleEntries.ResetBindings();

            if (firstDisplayedRowIndex < dataGridView1.RowCount)
            {
                try { dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex; } catch { }
            }

            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.DataBoundItem is LogEntryView entry && entry.Id == selectedId.Value)
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }

            toolStripStatusLabel1.Text = $"Loaded: {allEntries.Count}, Visible: {visibleEntries.Count}, Marked: {markedIds.Count}";
        }


        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            filters.Message = textBoxSearch.Text.Trim();
            ApplyFilters();
        }

        private void ToggleMarkSelected()
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (row.DataBoundItem is LogEntryView entry)
                {
                    entry.Marked = !entry.Marked;
                    if (entry.Marked)
                        markedIds.Add(entry.Id);
                    else
                        markedIds.Remove(entry.Id);
                }
            }
            dataGridView1.Refresh();
            //sApplyFilters();
        }

        private void markSelectedToolStripMenuItem_Click(object sender, EventArgs e) => ToggleMarkSelected();
        private void contextMarkToolStripMenuItem_Click(object sender, EventArgs e) => ToggleMarkSelected();

        private void dataGridView1_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].DataBoundItem is LogEntryView entry && entry.Marked)
            {
                e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                e.CellStyle.BackColor = Color.LightYellow;
            }
            if (dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "Timestamp")
            {
                if (e.Value is DateTime dt)
                    e.Value = dt.ToString("yyyy-MM-dd HH:mm:ss.fff"); // or "HH:mm:ss.fffffff" for more
            }
        }

        // Needed to commit checkbox change immediately when clicked
        private void DataGridView1_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataGridView1_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].DataPropertyName == "Marked")
            {
                var entry = dataGridView1.Rows[e.RowIndex].DataBoundItem as LogEntryView;
                if (entry != null)
                {
                    if (entry.Marked)
                        markedIds.Add(entry.Id);
                    else
                        markedIds.Remove(entry.Id);

                    ApplyFilters();
                }
            }
        }

        private void openLogFileToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\ProgramData\NextGen\Logs";
            ofd.Filter = "Log Files|Log_*.bin|All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentLogFile = ofd.FileName;
                lastReadPosition = 0;
                allEntries.Clear();
                visibleEntries.Clear();
                LoadNewLogs();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SelectedRowIndex = e.RowIndex;
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridView1.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hitTest.RowIndex].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[hitTest.RowIndex].Cells[hitTest.ColumnIndex];
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].DataBoundItem is LogEntryView entry)
            {
                var detailsForm = new LogDetailsForm(entry);
                detailsForm.ShowDialog();
            }
        }

        private void viewDetailsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (SelectedRowIndex >= 0 && dataGridView1.Rows[SelectedRowIndex].DataBoundItem is LogEntryView entry)
            {
                var detailsForm = new LogDetailsForm(entry);
                detailsForm.ShowDialog();
            }
        }

        private void dataGridView1_SelectionChanged(object? sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void WriteLogEntry(Guid id, DateTime timestamp, LogLevel level, string application, string instance, string message, BinaryWriter writer)
        {
            writer.Write(id.ToByteArray());
            writer.Write(timestamp.ToUniversalTime().Ticks);
            writer.Write((byte)level);
            writer.Write(Encoding.UTF8.GetBytes(application.PadRight(32).Substring(0, 32)));
            writer.Write(Encoding.UTF8.GetBytes(instance.PadRight(32).Substring(0, 32)));
            writer.Write(Encoding.UTF8.GetBytes(message.PadRight(160).Substring(0, 160)));
            writer.Flush();
        }

        private async Task<bool> SaveFiltred()
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return await Task.FromResult(false);

            var currentFileName = $"{saveFileDialog1.FileName}.bin";
            using var stream = new FileStream(currentFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new BinaryWriter(stream);

            try
            {
                foreach (var entry in visibleEntries)
                {
                    WriteLogEntry(entry.Id, entry.Timestamp, entry.Level,
                        entry.Application ?? string.Empty,
                        entry.Instance ?? string.Empty,
                        entry.Message ?? string.Empty, writer);
                }
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
            finally
            {
                writer.Close();
                stream.Close();
            }
        }

        private void applicationToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string value = filters.Application ?? "";
            if (ShowInputDialog(ref value, "Filter by Application") == DialogResult.OK)
            {
                filters.Application = value;
                ApplyFilters();
            }
        }

        private void SaveFiltredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFiltred().GetAwaiter().GetResult();
        }

        private void clearFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            filters.Clear();
            ApplyFilters();
        }


        public static DialogResult ShowInputDialog(ref string input, string prompt, string title = "Input Box", int width = 300, int height = 150)
        {
            Form form = new Form();
            form.Text = title;
            form.ClientSize = new Size(width, height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;

            Label label = new Label() { Left = 10, Top = 10, Text = prompt, Width = width - 20 };
            TextBox textBox = new TextBox() { Left = 10, Top = 40, Width = width - 20, Text = input };
            Button ok = new Button() { Text = "OK", Left = width - 170, Width = 75, Top = height - 40, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = width - 90, Width = 75, Top = height - 40, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] { label, textBox, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            var dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK)
                input = textBox.Text;
            return dialogResult;
        }

        private void componentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string value = filters.Instance ?? "";
            if (ShowInputDialog(ref value, "Filter by Instance") == DialogResult.OK)
            {
                filters.Instance = value;
                ApplyFilters();
            }
        }

        private void messageToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string value = filters.Message ?? "";
            if (ShowInputDialog(ref value, "Filter by Message") == DialogResult.OK)
            {
                filters.Message = value;
                ApplyFilters();
            }
        }

        private void dateTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string from = filters.FromDate?.ToString("yyyy-MM-dd") ?? "";
            string to = filters.ToDate?.ToString("yyyy-MM-dd") ?? "";

            if (ShowInputDialog(ref from, "From Date (yyyy-MM-dd)") == DialogResult.OK &&
                ShowInputDialog(ref to, "To Date (yyyy-MM-dd)") == DialogResult.OK)
            {
                if (DateTime.TryParse(from, out var fromDate)) filters.FromDate = fromDate;
                if (DateTime.TryParse(to, out var toDate)) filters.ToDate = toDate;
                ApplyFilters();
            }
        }

        private void levelToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var level = filters.Level?.ToString() ?? "";
            if (ShowInputDialog(ref level, "Filter by Level (e.g. Info, Error)") == DialogResult.OK)
            {
                if (Enum.TryParse<LogLevel>(level, true, out var parsedLevel))
                {
                    filters.Level = parsedLevel;
                    ApplyFilters();
                }
            }
        }

        private void typeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }

    class LogFilters
    {
        public string? Application { get; set; }
        public string? Instance { get; set; }
        public LogLevel? Level { get; set; }
        public string? Message { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public void Clear()
        {
            Application = null;
            Instance = null;
            Level = null;
            Message = null;
            FromDate = null;
            ToDate = null;
        }

        public bool IsMatch(LogEntryView entry)
        {
            if (Application != null && !entry.Application?.ToLower().Contains(Application.ToLower()) == true)
                return false;

            if (Instance != null && !entry.Instance?.ToLower().Contains(Instance.ToLower()) == true)
                return false;

            if (Level != null && entry.Level != Level)
                return false;

            if (Message != null && !entry.Message?.ToLower().Contains(Message.ToLower()) == true)
                return false;

            if (FromDate != null && entry.Timestamp < FromDate)
                return false;

            if (ToDate != null && entry.Timestamp > ToDate)
                return false;

            return true;
        }
    }


    public class LogEntryView
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string? Application { get; set; }
        public string? Instance { get; set; }
        public string? Message { get; set; }
        public bool Marked { get; set; }
    }

    public enum LogLevel : byte
    {
        Trace, Debug, Info, Warn, Error, Fatal
    }
}
