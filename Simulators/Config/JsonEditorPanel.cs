using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Simulators.Config
{
    public partial class JsonEditorPanel : UserControl
    {
        private JsonNode? _currentNode;
        private TreeNode? _treeNode;

        private Label lblPath;
        private TextBox txtValue;
        private ComboBox cmbType;
        private Button btnApply;
        private Button btnDelete;
        private Label lblType;
        private Label lblValue;

        public event EventHandler? NodeChanged;

        public JsonEditorPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            lblPath = new Label();
            lblType = new Label();
            cmbType = new ComboBox();
            lblValue = new Label();
            txtValue = new TextBox();
            btnApply = new Button();
            btnDelete = new Button();
            SuspendLayout();
            // 
            // lblPath
            // 
            lblPath.Location = new Point(3, 12);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(479, 47);
            lblPath.TabIndex = 0;
            lblPath.Text = "lbl";
            // 
            // lblType
            // 
            lblType.Location = new Point(3, 81);
            lblType.Name = "lblType";
            lblType.Size = new Size(131, 45);
            lblType.TabIndex = 1;
            lblType.Text = "type";
            // 
            // cmbType
            // 
            cmbType.Items.AddRange(new object[] { "string", "int", "double", "bool", "null" });
            cmbType.Location = new Point(152, 81);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(330, 40);
            cmbType.TabIndex = 2;
            // 
            // lblValue
            // 
            lblValue.Location = new Point(3, 126);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(131, 40);
            lblValue.TabIndex = 3;
            lblValue.Text = "Value";
            // 
            // txtValue
            // 
            txtValue.Location = new Point(152, 127);
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(330, 39);
            txtValue.TabIndex = 4;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(112, 196);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(126, 43);
            btnApply.TabIndex = 5;
            btnApply.Text = "Apply";
            btnApply.Click += BtnApply_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(257, 196);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(126, 43);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "Delete";
            btnDelete.Click += BtnDelete_Click;
            // 
            // JsonEditorPanel
            // 
            Controls.Add(lblPath);
            Controls.Add(lblType);
            Controls.Add(cmbType);
            Controls.Add(lblValue);
            Controls.Add(txtValue);
            Controls.Add(btnApply);
            Controls.Add(btnDelete);
            Name = "JsonEditorPanel";
            Size = new Size(503, 269);
            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Displays the selected JsonNode for editing.
        /// </summary>
        public void LoadNode(TreeNode treeNode)
        {
            _treeNode = treeNode;
            _currentNode = treeNode.Tag as JsonNode;

            if (_currentNode == null)
            {
                lblPath.Text = "Invalid node";
                txtValue.Text = "";
                return;
            }

            lblPath.Text = $"Editing: {treeNode.FullPath}";
            txtValue.Enabled = true;
            btnApply.Enabled = true;
            btnDelete.Enabled = true;

            if (_currentNode is JsonValue val)
            {
                string json = val.ToJsonString();
                txtValue.Text = json.Trim('"');
                cmbType.SelectedItem = DetectType(val);
            }
            else if (_currentNode is JsonArray)
            {
                txtValue.Text = "(Array)";
                txtValue.Enabled = false;
                cmbType.SelectedItem = "array";
            }
            else if (_currentNode is JsonObject)
            {
                txtValue.Text = "(Object)";
                txtValue.Enabled = false;
                cmbType.SelectedItem = "object";
            }
        }

        private string DetectType(JsonValue val)
        {
            var json = val.ToJsonString();
            if (json == "true" || json == "false") return "bool";
            if (int.TryParse(json, out _)) return "int";
            if (double.TryParse(json, out _)) return "double";
            return "string";
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (_currentNode == null || _treeNode == null)
                return;

            string? type = cmbType.SelectedItem?.ToString();
            string text = txtValue.Text;

            try
            {
                JsonNode? newValue = type switch
                {
                    "string" => JsonValue.Create(text),
                    "int" => JsonValue.Create(int.Parse(text)),
                    "double" => JsonValue.Create(double.Parse(text)),
                    "bool" => JsonValue.Create(bool.Parse(text)),
                    "null" => null,
                    _ => JsonValue.Create(text)
                };

                // Replace in parent
                if (_treeNode.Parent?.Tag is JsonObject parentObj)
                {
                    string key = GetNodeKey(_treeNode.Text);
                    parentObj[key] = newValue;
                }
                else if (_treeNode.Parent?.Tag is JsonArray parentArr)
                {
                    int index = GetArrayIndex(_treeNode.Text);
                    parentArr[index] = newValue;
                }

                // Update UI
                _treeNode.Tag = newValue;
                _treeNode.Text = JsonTreeHelper.FormatNodeLabel(newValue!, GetNodeKey(_treeNode.Text));

                NodeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update value: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_currentNode == null || _treeNode == null || _treeNode.Parent == null)
                return;

            if (MessageBox.Show("Delete this property?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            if (_treeNode.Parent.Tag is JsonObject parentObj)
            {
                string key = GetNodeKey(_treeNode.Text);
                parentObj.Remove(key);
            }
            else if (_treeNode.Parent.Tag is JsonArray parentArr)
            {
                int index = GetArrayIndex(_treeNode.Text);
                if (index >= 0 && index < parentArr.Count)
                    parentArr.RemoveAt(index);
            }

            _treeNode.Remove();
            NodeChanged?.Invoke(this, EventArgs.Empty);
        }

        private string GetNodeKey(string label)
        {
            int idx = label.IndexOf(':');
            return idx > 0 ? label.Substring(0, idx).Trim() : label.Replace("[", "").Replace("]", "");
        }

        private int GetArrayIndex(string label)
        {
            if (label.StartsWith("[") && label.EndsWith("]") && int.TryParse(label.Trim('[', ']'), out int idx))
                return idx;
            if (label.StartsWith("[") && int.TryParse(label.Substring(1, label.IndexOf(']') - 1), out idx))
                return idx;
            return -1;
        }
    }
}
