using Newtonsoft.Json;
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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static TOGA.Form1;
using static TOGA.MSFSSetup;


namespace TOGA
{
    public partial class RNSForm : Form
    {

        public class RNSSettings      // A class for the RNS settings
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }
            public string COMMMAX { get; set; }
            public string COMMMIN { get; set; }
            public int COMMINTINC { get; set; }
            public int COMMDECINC { get; set; }
            public string NAVMAX { get; set; }
            public string NAVMIN { get; set; }
            public int NAVINTINC { get; set; }
            public int NAVDECINC { get; set; }
            public int ADFMAX { get; set; }
            public int ADFMIN { get; set; }
            public int ADFINTINC { get; set; }
            public int ADFDECINC { get; set; }
            public int XPDRMAX { get; set; }
            public int XPDRMIN { get; set; }
            public int XPDRINTINC { get; set; }
            public int XPDRDECINC { get; set; }
            public byte COMMBUTTON { get; set; }
            public byte COMMBUTTONTYPE { get; set; }
            public byte NAVBUTTON { get; set; }
            public byte NAVBUTTONTYPE { get; set; }
            public byte ADFBUTTON { get; set; }
            public byte ADFBUTTONTYPE { get; set; }
            public byte XPDRBUTTON { get; set; }
            public byte XPDRBUTTONTYPE { get; set; }

        }

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
            public string serialNumber;
            public int Id;
        }

        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";
        public string JSonPath = BasePath;
        public string StylesFileName = BasePath + ".\\Styles.json";
        public string nl = Environment.NewLine;

        public Color backColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color buttonColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color borderColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public decimal borderSize = 2;
        public Color mouseOverColor = (Color)ColorTranslator.FromHtml("#E0E0E0");
        public Font CharFont = new Font("Verdana", 8);

        public RNSForm()
        {
            InitializeComponent();
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
                    if (control is System.Windows.Forms.StatusStrip)
                    {
                        control.BackColor = fcolor;
                        control.ForeColor = bcolor;
                        control.Font = CharFont;
                        StatusLabel.BackColor = fcolor;
                        StatusLabel.ForeColor = bcolor;
                        StatusLabel.Font = CharFont;
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
            statusStrip1.BackColor = bcolor;
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

            lines = verifyBrackects(lines);

            style = JsonConvert.DeserializeObject<Styles>(lines);  // Deserialize 'lines'

            backColor = ColorTranslator.FromHtml(style.backColor);
            frontColor = ColorTranslator.FromHtml(style.frontColor);
            buttonColor = ColorTranslator.FromHtml(style.buttonColor);
            mouseOverColor = ColorTranslator.FromHtml(style.mouseoverColor);
            borderSize = style.borderSize;
            borderColor = ColorTranslator.FromHtml(style.borderColor);
            CharFont = new Font(style.fontName, style.fontSize);
        }

        public DialogResult Show(string serial, int id, string desc, Color bck, Color frt)
        {
            SERIAL.Text = serial;
            ID.Text = id.ToString();
            backColor = bck;
            frontColor = frt;
            ReadStyles();
            SetTheme(this.Controls, backColor, frontColor);
            this.BackColor = backColor;
            SetDefaults();
            FillFields(serial, id, desc);
            this.ActiveControl = DESCRIPTION;
            this.StartPosition = FormStartPosition.CenterParent;
            return (this.ShowDialog());
        }

        private void SetDefaults()
        {
            /// <summary>
            /// Sets up default values to comboboxes
            /// </summary>

            COMMBUTTON.Items.Clear();
            NAVBUTTON.Items.Clear();
            ADFBUTTON.Items.Clear();
            XPDRBUTTON.Items.Clear();
            for (int i = 1; i < 201; i++)
            {
                COMMBUTTON.Items.Add(i.ToString("D3"));
                NAVBUTTON.Items.Add(i.ToString("D3"));
                ADFBUTTON.Items.Add(i.ToString("D3"));
                XPDRBUTTON.Items.Add(i.ToString("D3"));
            }
        }


        public bool CreateDefaultRNSConfig(string serial, int id,string desc)
        {
            /// <summary>
            /// Creates a default definitions file for the first interaction
            /// </summary>

            bool exitCode = true;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string JSonName = serial;
            string JSonFile = JSonPath + "\\" + JSonName + ".json";

            RNSSettings rnsSets = new RNSSettings();

            rnsSets.SERIAL = serial;
            rnsSets.ID = id;
            rnsSets.DESCRIPTION = desc;
            rnsSets.COMMBUTTON = 1;
            rnsSets.COMMBUTTONTYPE = 0;
            rnsSets.NAVBUTTON = 2;
            rnsSets.NAVBUTTONTYPE = 0;
            rnsSets.ADFBUTTON = 3;
            rnsSets.ADFBUTTONTYPE = 0;
            rnsSets.XPDRBUTTON = 4;
            rnsSets.XPDRBUTTONTYPE = 0;
            rnsSets.COMMMAX = "136.975";
            rnsSets.COMMMIN = "118.000";
            rnsSets.COMMINTINC = 1;
            rnsSets.COMMDECINC = 25;
            rnsSets.NAVMAX = "117.950";
            rnsSets.NAVMIN = "108.000";
            rnsSets.NAVINTINC = 1;
            rnsSets.NAVDECINC = 50;
            rnsSets.ADFMAX = 1750;
            rnsSets.ADFMIN = 0000;
            rnsSets.ADFINTINC = 1;
            rnsSets.ADFDECINC = 1;
            rnsSets.XPDRMAX = 7777;
            rnsSets.XPDRMIN = 0000;
            rnsSets.XPDRINTINC = 1;
            rnsSets.XPDRDECINC = 1;

            try
            {

                string line = JsonConvert.SerializeObject(rnsSets, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch
            {
                exitCode = false;
            }

            return exitCode;
        }

        public RNSSettings LoadData(string serial, int id, string desc = null)
        {
            /// <summary>
            /// Load definitions from a file.
            /// Returns a RNSSettings object.
            /// </summary>

            string JSonFile = JSonPath + "\\" + serial + ".json";

            RNSSettings rnsSets = new RNSSettings();

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            if (!File.Exists(JSonFile))
            {
                if (desc == null)
                {
                    return null;
                }
                CreateDefaultRNSConfig(serial, id,desc);
            }

            string lines = File.ReadAllText(JSonFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            try
            {
                rnsSets = JsonConvert.DeserializeObject<RNSSettings>(lines);  // Deserialize 'lines'

            }
            catch
            {
                return null;
            }

            return rnsSets;
        }

        public string verifyBrackects(string line)
        {
            /// <summary>
            /// Why verify brackets? At present time, C# JSON has some issues with jagged arrays and other objects.
            /// Sometimes, a freaking curl bracket is added for no reason. It was found that an object with a string
            /// field followed by a list of objects has this side effect after resizing the string. To minimize this,
            /// verifyBrackets() cleans the retrieved JSON string unbalanced brackets before deserialize it.
            /// </summary>

            int curlBrackets = 0;
            int squareBrackets = 0;
            int curlBrPosition = 0;
            int squareBrPosition = 0;
            int lastPosition = 0;
            string LineToCheck = line;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].Equals('{'))
                {
                    curlBrackets--;
                    curlBrPosition = i;
                }
                if (line[i].Equals('}'))
                {
                    curlBrackets++;
                    curlBrPosition = i;
                }
                if (line[i].Equals('['))
                {
                    squareBrackets--;
                    squareBrPosition = i;
                }
                if (line[i].Equals(']'))
                {
                    squareBrackets++;
                    squareBrPosition = i;
                }
            }

            if (squareBrackets != 0)
            {
                // excessive close square brackets
                LineToCheck = line.Substring(0, squareBrPosition); //  LineToCheck.Length - 1)
                LineToCheck = verifyBrackects(LineToCheck); // Recursively test for more unbalanced brackets
                Debug.WriteLine(LineToCheck);
            }
            if (curlBrackets != 0)
            {
                // excessive close curl brackets
                LineToCheck = line.Substring(0, curlBrPosition); //  LineToCheck.Length - 1)
                LineToCheck = verifyBrackects(LineToCheck); // Recursively test for more unbalanced brackets
            }

            if (squareBrPosition > curlBrPosition)
            {
                lastPosition = squareBrPosition;
            }
            if (squareBrPosition < curlBrPosition)
            {
                lastPosition = curlBrPosition;
            }
            if ((LineToCheck.Length - 1) > lastPosition)
            {
                LineToCheck = LineToCheck.Substring(0, lastPosition + 1);
            }
            return LineToCheck;
        }

        public void FillFields(string serial, int id, string desc)
        {
            /// <summary>
            /// After loading data definitions, form fields are filled
            /// with loaded values
            /// </summary>

            RNSSettings rnsSets = LoadData(serial, id, desc);

            SERIAL.Text = rnsSets.SERIAL;
            ID.Text = rnsSets.ID.ToString();
            DESCRIPTION.Text = desc;

            COMMMAX.Text = rnsSets.COMMMAX;
            COMMMIN.Text = rnsSets.COMMMIN;
            COMMINTINC.Text = rnsSets.COMMINTINC.ToString();
            COMMDECINC.Text = rnsSets.COMMDECINC.ToString();
            COMMBUTTON.SelectedIndex = rnsSets.COMMBUTTON - 1;
            COMMBUTTONTYPE.SelectedIndex = rnsSets.COMMBUTTONTYPE;

            NAVMAX.Text = rnsSets.NAVMAX;
            NAVMIN.Text = rnsSets.NAVMIN;
            NAVINTINC.Text = rnsSets.NAVINTINC.ToString();
            NAVDECINC.Text = rnsSets.NAVDECINC.ToString();
            NAVBUTTON.SelectedIndex = rnsSets.NAVBUTTON - 1;
            NAVBUTTONTYPE.SelectedIndex = rnsSets.NAVBUTTONTYPE;

            ADFMAX.Text = rnsSets.ADFMAX.ToString();
            ADFMIN.Text = rnsSets.ADFMIN.ToString();
            ADFINTINC.Text = rnsSets.ADFINTINC.ToString();
            ADFDECINC.Text = rnsSets.ADFDECINC.ToString();
            ADFBUTTON.SelectedIndex = rnsSets.ADFBUTTON - 1;
            ADFBUTTONTYPE.SelectedIndex = rnsSets.ADFBUTTONTYPE;

            XPDRMAX.Text = rnsSets.XPDRMAX.ToString();
            XPDRMIN.Text = rnsSets.XPDRMIN.ToString();
            XPDRINTINC.Text = rnsSets.XPDRINTINC.ToString();
            XPDRDECINC.Text = rnsSets.XPDRDECINC.ToString();
            XPDRBUTTON.SelectedIndex = rnsSets.XPDRBUTTON - 1;
            XPDRBUTTONTYPE.SelectedIndex = (int)rnsSets.XPDRBUTTONTYPE;

        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Exit button press event handler
            /// </summary>

            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Save definitions to a file
            /// </summary>

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string JSonName = SERIAL.Text;
            string JSonFile = JSonPath + "\\" + JSonName + ".json";

            RNSSettings rnsSets = new RNSSettings();

            try
            {
                rnsSets.SERIAL = SERIAL.Text;
                rnsSets.ID = int.Parse(ID.Text);
                rnsSets.DESCRIPTION = DESCRIPTION.Text;

                rnsSets.COMMBUTTON = (byte)(COMMBUTTON.SelectedIndex + 1);

                rnsSets.COMMBUTTONTYPE = (byte)COMMBUTTONTYPE.SelectedIndex;

                rnsSets.NAVBUTTON = (byte)(NAVBUTTON.SelectedIndex + 1);

                rnsSets.NAVBUTTONTYPE = (byte)NAVBUTTONTYPE.SelectedIndex;

                rnsSets.ADFBUTTON = (byte)(ADFBUTTON.SelectedIndex + 1);

                rnsSets.ADFBUTTONTYPE = (byte)ADFBUTTONTYPE.SelectedIndex;

                rnsSets.XPDRBUTTON = (byte)(XPDRBUTTON.SelectedIndex + 1);

                rnsSets.XPDRBUTTONTYPE = (byte)XPDRBUTTONTYPE.SelectedIndex;

                rnsSets.COMMMAX = COMMMAX.Text;
                rnsSets.COMMMIN = COMMMIN.Text;
                rnsSets.COMMINTINC = int.Parse(COMMINTINC.Text);
                rnsSets.COMMDECINC = int.Parse(COMMDECINC.Text);

                rnsSets.NAVMAX = NAVMAX.Text;
                rnsSets.NAVMIN = NAVMIN.Text;
                rnsSets.NAVINTINC = int.Parse(NAVINTINC.Text);
                rnsSets.NAVDECINC = int.Parse(NAVDECINC.Text);

                rnsSets.ADFMAX = int.Parse(ADFMAX.Text);
                rnsSets.ADFMIN = int.Parse(ADFMIN.Text);
                rnsSets.ADFINTINC = int.Parse(ADFINTINC.Text);
                rnsSets.ADFDECINC = int.Parse(ADFDECINC.Text);

                rnsSets.XPDRMAX = int.Parse(XPDRMAX.Text);
                rnsSets.XPDRMIN = int.Parse(XPDRMIN.Text);
                rnsSets.XPDRINTINC = int.Parse(XPDRINTINC.Text);
                rnsSets.XPDRDECINC = int.Parse(XPDRDECINC.Text);

                string line = JsonConvert.SerializeObject(rnsSets, settings);

                File.WriteAllText(JSonFile, line);

                MessageBox.Show("Configuration saved", "RNS save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch 
            {
                Say("An error occoured while saving RNS configuration.");
            }

            return;
        }

        public void Say(string s)
        {
            /// <summary>
            /// Show messages in a messagebox
            /// </summary>
            MessageBox.Show(s, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
