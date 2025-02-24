using NorAir;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using static TOGA.RNSForm;

namespace TOGA
{
    public partial class MBx24Form : Form
    {
        public class PeripheralProperty
        {
            public string SERIALNUMBER { get; set; }
            public string DESCRIPTION { get; set; }
            public int ID { get; set; }
        }

        public class MBx24Input // Class of input types
        {
            public byte CONNECTOR { get; set; }
            public string DESCRIPTION { get; set; }
            public byte JOY { get; set; }
            public byte JOYTYPE { get; set; }
            public byte VICE { get; set; }
        }

        public class MBx24Output    // Class for output types
        {
            public byte CONNECTOR { get; set; }
            public string DESCRIPTION { get; set; }
            public byte FLASHING { get; set; }
            public byte INVERTED { get; set; }
        }

        public class MBx24Settings      // A class for the MBx24 settings
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }
            public int FIRSTBUTTON { get; set; }
            public IList<MBx24Input> INPUTS { get; set; }
            public IList<MBx24Output> OUTPUTS { get; set; }

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

        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";
        public string StylesFileName = BasePath + ".\\Styles.json";
        public string JSonPath = BasePath;
        public string nl = Environment.NewLine;

        public System.Windows.Forms.TextBox[] InputDescriptions = new System.Windows.Forms.TextBox[32];
        public System.Windows.Forms.TextBox[] InputConnectors = new System.Windows.Forms.TextBox[32];
        public System.Windows.Forms.ComboBox[] JoyButton = new System.Windows.Forms.ComboBox[32];
        public System.Windows.Forms.ComboBox[] JoyType = new System.Windows.Forms.ComboBox[32];
        public System.Windows.Forms.ComboBox[] VICE = new System.Windows.Forms.ComboBox[32];
        public System.Windows.Forms.CheckBox[] Flashing = new System.Windows.Forms.CheckBox[32];
        public System.Windows.Forms.CheckBox[] Inverted = new System.Windows.Forms.CheckBox[32];
        public System.Windows.Forms.TextBox[] OutputDescriptions = new System.Windows.Forms.TextBox[32];

        public Color backColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color buttonColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color borderColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public decimal borderSize = 2;
        public Color mouseOverColor = (Color)ColorTranslator.FromHtml("#E0E0E0");
        public Font CharFont = new Font("Verdana", 8);

        public OBCS CPort = null;
        public MBx24 MB = null;
        public int[] ConnectorsLogics; // Array for the connectors logic state

        public Thread ReadConnectors;   // A thread for periodically read connectors an update the form
        public bool ReadConnectorsIsRunning = false; // Flag for thread control & status
        public bool stop = true;    // Flag of thread continuity


        public MBx24Form()
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
            string fontcopy = CharFont.FontFamily.ToString();
            float fontsize = CharFont.SizeInPoints;
            Font BoldCharFont = new Font(fontcopy, fontsize + 1.0f, FontStyle.Bold);
            label8.Font = BoldCharFont;
            label8.BackColor = frontColor;
            label8.ForeColor = backColor;
            label9.Font = BoldCharFont;
            label9.BackColor = frontColor;
            label9.ForeColor = backColor;
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

                //using (FileStream fs = new FileStream(StylesFileName, FileMode.OpenOrCreate))
                //{
                //    //                    JsonConvert.SerializeObject<styles>(fs, style, settings);
                //    JsonConvert.SerializeObject(style);
                //}
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
        public DialogResult Show(string serial, int id, string desc, Color Bck, Color Frt, OBCS CPORT, MBx24 mbx24)
        {
            if (CPort == null)
            {
                CPort = CPORT;
            }
            if (MB == null)
            {
                MB = mbx24;
            }

            SERIAL.Text = serial;
            ID.Text = id.ToString();
            DESCRIPTION.Text = desc;
            backColor = Bck;
            frontColor = Frt;
            ReadStyles();
            SetTheme(this.Controls, backColor, frontColor);
            this.BackColor = backColor;
            DrawPanels();
            FillData(serial, id, desc);
            this.ActiveControl = DESCRIPTION;
            this.StartPosition = FormStartPosition.CenterParent;
            ReadConnectors = new Thread(() => ReadMBx24Connectors());
            ReadConnectors.IsBackground = true;
            stop = false;
            ReadConnectors.Start();
            return (this.ShowDialog());
        }


        public void ReadMBx24Connectors()
        {
            /// <summary>
            /// This is a thread to periodicaly read the connectors to 
            /// update the form with their logic states
            /// </summary>

            ReadConnectorsIsRunning = true;
            while (!stop)
            {
                if (CPort.Found)
                {
                    byte[] answer = new byte[32];
                    answer = CPort.Send(MB.ReportConnectors());
                    if (answer != null)
                    {
                        this.ConnectorsLogics = MB.DecodeConnectors(answer);
                        if (ConnectorsLogics != null)
                        {
                            for (int i = 0; i < 32; i++)
                            {
                                if (ConnectorsLogics[i] == 0)
                                {
                                    this.InputConnectors[i].BackColor = backColor;
                                    this.InputConnectors[i].ForeColor = frontColor;
                                }
                                else
                                {
                                    this.InputConnectors[i].BackColor = frontColor;
                                    this.InputConnectors[i].ForeColor = backColor;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(200);
            }
            ReadConnectorsIsRunning = false;
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Save definitions to a file
            /// </summary>

            //var settings = new JsonConvertOptions()
            //{
            //    IncludeFields = true,
            //    PropertyNameCaseInsensitive = true,
            //    WriteIndented = true
            //};

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string serial = SERIAL.Text;
            int id = int.Parse(ID.Text);

            string JSonName = SERIAL.Text;
            string JSonFile = JSonPath + "\\" + JSonName + ".json";
            try
            {
                MBx24Settings mbx24Sets = new MBx24Settings();
                List<MBx24Input> LI = new List<MBx24Input>();
                List<MBx24Output> LO = new List<MBx24Output>();


                mbx24Sets.SERIAL = serial;
                mbx24Sets.ID = id;
                mbx24Sets.DESCRIPTION = DESCRIPTION.Text;
                mbx24Sets.FIRSTBUTTON = (int)FirstButton.Value;

                for (int i = 0; i < 32; i++)
                {
                    MBx24Input mbx24Input = new MBx24Input();
                    MBx24Output mbx24Output = new MBx24Output();

                    mbx24Input.CONNECTOR = (byte)(i + 1);
                    mbx24Input.DESCRIPTION = InputDescriptions[i].Text;
                    mbx24Input.JOY = (byte)(JoyButton[i].SelectedIndex + 1);
                    mbx24Input.JOYTYPE = (byte)(JoyType[i].SelectedIndex);
                    mbx24Input.VICE = (byte)(VICE[i].SelectedIndex);

                    mbx24Output.CONNECTOR = (byte)(i + 1);
                    mbx24Output.DESCRIPTION = OutputDescriptions[i].Text;
                    mbx24Output.FLASHING = 0;
                    mbx24Output.INVERTED = 0;
                    if (Flashing[i].Checked == true)
                    {
                        mbx24Output.FLASHING = 1;
                    }
                    if (Inverted[i].Checked == true)
                    {
                        mbx24Output.INVERTED = 1;
                    }

                    LI.Add(mbx24Input);
                    LO.Add(mbx24Output);
                }

                mbx24Sets.INPUTS = LI;
                mbx24Sets.OUTPUTS = LO;

                string line = JsonConvert.SerializeObject(mbx24Sets, settings);

                File.WriteAllText(JSonFile, line);

                StatusLabel.Text = "Saved";
                MessageBox.Show("Configuration saved", "MBx24 save", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch
            {
                Say("An error occoured while saving MBx24 configuration.");
            }

            return;
        }


        public MBx24Settings LoadData(string serial = null, int id = 0, string desc = null)
        {
            /// <summary>
            /// Load definitions from a file.
            /// Returns a MBx24XPLANEDATA object.
            /// </summary>
            /// 

            if (serial == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string JSonFile = JSonPath + "\\" + serial + ".json";

            MBx24Settings mbx24Sets = new MBx24Settings();

            if (!File.Exists(JSonFile))
            {
                if (desc == null)
                {
                    return null;
                }

                CreateDefaultMBx24Config(serial, id, desc);
            }

            string lines = File.ReadAllText(JSonFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            try
            {
                mbx24Sets = JsonConvert.DeserializeObject<MBx24Settings>(lines);  // Deserialize 'lines'
            }
            catch
            {
                return null;
            }

            return mbx24Sets;
        }


        public void FillData(string serial, int id, string desc)
        {
            /// <summary>
            /// Fills the setup panel fields with MBx24 input/output definitions
            /// </summary>

            Debug.WriteLine(serial);
            MBx24Settings mbx24Sets = LoadData(serial, id, desc);

            SERIAL.Text = mbx24Sets.SERIAL;
            ID.Text = mbx24Sets.ID.ToString();
            DESCRIPTION.Text = desc;
            FirstButton.Value = (decimal)mbx24Sets.FIRSTBUTTON;

            foreach (MBx24Input inp in mbx24Sets.INPUTS)
            {
                int i = inp.CONNECTOR - 1;
                InputDescriptions[i].Text = mbx24Sets.INPUTS[i].DESCRIPTION;
                JoyButton[i].SelectedIndex = (int)mbx24Sets.INPUTS[i].JOY - 1;
                JoyType[i].SelectedIndex = (int)mbx24Sets.INPUTS[i].JOYTYPE;
                VICE[i].SelectedIndex = (int)mbx24Sets.INPUTS[i].VICE;
            }

            foreach (MBx24Output outp in mbx24Sets.OUTPUTS)
            {
                int i = outp.CONNECTOR - 1;
                Flashing[i].Checked = false;
                OutputDescriptions[i].Text = mbx24Sets.OUTPUTS[i].DESCRIPTION;
                if (mbx24Sets.OUTPUTS[i].FLASHING != 0)
                {
                    Flashing[i].Checked = true;
                }
                Inverted[i].Checked = false;
                if (mbx24Sets.OUTPUTS[i].INVERTED != 0)
                {
                    Inverted[i].Checked = true;
                }
            }
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

        public bool CreateDefaultMBx24Config(string serial, int id, string desc)
        {
            /// <summary>
            /// Creates a default definitions file for the first interaction
            /// </summary>

            bool exitCode = true;
            FirstButton.Value = decimal.Parse(serial.Substring(0, 2));

            //var settings = new JsonConvertOptions()
            //{
            //    IncludeFields = true,
            //    PropertyNameCaseInsensitive = true,
            //    WriteIndented = true
            //};

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string JSonName = serial;
            string JSonFile = JSonPath + "\\" + JSonName + ".json";
            try
            {
                MBx24Settings mbx24Sets = new MBx24Settings();
                List<MBx24Input> LI = new List<MBx24Input>();
                List<MBx24Output> LO = new List<MBx24Output>();


                mbx24Sets.SERIAL = serial;
                mbx24Sets.ID = id;
                mbx24Sets.DESCRIPTION = desc;
                mbx24Sets.FIRSTBUTTON = (int)FirstButton.Value; ;
                for (int i = 0; i < 32; i++)
                {
                    MBx24Input mbx24Input = new MBx24Input();
                    MBx24Output mbx24Output = new MBx24Output();

                    mbx24Input.CONNECTOR = (byte)(i + 1);
                    mbx24Input.DESCRIPTION = "";
                    mbx24Input.JOY = (byte)(FirstButton.Value + i);
                    mbx24Input.JOYTYPE = 0;
                    mbx24Input.VICE = 0;

                    mbx24Output.CONNECTOR = (byte)(i + 1);
                    mbx24Output.DESCRIPTION = "";
                    mbx24Output.FLASHING = 0;
                    mbx24Output.INVERTED = 0;

                    LI.Add(mbx24Input);
                    LO.Add(mbx24Output);
                }

                mbx24Sets.INPUTS = LI;
                mbx24Sets.OUTPUTS = LO;

                string line = JsonConvert.SerializeObject(mbx24Sets, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch
            {
                exitCode = false;
            }

            return exitCode;
        }


        public void DrawPanels()
        {
            /// <summary>
            /// Draw a user control panel. Values are set to defaults
            /// </summary>

            SERIAL.BackColor = backColor;
            SERIAL.ForeColor = frontColor;
            SERIAL.BorderStyle = BorderStyle.FixedSingle;

            ID.BackColor = backColor;
            ID.ForeColor = frontColor;
            ID.BorderStyle = BorderStyle.FixedSingle;

            DESCRIPTION.BackColor = backColor;
            DESCRIPTION.ForeColor = frontColor;
            DESCRIPTION.BorderStyle = BorderStyle.FixedSingle;

            int locy = 4;
            for (int i = 0; i < 32; i++)
            {
                Point p = new Point();
                p.Y = locy;
                Size z = new Size();
                z.Height = 22;

                InputDescriptions[i] = new System.Windows.Forms.TextBox();
                JoyButton[i] = new System.Windows.Forms.ComboBox();
                JoyType[i] = new System.Windows.Forms.ComboBox();
                VICE[i] = new System.Windows.Forms.ComboBox();
                OutputDescriptions[i] = new System.Windows.Forms.TextBox();
                Flashing[i] = new System.Windows.Forms.CheckBox();
                Inverted[i] = new System.Windows.Forms.CheckBox();

                p.X = 2;            // Instatiate input index 
                z.Width = 24;
                var il = new System.Windows.Forms.TextBox();
                il.Location = p;
                il.Size = z;
                il.Name = "IL" + i.ToString("D2");
                il.Text = (i + 1).ToString("D2");
                il.TextAlign = HorizontalAlignment.Center;
                il.ReadOnly = true;
                il.BackColor = backColor;
                il.ForeColor = frontColor;
                il.BorderStyle = BorderStyle.FixedSingle;
                InputConnectors[i] = il;
                INPUTSPANEL.Controls.Add(il);


                p.X += z.Width + 2; // Instatiate input description textbox
                z.Width = 200;
                var inp = InputDescriptions[i];
                inp.Location = p;
                inp.Size = z;
                inp.Name = "INPDESC" + i.ToString("D2");
                inp.Text = "";
                inp.MaxLength = 20;
                inp.BackColor = backColor;
                inp.ForeColor = frontColor;
                inp.BorderStyle = BorderStyle.FixedSingle;
                InputDescriptions[i] = inp;
                INPUTSPANEL.Controls.Add(InputDescriptions[i]);

                p.X += z.Width + 20; // Instatiate joystick input number
                z.Width = 56;
                var jb = JoyButton[i];
                jb.Location = p;
                jb.Size = z;
                jb.Name = "JOY" + i.ToString("D2");
                for (int j = 1; j != 201; j++)
                {
                    jb.Items.Add((j).ToString("D3"));
                }
                jb.BackColor = backColor;
                jb.ForeColor = frontColor;
                JoyButton[i] = jb;
                jb.FlatStyle = FlatStyle.Standard;
                jb.DropDownStyle = ComboBoxStyle.DropDownList;
                INPUTSPANEL.Controls.Add(JoyButton[i]);
                JoyButton[i].SelectedIndex = JoyButton[i].FindStringExact(i.ToString());

                p.X += z.Width + 20; // Instatiate joystick input type
                z.Width = 140;
                var jbtype = JoyType[i];
                jbtype.Location = p;
                jbtype.Size = z;
                jbtype.Name = "JOYTYPE" + i.ToString("D2");
                jbtype.Items.Add("Push button");
                jbtype.Items.Add("Toggle");
                jbtype.Items.Add("Inverted push button");
                jbtype.Items.Add("Inverted toggle button");
                jbtype.BackColor = backColor;
                jbtype.ForeColor = frontColor;
                jbtype.FlatStyle = FlatStyle.Standard;
                jbtype.DropDownStyle = ComboBoxStyle.DropDownList;
                JoyType[i] = jbtype;
                INPUTSPANEL.Controls.Add(JoyType[i]);
                JoyType[i].SelectedIndex = 0;

                p.X += z.Width + 20; // Instatiate VICE joystick input number
                z.Width = 56;
                var vc = VICE[i];
                vc.Location = p;
                vc.Size = z;
                vc.Name = "VICE" + i.ToString("D2");
                for (int j = 0; j != 201; j++)
                {
                    vc.Items.Add((j).ToString("D3"));
                }
                vc.BackColor = backColor;
                vc.ForeColor = frontColor;
                vc.FlatStyle = FlatStyle.Standard;
                vc.DropDownStyle = ComboBoxStyle.DropDownList;
                VICE[i] = vc;
                INPUTSPANEL.Controls.Add(VICE[i]);
                VICE[i].SelectedIndex = 0;

                p.X = 2;                            // Instatiate output index
                z.Width = 24;
                var ol = new System.Windows.Forms.TextBox();
                ol.Location = p;
                ol.Size = z;
                ol.Name = "IL" + i.ToString("D2");
                ol.Text = (i + 1).ToString("D2");
                ol.TextAlign = HorizontalAlignment.Center;
                ol.ReadOnly = true;
                ol.BackColor = backColor;
                ol.ForeColor = frontColor;
                ol.BorderStyle = BorderStyle.FixedSingle;
                OUTPUTSPANEL.Controls.Add(ol);

                Point pp = new Point();             // Instatiate panel to shelter flashing checkbox
                Size zz = new Size(80, 28);
                p.X += z.Width + 4;
                z.Width = 94;
                pp.Y = locy - 4;
                pp.X = p.X;
                var panelFlash = new System.Windows.Forms.Panel();
                panelFlash.BorderStyle = BorderStyle.None;
                panelFlash.Location = pp;
                panelFlash.Size = zz;
                panelFlash.Name = "PL" + i.ToString("D2");
                OUTPUTSPANEL.Controls.Add(panelFlash);

                var fl = Flashing[i];               // Instatiate flashing checkbox
                fl.Appearance = Appearance.Button;
                fl.FlatAppearance.CheckedBackColor = Color.DarkRed;
                fl.FlatAppearance.BorderSize = 0;
                fl.FlatAppearance.BorderColor = Color.Gray;
                fl.FlatStyle = FlatStyle.Flat;
                fl.Size = z;
                pp.X = 2;
                pp.Y = 2;
                fl.Location = pp;
                fl.Checked = false;
                fl.BackColor = backColor;
                fl.ForeColor = frontColor;
                fl.Text = "Flashing";
                fl.Name = "FLASHING" + i.ToString("D2");
                panelFlash.Controls.Add(Flashing[i]);


                p.X += zz.Width + 2;                // Instatiate panel to shelter inverted checkbox
                z.Width = 80;
                pp.Y = locy - 4;
                pp.X = p.X;
                var panelInverted = new System.Windows.Forms.Panel();
                panelInverted.BorderStyle = BorderStyle.None;
                panelInverted.Location = pp;
                panelInverted.Size = zz;
                panelInverted.Name = "PL" + i.ToString("D2");
                OUTPUTSPANEL.Controls.Add(panelInverted);

                var inv = Inverted[i];              // Instatiate inverted checkbox
                inv.Appearance = Appearance.Button;
                inv.FlatAppearance.CheckedBackColor = Color.DarkRed;
                inv.FlatAppearance.BorderSize = 0;
                inv.FlatAppearance.BorderColor = Color.Orange;
                inv.FlatStyle = FlatStyle.Flat;
                inv.Size = z;
                pp.X = 2;
                pp.Y = 2;
                inv.Location = pp;
                inv.Checked = false;
                inv.BackColor = backColor;
                inv.ForeColor = frontColor;
                inv.Text = "Inverted";
                inv.Name = "INVERTED" + i.ToString("D2");
                panelInverted.Controls.Add(Inverted[i]);

                var tb = new System.Windows.Forms.Button();              // Instatiate test button
                p.X += zz.Width + 2;
                z.Width = 48;
                tb.FlatStyle = FlatStyle.Flat;
                tb.Size = z;
                pp.Y = locy;
                pp.X = 200;
                tb.Location = pp;
                tb.BackColor = backColor;
                tb.ForeColor = frontColor;
                tb.Text = "Test";
                tb.Name = "TEST" + (i + 1).ToString("D3");
                OUTPUTSPANEL.Controls.Add(tb);
                tb.Click += new EventHandler(TestConnector);

                p.X += z.Width + 16; // Instatiate output description textbox
                z.Width = 200;
                var outp = OutputDescriptions[i];
                outp.Location = p;
                outp.Size = z;
                outp.Name = "OUTDESC" + i.ToString("D2");
                outp.Text = "";
                outp.MaxLength = 20;
                outp.BackColor = backColor;
                outp.ForeColor = frontColor;
                outp.BorderStyle = BorderStyle.FixedSingle;
                OutputDescriptions[i] = outp;
                OUTPUTSPANEL.Controls.Add(OutputDescriptions[i]);

                locy = z.Height + locy + 2;
            }
        }

        private void TestConnector(object sender, EventArgs e)
        {
            Button button = sender as Button;
            //Debug.WriteLine(button.Name.Substring(4, 3));
            byte connector = byte.Parse(button.Name.Substring(4, 3));
            int index = (int)(connector - 1);
            if (ReadConnectorsIsRunning)
            {
                byte[] answer = new byte[32];
                byte flashing = 0;
                byte inverted = 0;
                if (Flashing[index].Checked)
                {
                    flashing = MB.OutputTypeFlashing;
                }
                if (Inverted[index].Checked)
                {
                    inverted = MB.Inverted;
                }
                answer = CPort.Send(MB.SetOutputType(connector, flashing, 0, inverted));

                Debug.WriteLine(MB.Id);

                if (answer[2] == 1)
                {
                    answer = CPort.Send(MB.SetOutput(connector, MB.ON));
                    MessageBox.Show("Turning ON connector " + connector.ToString("D2"), "Output connector test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    answer = CPort.Send(MB.SetOutput(connector, MB.OFF));
                }
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Exit button press event
            /// </summary>

            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FirstButton_ValueChanged(object sender, EventArgs e)
        {
            /// <summary>
            /// Event to track changes in the first button field and update the joystick buttons number
            /// </summary>

            int multiplier = 1;
            if (Interpolation.Checked)
            {
                multiplier = 2;
            }

            try
            {
                for (int i = 0; i < JoyButton.Length; i++)
                {
                    if (JoyButton != null)
                    {
                        if (JoyButton[i] != null)
                        {
                            JoyButton[i].SelectedIndex = (int)FirstButton.Value + (i * multiplier) - 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MBx24Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            /// <summary>
            /// Exiting the form, there should be a proper thread stop an disposal.
            /// </summary>

            stop = true;
            //while (ReadConnectorsIsRunning)
            //{
            //    // Wait thread stops

            //}
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
