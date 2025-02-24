using Newtonsoft.Json;
using NorAir;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using TOGA.Properties;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using static TOGA.Form1;
using static TOGA.MBx24Form;
using static TOGA.RNSForm;
using static TOGA.XPlaneSetup;
using Font = System.Drawing.Font;


namespace TOGA
{
    public partial class XPlaneSetup : Form
    {

        public class RNSXPLANEDATA
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }
            public string BACKLITCOLOR { get; set; }
            public string ADFGHOSTBUTTON { get; set; }
            public string ADFBUTTONDATAREF { get; set; }
            public string XPDRGHOSTBUTTON { get; set; }
            public string XPDRBUTTONDATAREF { get; set; }
            public string COMMACTTOREAD { get; set; }
            public string COMMACTTOWRITE { get; set; }
            public string COMMSTBTOREAD { get; set; }
            public string COMMSTBTOWRITE { get; set; }
            public string NAVACTTOREAD { get; set; }
            public string NAVACTTOWRITE { get; set; }
            public string NAVSTBTOREAD { get; set; }
            public string NAVSTBTOWRITE { get; set; }
            public string ADFTOREAD { get; set; }
            public string ADFTOWRITE { get; set; }
            public string XPDRTOREAD { get; set; }
            public string XPDRTOWRITE { get; set; }

        }

        public class MBx24Reference // Class for each MBx24 input/output connector
        {
            public byte CONNECTOR { get; set; }
            public string DATAREFTOWRITE { get; set; }
            public byte ONVALUE { get; set; }
            public string GHOSTKEY { get; set; }
            public string DATAREFTOREAD { get; set; }
        }


        public class MBx24XPLANEDATA      // A class for the MBx24 settings
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }  //var newText2 = text2 + new string('.', 25 - text2.Length);
            public string BACKLITCOLOR { get; set; }
            public int FREQUENCY { get; set; }
            public IList<MBx24Reference> REFERENCES { get; set; }

        }

        public class BackLitColor
        {
            public Byte Red;    // Color channels
            public Byte Green;
            public Byte Blue;
            public Byte Alpha;
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

        public string XPlanePath = BasePath + @"\X-Plane";
        public string BackLitFileName = BasePath + ".\\BackLit.json";
        public string StylesFileName = BasePath + ".\\Styles.json";

        public List<PeripheralProperty> PeripheralsPresent = new List<PeripheralProperty>();
        public List<string> dataRefs = new List<string>();

        public TextBox[] DRTOREAD = new TextBox[32];
        public TextBox[] DRTOWRITE = new TextBox[32];
        public TextBox[] MBx24GHOSTKEYS = new TextBox[32];
        public NumericUpDown[] MBX24DRVALUE = new NumericUpDown[32];
        public const int BackLitColorSlots = 10;

        public string nl = Environment.NewLine;
        public string currentProfile;
        public string currentFileName;
        public string peripheralName;


        public Color backColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color buttonColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color borderColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public decimal borderSize = 2;
        public Color mouseOverColor = (Color)ColorTranslator.FromHtml("#E0E0E0");
        public System.Drawing.Font CharFont = new System.Drawing.Font("Verdana", 8);

        string GhostKey = "";
        string ADFGhostKey = "";
        string XPDRGhostKey = "";

        public XPlaneSetup()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
            this.Dispose();
        }

        public void Say(string s)
        {
            MessageBox.Show(s, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public DialogResult Show(string serial, int id, Color bck, Color frt, string fileName, string profileName, string peripheralname)
        {
            PeripheralsPresent.Clear();
            this.backColor = bck;
            this.frontColor = frt;
            this.BackColor = backColor;
            this.ForeColor = frontColor;
            XPlanePanel.BackColor = backColor;
            XPlanePanel.ForeColor = frontColor;
            SERIAL.Text = serial;
            ID.Text = id.ToString();
            ReadStyles();
            SetTheme(this.Controls, backColor, frontColor);
            dataRefs = ReadDataRefs();
            ReadPreferredBackLitColors();
            int hardware = int.Parse(serial.Substring(0, 2));
            bool newProfile = !File.Exists(fileName);
            currentProfile = profileName;
            currentFileName = fileName;
            peripheralName = peripheralname.Split(',')[1];
            switch (hardware)
            {
                case 31:
                    DrawXPlaneRNS();
                    RNSLoadData(serial, id);
                    //this.ActiveControl = DESCRIPTION; // Set focus on a field
                    break;

                case 50:
                    DrawXPlaneMBx24();
                    MBx24FillData(serial, id);
                    this.ActiveControl = label1; // Set focus on a field
                    break;
            }

            this.Refresh();
            return (this.ShowDialog());
        }

        public void ReadPreferredBackLitColors()
        {
            /// <summary>
            /// Read the backlit preferred colors to color slots.
            /// There can be 10 of them and can be changed by using the color dialog.
            /// 
            /// Remarks:
            /// - The RGB led colors may differ from the ones showed on screen. This
            /// is due to the different color platforms such as between screen and RGB
            /// led lights. User may try to approach by tunning the color. This can
            /// be done on-the-fly if peripherals are already attached turned on. After
            /// leaving the color dialog, if an OBCS is already found, the
            /// backlit color of peripherals changes to the selected color.
            /// </summary>

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string lines;

            int count = 0;
            int DefaultColor = 0;

            List<PrefColor> colorsList = new List<PrefColor>();

            try
            {
                // Create the file with defaults if it doesn't exists
                if (!File.Exists(BackLitFileName))
                {
                    List<PrefColor> tempList = new List<PrefColor>();
                    tempList.Add(new PrefColor { Red = 17, Green = 15, Blue = 0, White = 255, Slot = 0, Default = true });
                    tempList.Add(new PrefColor { Red = 201, Green = 254, Blue = 0, White = 255, Slot = 1, Default = false });
                    tempList.Add(new PrefColor { Red = 2, Green = 0, Blue = 56, White = 255, Slot = 2, Default = false });
                    tempList.Add(new PrefColor { Red = 0, Green = 9, Blue = 255, White = 255, Slot = 3, Default = false });
                    tempList.Add(new PrefColor { Red = 76, Green = 25, Blue = 0, White = 255, Slot = 4, Default = false });
                    tempList.Add(new PrefColor { Red = 27, Green = 0, Blue = 225, White = 255, Slot = 5, Default = false });
                    tempList.Add(new PrefColor { Red = 255, Green = 255, Blue = 0, White = 255, Slot = 6, Default = false });
                    tempList.Add(new PrefColor { Red = 141, Green = 58, Blue = 0, White = 255, Slot = 7, Default = false });
                    tempList.Add(new PrefColor { Red = 0, Green = 53, Blue = 150, White = 255, Slot = 8, Default = false });
                    tempList.Add(new PrefColor { Red = 68, Green = 162, Blue = 0, White = 255, Slot = 9, Default = false });

                    string line = JsonConvert.SerializeObject(tempList, settings);

                    File.WriteAllText(BackLitFileName, line);

                }
            }
            catch
            {

            }

            // Read the file 

            lines = File.ReadAllText(BackLitFileName);    // Read all lines at once

            lines = verifyBrackects(lines);

            colorsList = JsonConvert.DeserializeObject<List<PrefColor>>(lines);  // Deserialize 'lines'

            // Fill radio buttons - aka, custom colors boxes - with the BackLitColors array values

            foreach (PrefColor c in colorsList)
            {
                var radiob = (XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(count);

                BackLitColor blc = new BackLitColor();
                blc.Red = c.Red;
                blc.Green = c.Green;
                blc.Blue = c.Blue;
                blc.Alpha = c.White;

                radiob.BackColor = Color.FromArgb(
                    c.White,
                    c.Red,
                    c.Green,
                    c.Blue);

                if (c.Default)
                {
                    radiob.Checked = true;
                    DefaultColor = (int)c.Slot;
                }

                count++;
                if (count >= BackLitColorSlots)
                {
                    break;
                }

            }
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
                        System.Drawing.Font AlternateCharFont = new System.Drawing.Font(font, 9);

                        control.Font = AlternateCharFont;

                    }
                    if (control is System.Windows.Forms.TreeView)
                    {
                        control.BackColor = bcolor;
                        control.ForeColor = fcolor;
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

            lines = verifyBrackects(lines);

            style = JsonConvert.DeserializeObject<Styles>(lines);  // Deserialize 'lines'

            backColor = ColorTranslator.FromHtml(style.backColor);
            frontColor = ColorTranslator.FromHtml(style.frontColor);
            buttonColor = ColorTranslator.FromHtml(style.buttonColor);
            mouseOverColor = ColorTranslator.FromHtml(style.mouseoverColor);
            borderSize = style.borderSize;
            borderColor = ColorTranslator.FromHtml(style.borderColor);
            CharFont = new System.Drawing.Font(style.fontName, style.fontSize);
        }

        public List<string> ReadDataRefs()
        {
            /// <summary>
            /// This constructs a TreeView based on the XPlane DataRefs
            /// It is also created a right mouse button menu to allow
            /// user to copy the meaningful data to paste in the fields
            /// </summary>

            string dataRefFileName = XPlanePath + ".\\DataRefs.txt";

            if (!File.Exists(dataRefFileName))
            {
                Say(dataRefFileName + "\n file DataRefs.txt does not exits");
                return null;
            }

            List<string> values = new List<string>();
            List<string> lines = new List<string>();

            // Open the stream and retrieve data to list 'values'.
            using (StreamReader reader = new StreamReader(dataRefFileName))
            {
                UTF8Encoding temp = new UTF8Encoding(true);

                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("sim"))
                    {
                        lines.Add(line);
                        string l = line.Split('\t')[0];
                        values.Add(l);
                    }
                }
            }


            treeView1.PathSeparator = "/";  // Define the path separation char

            TreeNode lastNode = null;
            foreach (string path in lines) // Fill in the TreeView
            {
                string pathDescrition = "";
                string auxPath = "";
                string[] elementsToRead = path.Split('\t');
                string pathToRead = elementsToRead[0];
                string[] realPath = pathToRead.Split('/');
                if (elementsToRead.Length > 4)
                {
                    pathDescrition = elementsToRead[4];
                }
                int counter = 0;
                foreach (string pathElement in realPath)
                {

                    if (pathElement.EndsWith("/"))
                    {
                        auxPath += pathElement;
                    }
                    else
                    {
                        auxPath += pathElement + '/';
                    }
                    TreeNode[] nodes = treeView1.Nodes.Find(auxPath, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                        {
                            lastNode = treeView1.Nodes.Add(auxPath, pathElement);
                        }
                        else
                        {
                            lastNode = lastNode.Nodes.Add(auxPath, pathElement);
                        }
                    else
                    {
                        lastNode = nodes[0];
                    }
                    if (realPath.Last().Equals(pathElement))
                    {
                        // The data to setup XPlane fields is at the "end of the branch"
                        // This reduces the chance of filling them with non-valid data from
                        // the TreeView

                        lastNode.Text += ": " + pathDescrition;
                    }
                    counter++;
                }
            }

            // Build a context menu to be called by the mouse right button

            ContextMenuStrip docMenu = new ContextMenuStrip(); // Instantiate
            docMenu.Click += new EventHandler(CopyDataRef);     // Event handler
            ToolStripMenuItem copyLabel = new ToolStripMenuItem();  // Create a label "Copy"
            copyLabel.Text = "Copy, please";
            docMenu.Items.AddRange(new ToolStripMenuItem[] { copyLabel }); // Fill the menu

            treeView1.ContextMenuStrip = docMenu; // Add it to the TreeView

            return values;
        }

        private void CopyDataRef(object sender, EventArgs e)
        {
            string t = treeView1.SelectedNode.Name.Trim();
            if (t.Length > 10)
            {
                if (t.EndsWith("/"))
                {
                    t = t.Substring(0, t.Length - 1);
                }
                Clipboard.SetText(t);
            }
        }


        // =============================== RNS hardware setup section =================================

        public void DrawXPlaneRNS()
        {
            /// <summary>
            /// Method to paint the XPlane RNS definitions panel
            /// </summary>

            // layout references

            int origin = 22; // Vertical reference point
            int p = origin + 4;
            int h = 28;
            int col0 = 2;
            int col1 = 90;
            int col2 = 490;

            Label Instr = new Label()       // Headers
            {
                Name = "Instr",
                Height = 20,
                Location = new Point(col0, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Instrument"
            };

            Label DRtoWrite = new Label()
            {
                Name = "DRtoWrite",
                Height = 20,
                Location = new Point(col1 + 100, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "DataRef to write"
            };

            Label DRtoRead = new Label()
            {
                Name = "DRtoRead",
                Height = 20,
                Location = new Point(col2 + 100, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "DataRef to read"
            };

            Label COMMACT = new Label()         // Labels
            {
                Name = "COMMACT",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "COMM act."
            };

            p += h;
            Label COMMSTB = new Label()
            {
                Name = "COMMSTB",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "COMM stb."
            };

            p += h;
            Label NAVACT = new Label()
            {
                Name = "NAVACT",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "NAV active"
            };

            p += h;
            Label NAVSTB = new Label()
            {
                Name = "NAVSTB",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "NAV standby"
            };

            p += h;
            Label ADF = new Label()
            {
                Name = "ADF",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "ADF"
            };

            p += h;
            Label XPDR = new Label()
            {
                Name = "XPDR",
                Size = new Size(69, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "XPDR"
            };

            p += h;
            Label ADFGHOST = new Label()
            {
                Name = "ADFGHOSTLABEL",
                Size = new Size(100, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "ADF ghost key"
            };

            Label ADFBUTTONDATAREFLABEL = new Label()
            {
                Name = "ADFDATAREFLABEL",
                Size = new Size(100, 16),
                Location = new Point(col1 + 200, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Button DataRef"
            };

            p += h;
            Label XPDRGHOST = new Label()
            {
                Name = "XPDRGHOSTLABEL",
                Size = new Size(100, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "XPDR ghost key"
            };

            Label XPDRBUTTONDATAREFLABEL = new Label()
            {
                Name = "XPDRBUTTONDATAREFLABEL",
                Size = new Size(100, 16),
                Location = new Point(col1 + 200, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Button DataRef"
            };

            p = origin + 2;
            TextBox COMMACTTOWRITE = new TextBox()      // TextBoxes
            {
                Name = "COMMACTTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10)
            };

            TextBox COMMACTTOREAD = new TextBox()
            {
                Name = "COMMACTTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox COMMSTBTOWRITE = new TextBox()
            {
                Name = "COMMSTBTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox COMMSTBTOREAD = new TextBox()
            {
                Name = "COMMSTBTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox NAVACTTOWRITE = new TextBox()
            {
                Name = "NAVACTTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox NAVACTTOREAD = new TextBox()
            {
                Name = "NAVACTTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox NAVSTBTOWRITE = new TextBox()
            {
                Name = "NAVSTBTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox NAVSTBTOREAD = new TextBox()
            {
                Name = "NAVSTBTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox ADFTOWRITE = new TextBox()
            {
                Name = "ADFTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox ADFTOREAD = new TextBox()
            {
                Name = "ADFTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox XPDRTOWRITE = new TextBox()
            {
                Name = "XPDRTOWRITE",
                Size = new Size(400, 16),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox XPDRTOREAD = new TextBox()
            {
                Name = "XPDRTOREAD",
                Size = new Size(400, 16),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox ADFGHOSTBUTTON = new TextBox()
            {
                Name = "ADFGHOSTBUTTON",
                Size = new Size(120, 16),
                Location = new Point(col1 + 50, p),
                ReadOnly = true,
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox ADFBUTTONDATAREF = new TextBox()
            {
                Name = "ADFGHOSTDATAREF",
                Size = new Size(400, 16),
                Location = new Point(col1 + 320, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            p += h;
            TextBox XPDRGHOSTBUTTON = new TextBox()
            {
                Name = "XPDRGHOSTBUTTON",
                Size = new Size(120, 16),
                Location = new Point(col1 + 50, p),
                ReadOnly = true,
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            TextBox XPDRBUTTONDATAREF = new TextBox()
            {
                Name = "XPDRBUTTONDATAREF",
                Size = new Size(400, 16),
                Location = new Point(col1 + 320, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            // Add panel composition
            XPlanePanel.Controls.Add(Instr);
            XPlanePanel.Controls.Add(DRtoWrite);
            XPlanePanel.Controls.Add(DRtoRead);
            XPlanePanel.Controls.Add(COMMACT);
            XPlanePanel.Controls.Add(COMMSTB);
            XPlanePanel.Controls.Add(NAVACT);
            XPlanePanel.Controls.Add(NAVSTB);
            XPlanePanel.Controls.Add(ADF);
            XPlanePanel.Controls.Add(XPDR);
            XPlanePanel.Controls.Add(ADFGHOST);
            XPlanePanel.Controls.Add(ADFBUTTONDATAREFLABEL);
            XPlanePanel.Controls.Add(XPDRBUTTONDATAREFLABEL);
            XPlanePanel.Controls.Add(ADFBUTTONDATAREF);
            XPlanePanel.Controls.Add(XPDRBUTTONDATAREF);
            XPlanePanel.Controls.Add(XPDRGHOST);
            XPlanePanel.Controls.Add(COMMACTTOREAD);
            XPlanePanel.Controls.Add(COMMSTBTOREAD);
            XPlanePanel.Controls.Add(NAVACTTOREAD);
            XPlanePanel.Controls.Add(NAVSTBTOREAD);
            XPlanePanel.Controls.Add(ADFTOREAD);
            XPlanePanel.Controls.Add(XPDRTOREAD);
            XPlanePanel.Controls.Add(COMMACTTOWRITE);
            XPlanePanel.Controls.Add(COMMSTBTOWRITE);
            XPlanePanel.Controls.Add(NAVACTTOWRITE);
            XPlanePanel.Controls.Add(NAVSTBTOWRITE);
            XPlanePanel.Controls.Add(ADFTOWRITE);
            XPlanePanel.Controls.Add(XPDRTOWRITE);
            XPlanePanel.Controls.Add(ADFGHOSTBUTTON);
            XPlanePanel.Controls.Add(XPDRGHOSTBUTTON);
            KeyPreview = true;

            // Add events
            ADFGHOSTBUTTON.KeyDown += new KeyEventHandler(RNSGHOSTBUTTON_KeyDown);
            XPDRGHOSTBUTTON.KeyDown += new KeyEventHandler(RNSGHOSTBUTTON_KeyDown);

            XPlanePanel.Refresh();
            // Done
        }

        private void RNSGHOSTBUTTON_KeyDown(object sender, KeyEventArgs e)
        {
            /// <summary>
            /// Event to capture keys pressed for the ADF and XPDR buttons ghosting
            /// Ctrl, Shift or Alt keys alone are not allowed
            /// </summary>
            /// 
            /// Description:
            /// Ghosting a button mimics of a key stroke as if it would be from a keyboard.
            /// This technique consists on mapping joystick buttons triggered and re-route
            /// to simulation as a keyboard combination. Although could be made by hardware,
            /// the frequency of situations that require ghosting is not so often that
            /// justifies more hardware build and firmware. So, it is implemented by the
            /// TOGA software.
            /// 
            /// Remarks:
            /// Care should be taken while sending keyboard strokes to an application. It may
            /// have unpredicted results if not implemented conveniently.
            /// 
            /// Tip:
            /// Also, here is displayed the result of the keydown event, otherwise,
            /// in cases like {CONTROL} 'V', the displayed text would be the
            /// contents of the clipboard and the key sequence string. Only
            /// the last one should be displayed, so both textboxes are readonly
            /// not allowing the writting by the user and fill it with the proper values.
            /// 
            /// This event method is shared by both ADF and XPDR ghost definitions
            /// 

            Keys k = e.KeyCode; // Holds the key or keys combination

            TextBox Sndr = (TextBox)sender; // Find out who was the sender (ADFGHOSTBUTTON / XPDRGHOSTBUTTON)

            if (k != Keys.ControlKey && k != Keys.ShiftKey && k != Keys.Alt)
            {

                if (e.Modifiers.ToString().Equals("None"))  // No Ctrl/Shift/Alt combination
                {
                    GhostKey = "{" + k.ToString() + "}";
                }
                else                                        // Combinations with modifiers 
                {
                    GhostKey = "{" + e.Modifiers.ToString() + "}" + "{" + k.ToString() + "}";
                }

                Control[] fields = Controls.Find(Sndr.Name, true); // Get the correspondent textbox

                if (k != Keys.Escape)
                {
                    fields[0].Text = GhostKey;      // Fill in the respective textbox field
                }
                else
                {
                    fields[0].Text = "";
                }

                if (fields[0].Name.Equals("ADFGHOSTBUTTON"))    // Variable attribution
                {
                    ADFGhostKey = GhostKey;
                }

                if (fields[0].Name.Equals("XPDRGHOSTBUTTON"))
                {
                    XPDRGhostKey = GhostKey;
                }
            }
        }



        public bool SaveDefaultRNS(string serial, int id)
        {
            string JSonFile = currentFileName;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string selectedColor = "";

            var SelectedCustomColor = XPlaneColorPanel.Controls.OfType<RadioButton>();

            if (SelectedCustomColor.Count() == 0)
            {
                Say("Error on custom colors panel.\n Colors not saved.");
                return false;
            }

            foreach (RadioButton rb in SelectedCustomColor) // Find out the default color or assume 0
            {

                if (rb.Checked)
                {
                    selectedColor = ColorTranslator.ToHtml(rb.BackColor);
                    break;
                }
            }

            RNSXPLANEDATA rnsXplanData = new RNSXPLANEDATA
            {
                SERIAL = serial,
                ID = id,
                DESCRIPTION = peripheralName,
                BACKLITCOLOR = selectedColor,
                ADFGHOSTBUTTON = "",
                XPDRGHOSTBUTTON = "",
                ADFBUTTONDATAREF = "",
                XPDRBUTTONDATAREF = "",
                COMMACTTOREAD = "",
                COMMACTTOWRITE = "sim/cockpit2/radios/actuators/com1_frequency_hz",
                COMMSTBTOREAD = "",
                COMMSTBTOWRITE = "sim/cockpit2/radios/actuators/com1_standby_frequency_hz",
                NAVACTTOREAD = "",
                NAVACTTOWRITE = "sim/cockpit2/radios/actuators/nav1_frequency_hz",
                NAVSTBTOREAD = "",
                NAVSTBTOWRITE = "sim/cockpit2/radios/actuators/nav1_standby_frequency_hz",
                ADFTOREAD = "",
                ADFTOWRITE = "sim/cockpit2/radios/actuators/adf1_frequency_hz",
                XPDRTOREAD = "",
                XPDRTOWRITE = "sim/cockpit2/radios/actuators/transponder_code"
            };

            try
            {
                string line = JsonConvert.SerializeObject(rnsXplanData, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch
            {
                return false;
            }
            return true;
        }

        public RNSXPLANEDATA RNSLoadData(string serial, int id)
        {
            RNSXPLANEDATA rnsXplaneData = new RNSXPLANEDATA();

            string JSonFile = currentFileName;

            // As arguments may be used by outside form calls, check things
            if (serial != null)
            {
                SERIAL.Text = serial;
            }
            if (id != 0)
            {
                ID.Text = id.ToString(); ;
            }

            if (!File.Exists(currentFileName))
            {
                SaveDefaultRNS(serial, id);
            }

            int trials = 10;
            string lines = "";
            while (trials != 0)
            {
                try
                {
                    lines = File.ReadAllText(JSonFile);    // Read all lines at once
                }
                catch
                {

                }
                if (lines.Length != 0)
                {
                    break;
                }
                trials--;
            }

            if (lines.Length == 0)
            {
                return null;
            }
            lines = verifyBrackects(lines);

            try
            {
                rnsXplaneData = JsonConvert.DeserializeObject<RNSXPLANEDATA>(lines);  // Deserialize 'lines'

            }
            catch
            {
                return null;
            }

            SERIAL.Text = serial;
            ID.Text = id.ToString();
            SetField("DESCRIPTION", rnsXplaneData.DESCRIPTION);
            SetField("ADFGHOSTBUTTON", rnsXplaneData.ADFGHOSTBUTTON);
            SetField("ADFBUTTONDATAREF", rnsXplaneData.ADFBUTTONDATAREF);
            SetField("XPDRGHOSTBUTTON", rnsXplaneData.XPDRGHOSTBUTTON);
            SetField("XPDRBUTTONDATAREF", rnsXplaneData.XPDRBUTTONDATAREF);
            SetField("COMMACTTOREAD", rnsXplaneData.COMMACTTOREAD);
            SetField("COMMACTTOWRITE", rnsXplaneData.COMMACTTOWRITE);
            SetField("COMMSTBTOREAD", rnsXplaneData.COMMSTBTOREAD);
            SetField("COMMSTBTOWRITE", rnsXplaneData.COMMSTBTOWRITE);
            SetField("NAVACTTOREAD", rnsXplaneData.NAVACTTOREAD);
            SetField("NAVACTTOWRITE", rnsXplaneData.NAVACTTOWRITE);
            SetField("NAVSTBTOREAD", rnsXplaneData.NAVSTBTOREAD);
            SetField("NAVSTBTOWRITE", rnsXplaneData.NAVSTBTOWRITE);
            SetField("ADFTOREAD", rnsXplaneData.ADFTOREAD);
            SetField("ADFTOWRITE", rnsXplaneData.ADFTOWRITE);
            SetField("XPDRTOREAD", rnsXplaneData.XPDRTOREAD);
            SetField("XPDRTOWRITE", rnsXplaneData.XPDRTOWRITE);

            bool FoundSelected = false;

            var SelectedCustomColor = XPlaneColorPanel.Controls.OfType<RadioButton>();

            Color cmp = ColorTranslator.FromHtml(rnsXplaneData.BACKLITCOLOR);

            foreach (RadioButton rb in SelectedCustomColor) // Find out the default color or assume 0
            {

                if (rb.BackColor.Equals(cmp))
                {
                    rb.Checked = true;
                    FoundSelected = true;
                    break;
                }
            }
            if (!FoundSelected)
            {
                (XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).Checked = true;
                (XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor = ColorTranslator.FromHtml(rnsXplaneData.BACKLITCOLOR);
            }

            return rnsXplaneData;

        }

        public string RetrieveSelectedBackLitColor()
        {
            var SelectedCustomColor = XPlaneColorPanel.Controls.OfType<RadioButton>();

            foreach (RadioButton rb in SelectedCustomColor) // Find out the default color or assume 0
            {

                if (rb.Checked)
                {
                    return ColorTranslator.ToHtml(rb.BackColor);
                }
            }
            return ColorTranslator.ToHtml((XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor);
        }

        private void SaveRNSData()
        {
            /// <summary>
            /// Saves the setup data to a file
            /// </summary>

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            RNSXPLANEDATA rnsXplaneData = new RNSXPLANEDATA();

            string JSonFile = currentFileName;

            string serial = this.SERIAL.Text;
            string id = this.ID.Text;

            try
            {
                rnsXplaneData.SERIAL = serial;
                rnsXplaneData.ID = int.Parse(id);
                rnsXplaneData.DESCRIPTION = DESCRIPTION.Text;
                rnsXplaneData.BACKLITCOLOR = RetrieveSelectedBackLitColor();
                rnsXplaneData.ADFGHOSTBUTTON = FieldText("ADFGHOSTBUTTON");
                rnsXplaneData.XPDRGHOSTBUTTON = FieldText("XPDRGHOSTBUTTON");
                rnsXplaneData.ADFBUTTONDATAREF = FieldText("ADFBUTTONDATAREF");
                rnsXplaneData.XPDRBUTTONDATAREF = FieldText("XPDRBUTTONDATAREF");
                rnsXplaneData.COMMACTTOREAD = FieldText("COMMACTTOREAD");
                rnsXplaneData.COMMACTTOWRITE = FieldText("COMMACTTOWRITE");
                rnsXplaneData.COMMSTBTOREAD = FieldText("COMMSTBTOREAD");
                rnsXplaneData.COMMSTBTOWRITE = FieldText("COMMSTBTOWRITE");
                rnsXplaneData.NAVACTTOREAD = FieldText("NAVACTTOREAD");
                rnsXplaneData.NAVACTTOWRITE = FieldText("NAVACTTOWRITE");
                rnsXplaneData.NAVSTBTOREAD = FieldText("NAVSTBTOREAD");
                rnsXplaneData.NAVSTBTOWRITE = FieldText("NAVSTBTOWRITE");
                rnsXplaneData.ADFTOREAD = FieldText("ADFTOREAD");
                rnsXplaneData.ADFTOWRITE = FieldText("ADFTOWRITE");
                rnsXplaneData.XPDRTOREAD = FieldText("XPDRTOREAD");
                rnsXplaneData.XPDRTOWRITE = FieldText("XPDRTOWRITE");

                string line = JsonConvert.SerializeObject(rnsXplaneData, settings);

                File.WriteAllText(JSonFile, line);

                MessageBox.Show("Configuration saved", "RNS save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                Say("An error occoured while saving RNS configuration.");
            }

        }

        private string FieldText(string FieldName)
        {
            /// <summary>
            /// Return the text from the textbox 'FieldName'
            /// </summary>

            Control[] fields = Controls.Find(FieldName, true);
            if (fields.Length != 0)
            {
                return fields[0].Text.Trim();
            }
            return "";

        }

        private void SetField(string FieldName, string FieldValue)
        {
            /// <summary>
            /// Sets the text of a textbox 'FieldName' with the 'FieldValue'
            /// </summary>

            Control[] fields = Controls.Find(FieldName, true);
            if (fields.Length != 0)
            {
                fields[0].Text = FieldValue.Trim();
            }
        }

        private void SetNumericUpDownField(string FieldName, int FieldValue)
        {
            /// <summary>
            /// Sets the text of a textbox 'FieldName' with the 'FieldValue'
            /// </summary>

            Control[] fields = Controls.Find(FieldName, true);
            if (fields.Length != 0)
            {
                PropertyInfo v = fields[0].GetType().GetProperty("Value");
                v.SetValue(fields[0], (decimal)FieldValue);
            }
        }


        // =============================== General save section =======================================

        private void SaveButton_Click(object sender, EventArgs e)
        {
            int PeripheralType = int.Parse(SERIAL.Text.Substring(0, 2));
            switch (PeripheralType)
            {
                case 31:
                    SaveRNSData();
                    break;
                case 50:
                    SaveMBx24Data();
                    break;
            }

        }

        // =============================== MBx24 hardware setup section =================================

        private void DrawXPlaneMBx24()
        {

            if (dataRefs == null)  // If no DataRefs, there is nothing to do here. Return
            {
                Say("DataRefs File empty");
            }

            Label[] Outputlabels = new Label[32];
            Label[] Inputlabels = new Label[32];
            Panel[] inputPanel = new Panel[32];

            int origin = 32; // Vertical begin point
            int p = origin + 10;
            int h = 25;
            int col0 = 2;
            int col1 = 55;
            int col2 = 550;
            //int ReceiveFrequency = 5; // Default number of packets per second XPlane will send

            // To implement if programmable X-Plane's packet send frequency is desired
            //
            //Label RecFrequency = new Label()    // Frequency input label
            //{
            //    Name = "RecFrequency",    
            //    AutoSize = true,
            //    Location = new Point(col1 + 250, 2),
            //    BackColor = backColor,
            //    ForeColor = frontColor,
            //    Text = "Packet receiveing frequency"
            //};
            //XPlanePanel.Controls.Add(RecFrequency);

            MBx24Settings mbx24S = new MBx24Settings();
            MBx24Form mbx24f = new MBx24Form();
            mbx24S = mbx24f.LoadData(SERIAL.Text);

            string[] outputlabeltext = new string[32];
            string[] inputlabeltext = new string[32];
            string[] joysticknumber = new string[32];


            foreach (var lbl in mbx24S.OUTPUTS)
            {
                outputlabeltext[lbl.CONNECTOR - 1] = lbl.DESCRIPTION;
            }
            foreach (var lbl in mbx24S.INPUTS)
            {
                inputlabeltext[lbl.CONNECTOR - 1] = lbl.DESCRIPTION + new string(' ', 32 - lbl.DESCRIPTION.Length);
                joysticknumber[lbl.CONNECTOR - 1] = lbl.JOY.ToString("D3");
            }


            // Headers
            Label InputConnector = new Label()
            {
                Name = "InputConnector",
                AutoSize = true,
                Location = new Point(col0, 7),
                BackColor = backColor,
                ForeColor = frontColor,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Text = "Joystick\nbutton"
            };
            XPlanePanel.Controls.Add(InputConnector);

            Label DRtoWrite = new Label()
            {
                Name = "DRtoWrite",
                AutoSize = true,
                Location = new Point(col1 + 20, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "DataRef destination ( Button, etc )"
            };
            XPlanePanel.Controls.Add(DRtoWrite);

            Label dataRefValue = new Label()
            {
                Name = "DataRefValue",
                AutoSize = true,
                Location = new Point(col2 - 190, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "ON value"
            };
            XPlanePanel.Controls.Add(dataRefValue);

            Label GhostLabel = new Label()
            {
                Name = "GhostLabel",
                AutoSize = true,
                Location = new Point(col2 - 120, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Ghost key"
            };
            XPlanePanel.Controls.Add(GhostLabel);

            Label OutputConnector = new Label()       // Headers
            {
                Name = "OutputConnector",
                AutoSize = true,
                Location = new Point(col2, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Text = "Output connector"
            };
            XPlanePanel.Controls.Add(OutputConnector);

            Label DRtoRead = new Label()
            {
                Name = "DRtoRead",
                AutoSize = true,
                Location = new Point(col2 + 150, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "DataRef origin ( Led annunciator, etc )"
            };
            XPlanePanel.Controls.Add(DRtoRead);

            for (int i = 0; i != 32; i++)   // Input sub-panels creation
            {
                Panel cont = new Panel();

                cont.Location = new Point(col0, (h + 5) * i * 2 + h * 2);
                cont.Size = new Size(col2 - 35, h * 2 + 5);
                cont.BorderStyle = BorderStyle.Fixed3D;
                cont.BackColor = frontColor;
                cont.Name = "INPUTPANEL" + i.ToString("D2");
                inputPanel[i] = cont;
            }

            // Numeric up/down elements

            p = origin + 10;

            // To implement if programmable frequency is desired
            //
            //NumericUpDown Freq = new NumericUpDown()  
            //{
            //    Name = "FREQ",
            //    Width = 54,
            //    Location = new Point(col2, 2),
            //    BackColor = backColor,
            //    ForeColor = frontColor,
            //    Minimum = 1,
            //    Maximum = 8,
            //    Value = ReceiveFrequency
            //};
            //XPlanePanel.Controls.Add(Freq);


            for (int i = 0; i != 32; i++)
            {

                Label label4 = new Label();         // Label for the hardware input connector description
                label4.Name = "CONNIN" + (i + 1).ToString("D2"); // Format label name to "DESCINDD" where DD is a two digit the index
                label4.Size = new Size(32, 48);
                label4.Location = new Point(3, 3);
                label4.BackColor = backColor;
                label4.ForeColor = frontColor;
                label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                label4.BorderStyle = BorderStyle.Fixed3D;
                label4.Text = (i + 1).ToString("D2");
                label4.Visible = false;
                inputPanel[i].Controls.Add(label4);

                Label descr = new Label();     // Label for connection description
                descr.Name = "DESCRIN" + (i + 1).ToString("D2");
                descr.Size = new Size(300, 22);
                descr.Location = new Point(58, 3);
                descr.BackColor = frontColor;
                descr.ForeColor = backColor;
                descr.FlatStyle = FlatStyle.Flat;
                descr.BorderStyle = BorderStyle.None;
                string font = CharFont.FontFamily.ToString();
                Font AlternateCharFont = new Font(font, 10, FontStyle.Bold);
                descr.Font = AlternateCharFont;
                descr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                descr.Text = inputlabeltext[i];
                inputPanel[i].Controls.Add(descr);

                Label joynum = new Label();     // Label for joystick number related to input connector number
                joynum.Name = "JOYNUMB" + (i + 1).ToString("D3");
                joynum.Size = new Size(48, 22 * 2);
                joynum.Location = new Point(2, 3);
                joynum.BackColor = backColor;
                joynum.ForeColor = frontColor;
                joynum.BorderStyle = BorderStyle.FixedSingle;

                font = CharFont.FontFamily.ToString();
                AlternateCharFont = new Font(font, 10, FontStyle.Bold);
                joynum.Font = AlternateCharFont;
                joynum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                joynum.Text = joysticknumber[i];
                inputPanel[i].Controls.Add(joynum);

                DRTOWRITE[i] = new TextBox();       // Dataref to write
                TextBox txtbox = DRTOWRITE[i];
                txtbox.Name = "DRTOWRITE" + (i + 1).ToString("D2");
                txtbox.Size = new Size(300, 16);
                txtbox.Location = new Point(58, h + 1);
                txtbox.BackColor = backColor;
                txtbox.ForeColor = frontColor;
                txtbox.Font = new Font("Microsoft Sans Serif", 10);
                DRTOWRITE[i] = txtbox;
                inputPanel[i].Controls.Add(DRTOWRITE[i]);

                MBX24DRVALUE[i] = new NumericUpDown();  // Input dataref's value
                NumericUpDown num = MBX24DRVALUE[i];
                num.Name = "MBX24DRVALUE" + (i + 1).ToString("D2");
                num.Size = new Size(50, 16);
                num.Location = new Point(360, h + 1);
                num.BackColor = backColor;
                num.ForeColor = frontColor;
                num.Font = new Font("Microsoft Sans Serif", 10);
                num.Minimum = 0;
                num.Maximum = 9;
                num.Value = 1;
                MBX24DRVALUE[i] = num;
                inputPanel[i].Controls.Add(MBX24DRVALUE[i]);

                MBx24GHOSTKEYS[i] = new TextBox();  // Input ghostkey
                TextBox txtbox2 = MBx24GHOSTKEYS[i];
                txtbox2.Name = "MBx24GHOSTKEYS" + (i + 1).ToString("D2");
                txtbox2.Size = new Size(90, 16);
                txtbox2.Location = new Point(420, h + 1);
                txtbox2.BackColor = backColor;
                txtbox2.ForeColor = frontColor;
                txtbox2.Font = new Font("Microsoft Sans Serif", 10);
                txtbox2.ReadOnly = true;
                txtbox2.Text = "";
                MBx24GHOSTKEYS[i] = txtbox2;
                inputPanel[i].Controls.Add(MBx24GHOSTKEYS[i]);
                // Add events
                MBx24GHOSTKEYS[i].KeyDown += new KeyEventHandler(MBx24GHOSTKEYS_KeyDown);

                XPlanePanel.Controls.Add(inputPanel[i]);        // Add current input sub-panel to X-Plane panel

            }

            p = origin + 10;
            for (int i = 0; i != 32; i++)
            {
                DRTOREAD[i] = new TextBox();        // Dataref for annunciator
                TextBox txtbox = DRTOREAD[i];
                txtbox.Name = "DRTOREAD" + (i + 1).ToString("D2");
                txtbox.Size = new Size(300, 16);
                txtbox.Location = new Point(col2 + 150, p);
                txtbox.BackColor = backColor;
                txtbox.ForeColor = frontColor;
                txtbox.Font = new Font("Microsoft Sans Serif", 10);
                DRTOREAD[i] = txtbox;
                XPlanePanel.Controls.Add(DRTOREAD[i]);

                Label label3 = new Label();         // Label for the hardware output connector description
                label3.Name = "DESCOUT" + (i + 1).ToString("D2"); // Format label name to "DESCOUTDD" where DD is a two digit the index
                label3.Size = new Size(128, 16);
                label3.Location = new Point(col2, p + 5);
                label3.BackColor = frontColor;
                label3.ForeColor = backColor;
                label3.Text = " " + (i + 1).ToString("D2") + " - " + outputlabeltext[i];
                XPlanePanel.Controls.Add(label3);

                p += h;
            }

            Panel divider = new Panel()     // A divider made with a panel (sick)
            {
                Location = new Point(col2 - 20, 2),
                Size = new Size(2, p - h),
                BorderStyle = BorderStyle.None,
                BackColor = frontColor
            };


            XPlanePanel.Controls.Add(divider);
            XPlanePanel.Refresh();
        }


        private void MBx24GHOSTKEYS_KeyDown(object sender, KeyEventArgs e)
        {
            /// <summary>
            /// Event to capture keys pressed for the MBx24 buttons ghosting
            /// Ctrl, Shift or Alt keys alone are not allowed
            /// 
            /// /// Description:
            /// Ghosting a button mimics of a key stroke as if it would be from a keyboard.
            /// This technique consists on mapping joystick buttons triggered and re-route
            /// to simulation as a keyboard combination. Although could be made by hardware,
            /// the frequency of situations that require ghosting is not so often that
            /// justifies more hardware build and firmware. So, it is implemented by the
            /// TOGA software.
            /// 
            /// Remarks:
            /// Care should be taken while sending keyboard strokes to an application. It may
            /// have unpredicted results if not implemented conveniently.
            /// 
            /// Tip:
            /// Also, here is displayed the result of the keydown event, otherwise,
            /// in cases like {CONTROL} 'V', the displayed text would be the
            /// contents of the clipboard and the key sequence string. Only
            /// the last one should be displayed, so MBx24 ghosting textboxes are readonly
            /// not allowing the writting by the user and fill it with the proper values.
            /// 
            /// This event method is shared by all MBx24 ghost definitions
            /// 
            /// </summary>

            Keys k = e.KeyCode; // Holds the key or keys combination

            TextBox Sndr = (TextBox)sender; // Find out who was the sender (ADFGHOSTBUTTON / XPDRGHOSTBUTTON)

            if (k != Keys.ControlKey && k != Keys.ShiftKey && k != Keys.Alt)
            {
                if (e.Modifiers.ToString().Equals("None"))  // No Ctrl/Shift/Alt combination
                {
                    //GhostKey = ((char)e.KeyValue).ToString();
                    GhostKey = "{" + k.ToString() + "}";
                }
                else                                        // Combinations with modifiers 
                {
                    GhostKey = "{" + e.Modifiers.ToString() + "}" + "{" + k.ToString() + "}";

                }


                Control[] fields = Controls.Find(Sndr.Name, true); // Get the correspondent textbox

                if (k != Keys.Escape)
                {
                    fields[0].Text = GhostKey;      // Fill in tyhe respective textbox field
                    Sndr.Text = fields[0].Text;
                }
                else
                {
                    Sndr.Text = "";
                }

            }
        }

        public MBx24XPLANEDATA MBx24LoadData(string serial, int id)
        {

            MBx24XPLANEDATA mbx24XplaneData = new MBx24XPLANEDATA();

            string JSonFile = currentFileName;

            // As arguments may be used by outside form calls, check things
            if (serial != null)
            {
                SERIAL.Text = serial;
            }
            if (id != 0)
            {
                ID.Text = id.ToString(); ;
            }

            if (!File.Exists(currentFileName))
            {
                SaveDefaultMBx24(serial, id);
            }

            string lines = File.ReadAllText(JSonFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            try
            {
                mbx24XplaneData = JsonConvert.DeserializeObject<MBx24XPLANEDATA>(lines);  // Deserialize 'lines'
            }
            catch
            {
                return null;
            }

            return mbx24XplaneData;

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
            string LineToCheck = line;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].Equals('{'))
                {
                    curlBrackets--;
                }
                if (line[i].Equals('}'))
                {
                    curlBrackets++;
                }
                if (line[i].Equals('['))
                {
                    squareBrackets--;
                }
                if (line[i].Equals(']'))
                {
                    squareBrackets++;
                }
            }

            if (curlBrackets > 0 || squareBrackets > 0)
            {
                // excessive close brackets
                LineToCheck = line.Substring(0, LineToCheck.Length - 1);
                LineToCheck = verifyBrackects(LineToCheck); // Recursively test for more unbalanced brackets
            }
            return LineToCheck;
        }

        public void MBx24FillData(string serial, int id)
        {
            /// <summary>
            /// Fills the setup panel fields with MBx24 input/output definitions
            /// </summary>

            MBx24XPLANEDATA mbx24Sets = MBx24LoadData(serial, id);

            SERIAL.Text = mbx24Sets.SERIAL;
            ID.Text = mbx24Sets.ID.ToString();
            DESCRIPTION.Text = mbx24Sets.DESCRIPTION;
            bool FoundSelected = false;

            var SelectedCustomColor = XPlaneColorPanel.Controls.OfType<RadioButton>();

            Color cmp = ColorTranslator.FromHtml(mbx24Sets.BACKLITCOLOR);

            // Find out the default color or assume the first one

            foreach (RadioButton rb in SelectedCustomColor)
            {

                if (rb.BackColor.Equals(cmp))
                {
                    rb.Checked = true;
                    FoundSelected = true;
                    break;
                }
            }
            if (!FoundSelected)
            {
                (XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).Checked = true;
                (XPlaneColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor = ColorTranslator.FromHtml(mbx24Sets.BACKLITCOLOR);
            }

            foreach (MBx24Reference r in mbx24Sets.REFERENCES)
            {
                int i = r.CONNECTOR - 1;
                string con = r.CONNECTOR.ToString("D2");
                SetField("DRTOREAD" + con, r.DATAREFTOREAD);
                SetField("DRTOWRITE" + con, r.DATAREFTOWRITE);
                SetField("MBx24GHOSTKEYS" + con, r.GHOSTKEY);
                SetNumericUpDownField("MBX24DRVALUE" + con, (int)r.ONVALUE);
            }
        }




        public void SaveDefaultMBx24(string serial, int id)
        {
            string JSonFile = currentFileName;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            List<MBx24Reference> refList = new List<MBx24Reference>();

            for (int i = 0; i < 32; i++)
            {
                MBx24Reference r = new MBx24Reference();
                r.CONNECTOR = (byte)(i + 1);
                r.DATAREFTOREAD = "";
                r.DATAREFTOWRITE = "";
                r.ONVALUE = 1;
                r.GHOSTKEY = "";
                refList.Add(r);
            }

            MBx24XPLANEDATA mbx24XplaneData = new MBx24XPLANEDATA();

            mbx24XplaneData.SERIAL = serial;
            mbx24XplaneData.ID = id;
            mbx24XplaneData.DESCRIPTION = peripheralName;
            mbx24XplaneData.BACKLITCOLOR = RetrieveSelectedBackLitColor();
            mbx24XplaneData.FREQUENCY = 5;
            mbx24XplaneData.REFERENCES = refList;

            try
            {

                string line = JsonConvert.SerializeObject(mbx24XplaneData, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
            return;
        }

        public void SaveMBx24Data()
        {
            string JSonFile = currentFileName;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            List<MBx24Reference> refList = new List<MBx24Reference>();

            for (int i = 0; i < 32; i++)
            {
                MBx24Reference r = new MBx24Reference();
                r.CONNECTOR = (byte)(i + 1);
                string con = (i + 1).ToString("D2");
                r.DATAREFTOREAD = FieldText("DRTOREAD" + con);
                r.DATAREFTOWRITE = FieldText("DRTOWRITE" + con);
                r.ONVALUE = (byte)MBX24DRVALUE[i].Value;
                r.GHOSTKEY = FieldText("MBx24GHOSTKEYS" + con);
                refList.Add(r);
            }

            MBx24XPLANEDATA mbx24XplaneData = new MBx24XPLANEDATA();

            mbx24XplaneData.SERIAL = SERIAL.Text;
            mbx24XplaneData.ID = int.Parse(ID.Text);
            mbx24XplaneData.DESCRIPTION = DESCRIPTION.Text;
            mbx24XplaneData.BACKLITCOLOR = RetrieveSelectedBackLitColor();
            mbx24XplaneData.FREQUENCY = 5;
            mbx24XplaneData.REFERENCES = refList;

            try
            {

                string line = JsonConvert.SerializeObject(mbx24XplaneData, settings);

                File.WriteAllText(JSonFile, line);

                MessageBox.Show("Configuration saved", "MBx24 save", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch
            {
                Say("An error occoured while saving MBx24 configuration.");
            }

            return;

        }
    }

}
