using NorAir;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TOGA.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TOGA.Form1;
using static TOGA.RNSForm;

namespace TOGA
{
    public partial class AddNewPeripheral : Form
    {
        public class Styles
        {
            public string styleName { get; set; }
            public string backColor { get; set; }
            public string frontColor { get; set; }
            public string buttonColor { get; set; }
            public string mouseoverColor { get; set; }
            public string fontName { get; set; }
            public float fontSize { get; set; }
            public string borderColor { get; set; }
            public decimal borderSize { get; set; }
        }

        public class PeripheralProperty
        {
            public string SERIALNUMBER { get; set; }
            public string DESCRIPTION { get; set; }
            public int ID { get; set; }
        }


        public List<string> serials = new List<string>();
        public int[] ValidIds = new int[100];
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";
        public string PeripheralsFileName = BasePath + ".\\Peripherals.json";
        public string StylesFileName = BasePath + ".\\Styles.json";
        public string nl = Environment.NewLine;

        public Color backColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color buttonColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color borderColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public decimal borderSize = 2;
        public Color mouseOverColor = (Color)ColorTranslator.FromHtml("#E0E0E0");
        public Font CharFont = new Font("Verdana", 8);

        public AddNewPeripheral()
        {
            InitializeComponent();
        }

        public DialogResult Show(string serial, List<string> l, List<string> sn, Color bck, Color frt)
        {
            foreach (string item in sn)
            {
                if (serial.Length != 0)
                {
                    if (!item.Equals(serial))
                    {
                        serials.Add(item);
                    }
                }
                else
                {
                    serials.Add(item);
                }
            }
            foreach (string id in l)
            {
                int i = int.Parse(id);
                ValidIds[i] = 1;
            }
            ReadStyles();
            SetTheme(this.Controls, backColor, frontColor);
            this.BackColor = backColor;
            ID.BackColor = Color.LightGreen;
            ID.ForeColor = Color.Black;
            IDInUse.BackColor = Color.Red;
            if (serial.Length != 0)
            {
                if (!LoadData(serial))
                {
                    return DialogResult.Cancel;
                }
                SERIAL.ReadOnly = true;
                IDInUse.Visible = false;
                ID.Enabled = false;
                AddPeripheral.Enabled = true;
                ID.BackColor = Color.LightGreen;
                ID.ForeColor = Color.Black;
                IDInUse.BackColor = Color.LightGreen;
            }
            this.Refresh();
            return (this.ShowDialog());
        }

        private void SetTheme(Control.ControlCollection ctrs, Color bcolor, Color fcolor)
        {
            /// <summary>
            /// Change foreground and background colors of controls
            /// It is restrited to the ones mentioned
            /// </summary>
            /// 

            ApplyTheme(ctrs, bcolor, fcolor);   // Theme settings to general objects

            this.BackColor = backColor; // Tabs and main's form entities update
            this.ForeColor = frontColor;

        }

        void ApplyTheme(Control.ControlCollection ctrs, Color bcolor, Color fcolor)
        {
            foreach (Control control in ctrs)
            {
                if (control.HasChildren)
                {
                    // Recursively loop through the child controls
                    ApplyTheme(control.Controls, bcolor, fcolor);
                }
                else
                {
                    var ctrl = control.GetType();

                    if (control is System.Windows.Forms.TextBox)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.RadioButton)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.Label)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.Panel)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.ListBox)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        string font = CharFont.FontFamily.ToString();
                        Font AlternateCharFont = new Font(font, 9);

                        control.Font = AlternateCharFont;

                    }

                    if (control is System.Windows.Forms.ComboBox)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.Form)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.TabControl)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                    if (control is System.Windows.Forms.TabPage)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
                        control.Font = CharFont;
                    }
                }

            }
            var ctr = from controls in ctrs.OfType<System.Windows.Forms.Button>()    // Theme settings to buttons
                      select controls;
            foreach (var control in ctr)
            {
                control.BackColor = buttonColor;
                control.ForeColor = fcolor;
                control.Font = CharFont;
                control.FlatStyle = FlatStyle.Flat;
                control.FlatAppearance.BorderColor = borderColor;
                control.FlatAppearance.BorderSize = (int)borderSize;
                control.FlatAppearance.MouseOverBackColor = mouseOverColor;

            }
        }

        public void ReadStyles()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            Styles style = new Styles();

            // Read the file 

            if (!File.Exists(StylesFileName))
            {

                style.styleName = "DarkMode";
                style.backColor = ColorTranslator.ToHtml(backColor);
                style.frontColor = ColorTranslator.ToHtml(frontColor);
                style.fontName = CharFont.Name;
                style.fontSize = CharFont.Size;
                style.borderColor = ColorTranslator.ToHtml(frontColor);
                style.borderSize = borderSize;
                style.mouseoverColor = ColorTranslator.ToHtml(mouseOverColor);

                string line = JsonConvert.SerializeObject(style, settings);

                File.WriteAllText(StylesFileName, line);

            }

            string lines = File.ReadAllText(StylesFileName);    // Read all lines at once

            if (lines.Count(c => c == '}') != lines.Count(c => c == '{'))
            {
                lines = lines.Substring(0, lines.Length - 2);
            }

            style = JsonConvert.DeserializeObject<Styles>(lines);  // Deserialize 'lines'

            backColor = ColorTranslator.FromHtml(style.backColor);
            frontColor = ColorTranslator.FromHtml(style.frontColor);
            buttonColor = ColorTranslator.FromHtml(style.buttonColor);
            mouseOverColor = ColorTranslator.FromHtml(style.mouseoverColor);
            borderSize = style.borderSize;
            borderColor = ColorTranslator.FromHtml(style.borderColor);
            CharFont = new Font(style.fontName, style.fontSize);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            if (SERIAL.Text.Length > 1)
            {
                int t = int.Parse(SERIAL.Text.Substring(0, 2));
                int r = BestFit(t);                                 // Sugest the best fit for ID
                if (r != 0)
                {
                    ID.Value = (decimal)r;
                }
                else
                {
                    MessageBox.Show("No more Ids available!",
                    "Add peripheral",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                if (SERIAL.Text.StartsWith("31"))
                {
                    HardwareTypeLabel.Text = "Radio & Navigation System ";

                }
                if (SERIAL.Text.StartsWith("50"))
                {
                    HardwareTypeLabel.Text = "Modules Bay System ";

                }
            }
            if (SERIAL.Text.Length > 8)
            {
                if (DESCRIPTION.Text.Length != 0)
                {
                    AddPeripheral.Enabled = true;
                }
            }
            else
            {
                AddPeripheral.Enabled = false;
            }

        }

        public int BestFit(int r)       // Recursively finds the first free ID for the peripheral
        {
            int m = (int)ID.Minimum;
            int M = (int)ID.Maximum;
            if (r < m || r > M)
            {
                return 0;
            }
            else
            {

                if (ValidIds[r] == 1)
                {
                    return r;
                }
                else
                {
                    r++;
                    if (r <= M)
                    {
                        int v = BestFit(r);
                        return v;
                    }
                }

            }
            return 0;
        }
        public bool AddPeripheralToFile()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            PeripheralProperty pp = new PeripheralProperty();
            PeripheralProperty ppAux = new PeripheralProperty();

            List<PeripheralProperty> PeripheralsPresent = new List<PeripheralProperty>();

            if (File.Exists(PeripheralsFileName))
            {

                // Open the file and read it back.
                string lines = File.ReadAllText(PeripheralsFileName);    // Read all lines at once

                try
                {
                    PeripheralsPresent = JsonConvert.DeserializeObject<List<PeripheralProperty>>(lines);  // Deserialize 'lines'

                }
                catch
                {
                    return false;
                }

            }

            pp = PeripheralsPresent.Find(pp => pp.SERIALNUMBER == SERIAL.Text.Trim());

            if (pp != null)
            {
                if (pp.SERIALNUMBER == SERIAL.Text.Trim())
                {
                    int index = PeripheralsPresent.IndexOf(pp);
                    PeripheralsPresent.RemoveAt(index);
                }
            }
            ppAux.SERIALNUMBER = this.SERIAL.Text;
            ppAux.DESCRIPTION = this.DESCRIPTION.Text;
            ppAux.ID = (int)this.ID.Value;

            PeripheralsPresent.Add(ppAux);

            try
            {

                string line = JsonConvert.SerializeObject(PeripheralsPresent, settings);

                File.WriteAllText(PeripheralsFileName, line);

            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool LoadData(string serial)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            PeripheralProperty pp = new PeripheralProperty();

            List<PeripheralProperty> PeripheralsPresent = new List<PeripheralProperty>();

            if (File.Exists(PeripheralsFileName))
            {

                // Open the file and read it back.
                string lines = File.ReadAllText(PeripheralsFileName);    // Read all lines at once

                try
                {
                    PeripheralsPresent = JsonConvert.DeserializeObject<List<PeripheralProperty>>(lines);  // Deserialize 'lines'

                }
                catch
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

            pp = PeripheralsPresent.Find(pp => pp.SERIALNUMBER.Trim() == serial.Trim());

            if (pp.SERIALNUMBER == null)
            {
                return false;
            }

            if (pp.SERIALNUMBER.Length == 0)
            {
                return false;
            }

            this.SERIAL.Text = pp.SERIALNUMBER;
            this.DESCRIPTION.Text = pp.DESCRIPTION;
            this.ID.Value = (decimal)pp.ID;
            ValidIds[pp.ID] = 1;
            return true;
        }


        private void AddPeripheralValidation()
        {

            if (ID.BackColor != Color.LightGreen)
            {
                MessageBox.Show("Id in use by other peripheral.\nChoose another Id.",
                    "Add peripheral",
                    MessageBoxButtons.OK);
                return;
            }
            foreach (var sn in serials)
            {
                if (sn.Equals(SERIAL.Text))
                {
                    MessageBox.Show("Serial number already exists",
                    "Add peripheral",
                    MessageBoxButtons.OK);
                    return;
                }
            }
            if (SERIAL.Text.Length != 10)
            {
                MessageBox.Show("Invalid serial number",
                    "Add peripheral",
                    MessageBoxButtons.OK);
                return;
            }

            if (DESCRIPTION.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please, provide a description",
                    "Mandatory description",
                    MessageBoxButtons.OK);
                return;
            }

            var confirmResult = MessageBox.Show("Save serial to peripheral list?",
                "Add peripheral",
                 MessageBoxButtons.OKCancel);
            if (confirmResult == DialogResult.OK)
            {
                // Set return message before changing IdCombobox
                string text = "Serial " + SERIAL.Text + " added identified by " + ID.Value.ToString(); ;

                // Try to save the new peripheral to file
                if (!AddPeripheralToFile())
                {
                    MessageBox.Show("There was a problem registering the peripheral",
                    "Add peripheral",
                    MessageBoxButtons.OK);
                }
                else
                {
                    this.Close();
                }
            }

        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddPeripheral_Click_1(object sender, EventArgs e)
        {
            AddPeripheralValidation();
        }

        private void RollId_ValueChanged(object sender, EventArgs e)
        {
            int v = (int)ID.Value;
            if (ValidIds[v] != 1)
            {
                ID.BackColor = Color.Red;
                //IDInUse.Visible = true;   // ID is out off reach of the user to allow more simple setup
            }
            else
            {
                ID.BackColor = Color.LightGreen;
                IDInUse.Visible = false;
            }
        }

        private void DESCRIPTION_TextChanged(object sender, EventArgs e)
        {
            bool isValid = DESCRIPTION.Text.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ') || c.Equals('-'));

            if (isValid && DESCRIPTION.Text.Length > 0)
            {
                InvalidName.Visible = false;
            }
            else
            {
                AddPeripheral.Enabled = false;
                InvalidName.Visible = true;
                InvalidName.ForeColor = Color.Red;
                return;
            }

            if (!(InvalidName.Visible) && DESCRIPTION.Text.Length != 0)
            {
                AddPeripheral.Enabled = true;
            }
            else
            {
                AddPeripheral.Enabled = false;
            }
            if (DESCRIPTION.Text.Length == 0)
            {
                InvalidName.Visible = false;
                AddPeripheral.Enabled = false;
            }
        }
    }
}

