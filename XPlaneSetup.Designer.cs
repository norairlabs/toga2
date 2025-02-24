namespace TOGA
{
    partial class XPlaneSetup
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
            ExitButton = new System.Windows.Forms.Button();
            SaveButton = new System.Windows.Forms.Button();
            ID = new System.Windows.Forms.TextBox();
            XPlanePanel = new System.Windows.Forms.Panel();
            treeView1 = new System.Windows.Forms.TreeView();
            SERIAL = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            DESCRIPTION = new System.Windows.Forms.TextBox();
            XPlaneColorPanel = new System.Windows.Forms.Panel();
            label12 = new System.Windows.Forms.Label();
            XPlaneCustomColor10 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor9 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor8 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor7 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor6 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor5 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor4 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor3 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor2 = new System.Windows.Forms.RadioButton();
            XPlaneCustomColor1 = new System.Windows.Forms.RadioButton();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            XPlaneColorPanel.SuspendLayout();
            SuspendLayout();
            // 
            // ExitButton
            // 
            ExitButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 128, 0);
            ExitButton.FlatAppearance.BorderSize = 2;
            ExitButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Tomato;
            ExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Sienna;
            ExitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ExitButton.Location = new System.Drawing.Point(912, 15);
            ExitButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new System.Drawing.Size(116, 51);
            ExitButton.TabIndex = 83;
            ExitButton.Text = "Close";
            ExitButton.UseVisualStyleBackColor = true;
            ExitButton.Click += ExitButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 128, 0);
            SaveButton.FlatAppearance.BorderSize = 2;
            SaveButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Tomato;
            SaveButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Sienna;
            SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            SaveButton.Location = new System.Drawing.Point(1071, 15);
            SaveButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(116, 51);
            SaveButton.TabIndex = 82;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // ID
            // 
            ID.Location = new System.Drawing.Point(79, 45);
            ID.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ID.Name = "ID";
            ID.ReadOnly = true;
            ID.Size = new System.Drawing.Size(43, 27);
            ID.TabIndex = 85;
            ID.Visible = false;
            // 
            // XPlanePanel
            // 
            XPlanePanel.AutoScroll = true;
            XPlanePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            XPlanePanel.Location = new System.Drawing.Point(12, 110);
            XPlanePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            XPlanePanel.Name = "XPlanePanel";
            XPlanePanel.Size = new System.Drawing.Size(1204, 482);
            XPlanePanel.TabIndex = 90;
            // 
            // treeView1
            // 
            treeView1.Location = new System.Drawing.Point(12, 605);
            treeView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(1001, 369);
            treeView1.TabIndex = 0;
            // 
            // SERIAL
            // 
            SERIAL.Location = new System.Drawing.Point(79, 15);
            SERIAL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            SERIAL.Name = "SERIAL";
            SERIAL.ReadOnly = true;
            SERIAL.Size = new System.Drawing.Size(138, 27);
            SERIAL.TabIndex = 1;
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(12, 19);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(61, 20);
            label5.TabIndex = 0;
            label5.Text = "Serial";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(23, 48);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(24, 20);
            label6.TabIndex = 93;
            label6.Text = "ID";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            label6.Visible = false;
            // 
            // label1
            // 
            label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(12, 76);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(1204, 30);
            label1.TabIndex = 95;
            label1.Text = "Press Esc to clear the selected ghostkey field";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(244, 19);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(85, 20);
            label2.TabIndex = 97;
            label2.Text = "Description";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DESCRIPTION
            // 
            DESCRIPTION.Location = new System.Drawing.Point(364, 14);
            DESCRIPTION.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            DESCRIPTION.MaxLength = 64;
            DESCRIPTION.Name = "DESCRIPTION";
            DESCRIPTION.ReadOnly = true;
            DESCRIPTION.Size = new System.Drawing.Size(490, 27);
            DESCRIPTION.TabIndex = 1;
            // 
            // XPlaneColorPanel
            // 
            XPlaneColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            XPlaneColorPanel.Controls.Add(label12);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor10);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor9);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor8);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor7);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor6);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor5);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor4);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor3);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor2);
            XPlaneColorPanel.Controls.Add(XPlaneCustomColor1);
            XPlaneColorPanel.Location = new System.Drawing.Point(1019, 605);
            XPlaneColorPanel.Name = "XPlaneColorPanel";
            XPlaneColorPanel.Size = new System.Drawing.Size(197, 369);
            XPlaneColorPanel.TabIndex = 118;
            // 
            // label12
            // 
            label12.BackColor = System.Drawing.SystemColors.Control;
            label12.Location = new System.Drawing.Point(13, 9);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(98, 37);
            label12.TabIndex = 38;
            label12.Text = "BackLit color";
            label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // XPlaneCustomColor10
            // 
            XPlaneCustomColor10.Location = new System.Drawing.Point(7, 323);
            XPlaneCustomColor10.Name = "XPlaneCustomColor10";
            XPlaneCustomColor10.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor10.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor10.TabIndex = 9;
            XPlaneCustomColor10.TabStop = true;
            XPlaneCustomColor10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor10.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor9
            // 
            XPlaneCustomColor9.Location = new System.Drawing.Point(7, 293);
            XPlaneCustomColor9.Name = "XPlaneCustomColor9";
            XPlaneCustomColor9.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor9.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor9.TabIndex = 8;
            XPlaneCustomColor9.TabStop = true;
            XPlaneCustomColor9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor9.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor8
            // 
            XPlaneCustomColor8.Location = new System.Drawing.Point(7, 263);
            XPlaneCustomColor8.Name = "XPlaneCustomColor8";
            XPlaneCustomColor8.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor8.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor8.TabIndex = 7;
            XPlaneCustomColor8.TabStop = true;
            XPlaneCustomColor8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor8.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor7
            // 
            XPlaneCustomColor7.Location = new System.Drawing.Point(7, 233);
            XPlaneCustomColor7.Name = "XPlaneCustomColor7";
            XPlaneCustomColor7.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor7.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor7.TabIndex = 6;
            XPlaneCustomColor7.TabStop = true;
            XPlaneCustomColor7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor7.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor6
            // 
            XPlaneCustomColor6.Location = new System.Drawing.Point(7, 203);
            XPlaneCustomColor6.Name = "XPlaneCustomColor6";
            XPlaneCustomColor6.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor6.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor6.TabIndex = 5;
            XPlaneCustomColor6.TabStop = true;
            XPlaneCustomColor6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor6.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor5
            // 
            XPlaneCustomColor5.Location = new System.Drawing.Point(7, 173);
            XPlaneCustomColor5.Name = "XPlaneCustomColor5";
            XPlaneCustomColor5.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor5.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor5.TabIndex = 4;
            XPlaneCustomColor5.TabStop = true;
            XPlaneCustomColor5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor5.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor4
            // 
            XPlaneCustomColor4.Location = new System.Drawing.Point(7, 143);
            XPlaneCustomColor4.Name = "XPlaneCustomColor4";
            XPlaneCustomColor4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor4.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor4.TabIndex = 3;
            XPlaneCustomColor4.TabStop = true;
            XPlaneCustomColor4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor4.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor3
            // 
            XPlaneCustomColor3.Location = new System.Drawing.Point(7, 113);
            XPlaneCustomColor3.Name = "XPlaneCustomColor3";
            XPlaneCustomColor3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor3.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor3.TabIndex = 2;
            XPlaneCustomColor3.TabStop = true;
            XPlaneCustomColor3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor3.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor2
            // 
            XPlaneCustomColor2.Location = new System.Drawing.Point(7, 83);
            XPlaneCustomColor2.Name = "XPlaneCustomColor2";
            XPlaneCustomColor2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor2.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor2.TabIndex = 1;
            XPlaneCustomColor2.TabStop = true;
            XPlaneCustomColor2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor2.UseVisualStyleBackColor = true;
            // 
            // XPlaneCustomColor1
            // 
            XPlaneCustomColor1.Location = new System.Drawing.Point(7, 53);
            XPlaneCustomColor1.Name = "XPlaneCustomColor1";
            XPlaneCustomColor1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            XPlaneCustomColor1.Size = new System.Drawing.Size(156, 24);
            XPlaneCustomColor1.TabIndex = 0;
            XPlaneCustomColor1.TabStop = true;
            XPlaneCustomColor1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            XPlaneCustomColor1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Location = new System.Drawing.Point(0, 1033);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1247, 22);
            statusStrip1.TabIndex = 119;
            statusStrip1.Text = "statusStrip1";
            // 
            // XPlaneSetup
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new System.Drawing.Size(1247, 1055);
            ControlBox = false;
            Controls.Add(statusStrip1);
            Controls.Add(XPlaneColorPanel);
            Controls.Add(label2);
            Controls.Add(DESCRIPTION);
            Controls.Add(label1);
            Controls.Add(treeView1);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(SERIAL);
            Controls.Add(XPlanePanel);
            Controls.Add(ID);
            Controls.Add(ExitButton);
            Controls.Add(SaveButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "XPlaneSetup";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "X-Plane Setup";
            XPlaneColorPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.TextBox ID;
        private System.Windows.Forms.Panel XPlanePanel;
        private System.Windows.Forms.TextBox SERIAL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DESCRIPTION;
        private System.Windows.Forms.Panel XPlaneColorPanel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.RadioButton XPlaneCustomColor10;
        private System.Windows.Forms.RadioButton XPlaneCustomColor9;
        private System.Windows.Forms.RadioButton XPlaneCustomColor8;
        private System.Windows.Forms.RadioButton XPlaneCustomColor7;
        private System.Windows.Forms.RadioButton XPlaneCustomColor6;
        private System.Windows.Forms.RadioButton XPlaneCustomColor5;
        private System.Windows.Forms.RadioButton XPlaneCustomColor4;
        private System.Windows.Forms.RadioButton XPlaneCustomColor3;
        private System.Windows.Forms.RadioButton XPlaneCustomColor2;
        private System.Windows.Forms.RadioButton XPlaneCustomColor1;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}