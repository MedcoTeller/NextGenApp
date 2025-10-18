using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace Simulators.Config
{
    public partial class ConfigForm : Form
    {
        private JsonNode? _rootNode;
        private string? _currentFilePath;

        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem exitMenu;
        private SplitContainer splitContainer;
        private TreeView treeJson;
        private JsonEditorPanel jsonEditorPanel;
        private StatusStrip statusBar;
        private ToolStripStatusLabel statusLabel;
        private ContextMenuStrip contextMenu;
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            components = new Container();
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            openMenu = new ToolStripMenuItem();
            saveMenu = new ToolStripMenuItem();
            saveAsMenu = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitMenu = new ToolStripMenuItem();
            splitContainer = new SplitContainer();
            treeJson = new TreeView();
            contextMenu = new ContextMenuStrip(components);
            addMenu = new ToolStripMenuItem();
            deleteMenu = new ToolStripMenuItem();
            jsonEditorPanel = new JsonEditorPanel();
            statusBar = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            openToolStripMenuItem = new ToolStripMenuItem();
            ygggyToolStripMenuItem = new ToolStripMenuItem();
            menuStrip.SuspendLayout();
            ((ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenu.SuspendLayout();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(32, 32);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1093, 40);
            menuStrip.TabIndex = 0;
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openMenu, saveMenu, saveAsMenu, toolStripSeparator1, exitMenu });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(71, 36);
            fileMenu.Text = "File";
            // 
            // openMenu
            // 
            openMenu.Name = "openMenu";
            openMenu.Size = new Size(222, 44);
            openMenu.Text = "Open";
            openMenu.Click += OpenMenu_Click;
            // 
            // saveMenu
            // 
            saveMenu.Name = "saveMenu";
            saveMenu.Size = new Size(222, 44);
            saveMenu.Text = "Save";
            saveMenu.Click += SaveMenu_Click;
            // 
            // saveAsMenu
            // 
            saveAsMenu.Name = "saveAsMenu";
            saveAsMenu.Size = new Size(222, 44);
            saveAsMenu.Text = "SaveAs";
            saveAsMenu.Click += SaveAsMenu_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(219, 6);
            // 
            // exitMenu
            // 
            exitMenu.Name = "exitMenu";
            exitMenu.Size = new Size(222, 44);
            exitMenu.Text = "Exit";
            exitMenu.Click += exitMenu_Click;
            // 
            // splitContainer
            // 
            splitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer.Location = new Point(0, 43);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(treeJson);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(jsonEditorPanel);
            splitContainer.Size = new Size(1093, 647);
            splitContainer.SplitterDistance = 464;
            splitContainer.TabIndex = 1;
            // 
            // treeJson
            // 
            treeJson.ContextMenuStrip = contextMenu;
            treeJson.Dock = DockStyle.Fill;
            treeJson.Location = new Point(0, 0);
            treeJson.Name = "treeJson";
            treeJson.Size = new Size(464, 647);
            treeJson.TabIndex = 0;
            treeJson.AfterSelect += TreeJson_AfterSelect;
            // 
            // contextMenu
            // 
            contextMenu.ImageScalingSize = new Size(32, 32);
            contextMenu.Items.AddRange(new ToolStripItem[] { addMenu, deleteMenu });
            contextMenu.Name = "contextMenu";
            contextMenu.Size = new Size(301, 124);
            // 
            // addMenu
            // 
            addMenu.Name = "addMenu";
            addMenu.Size = new Size(300, 38);
            addMenu.Text = "Add key/Value";
            addMenu.Click += addMenu_Click;
            // 
            // deleteMenu
            // 
            deleteMenu.Name = "deleteMenu";
            deleteMenu.Size = new Size(300, 38);
            deleteMenu.Text = "Delete";
            deleteMenu.Click += deleteMenu_Click;
            // 
            // jsonEditorPanel
            // 
            jsonEditorPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            jsonEditorPanel.Location = new Point(3, 3);
            jsonEditorPanel.Name = "jsonEditorPanel";
            jsonEditorPanel.Size = new Size(622, 251);
            jsonEditorPanel.TabIndex = 0;
            jsonEditorPanel.NodeChanged += JsonEditorPanel_NodeChanged;
            // 
            // statusBar
            // 
            statusBar.ImageScalingSize = new Size(32, 32);
            statusBar.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusBar.Location = new Point(0, 693);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(1093, 42);
            statusBar.TabIndex = 2;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(78, 32);
            statusLabel.Text = "Ready";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(359, 44);
            openToolStripMenuItem.Text = "open";
            // 
            // ygggyToolStripMenuItem
            // 
            ygggyToolStripMenuItem.Name = "ygggyToolStripMenuItem";
            ygggyToolStripMenuItem.Size = new Size(359, 44);
            ygggyToolStripMenuItem.Text = "ygggy";
            // 
            // ConfigForm
            // 
            ClientSize = new Size(1093, 735);
            Controls.Add(menuStrip);
            Controls.Add(splitContainer);
            Controls.Add(statusBar);
            MainMenuStrip = menuStrip;
            Name = "ConfigForm";
            Text = "ConfigManager - JSON Editor";
            FormClosing += MainForm_FormClosing;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenu.ResumeLayout(false);
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // === Event Handlers ===
        private void OpenMenu_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string json = File.ReadAllText(dlg.FileName);
                _rootNode = JsonNode.Parse(json, new JsonNodeOptions { PropertyNameCaseInsensitive = true });
                _currentFilePath = dlg.FileName;
                LoadTree();
                statusLabel.Text = $"Loaded: {Path.GetFileName(dlg.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMenu_Click(object? sender, EventArgs e)
        {
            if (_currentFilePath == null)
            {
                SaveAsMenu_Click(sender, e);
                return;
            }

            try
            {
                string json = _rootNode?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "{}";
                File.WriteAllText(_currentFilePath, json);
                statusLabel.Text = $"Saved: {Path.GetFileName(_currentFilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAsMenu_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = Path.GetFileName(_currentFilePath ?? "config.json")
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            _currentFilePath = dlg.FileName;
            SaveMenu_Click(sender, e);
        }

        private void TreeJson_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is JsonNode node)
            {
                jsonEditorPanel.LoadNode(e.Node);
            }
        }

        private void JsonEditorPanel_NodeChanged(object? sender, EventArgs e)
        {
            // Rebuild tree for consistent display (optional)
            if (_rootNode != null)
            {
                LoadTree();
            }
        }

        private void LoadTree()
        {
            treeJson.BeginUpdate();
            treeJson.Nodes.Clear();
            if (_rootNode != null)
            {
                JsonTreeHelper.PopulateTree(treeJson, _rootNode);
                treeJson.ExpandAll();
            }
            treeJson.EndUpdate();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_rootNode == null) return;

            var result = MessageBox.Show("Do you want to save changes before exiting?", "Confirm",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                SaveMenu_Click(sender, e);
            else if (result == DialogResult.Cancel)
                e.Cancel = true;
        }

        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem ygggyToolStripMenuItem;
        private ToolStripMenuItem openMenu;
        private ToolStripMenuItem saveMenu;
        private ToolStripMenuItem saveAsMenu;
        private ToolStripSeparator toolStripSeparator1;


        private void exitMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private IContainer components;
        private ToolStripMenuItem addMenu;
        private ToolStripMenuItem deleteMenu;

        private void addMenu_Click(object sender, EventArgs e)
        {
            if (treeJson.SelectedNode?.Tag is not JsonNode selectedNode)
                return;

            using var dlg = new AddKeyValueForm(selectedNode);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            JsonNode? newNode = dlg.CreatedNode;
            string? key = dlg.NewKey;

            if (selectedNode is JsonObject obj)
            {
                if (key == null)
                {
                    MessageBox.Show("Key required for object.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                obj[key] = newNode;
            }
            else if (selectedNode is JsonArray arr)
            {
                arr.Add(newNode);
            }
            else
            {
                MessageBox.Show("Can only add inside objects or arrays.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadTree(); // Refresh
            statusLabel.Text = $"Added {(key ?? "item")} successfully.";
        }

        private void deleteMenu_Click(object sender, EventArgs e)
        {
            if (treeJson.SelectedNode?.Tag is not JsonNode node)
                return;

            var parentNode = treeJson.SelectedNode.Parent;
            if (parentNode?.Tag is JsonObject parentObj)
            {
                // Find key to remove
                string? key = null;
                foreach (var kvp in parentObj)
                {
                    if (ReferenceEquals(kvp.Value, node))
                    {
                        key = kvp.Key;
                        break;
                    }
                }
                if (key != null)
                    parentObj.Remove(key);
            }
            else if (parentNode?.Tag is JsonArray parentArr)
            {
                for (int i = 0; i < parentArr.Count; i++)
                {
                    if (ReferenceEquals(parentArr[i], node))
                    {
                        parentArr.RemoveAt(i);
                        break;
                    }
                }
            }

            LoadTree();
            statusLabel.Text = "Node deleted.";
        }
    }
}
