using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TOGA
{
    public partial class AddProfile : Form
    {

        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";
        public string nl = Environment.NewLine;

        public static string XPlanePath = BasePath + @"\X-Plane";
        public static string XPlaneProfilesPath = XPlanePath + @"\Profiles";

        public Color backColor = (Color)ColorTranslator.FromHtml("#101010");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");


        public AddProfile()
        {
            InitializeComponent();
        }

        new public DialogResult Show()
        {
            SetTheme(this.Controls, backColor, frontColor);
            this.BackColor = backColor;
            this.Refresh();
            return (this.ShowDialog());
        }

        private void SetTheme(Control.ControlCollection controls, Color bcolor, Color fcolor)
        {
            /// <summary>
            /// Change foreground and background colors of controls
            /// </summary>
            /// 
            foreach (Control control in controls)
            {
                if (control.HasChildren)
                {
                    // Recursively loop through the child controls
                    SetTheme(control.Controls, bcolor, fcolor);
                }
                else
                {
                    var ctrl = control.GetType();

                    if (control is System.Windows.Forms.TextBox)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.RadioButton)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.Label)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.Panel)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.Button)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        if (control.GetType().GetProperty(control.Name) != null)
                        {
                            control.GetType().GetProperty(control.Name).SetValue(control, FlatStyle.Flat, null);
                            control.GetType().GetProperty(control.Name).SetValue(control, BorderType.Ellipse, null);
                        }
                    }
                    if (control is System.Windows.Forms.ComboBox)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.Form)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }
                    if (control is System.Windows.Forms.TabControl)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                    }

                }
            }
        }

        private void AddPeripheral_Click(object sender, EventArgs e)
        {

            string ThisProfilePath = XPlaneProfilesPath + ".\\" + NAME.Text.Trim();
            Directory.CreateDirectory(ThisProfilePath);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NAME_TextChanged(object sender, EventArgs e)
        {
            bool isValid = NAME.Text.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ') || c.Equals('-'));

            if (isValid && NAME.Text.Length > 0)
            {
                InvalidName.Visible = false;
            }
            else
            {
                InvalidName.Visible = true;
                InvalidName.ForeColor = Color.Red;
            }
            if (Directory.Exists(Path.Combine(XPlaneProfilesPath, NAME.Text.Trim())))
            {
                PathExists.Visible = true;
                PathExists.ForeColor = Color.Red;
            }
            else
            {
                PathExists.Visible = false;
            }
            if (!(InvalidName.Visible || PathExists.Visible))
            {
                AddPeripheral.Enabled = true;
            }
            else
            {
                AddPeripheral.Enabled = false;
            }
            if (NAME.Text.Length == 0)
            {
                PathExists.Visible = false;
                InvalidName.Visible = false;
                AddPeripheral.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
