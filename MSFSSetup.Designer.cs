namespace TOGA
{
    partial class MSFSSetup
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
            label2 = new System.Windows.Forms.Label();
            DESCRIPTION = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            SERIAL = new System.Windows.Forms.TextBox();
            MSFSPanel = new System.Windows.Forms.Panel();
            ID = new System.Windows.Forms.TextBox();
            MSFSExitButton = new System.Windows.Forms.Button();
            MSFSSaveButton = new System.Windows.Forms.Button();
            MSFSColorPanel = new System.Windows.Forms.Panel();
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
            SimVarsTreeView = new System.Windows.Forms.TreeView();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            MSFSColorPanel.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(244, 30);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(85, 20);
            label2.TabIndex = 107;
            label2.Text = "Description";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DESCRIPTION
            // 
            DESCRIPTION.Location = new System.Drawing.Point(364, 25);
            DESCRIPTION.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            DESCRIPTION.MaxLength = 64;
            DESCRIPTION.Name = "DESCRIPTION";
            DESCRIPTION.ReadOnly = true;
            DESCRIPTION.Size = new System.Drawing.Size(490, 27);
            DESCRIPTION.TabIndex = 99;
            // 
            // label1
            // 
            label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(12, 87);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(1204, 30);
            label1.TabIndex = 106;
            label1.Text = "Press Esc to clear the selected ghostkey field";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(23, 59);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(24, 20);
            label6.TabIndex = 105;
            label6.Text = "ID";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            label6.Visible = false;
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(12, 30);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(61, 20);
            label5.TabIndex = 98;
            label5.Text = "Serial";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SERIAL
            // 
            SERIAL.Location = new System.Drawing.Point(79, 26);
            SERIAL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            SERIAL.Name = "SERIAL";
            SERIAL.ReadOnly = true;
            SERIAL.Size = new System.Drawing.Size(138, 27);
            SERIAL.TabIndex = 100;
            // 
            // MSFSPanel
            // 
            MSFSPanel.AutoScroll = true;
            MSFSPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            MSFSPanel.Location = new System.Drawing.Point(12, 125);
            MSFSPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MSFSPanel.Name = "MSFSPanel";
            MSFSPanel.Size = new System.Drawing.Size(1204, 478);
            MSFSPanel.TabIndex = 104;
            // 
            // ID
            // 
            ID.Location = new System.Drawing.Point(79, 56);
            ID.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ID.Name = "ID";
            ID.ReadOnly = true;
            ID.Size = new System.Drawing.Size(43, 27);
            ID.TabIndex = 103;
            ID.Visible = false;
            // 
            // MSFSExitButton
            // 
            MSFSExitButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 128, 0);
            MSFSExitButton.FlatAppearance.BorderSize = 2;
            MSFSExitButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Tomato;
            MSFSExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Sienna;
            MSFSExitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            MSFSExitButton.Location = new System.Drawing.Point(912, 26);
            MSFSExitButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MSFSExitButton.Name = "MSFSExitButton";
            MSFSExitButton.Size = new System.Drawing.Size(116, 51);
            MSFSExitButton.TabIndex = 102;
            MSFSExitButton.Text = "Close";
            MSFSExitButton.UseVisualStyleBackColor = true;
            MSFSExitButton.Click += MSFSExitButton_Click;
            // 
            // MSFSSaveButton
            // 
            MSFSSaveButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 128, 0);
            MSFSSaveButton.FlatAppearance.BorderSize = 2;
            MSFSSaveButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.Tomato;
            MSFSSaveButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Sienna;
            MSFSSaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            MSFSSaveButton.Location = new System.Drawing.Point(1071, 26);
            MSFSSaveButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MSFSSaveButton.Name = "MSFSSaveButton";
            MSFSSaveButton.Size = new System.Drawing.Size(116, 51);
            MSFSSaveButton.TabIndex = 101;
            MSFSSaveButton.Text = "Save";
            MSFSSaveButton.UseVisualStyleBackColor = true;
            MSFSSaveButton.Click += MSFSSaveButton_Click;
            // 
            // MSFSColorPanel
            // 
            MSFSColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            MSFSColorPanel.Controls.Add(label12);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor10);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor9);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor8);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor7);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor6);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor5);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor4);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor3);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor2);
            MSFSColorPanel.Controls.Add(XPlaneCustomColor1);
            MSFSColorPanel.Location = new System.Drawing.Point(1019, 611);
            MSFSColorPanel.Name = "MSFSColorPanel";
            MSFSColorPanel.Size = new System.Drawing.Size(197, 369);
            MSFSColorPanel.TabIndex = 120;
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
            // SimVarsTreeView
            // 
            SimVarsTreeView.Location = new System.Drawing.Point(12, 611);
            SimVarsTreeView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            SimVarsTreeView.Name = "SimVarsTreeView";
            SimVarsTreeView.Size = new System.Drawing.Size(864, 369);
            SimVarsTreeView.TabIndex = 119;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusLabel });
            statusStrip1.Location = new System.Drawing.Point(0, 1029);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1247, 26);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 121;
            statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            StatusLabel.AutoSize = false;
            StatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            StatusLabel.Margin = new System.Windows.Forms.Padding(4, 4, 0, 2);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new System.Drawing.Size(1240, 20);
            StatusLabel.Text = "Ready";
            StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MSFSSetup
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new System.Drawing.Size(1247, 1055);
            ControlBox = false;
            Controls.Add(statusStrip1);
            Controls.Add(MSFSColorPanel);
            Controls.Add(SimVarsTreeView);
            Controls.Add(label2);
            Controls.Add(DESCRIPTION);
            Controls.Add(label1);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(SERIAL);
            Controls.Add(MSFSPanel);
            Controls.Add(ID);
            Controls.Add(MSFSExitButton);
            Controls.Add(MSFSSaveButton);
            Name = "MSFSSetup";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Microsoft Flight Sim Setup";
            MSFSColorPanel.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DESCRIPTION;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SERIAL;
        private System.Windows.Forms.Panel MSFSPanel;
        private System.Windows.Forms.TextBox ID;
        private System.Windows.Forms.Button MSFSExitButton;
        private System.Windows.Forms.Button MSFSSaveButton;
        private System.Windows.Forms.Panel MSFSColorPanel;
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
        private System.Windows.Forms.TreeView SimVarsTreeView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
    }
}