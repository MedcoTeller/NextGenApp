using System.Data;

namespace Simulators.Config
{
    partial class AddKeyValueForm
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

        private Label lblKey;
        private TextBox txtKey;
        private Label lblType;
        private ComboBox cboType;
        private Label lblValue;
        private TextBox txtValue;
        private Button btnOK;
        private Button btnCancel;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblKey = new Label();
            txtKey = new TextBox();
            lblType = new Label();
            cboType = new ComboBox();
            lblValue = new Label();
            txtValue = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblKey
            // 
            lblKey.Location = new Point(10, 75);
            lblKey.Name = "lblKey";
            lblKey.Size = new Size(100, 38);
            lblKey.TabIndex = 0;
            lblKey.Text = "Key:";
            // 
            // txtKey
            // 
            txtKey.Location = new Point(116, 75);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(458, 39);
            txtKey.TabIndex = 1;
            txtKey.Text = "txt";
            txtKey.TextChanged += txtKey_TextChanged;
            // 
            // lblType
            // 
            lblType.Location = new Point(6, 14);
            lblType.Name = "lblType";
            lblType.Size = new Size(104, 39);
            lblType.TabIndex = 2;
            lblType.Text = "Type:";
            // 
            // cboType
            // 
            cboType.Items.AddRange(new object[] { "string", "int", "bool", "object", "array", "null" });
            cboType.Location = new Point(116, 11);
            cboType.Name = "cboType";
            cboType.Size = new Size(458, 40);
            cboType.TabIndex = 3;
            // 
            // lblValue
            // 
            lblValue.Location = new Point(6, 128);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(104, 38);
            lblValue.TabIndex = 4;
            lblValue.Text = "Value:";
            // 
            // txtValue
            // 
            txtValue.Location = new Point(112, 142);
            txtValue.Multiline = true;
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(462, 245);
            txtValue.TabIndex = 5;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(345, 406);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(171, 60);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(70, 406);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(171, 60);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click_1;
            // 
            // AddKeyValueForm
            // 
            ClientSize = new Size(592, 474);
            Controls.Add(lblKey);
            Controls.Add(txtKey);
            Controls.Add(lblType);
            Controls.Add(cboType);
            Controls.Add(lblValue);
            Controls.Add(txtValue);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddKeyValueForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add Key/Value";
            Load += AddKeyValueForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion
    }
}