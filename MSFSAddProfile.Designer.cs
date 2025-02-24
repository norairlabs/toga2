namespace TOGA
{
    partial class MSFSAddProfile
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
            PathExists = new System.Windows.Forms.Label();
            InvalidName = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            button5 = new System.Windows.Forms.Button();
            AddPeripheral = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            NAME = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // PathExists
            // 
            PathExists.AutoSize = true;
            PathExists.Location = new System.Drawing.Point(178, 184);
            PathExists.Name = "PathExists";
            PathExists.Size = new System.Drawing.Size(145, 20);
            PathExists.TabIndex = 125;
            PathExists.Text = "Profile already exists";
            PathExists.Visible = false;
            // 
            // InvalidName
            // 
            InvalidName.AutoSize = true;
            InvalidName.Location = new System.Drawing.Point(78, 184);
            InvalidName.Name = "InvalidName";
            InvalidName.Size = new System.Drawing.Size(94, 20);
            InvalidName.TabIndex = 124;
            InvalidName.Text = "Invalid name";
            InvalidName.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(259, 97);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(264, 20);
            label1.TabIndex = 123;
            label1.Text = "Valid characters: letters, numbers and '_'";
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(644, 158);
            button5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(120, 46);
            button5.TabIndex = 122;
            button5.Text = "Cancel";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // AddPeripheral
            // 
            AddPeripheral.Enabled = false;
            AddPeripheral.Location = new System.Drawing.Point(454, 158);
            AddPeripheral.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            AddPeripheral.Name = "AddPeripheral";
            AddPeripheral.Size = new System.Drawing.Size(184, 46);
            AddPeripheral.TabIndex = 121;
            AddPeripheral.Text = "Create profile";
            AddPeripheral.UseVisualStyleBackColor = true;
            AddPeripheral.Click += AddPeripheral_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(78, 70);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(93, 20);
            label5.TabIndex = 120;
            label5.Text = "Profile name";
            // 
            // NAME
            // 
            NAME.Location = new System.Drawing.Point(177, 67);
            NAME.MaxLength = 35;
            NAME.Name = "NAME";
            NAME.Size = new System.Drawing.Size(461, 27);
            NAME.TabIndex = 119;
            NAME.TextChanged += NAME_TextChanged;
            // 
            // MSFSAddProfile
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(842, 271);
            Controls.Add(PathExists);
            Controls.Add(InvalidName);
            Controls.Add(label1);
            Controls.Add(button5);
            Controls.Add(AddPeripheral);
            Controls.Add(label5);
            Controls.Add(NAME);
            Name = "MSFSAddProfile";
            Text = "Add profile";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label PathExists;
        private System.Windows.Forms.Label InvalidName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button AddPeripheral;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox NAME;
    }
}