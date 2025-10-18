using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace Simulators.Config
{
    public partial class AddKeyValueForm : Form
    {

        public string? NewKey { get; private set; }
        public JsonNode? CreatedNode { get; private set; }

        //public AddKeyValueForm()
        //{
        //    InitializeComponent();
        //}

        public AddKeyValueForm(JsonNode parent)
        {
            InitializeComponent();
            lblKey.Visible = txtKey.Visible = parent is JsonObject;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            NewKey = string.IsNullOrWhiteSpace(txtKey.Text) ? null : txtKey.Text.Trim();
            string type = cboType.SelectedItem?.ToString() ?? "string";
            string value = txtValue.Text.Trim();

            switch (type)
            {
                case "string":
                    CreatedNode = JsonValue.Create(value);
                    break;
                case "int":
                    CreatedNode = int.TryParse(value, out var i) ? JsonValue.Create(i) : JsonValue.Create(0);
                    break;
                case "bool":
                    CreatedNode = bool.TryParse(value, out var b) ? JsonValue.Create(b) : JsonValue.Create(false);
                    break;
                case "object":
                    CreatedNode = new JsonObject();
                    break;
                case "array":
                    CreatedNode = new JsonArray();
                    break;
                case "null":
                    CreatedNode = null;
                    break;
            }

            DialogResult = DialogResult.OK;
        }

        private void AddKeyValueForm_Load(object sender, EventArgs e)
        {

        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
