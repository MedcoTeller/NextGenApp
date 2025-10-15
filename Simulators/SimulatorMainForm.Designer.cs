namespace Simulators
{
    partial class SimulatorMainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            CardReaderStatusLbl = new Label();
            TakeCardBtn = new Button();
            CardStatusLbl = new Label();
            InsertCardBtn = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(CardReaderStatusLbl);
            groupBox1.Controls.Add(TakeCardBtn);
            groupBox1.Controls.Add(CardStatusLbl);
            groupBox1.Controls.Add(InsertCardBtn);
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(348, 250);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Card Reader";
            // 
            // CardReaderStatusLbl
            // 
            CardReaderStatusLbl.AutoSize = true;
            CardReaderStatusLbl.Location = new Point(224, 203);
            CardReaderStatusLbl.Name = "CardReaderStatusLbl";
            CardReaderStatusLbl.Size = new Size(118, 32);
            CardReaderStatusLbl.TabIndex = 3;
            CardReaderStatusLbl.Text = "NoDevice";
            // 
            // TakeCardBtn
            // 
            TakeCardBtn.Enabled = false;
            TakeCardBtn.Location = new Point(6, 111);
            TakeCardBtn.Name = "TakeCardBtn";
            TakeCardBtn.Size = new Size(336, 67);
            TakeCardBtn.TabIndex = 2;
            TakeCardBtn.Text = "Take Card";
            TakeCardBtn.UseVisualStyleBackColor = true;
            TakeCardBtn.Click += TakeCardBtn_Click;
            // 
            // CardStatusLbl
            // 
            CardStatusLbl.AutoSize = true;
            CardStatusLbl.Location = new Point(6, 203);
            CardStatusLbl.Name = "CardStatusLbl";
            CardStatusLbl.Size = new Size(140, 32);
            CardStatusLbl.TabIndex = 1;
            CardStatusLbl.Text = "Not Present";
            // 
            // InsertCardBtn
            // 
            InsertCardBtn.Enabled = false;
            InsertCardBtn.Location = new Point(6, 36);
            InsertCardBtn.Name = "InsertCardBtn";
            InsertCardBtn.Size = new Size(336, 67);
            InsertCardBtn.TabIndex = 0;
            InsertCardBtn.Tag = "";
            InsertCardBtn.Text = "Inser Card";
            InsertCardBtn.UseVisualStyleBackColor = true;
            InsertCardBtn.Click += InsertCardBtn_Click;
            // 
            // SimulatorMainForm
            // 
            ClientSize = new Size(1275, 726);
            Controls.Add(groupBox1);
            Name = "SimulatorMainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ATM Device Simulators";
            Load += SimulatorMainForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Button InsertCardBtn;
        private Label CardReaderStatusLbl;
        private Button TakeCardBtn;
        private Label CardStatusLbl;
    }
}
