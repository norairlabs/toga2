using NorAirMSFS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TOGA.Properties;
using static System.Windows.Forms.LinkLabel;
using static TOGA.Form1;
using static TOGA.RNSForm;
using Microsoft.FlightSimulator.SimConnect;
using static TOGA.XPlaneSetup;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using static TOGA.MSFSSetup;
using static TOGA.MBx24Form;
using static NorAirMSFS.Connect;
using System.Collections;
using System.Threading;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics.Metrics;


namespace TOGA
{
    public partial class MSFSSetup : Form
    {

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

        public class RNSMSFSDATA
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }
            public string BACKLITCOLOR { get; set; }
            public string ADFGHOSTBUTTON { get; set; }
            public string ADFBUTTONSIMVAR { get; set; }
            public string XPDRGHOSTBUTTON { get; set; }
            public string XPDRBUTTONSIMVAR { get; set; }
            public SimVar COMMACTTOREAD { get; set; }
            public EventID COMMACTTOWRITE { get; set; }
            public SimVar COMMSTBTOREAD { get; set; }
            public EventID COMMSTBTOWRITE { get; set; }
            public SimVar NAVACTTOREAD { get; set; }
            public EventID NAVACTTOWRITE { get; set; }
            public SimVar NAVSTBTOREAD { get; set; }
            public EventID NAVSTBTOWRITE { get; set; }
            public SimVar ADFTOREAD { get; set; }
            public EventID ADFTOWRITE { get; set; }
            public SimVar XPDRTOREAD { get; set; }
            public EventID XPDRTOWRITE { get; set; }

        }

        public class MBx24Reference // Class for each MBx24 input/output connector
        {
            public byte CONNECTOR { get; set; }
            public string SIMVARTOREAD { get; set; }
            public string GHOSTKEY { get; set; }
        }

        public class MBx24MSFSDATA      // A class for the MBx24 settings
        {
            public string SERIAL { get; set; }
            public int ID { get; set; }
            public string DESCRIPTION { get; set; }
            public string BACKLITCOLOR { get; set; }
            public IList<MBx24Reference> REFERENCES { get; set; }

        }

        public enum EventIDType
        {
            COM_RADIO_SET_HZ,
            COM2_RADIO_SET_HZ,
            COM3_RADIO_SET_HZ,
            COM_STBY_RADIO_SET_HZ,
            COM2_STBY_RADIO_SET_HZ,
            COM3_STBY_RADIO_SET_HZ,
            NAV1_RADIO_SET_HZ,
            NAV2_RADIO_SET_HZ,
            NAV3_RADIO_SET_HZ,
            NAV4_RADIO_SET_HZ,
            NAV1_STBY_SET_HZ,
            NAV2_STBY_SET_HZ,
            NAV3_STBY_SET_HZ,
            NAV4_STBY_SET_HZ,
            ADF_ACTIVE_SET,
            ADF2_ACTIVE_SET,
            XPNDR_SET
        }

        public enum SimPriorities
        {
            SIMCONNECT_GROUP_PRIORITY_HIGHEST,
            SIMCONNECT_GROUP_PRIORITY_STANDARD,
            SIMCONNECT_GROUP_PRIORITY_DEFAULT,
            SIMCONNECT_GROUP_PRIORITY_LOWEST
        }

        public enum SIMCONNECT_DATATYPE
        {
            INVALID,
            INT32,
            INT64,
            FLOAT32,
            FLOAT64,
            STRING8,
            STRING32,
            STRING64,
            STRING128,
            STRING256,
            STRING260,
            STRINGV,
            INITPOSITION,
            MARKERSTATE,
            WAYPOINT,
            LATLONALT,
            XYZ,
            MAX
        }

        public class SimVar
        {
            public string Name { get; set; }            // Ex.: "COM ACTIVE FREQUENCY:1"
            public string Units { get; set; }           // Ex.: "KHz", "Boolean"
            public SIMCONNECT_DATATYPE DataType { get; set; }        // Ex.: SIMCONNECT_DATATYPE.INT64, SIMCONNECT_DATATYPE.FLOAT32
        }

        public class EventID
        {
            public string Name { get; set; }
            public EventIDType EventIdType { get; set; }
            public int Multiplier { get; set; }

        }

        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";

        // MSFS paths & files
        public static string MSFSPath = BasePath + @"\MSFS";                            // MSFS defs path
        public static string MSFSProfilesPath = MSFSPath + @"\Profiles";                // MSFS profiles base path
        public static string MSFSDefaultProfile = MSFSPath + @"\DefaultProfile.json";   // MSFS default profile
        public string RNSSimVarsFile = MSFSPath + @"\RNSSimVars.json";                     // RNS SimVars file
        public string RNSEventIDsFile = MSFSPath + @"\RNSEventIDs.json";                     // RNS EventIDs file
        public string MBx24SimVarsFile = MSFSPath + @"\MBx24SimVars.json";                     // MBx24 SimVars file

        public string BackLitFileName = BasePath + ".\\BackLit.json";
        public string StylesFileName = BasePath + ".\\Styles.json";

        public List<PeripheralProperty> PeripheralsPresent = new List<PeripheralProperty>();

        public List<SimVar> RNSSimVarsDef = new List<SimVar>();
        public List<EventID> RNSEventIDsDef = new List<EventID>();
        public TextBox[] RNSFIELDS = new TextBox[16];

        public List<SimVarEntity> MBx24SimVarsDef = new List<SimVarEntity>();

        public TextBox[] SVTOREAD = new TextBox[32];
        public TextBox[] EVTOWRITE = new TextBox[32];
        public TextBox[] MBx24GHOSTKEYS = new TextBox[32];
        public NumericUpDown[] MBX24SVVALUE = new NumericUpDown[32];
        public const int BackLitColorSlots = 10;

        public RNSMSFSDATA rnsmsfsdata = new RNSMSFSDATA();

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
        public Font CharFont = new Font("Verdana", 8);

        string GhostKey = "";
        string ADFGhostKey = "";
        string XPDRGhostKey = "";




        public MSFSSetup()
        {
            InitializeComponent();
        }

        public DialogResult Show(string serial, int id, Color bck, Color frt, string fileName, string profileName, string peripheralname)
        {
            PeripheralsPresent.Clear();
            this.backColor = bck;
            this.frontColor = frt;
            this.BackColor = backColor;
            this.ForeColor = frontColor;
            MSFSPanel.BackColor = backColor;
            MSFSPanel.ForeColor = frontColor;
            SERIAL.Text = serial;
            ID.Text = id.ToString();
            ReadStyles();
            SetTheme(this.Controls, backColor, frontColor);
            ReadPreferredBackLitColors();
            int hardware = int.Parse(serial.Substring(0, 2));
            bool newProfile = !File.Exists(fileName);
            currentProfile = profileName;
            currentFileName = fileName;
            peripheralName = peripheralname.Split(',')[1];
            switch (hardware)
            {
                case 31:
                    DrawMSFSRNS();
                    FillRNSFields(serial, id);
                    //this.ActiveControl = DESCRIPTION; // Set focus on a field
                    break;

                case 50:
                    DrawMSFSMBx24();
                    MBx24FillData(serial, id);
                    //this.ActiveControl = DESCRIPTION; // Set focus on a field
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
                var radiob = (MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(count);

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

        public void ShowStat(string s)
        {
            /// <summary>
            /// Updates the status bar
            /// </summary>
            StatusLabel.Text = s;
        }

        // ===================================== RNS setup ===================================================

        private void MSFSRNSFillSimVarsList()
        {

            // Build a context menu to be called by the mouse right button on SimVars text fields

            ContextMenuStrip EventIDsMenu = new ContextMenuStrip(); // Instantiate menu
            EventIDsMenu.AutoSize = true;
            EventIDsMenu.ShowItemToolTips = true;
            EventIDsMenu.ItemClicked += new ToolStripItemClickedEventHandler(CopyMenuData);     // Event handler

            ContextMenuStrip SimVarsMenu = new ContextMenuStrip(); // Instantiate menu
            SimVarsMenu.AutoSize = true;
            SimVarsMenu.ShowItemToolTips = true;
            SimVarsMenu.ItemClicked += new ToolStripItemClickedEventHandler(CopyMenuData);

            RNSSimVarsDef.Clear();
            RNSEventIDsDef.Clear();

            string lines;

            RNSCreateSimVarsList();
            RNSCreateEventIDsList();

            // Fill in SimVars context menu
            lines = File.ReadAllText(RNSSimVarsFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            RNSSimVarsDef = JsonConvert.DeserializeObject<List<SimVar>>(lines);  // Deserialize 'lines'

            var d = SimVarsMenu.Items.Add("Delete");
            d.ToolTipText = "Delete contents of the field";

            foreach (SimVar simVar in RNSSimVarsDef)
            {
                var h = SimVarsMenu.Items.Add(simVar.Name); // Add simvars to mouse right button menu ( context menu )
                h.ToolTipText = simVar.Units;
            }

            // Add menu strip to "read" simvars field
            RNSFIELDS[1].ContextMenuStrip = SimVarsMenu; // Active COM
            RNSFIELDS[3].ContextMenuStrip = SimVarsMenu; // Standby COM
            RNSFIELDS[5].ContextMenuStrip = SimVarsMenu; // Active NAV
            RNSFIELDS[7].ContextMenuStrip = SimVarsMenu; // Standby NAV
            RNSFIELDS[9].ContextMenuStrip = SimVarsMenu; // ADF
            RNSFIELDS[11].ContextMenuStrip = SimVarsMenu; // Transponder

            // Fill in EventIDs context menu
            lines = File.ReadAllText(RNSEventIDsFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            RNSEventIDsDef = JsonConvert.DeserializeObject<List<EventID>>(lines);  // Deserialize 'lines'

            foreach (EventID eventId in RNSEventIDsDef)
            {
                EventIDsMenu.Items.Add(eventId.EventIdType.ToString()); // Add simvars to mouse right button menu ( context menu )
            }

            // Add menu strip to "write" eventIds field
            RNSFIELDS[0].ContextMenuStrip = EventIDsMenu; // Active COM
            RNSFIELDS[2].ContextMenuStrip = EventIDsMenu; // Standby COM
            RNSFIELDS[4].ContextMenuStrip = EventIDsMenu; // Active NAV
            RNSFIELDS[6].ContextMenuStrip = EventIDsMenu; // Standby NAV
            RNSFIELDS[8].ContextMenuStrip = EventIDsMenu; // ADF
            RNSFIELDS[10].ContextMenuStrip = EventIDsMenu; // Transponder


        }

        private void CopyMenuData(object sender, ToolStripItemClickedEventArgs e)
        {
            // Get field caller name
            ContextMenuStrip ContextMenu = sender as ContextMenuStrip;
            string ControlName = ContextMenu.SourceControl.Name;
            // Get clicked item
            var c = e.ClickedItem;
            if (c.Text.Trim().Equals("Delete"))
            {
                // Set field with item contents
                SetField(ControlName, " ");
            }
            else
            {
                SetField(ControlName, c.Text);
            }
        }

        private void RNSCreateSimVarsList()
        {

            if (File.Exists(RNSSimVarsFile))
            {
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            try
            {
                List<SimVar> simlist = new List<SimVar>();

                simlist.Add(new SimVar { Name = "COM ACTIVE FREQUENCY:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM ACTIVE FREQUENCY:2", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM ACTIVE FREQUENCY:3", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM ACTIVE FREQUENCY:4", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM STANDBY FREQUENCY:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM STANDBY FREQUENCY:2", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM STANDBY FREQUENCY:3", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "COM STANDBY FREQUENCY:4", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV ACTIVE FREQUENCY:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV ACTIVE FREQUENCY:2", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV ACTIVE FREQUENCY:3", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV ACTIVE FREQUENCY:4", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV STANDBY FREQUENCY:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV STANDBY FREQUENCY:2", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV STANDBY FREQUENCY:3", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "NAV STANDBY FREQUENCY:4", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "ADF ACTIVE FREQUENCY:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "ADF ACTIVE FREQUENCY:2", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });
                simlist.Add(new SimVar { Name = "TRANSPONDER CODE:1", Units = "KHz", DataType = SIMCONNECT_DATATYPE.INT64 });

                string line = JsonConvert.SerializeObject(simlist, settings);

                File.WriteAllText(RNSSimVarsFile, line);

                ShowStat("RNS SimVars file created");
            }
            catch
            {
                ShowStat("There was an error creating a default RNS SimVars file");
            }

            NorAirMSFS.Connect MSFSConnect = new NorAirMSFS.Connect();

            List<SimVarEntity> MSFSsimvars = MSFSConnect.RetrieveSimVarsGroups();

        }

        private void RNSCreateEventIDsList()
        {

            if (File.Exists(RNSEventIDsFile))
            {
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            try
            {
                List<EventID> eventList = new List<EventID>();

                eventList.Add(new EventID { Name = EventIDType.COM_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM_RADIO_SET_HZ, Multiplier = 100000 });
                eventList.Add(new EventID { Name = EventIDType.COM2_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM2_RADIO_SET_HZ, Multiplier = 100000 });
                eventList.Add(new EventID { Name = EventIDType.COM3_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM3_RADIO_SET_HZ, Multiplier = 100000 });
                eventList.Add(new EventID { Name = EventIDType.COM_STBY_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM_STBY_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.COM2_STBY_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM2_STBY_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.COM3_STBY_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.COM3_STBY_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV1_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.NAV1_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV2_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.NAV2_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV3_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.NAV3_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV4_RADIO_SET_HZ.ToString(), EventIdType = EventIDType.NAV4_RADIO_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV1_STBY_SET_HZ.ToString(), EventIdType = EventIDType.NAV1_STBY_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV2_STBY_SET_HZ.ToString(), EventIdType = EventIDType.NAV2_STBY_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV3_STBY_SET_HZ.ToString(), EventIdType = EventIDType.NAV3_STBY_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.NAV4_STBY_SET_HZ.ToString(), EventIdType = EventIDType.NAV4_STBY_SET_HZ, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.ADF_ACTIVE_SET.ToString(), EventIdType = EventIDType.ADF_ACTIVE_SET, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.ADF2_ACTIVE_SET.ToString(), EventIdType = EventIDType.ADF2_ACTIVE_SET, Multiplier = 10000 });
                eventList.Add(new EventID { Name = EventIDType.XPNDR_SET.ToString(), EventIdType = EventIDType.XPNDR_SET, Multiplier = 0 });

                string line = JsonConvert.SerializeObject(eventList, settings);

                File.WriteAllText(RNSEventIDsFile, line);

                ShowStat("RNS Event IDs file created");
            }
            catch
            {
                ShowStat("There was an error creating a default RNS Event IDs file");
            }
        }

        public void DrawMSFSRNS()
        {
            /// <summary>
            /// Method to paint the MSFS RNS definitions panel
            /// </summary>

            // layout references

            int origin = 22; // Vertical reference point
            int p = origin + 4;
            int h = 28;
            int col0 = 2;
            int col1 = 140;
            int col2 = 540;

            for (int i = 0; i < RNSFIELDS.Length; i++)
            {
                RNSFIELDS[i] = new TextBox();
            }

            Label Instr = new Label()       // Headers
            {
                Name = "Instr",
                Height = 20,
                Location = new Point(col0, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Instrument"
            };

            Label SVtoWrite = new Label()
            {
                Name = "SVtoWrite",
                Height = 20,
                Location = new Point(col1 + 100, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Size = new Size(110, 22),
                Text = "Event ID to write"
            };

            Label SVtoRead = new Label()
            {
                Name = "SVtoRead",
                Height = 20,
                Location = new Point(col2 + 100, 2),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "SimVar to read"
            };

            Label COMMACT = new Label()         // Labels
            {
                Name = "COMMACT",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "COMM act."
            };

            p += h;
            Label COMMSTB = new Label()
            {
                Name = "COMMSTB",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "COMM stb."
            };

            p += h;
            Label NAVACT = new Label()
            {
                Name = "NAVACT",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "NAV active"
            };

            p += h;
            Label NAVSTB = new Label()
            {
                Name = "NAVSTB",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "NAV standby"
            };

            p += h;
            Label ADF = new Label()
            {
                Name = "ADF",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "ADF"
            };

            p += h;
            Label XPDR = new Label()
            {
                Name = "XPDR",
                Size = new Size(110, 16),
                Location = new Point(col0, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "XPDR"
            };

            p += h;

            Label BUTTONSBAR = new Label()
            {
                Name = "BUTTONSBAR",
                Size = new Size(100, 16),
                Location = new Point(col0, p),
                BackColor = frontColor,
                ForeColor = backColor,
                Text = " Buttons actions "
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

            Label ADFBUTTONVARSIMLABEL = new Label()
            {
                Name = "ADFVARSIMLABEL",
                Size = new Size(100, 16),
                Location = new Point(col1 + 200, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Button SimVar"
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

            Label XPDRBUTTONVARSIMLABEL = new Label()
            {
                Name = "XPDRBUTTONVARSIMLABEL",
                Size = new Size(100, 16),
                Location = new Point(col1 + 200, p),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Button SimVar"
            };

            p = origin + 2;                                 // TextBoxes, ListBoxes & others

            TextBox COMMACTTOWRITE = new TextBox()
            {
                Name = "COMMACTTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[0] = COMMACTTOWRITE;

            TextBox COMMACTTOREAD = new TextBox()
            {
                Name = "COMMACTTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[1] = COMMACTTOREAD;
            p += h;

            TextBox COMMSTBTOWRITE = new TextBox()
            {
                Name = "COMMSTBTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[2] = COMMSTBTOWRITE;


            TextBox COMMSTBTOREAD = new TextBox()
            {
                Name = "COMMSTBTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[3] = COMMSTBTOREAD;

            p += h;
            TextBox NAVACTTOWRITE = new TextBox()
            {
                Name = "NAVACTTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[4] = NAVACTTOWRITE;

            TextBox NAVACTTOREAD = new TextBox()
            {
                Name = "NAVACTTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[5] = NAVACTTOREAD;

            p += h;
            TextBox NAVSTBTOWRITE = new TextBox()
            {
                Name = "NAVSTBTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[6] = NAVSTBTOWRITE;

            TextBox NAVSTBTOREAD = new TextBox()
            {
                Name = "NAVSTBTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[7] = NAVSTBTOREAD;

            p += h;
            TextBox ADFTOWRITE = new TextBox()
            {
                Name = "ADFTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[8] = ADFTOWRITE;

            TextBox ADFTOREAD = new TextBox()
            {
                Name = "ADFTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[9] = ADFTOREAD;

            p += h;
            TextBox XPDRTOWRITE = new TextBox()
            {
                Name = "XPDRTOWRITE",
                Size = new Size(400, 34),
                Location = new Point(col1, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[10] = XPDRTOWRITE;

            TextBox XPDRTOREAD = new TextBox()
            {
                Name = "XPDRTOREAD",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[11] = XPDRTOREAD;

            p += h;

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

            TextBox ADFBUTTONVARSIM = new TextBox()
            {
                Name = "ADFGHOSTVARSIM",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[12] = ADFBUTTONVARSIM;

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

            TextBox XPDRBUTTONVARSIM = new TextBox()
            {
                Name = "XPDRBUTTONVARSIM",
                Size = new Size(400, 34),
                Location = new Point(col2, p),
                BackColor = backColor,
                ForeColor = frontColor,
                ReadOnly = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            RNSFIELDS[13] = XPDRBUTTONVARSIM;

            // Add panel composition
            MSFSPanel.Controls.Add(Instr);
            MSFSPanel.Controls.Add(SVtoWrite);
            MSFSPanel.Controls.Add(SVtoRead);
            MSFSPanel.Controls.Add(COMMACT);
            MSFSPanel.Controls.Add(COMMSTB);
            MSFSPanel.Controls.Add(NAVACT);
            MSFSPanel.Controls.Add(NAVSTB);
            MSFSPanel.Controls.Add(ADF);
            MSFSPanel.Controls.Add(XPDR);
            MSFSPanel.Controls.Add(BUTTONSBAR);
            MSFSPanel.Controls.Add(ADFGHOST);
            MSFSPanel.Controls.Add(ADFBUTTONVARSIMLABEL);
            MSFSPanel.Controls.Add(XPDRBUTTONVARSIMLABEL);
            MSFSPanel.Controls.Add(ADFBUTTONVARSIM);
            MSFSPanel.Controls.Add(XPDRBUTTONVARSIM);
            MSFSPanel.Controls.Add(XPDRGHOST);
            MSFSPanel.Controls.Add(COMMACTTOREAD);
            MSFSPanel.Controls.Add(COMMSTBTOREAD);
            MSFSPanel.Controls.Add(NAVACTTOREAD);
            MSFSPanel.Controls.Add(NAVSTBTOREAD);
            MSFSPanel.Controls.Add(ADFTOREAD);
            MSFSPanel.Controls.Add(XPDRTOREAD);
            MSFSPanel.Controls.Add(COMMACTTOWRITE);
            MSFSPanel.Controls.Add(COMMSTBTOWRITE);
            MSFSPanel.Controls.Add(NAVACTTOWRITE);
            MSFSPanel.Controls.Add(NAVSTBTOWRITE);
            MSFSPanel.Controls.Add(ADFTOWRITE);
            MSFSPanel.Controls.Add(XPDRTOWRITE);
            MSFSPanel.Controls.Add(ADFGHOSTBUTTON);
            MSFSPanel.Controls.Add(XPDRGHOSTBUTTON);
            KeyPreview = true;

            // Add events
            ADFGHOSTBUTTON.KeyDown += new KeyEventHandler(RNSGHOSTBUTTON_KeyDown);
            XPDRGHOSTBUTTON.KeyDown += new KeyEventHandler(RNSGHOSTBUTTON_KeyDown);

            MSFSRNSFillSimVarsList();

            MSFSPanel.Refresh();
            // Done
        }

        public RNSMSFSDATA MSFSRNSLoadData(string serial, int id)
        {
            /// <summary>
            /// Read RNS data from file and return a "rnsmsfsdata" data type object
            /// </summary>
            /// "rnsmsfsdata" is previously declare making it available through out the application

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
                MSFSSaveDefaultRNS(serial, id);
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
                rnsmsfsdata = JsonConvert.DeserializeObject<RNSMSFSDATA>(lines);  // Deserialize 'lines'

            }
            catch
            {
                return null;
            }


            return rnsmsfsdata;

        }


        public void FillRNSFields(string serial, int id)
        {
            RNSMSFSDATA rnsmsfsdata = MSFSRNSLoadData(serial, id);

            if (rnsmsfsdata != null)
            {
                SERIAL.Text = serial;
                ID.Text = id.ToString();
                SetField("DESCRIPTION", rnsmsfsdata.DESCRIPTION);
                SetField("ADFGHOSTBUTTON", rnsmsfsdata.ADFGHOSTBUTTON);
                SetField("ADFBUTTONDATAREF", rnsmsfsdata.ADFBUTTONSIMVAR);
                SetField("XPDRGHOSTBUTTON", rnsmsfsdata.XPDRGHOSTBUTTON);
                SetField("XPDRBUTTONDATAREF", rnsmsfsdata.XPDRBUTTONSIMVAR);
                SetField("COMMACTTOREAD", rnsmsfsdata.COMMACTTOREAD.Name);
                SetField("COMMACTTOWRITE", rnsmsfsdata.COMMACTTOWRITE.Name);
                SetField("COMMSTBTOREAD", rnsmsfsdata.COMMSTBTOREAD.Name);
                SetField("COMMSTBTOWRITE", rnsmsfsdata.COMMSTBTOWRITE.Name);
                SetField("NAVACTTOREAD", rnsmsfsdata.NAVACTTOREAD.Name);
                SetField("NAVACTTOWRITE", rnsmsfsdata.NAVACTTOWRITE.Name);
                SetField("NAVSTBTOREAD", rnsmsfsdata.NAVSTBTOREAD.Name);
                SetField("NAVSTBTOWRITE", rnsmsfsdata.NAVSTBTOWRITE.Name);
                SetField("ADFTOREAD", rnsmsfsdata.ADFTOREAD.Name);
                SetField("ADFTOWRITE", rnsmsfsdata.ADFTOWRITE.Name);
                SetField("XPDRTOREAD", rnsmsfsdata.XPDRTOREAD.Name);
                SetField("XPDRTOWRITE", rnsmsfsdata.XPDRTOWRITE.Name);


                bool FoundSelected = false;

                var SelectedCustomColor = MSFSColorPanel.Controls.OfType<RadioButton>();

                Color cmp = ColorTranslator.FromHtml(rnsmsfsdata.BACKLITCOLOR);

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
                    (MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).Checked = true;
                    (MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor = ColorTranslator.FromHtml(rnsmsfsdata.BACKLITCOLOR);
                }
            }
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



        private void SaveRNSData()
        {
            /// <summary>
            /// Saves the setup data to a file
            /// </summary>

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string JSonFile = currentFileName;

            string serial = this.SERIAL.Text;
            string id = this.ID.Text;
            string aux = "";

            try
            {
                rnsmsfsdata.SERIAL = serial;
                rnsmsfsdata.ID = int.Parse(id);
                rnsmsfsdata.DESCRIPTION = peripheralName;
                rnsmsfsdata.BACKLITCOLOR = RetrieveSelectedBackLitColor();
                rnsmsfsdata.ADFGHOSTBUTTON = FieldText("ADFGHOSTBUTTON");
                rnsmsfsdata.XPDRGHOSTBUTTON = FieldText("XPDRGHOSTBUTTON");
                rnsmsfsdata.ADFBUTTONSIMVAR = FieldText("ADFBUTTONDATAREF");
                rnsmsfsdata.XPDRBUTTONSIMVAR = FieldText("XPDRBUTTONDATAREF");
                aux = FieldText("COMMACTTOREAD");
                rnsmsfsdata.COMMACTTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "KHz" };
                aux = FieldText("COMMACTTOWRITE");
                rnsmsfsdata.COMMACTTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 10000 };
                aux = FieldText("COMMSTBTOREAD");
                rnsmsfsdata.COMMSTBTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "KHz" };
                aux = FieldText("COMMSTBTOWRITE");
                rnsmsfsdata.COMMSTBTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 10000 };
                aux = FieldText("NAVACTTOREAD");
                rnsmsfsdata.NAVACTTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "KHz" };
                aux = FieldText("NAVACTTOWRITE");
                rnsmsfsdata.NAVACTTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 10000 };
                aux = FieldText("NAVSTBTOREAD");
                rnsmsfsdata.NAVSTBTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "KHz" };
                aux = FieldText("NAVSTBTOWRITE");
                rnsmsfsdata.NAVSTBTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 10000 };
                aux = FieldText("ADFTOREAD");
                rnsmsfsdata.ADFTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "KHz" };
                aux = FieldText("ADFTOWRITE");
                rnsmsfsdata.ADFTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 10000 };
                aux = FieldText("XPDRTOREAD");
                rnsmsfsdata.XPDRTOREAD = new SimVar { Name = aux, DataType = SIMCONNECT_DATATYPE.INT64, Units = "Hz" };
                aux = FieldText("XPDRTOWRITE");
                rnsmsfsdata.XPDRTOWRITE = new EventID { Name = aux, EventIdType = Enum.Parse<EventIDType>(aux), Multiplier = 0 };

                string line = JsonConvert.SerializeObject(rnsmsfsdata, settings);

                File.WriteAllText(JSonFile, line);

                MessageBox.Show("Configuration saved", "RNS save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch 
            {
                Say("An error occoured while saving RNS configuration.");
            }

        }


        public bool MSFSSaveDefaultRNS(string serial, int id)
        {
            if (File.Exists(currentFileName))
            {
                return true;
            }

            string JSonFile = currentFileName;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string selectedColor = "";

            var SelectedCustomColor = MSFSColorPanel.Controls.OfType<RadioButton>();

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

            ///Simvars to read
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT32, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "KOHLSMAN SETTING HG", "inHG", SIMCONNECT_DATATYPE.FLOAT32, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "COM ACTIVE FREQUENCY:1", "KHz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "COM STANDBY FREQUENCY:1", "KHz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "NAV ACTIVE FREQUENCY:1", "KHz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "NAV STANDBY FREQUENCY:1", "KHz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "ADF ACTIVE FREQUENCY:1", "KHz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "TRANSPONDER CODE:1", "Hz", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "LIGHT BEACON", "Boolean", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);

            //EventID to write
            //simconnect.MapClientEventToSimEvent(SimEventType.COM_RADIO_SET_HZ, SimEventType.COM_RADIO_SET_HZ.ToString());

            //    public class SimVar
            //{
            //    public string Name { get; set; }            // Ex.: "COM ACTIVE FREQUENCY:1"
            //    public string Units { get; set; }           // Ex.: "KHz", "Boolean"
            //    public SIMCONNECT_DATATYPE DataType { get; set; }        // Ex.: SIMCONNECT_DATATYPE.INT64, SIMCONNECT_DATATYPE.FLOAT32
            //}

            SimVar s1 = new SimVar
            {
                Name = "COM ACTIVE FREQUENCY:1",
                Units = "KHz",
                DataType = SIMCONNECT_DATATYPE.INT64
            };

            SimVar nullSimVar = new SimVar
            {
                Name = "",
                Units = "",
                DataType = SIMCONNECT_DATATYPE.INT64
            };

            EventID[] evid = new EventID[6];
            evid[0] = new EventID { Name = "COM_RADIO_SET_HZ", EventIdType = EventIDType.COM_RADIO_SET_HZ, Multiplier = 1000 };
            evid[1] = new EventID { Name = "COM_STBY_RADIO_SET_HZ", EventIdType = EventIDType.COM_STBY_RADIO_SET_HZ, Multiplier = 1000 };
            evid[2] = new EventID { Name = "NAV1_RADIO_SET_HZ", EventIdType = EventIDType.NAV1_RADIO_SET_HZ, Multiplier = 1000 };
            evid[3] = new EventID { Name = "NAV1_STBY_SET_HZ", EventIdType = EventIDType.NAV1_STBY_SET_HZ, Multiplier = 1000 };
            evid[4] = new EventID { Name = "ADF_ACTIVE_SET", EventIdType = EventIDType.ADF_ACTIVE_SET, Multiplier = 1000 };
            evid[5] = new EventID { Name = "XPNDR_SET", EventIdType = EventIDType.XPNDR_SET, Multiplier = 1000 };

            RNSMSFSDATA rnsMSFSData = new RNSMSFSDATA
            {
                SERIAL = serial,
                ID = id,
                DESCRIPTION = peripheralName,
                BACKLITCOLOR = selectedColor,
                ADFGHOSTBUTTON = "",
                XPDRGHOSTBUTTON = "",
                ADFBUTTONSIMVAR = "",
                XPDRBUTTONSIMVAR = "",
                COMMACTTOREAD = s1,
                COMMACTTOWRITE = evid[0],
                COMMSTBTOREAD = nullSimVar,
                COMMSTBTOWRITE = evid[1],
                NAVACTTOREAD = nullSimVar,
                NAVACTTOWRITE = evid[2],
                NAVSTBTOREAD = nullSimVar,
                NAVSTBTOWRITE = evid[3],
                ADFTOREAD = nullSimVar,
                ADFTOWRITE = evid[4],
                XPDRTOREAD = nullSimVar,
                XPDRTOWRITE = evid[5]
            };

            try
            {
                string line = JsonConvert.SerializeObject(rnsMSFSData, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch 
            {
                Say("An error occoured while saving default MBx24 file.");
                return false;
            }

            return true;
        }


        // ============================== MBx24 =======================================

        private void DrawMSFSMBx24()
        {

            Label[] Outputlabels = new Label[32];
            Label[] Inputlabels = new Label[32];
            Panel[] inputPanel = new Panel[32];

            int origin = 32; // Vertical begin point
            int p = origin + 10;
            int h = 25;
            int col0 = 2;
            //int col1 = 55;
            int col2 = 550;

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

            Label InputConnector = new Label()       // Headers
            {
                Name = "InputConnector",
                AutoSize = true,
                Location = new Point(col0, 7),
                BackColor = backColor,
                ForeColor = frontColor,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Text = "Joystick\nbutton"
            };
            MSFSPanel.Controls.Add(InputConnector);

            //Label EventToWrite = new Label()
            //{
            //    Name = "EventToWrite",
            //    AutoSize = true,
            //    Location = new Point(col1 + 20, 22),
            //    BackColor = backColor,
            //    ForeColor = frontColor,
            //    Text = "Destination ( Button, etc )"
            //};
            //MSFSPanel.Controls.Add(EventToWrite);

            Label GhostLabel = new Label()
            {
                Name = "GhostLabel",
                AutoSize = true,
                Location = new Point(col2 - 120, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "Ghost key"
            };
            MSFSPanel.Controls.Add(GhostLabel);

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
            MSFSPanel.Controls.Add(OutputConnector);

            Label SimVarToRead = new Label()
            {
                Name = "SimVarToRead",
                AutoSize = true,
                Location = new Point(col2 + 150, 22),
                BackColor = backColor,
                ForeColor = frontColor,
                Text = "SimVar ( Led annunciator, etc )"
            };
            MSFSPanel.Controls.Add(SimVarToRead);

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

            p = origin + 10;

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

                //EVTOWRITE[i] = new TextBox();       // Event to write
                //TextBox txtbox = EVTOWRITE[i];
                //txtbox.Name = "DRTOWRITE" + (i + 1).ToString("D2");
                //txtbox.Size = new Size(300, 16);
                //txtbox.Location = new Point(50, h + 1);
                //txtbox.BackColor = backColor;
                //txtbox.ForeColor = frontColor;
                //txtbox.Font = new Font("Microsoft Sans Serif", 10);
                //EVTOWRITE[i] = txtbox;
                //inputPanel[i].Controls.Add(EVTOWRITE[i]);

                //MBX24DRVALUE[i] = new NumericUpDown();  // Input dataref's value
                //NumericUpDown num = MBX24DRVALUE[i];
                //num.Name = "MBX24DRVALUE" + (i + 1).ToString("D2");
                //num.Size = new Size(50, 16);
                //num.Location = new Point(360, h + 1);
                //num.BackColor = backColor;
                //num.ForeColor = frontColor;
                //num.Font = new Font("Microsoft Sans Serif", 10);
                //num.Minimum = 0;
                //num.Maximum = 9;
                //num.Value = 1;
                //MBX24DRVALUE[i] = num;
                //inputPanel[i].Controls.Add(MBX24DRVALUE[i]);

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

                MSFSPanel.Controls.Add(inputPanel[i]);        // Add current input sub-panel to MSFS panel

            }

            p = origin + 10;
            for (int i = 0; i != 32; i++)
            {
                SVTOREAD[i] = new TextBox();        // Simvar for annunciator
                TextBox txtbox = SVTOREAD[i];
                txtbox.Name = "SVTOREAD" + (i + 1).ToString("D2");
                txtbox.Size = new Size(300, 16);
                txtbox.Location = new Point(col2 + 150, p);
                txtbox.BackColor = backColor;
                txtbox.ForeColor = frontColor;
                txtbox.ReadOnly = false;
                txtbox.Font = new Font("Microsoft Sans Serif", 10);
                SVTOREAD[i] = txtbox;
                MSFSPanel.Controls.Add(SVTOREAD[i]);

                Label label3 = new Label();         // Label for the hardware output connector description
                label3.Name = "DESCOUT" + (i + 1).ToString("D2"); // Format label name to "DESCOUTDD" where DD is a two digit the index
                label3.Size = new Size(128, 16);
                label3.Location = new Point(col2, p + 5);
                label3.BackColor = frontColor;
                label3.ForeColor = backColor;
                label3.Text = " " + (i + 1).ToString("D2") + " - " + outputlabeltext[i];
                MSFSPanel.Controls.Add(label3);

                p += h;
            }

            Panel divider = new Panel()     // A divider made with a panel (sick)
            {
                Location = new Point(col2 - 25, 2),
                Size = new Size(2, p - h),
                BorderStyle = BorderStyle.None,
                BackColor = frontColor
            };

            MSFSPanel.Controls.Add(divider);

            //MSFSMBx24FillSimVarsList();
            MSFSMBx24FillSimVarsTreeView();

            MSFSPanel.Refresh();
        }

        private void MSFSMBx24FillSimVarsList()
        {
            /// <summary>
            /// Builds the context menu for MBx24 annunciators
            /// </summary>
            /// 
            /// The context menu is populated with boolean MSFS simvars only.
            /// It uses a Drop Down presentation divided by sections of interest.

            // Build a context menu to be called by the mouse right button on SimVars text fields

            ContextMenuStrip MBx24SimVarsMenu = new ContextMenuStrip(); // Instantiate menu
            MBx24SimVarsMenu.AutoSize = true;
            MBx24SimVarsMenu.ShowItemToolTips = true;

            ToolStripMenuItem AUTOPILOT, BRAKE, ELECTRIC, ENGINE, FUEL, RADIO, SYSTEM, HELI, OTHERS;

            MBx24SimVarsMenu.ItemClicked += new ToolStripItemClickedEventHandler(CopyMenuData);

            AUTOPILOT = new ToolStripMenuItem("Auto-Pilot");
            BRAKE = new ToolStripMenuItem("Aircraft Brake");
            ELECTRIC = new ToolStripMenuItem("Electric");
            ENGINE = new ToolStripMenuItem("Engine");
            FUEL = new ToolStripMenuItem("Fuel");
            RADIO = new ToolStripMenuItem("Radios");
            SYSTEM = new ToolStripMenuItem("System");
            HELI = new ToolStripMenuItem("Helicopter");
            OTHERS = new ToolStripMenuItem("Others");

            // Assign events to items groups
            AUTOPILOT.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            BRAKE.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            ELECTRIC.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            ENGINE.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            FUEL.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            RADIO.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            SYSTEM.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            HELI.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);
            OTHERS.DropDownItemClicked += new ToolStripItemClickedEventHandler(MBx24CopyMenuData);

            MBx24SimVarsDef.Clear();

            string lines;

            // Look for simvars compiled json files.
            // If not present, create them.
            MBx24CreateSimVarsList();

            ShowStat("Updating simvars...");

            // Fill in SimVars context menu
            lines = File.ReadAllText(MBx24SimVarsFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            MBx24SimVarsDef = JsonConvert.DeserializeObject<List<SimVarEntity>>(lines);  // Deserialize 'lines'

            // Create a "Delete" item to delete a previous selection from the readonly textbox
            var d = MBx24SimVarsMenu.Items.Add("Delete");
            d.ToolTipText = "Delete contents of the field";

            // Create and group simvar items
            foreach (SimVarEntity svv in MBx24SimVarsDef)
            {
                ToolStripMenuItem Item = new ToolStripMenuItem();
                Item.Text = svv.SimVar;
                Item.Tag = svv.Tag;
                Item.ToolTipText = svv.Tag;
                switch (svv.GroupType)
                {
                    case GroupType.AutoPilot:
                        AUTOPILOT.DropDownItems.Add(Item);
                        break;

                    case GroupType.BreakLanding:
                        BRAKE.DropDownItems.Add(Item);
                        break;

                    case GroupType.Electric:
                        ELECTRIC.DropDownItems.Add(Item);
                        break;

                    case GroupType.Engine:
                        ENGINE.DropDownItems.Add(Item);
                        break;

                    case GroupType.Fuel:
                        FUEL.DropDownItems.Add(Item);
                        break;

                    case GroupType.Heli:
                        HELI.DropDownItems.Add(Item);
                        break;

                    case GroupType.Radios:
                        RADIO.DropDownItems.Add(Item);
                        break;

                    case GroupType.Systems:
                        SYSTEM.DropDownItems.Add(Item);
                        break;

                    default:
                        OTHERS.DropDownItems.Add(Item);
                        break;
                }
            }

            // Add groups to context menu
            MBx24SimVarsMenu.Items.Add(AUTOPILOT);
            MBx24SimVarsMenu.Items.Add(BRAKE);
            MBx24SimVarsMenu.Items.Add(ELECTRIC);
            MBx24SimVarsMenu.Items.Add(ENGINE);
            MBx24SimVarsMenu.Items.Add(FUEL);
            MBx24SimVarsMenu.Items.Add(SYSTEM);
            MBx24SimVarsMenu.Items.Add(RADIO);
            MBx24SimVarsMenu.Items.Add(HELI);
            MBx24SimVarsMenu.Items.Add(OTHERS);

            // Assign menu strip to "read" simvars field
            for (int i = 0; i != 32; i++)
            {
                SVTOREAD[i].ContextMenuStrip = MBx24SimVarsMenu;
            }

            ShowStat("Done");

        }

        public void MBx24CopyMenuData(object sender, ToolStripItemClickedEventArgs e)
        {
            // Get item contents
            string c = e.ClickedItem.Text.Trim();

            // Get field name to apply context menu item contents
            ToolStripDropDownItem cm = sender as ToolStripDropDownItem; // Cast sender
            var t = cm.Owner as ToolStrip;                              // Get toolstrip owner
            ContextMenuStrip ContextMenu = t as ContextMenuStrip;       // Instantiate ContextMenuStrip of owner
            string ControlName = ContextMenu.SourceControl.Name;        // Get caller field name

            if (c.Equals("Delete"))
            {
                SetField(ControlName, " ");
            }
            else
            {
                // Fill the proper field
                SetField(ControlName, c);
            }
        }

        public void MBx24CreateSimVarsList()
        {

            if (File.Exists(MBx24SimVarsFile))
            {
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            ShowStat("Compiling MBx24 SimVars. Please wait...");

            NorAirMSFS.Connect MSFSConnect = new NorAirMSFS.Connect();

            List<SimVarEntity> MSFSsimvars = MSFSConnect.RetrieveSimVarsGroups();

            // NOTA: ********
            // simconnect.AddToDataDefinition(RequestType.PerFrameData, "LIGHT BEACON", "Boolean", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            //                              =>
            // simconnect.AddToDataDefinition(RequestType.PerFrameData, MSFSsimvars.ElementAt(0).SimVar , "Boolean", MSFSsimvars.ElementAt(0).GroupType, 0, SimConnect.SIMCONNECT_UNUSED);

            try
            {

                string line = JsonConvert.SerializeObject(MSFSsimvars, settings);

                File.WriteAllText(MBx24SimVarsFile, line);

                ShowStat("MBx24 SimVars file created");
            }
            catch
            {
                ShowStat("There was an error creating a default MBx24 SimVars file");
            }

            ShowStat("MBx24 SimVars file created");
            Thread.Sleep(1000);
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

        public void MBx24FillData(string serial, int id)
        {
            /// <summary>
            /// Fills the setup panel fields with MBx24 input/output definitions
            /// </summary>

            if (!File.Exists(currentFileName))
            {
                MSFSSaveDefaultMBx24(serial, id);
            }

            MBx24MSFSDATA mbx24Sets = MBx24LoadData(serial, id);

            SERIAL.Text = mbx24Sets.SERIAL;
            ID.Text = mbx24Sets.ID.ToString();
            DESCRIPTION.Text = mbx24Sets.DESCRIPTION;
            bool FoundSelected = false;

            var SelectedCustomColor = MSFSColorPanel.Controls.OfType<RadioButton>();

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
                (MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).Checked = true;
                (MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor = ColorTranslator.FromHtml(mbx24Sets.BACKLITCOLOR);
            }

            // Set each field
            foreach (MBx24Reference r in mbx24Sets.REFERENCES)
            {
                int i = r.CONNECTOR - 1;
                string con = r.CONNECTOR.ToString("D2");
                SetField("SVTOREAD" + con, r.SIMVARTOREAD);
                SetField("MBx24GHOSTKEYS" + con, r.GHOSTKEY);
            }
        }

        public MBx24MSFSDATA MBx24LoadData(string serial, int id)
        {

            MBx24MSFSDATA mbx24msfsData = new MBx24MSFSDATA();

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
                MSFSSaveDefaultMBx24(serial, id);
            }

            string lines = File.ReadAllText(JSonFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            try
            {
                mbx24msfsData = JsonConvert.DeserializeObject<MBx24MSFSDATA>(lines);  // Deserialize 'lines'
            }
            catch
            {
                return null;
            }

            return mbx24msfsData;

        }



        /*===========================================================================*/
        //public class MBx24Reference // Class for each MBx24 input/output connector
        //{
        //    public byte CONNECTOR { get; set; }
        //    public string SIMVARTOREAD { get; set; }
        //    public string GHOSTKEY { get; set; }
        //}

        //public class MBx24MSFSDATA      // A class for the MBx24 settings
        //{
        //    public string SERIAL { get; set; }
        //    public int ID { get; set; }
        //    public string DESCRIPTION { get; set; }
        //    public string BACKLITCOLOR { get; set; }
        //    public int FREQUENCY { get; set; }
        //    public IList<MBx24Reference> REFERENCES { get; set; }

        //}
        /*===========================================================================*/

        public void MSFSSaveDefaultMBx24(string serial, int id)
        {
            string JSonFile = currentFileName;

            if (File.Exists(currentFileName))
            {
                return;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            List<MBx24Reference> refList = new List<MBx24Reference>();

            for (int i = 0; i < 32; i++)
            {
                MBx24Reference r = new MBx24Reference();
                r.CONNECTOR = (byte)(i + 1);
                r.SIMVARTOREAD = "";
                r.GHOSTKEY = "";
                refList.Add(r);
            }

            MBx24MSFSDATA mbx24msfsData = new MBx24MSFSDATA();

            mbx24msfsData.SERIAL = serial;
            mbx24msfsData.ID = id;
            mbx24msfsData.DESCRIPTION = peripheralName;
            mbx24msfsData.BACKLITCOLOR = RetrieveSelectedBackLitColor();
            mbx24msfsData.REFERENCES = refList;

            try
            {

                string line = JsonConvert.SerializeObject(mbx24msfsData, settings);

                File.WriteAllText(JSonFile, line);

            }
            catch (Exception e)
            {
                Say("Error creating default MBx24 file\n" + e.Message);
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
                r.SIMVARTOREAD = FieldText("SVTOREAD" + con);
                r.GHOSTKEY = FieldText("MBx24GHOSTKEYS" + con);
                refList.Add(r);
            }

            MBx24MSFSDATA mbx24msfsData = new MBx24MSFSDATA();

            mbx24msfsData.SERIAL = SERIAL.Text;
            mbx24msfsData.ID = int.Parse(ID.Text);
            mbx24msfsData.DESCRIPTION = peripheralName;
            mbx24msfsData.BACKLITCOLOR = RetrieveSelectedBackLitColor();
            mbx24msfsData.REFERENCES = refList;

            try
            {

                string line = JsonConvert.SerializeObject(mbx24msfsData, settings);

                File.WriteAllText(JSonFile, line);

                MessageBox.Show("Configuration saved", "MBx24 save", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch 
            {
                Say("An error occoured while saving MBx24 configuration.");
            }

            return;

        }

        /*========================= TreeView construction methods & events ==========================*/

        public void MSFSMBx24FillSimVarsTreeView()
        {
            /// <summary>
            /// This constructs a TreeView based on the MSFS SimVars
            /// It is also created a right mouse button menu to allow
            /// user to copy the meaningful data to paste in the fields
            /// </summary>

            MBx24SimVarsDef.Clear();

            string lines;

            // Build a context menu to be called by the mouse right button

            ContextMenuStrip docMenu = new ContextMenuStrip(); // Instantiate
            docMenu.Click += new EventHandler(CopyDataRef);     // Event handler
            ToolStripMenuItem copyLabel = new ToolStripMenuItem();  // Create a label "Copy"
            copyLabel.Text = "Copy, please";
            docMenu.Items.AddRange(new ToolStripMenuItem[] { copyLabel }); // Fill the menu

            // Look for simvars compiled json files.
            // If not present, create them.
            MBx24CreateSimVarsList();

            ShowStat("Updating simvars...");

            // Fill in SimVars context menu
            lines = File.ReadAllText(MBx24SimVarsFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            MBx24SimVarsDef = JsonConvert.DeserializeObject<List<SimVarEntity>>(lines);  // Deserialize 'lines'

            SimVarsTreeView.Nodes.Add("SimVars");
            SimVarsTreeView.Nodes[0].Nodes.Add("AUTOPILOT");
            SimVarsTreeView.Nodes[0].Nodes.Add("BRAKE");
            SimVarsTreeView.Nodes[0].Nodes.Add("ELECTRIC");
            SimVarsTreeView.Nodes[0].Nodes.Add("ENGINE");
            SimVarsTreeView.Nodes[0].Nodes.Add("FUEL");
            SimVarsTreeView.Nodes[0].Nodes.Add("RADIO");
            SimVarsTreeView.Nodes[0].Nodes.Add("SYSTEM");
            SimVarsTreeView.Nodes[0].Nodes.Add("HELI");
            SimVarsTreeView.Nodes[0].Nodes.Add("OTHERS");

            SimVarsTreeView.Nodes[0].BackColor = frontColor;
            SimVarsTreeView.Nodes[0].ForeColor = backColor;

            for (int i = 0; i < SimVarsTreeView.Nodes[0].Nodes.Count; i++)
            {
                SimVarsTreeView.Nodes[0].Nodes[i].BackColor = frontColor;
                SimVarsTreeView.Nodes[0].Nodes[i].ForeColor = backColor;
            }

            foreach (SimVarEntity simv in MBx24SimVarsDef) // Fill in the TreeView
            {

                int group = GetEnum<GroupType>(simv.GroupType.ToString());
                string simvDescrition = simv.SimVar;
                TreeNode treeNode = new TreeNode();
                treeNode.Text = simvDescrition;
                treeNode.Tag = simv.Tag;
                treeNode.BackColor = backColor;
                treeNode.ForeColor = frontColor;
                SimVarsTreeView.Nodes[0].Nodes[group].Nodes.Add(simvDescrition);
                SimVarsTreeView.Nodes[0].Nodes[group].LastNode.ContextMenuStrip = docMenu;
            }

        }

        private void CopyDataRef(object sender, EventArgs e)
        {
            TreeNode h = SimVarsTreeView.SelectedNode;

            if (h != null)
            {
                if (h.Text.Trim().Length != 0)
                {
                    Clipboard.SetText(SimVarsTreeView.SelectedNode.Text.Trim());
                }
            }
        }




        /*========================= General events & other methods ==========================*/

        private void MSFSSaveButton_Click(object sender, EventArgs e)
        {
            int PeripheralType = int.Parse(SERIAL.Text.Substring(0, 2));
            string serial = SERIAL.Text;
            int id;
            bool convertedId = int.TryParse(ID.Text, out id);
            if (!convertedId)
            {
                Say("An error occoured while saving.\nThe peripheral ID is malformed");
                return;
            }
            switch (PeripheralType)
            {
                case 31:
                    MSFSSaveDefaultRNS(SERIAL.Text, int.Parse(ID.Text));
                    SaveRNSData();
                    break;
                case 50:
                    MSFSSaveDefaultMBx24(SERIAL.Text, int.Parse(ID.Text));
                    SaveMBx24Data();
                    break;
            }
        }

        public int GetEnum<T>(string name) where T : struct, IConvertible
        {
            if (!(typeof(T).IsEnum || name.Length != 0))
            {
                return -1;
            }
            int retval = (int)Enum.Parse(typeof(T), name);

            return retval;
        }

        public string RetrieveSelectedBackLitColor()
        {
            var SelectedCustomColor = MSFSColorPanel.Controls.OfType<RadioButton>();

            foreach (RadioButton rb in SelectedCustomColor) // Find out the default color or assume 0
            {

                if (rb.Checked)
                {
                    return ColorTranslator.ToHtml(rb.BackColor);
                }
            }
            return ColorTranslator.ToHtml((MSFSColorPanel.Controls.OfType<RadioButton>()).ElementAt(0).BackColor);
        }

        private void MSFSExitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
            this.Dispose();
        }

        public void Say(string s)
        {
            MessageBox.Show(s, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

    }

}
