using System;
using System.Windows.Forms;

namespace TOGA
{
    partial class AddNewPeripheral
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
            button5 = new Button();
            label1 = new Label();
            AddPeripheral = new Button();
            SERIAL = new TextBox();
            label2 = new Label();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ID = new NumericUpDown();
            IDInUse = new Label();
            label3 = new Label();
            DESCRIPTION = new TextBox();
            HardwareTypeLabel = new Label();
            InvalidName = new Label();
            ((System.ComponentModel.ISupportInitialize)ID).BeginInit();
            SuspendLayout();
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(304, 210);
            button5.Margin = new Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(78, 46);
            button5.TabIndex = 16;
            button5.Text = "Cancel";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(33, 41);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(101, 20);
            label1.TabIndex = 9;
            label1.Text = "Serial number";
            // 
            // AddPeripheral
            // 
            AddPeripheral.Enabled = false;
            AddPeripheral.Location = new System.Drawing.Point(388, 210);
            AddPeripheral.Margin = new Padding(3, 4, 3, 4);
            AddPeripheral.Name = "AddPeripheral";
            AddPeripheral.Size = new System.Drawing.Size(78, 46);
            AddPeripheral.TabIndex = 13;
            AddPeripheral.Text = "Save";
            AddPeripheral.UseVisualStyleBackColor = true;
            AddPeripheral.Click += AddPeripheral_Click_1;
            // 
            // SERIAL
            // 
            SERIAL.Location = new System.Drawing.Point(140, 38);
            SERIAL.Margin = new Padding(3, 4, 3, 4);
            SERIAL.Name = "SERIAL";
            SERIAL.Size = new System.Drawing.Size(100, 27);
            SERIAL.TabIndex = 1;
            SERIAL.KeyPress += textBox2_KeyPress;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(33, 129);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(94, 20);
            label2.TabIndex = 14;
            label2.Text = "Peripheral ID";
            label2.Visible = false;
            // 
            // ID
            // 
            ID.Location = new System.Drawing.Point(140, 125);
            ID.Margin = new Padding(3, 4, 3, 4);
            ID.Maximum = new decimal(new int[] { 98, 0, 0, 0 });
            ID.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            ID.Name = "ID";
            ID.ReadOnly = true;
            ID.Size = new System.Drawing.Size(57, 27);
            ID.TabIndex = 17;
            ID.TextAlign = HorizontalAlignment.Center;
            ID.Value = new decimal(new int[] { 20, 0, 0, 0 });
            ID.Visible = false;
            ID.ValueChanged += RollId_ValueChanged;
            // 
            // IDInUse
            // 
            IDInUse.BackColor = System.Drawing.Color.Red;
            IDInUse.ForeColor = System.Drawing.Color.Yellow;
            IDInUse.Location = new System.Drawing.Point(203, 125);
            IDInUse.Name = "IDInUse";
            IDInUse.Size = new System.Drawing.Size(82, 28);
            IDInUse.TabIndex = 18;
            IDInUse.Text = "ID in use";
            IDInUse.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            IDInUse.Visible = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(33, 83);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(85, 20);
            label3.TabIndex = 19;
            label3.Text = "Description";
            // 
            // DESCRIPTION
            // 
            DESCRIPTION.Location = new System.Drawing.Point(140, 80);
            DESCRIPTION.Margin = new Padding(3, 4, 3, 4);
            DESCRIPTION.Name = "DESCRIPTION";
            DESCRIPTION.Size = new System.Drawing.Size(326, 27);
            DESCRIPTION.TabIndex = 2;
            DESCRIPTION.TextChanged += DESCRIPTION_TextChanged;
            // 
            // HardwareTypeLabel
            // 
            HardwareTypeLabel.Location = new System.Drawing.Point(246, 41);
            HardwareTypeLabel.Name = "HardwareTypeLabel";
            HardwareTypeLabel.Size = new System.Drawing.Size(220, 20);
            HardwareTypeLabel.TabIndex = 21;
            // 
            // InvalidName
            // 
            InvalidName.AutoSize = true;
            InvalidName.Location = new System.Drawing.Point(372, 111);
            InvalidName.Name = "InvalidName";
            InvalidName.Size = new System.Drawing.Size(94, 20);
            InvalidName.TabIndex = 118;
            InvalidName.Text = "Invalid name";
            InvalidName.Visible = false;
            // 
            // AddNewPeripheral
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(478, 280);
            Controls.Add(InvalidName);
            Controls.Add(HardwareTypeLabel);
            Controls.Add(label3);
            Controls.Add(DESCRIPTION);
            Controls.Add(IDInUse);
            Controls.Add(ID);
            Controls.Add(button5);
            Controls.Add(label1);
            Controls.Add(AddPeripheral);
            Controls.Add(SERIAL);
            Controls.Add(label2);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(3, 4, 3, 4);
            Name = "AddNewPeripheral";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Register a new peripheral";
            ((System.ComponentModel.ISupportInitialize)ID).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddPeripheral;
        private System.Windows.Forms.TextBox SERIAL;
        private System.Windows.Forms.Label label2;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private NumericUpDown ID;
        private Label IDInUse;
        private Label label3;
        private TextBox DESCRIPTION;
        private Label HardwareTypeLabel;
        private Label InvalidName;
    }
}