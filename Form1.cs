using NorAir;
using NorAirXPlane;
using NorAirMSFS;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using TOGA.Properties;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static TOGA.RNSForm;
using static TOGA.XPlaneSetup;
using Microsoft.FlightSimulator.SimConnect;
using static TOGA.MSFSSetup;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Versioning;
using static System.Windows.Forms.AxHost;
using SharpDX.DirectInput;

using static NorAirXPlane.Connect;


namespace TOGA

{       /*
            *** DISCLAIMER ***
            
            *** THE SOFTWARE AND RELATED DOCUMENTATION IS PROVIDED “AS IS”, WITHOUT WARRANTY
            *** OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
            *** OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
            ***
            *** USE THIS SOFTWARE AT YOUR WON RISK.
            ***
            *** Northern Aircraft Labs, NorAirLabs and GAMAN are brands and trademarks
            *** of GAMAN Portugal, Lda. and can not be used without expressed written
            *** consent from GAMAN Portugal, Lda.
            *** 
            *** All other trademarks and brands are property of their respective owners.
            *** All company, product and service names mentioned in this software and/or
            *** documentation are for identification purposes only. Use of these names,
            *** trademarks and brands does not imply endorsement.
            *** 
            *** GAMAN Portugal, Lda. is not reliable or responsible for the use of this
            *** software in any form or shape. Read @ https://norairlabs.com/guarantee-returns/
            ***
            *** Althougt GAMAN Portugal, Lda.made all efforts to ensure the safety of its
            *** products, issues and problems may occur out of our control or responsibility.
            *** 
            *** The TOGA software is an open-source MIT licensed for users to enjoy and
            *** as an example for the use of our SDK.You may distribute, change and even
            *** sell as far as the Northern Aircraft Labs and GAMAN Portugal is properly
            *** referenced and MIT license followed. See https://opensource.org/license/MIT
            *** 
            *** 
            *** == Also ==
            ***
            *** Users should take some time to read this software and try to understand it 
            *** before adapt to the needs or making any changes. It has plenty of comments
            *** to easy its reading.
            ***
            *** Begin by reading the - Main body - comments and remarks.
            ***
            *** Also, read the SDK and NorAirFramework protocol documentation available at
            *** https://www.norairlabs.com
            ***
            */

    public partial class Form1 : Form
    {

        public string Version = "1.3.0c";       // Current version

        public class Styles // Styling the GUI
        {
            public string styleName { get; set; }
            public string backColor { get; set; }
            public string frontColor { get; set; }
            public string buttonColor { get; set; }
            public string buttonForecolor { get; set; }
            public string mouseoverColor { get; set; }
            public string fontName { get; set; }
            public float fontSize { get; set; }
            public string borderColor { get; set; }
            public decimal borderSize { get; set; }

        }

        public class PeripheralProperty // Peripherals are identified by these properties
        {
            public string SERIALNUMBER { get; set; }
            public string DESCRIPTION { get; set; }
            public int ID { get; set; }
        }

        public class BackLitColor
        {
            public Byte Red;    // Color channels
            public Byte Green;
            public Byte Blue;
            public Byte Alpha;  // Transparency 
        }

        // MSFS enums
        public enum SimEventType
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
            ADF_ACTIVE_SET,
            ADF2_ACTIVE_SET,
            XPNDR_SET
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

        public enum SimPriorities
        {
            SIMCONNECT_GROUP_PRIORITY_HIGHEST,
            SIMCONNECT_GROUP_PRIORITY_STANDARD,
            SIMCONNECT_GROUP_PRIORITY_DEFAULT,
            SIMCONNECT_GROUP_PRIORITY_LOWEST
        }

        public enum DefineID
        {
            ID0,
            ID1,
            ID2,
            ID3,
            ID4,
            ID5
        }

        // Xplane flags & instaces
        public bool stop = true;                           // A "Go/No Go" thread flag
        public bool XPlaneThreadIsRunning = false;          // Xplane write thread flag
        public bool XPlaneReceivingThreadIsRunning = false; // XPlane read thread flag
        public bool JPORTContinueReading = false;           // J-PORT "continue reading" thread flag

        public NorAirXPlane.Connect XPlane = new NorAirXPlane.Connect(); // XPlane object

        // MSFS flags & instances
        public bool MSFSReceivingThreadIsRunning = false;
        public bool MSFSSendingThreadIsRunning = false;
        public byte[] SIMVARsConnectors = new byte[32]; // List for definitions requested
        public byte[] MSFSConnectorsLastState = new byte[32];
        public int MSFSInteractions = 0;
        public int MSFSMaxInteractions = 1;
        public bool MSFSStartInteractions = false;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct RNSReceivedSimVars
        {
            /// <summary> Structure to store data received from the MSFS.</summary>
            /// The number of properties in this structure must match the number
            /// of AddToDataDefinition() calls registered and the types must match
            /// the SIMCONNECT_DATATYPE ones.
            /// See table for conversion in <see cref="#region MSFS region"/> comments.

            //public float Altitude;                          // INDICATED ALTITUDE (feet)
            //public float KohlsmanSettingHg;                 // KOHLSMAN SETTING HG (inHG)
            public Int64 Com1ActFreq;                       // COM ACTIVE FREQUENCY:1 (KHz)
            public Int64 Com1StbFreq;                       // COM STANDBY FREQUENCY:1 (KHz)
            public Int64 Nav1ActFreq;                       // NAV ACTIVE FREQUENCY:1 (KHz)
            public Int64 Nav1StbFreq;                       // NAV STANDBY FREQUENCY:1 (KHz)
            public Int64 ADF1ActFreq;                       // ADF ACTIVE FREQUENCY:1 (KHz)
            public Int64 Transponder;                       // TRANSPONDER CODE:1 (Hz)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct MBx24ReceivedSimVars
        {
            public float Annunciator_1;                      // First annunciator
            public float Annunciator_2;
            public float Annunciator_3;
            public float Annunciator_4;
            public float Annunciator_5;
            public float Annunciator_6;
            public float Annunciator_7;
            public float Annunciator_8;
            public float Annunciator_9;
            public float Annunciator_10;
            public float Annunciator_11;
            public float Annunciator_12;
            public float Annunciator_13;
            public float Annunciator_14;
            public float Annunciator_15;
            public float Annunciator_16;
            public float Annunciator_17;
            public float Annunciator_18;
            public float Annunciator_19;
            public float Annunciator_20;
            public float Annunciator_21;
            public float Annunciator_22;
            public float Annunciator_23;
            public float Annunciator_24;
            public float Annunciator_25;
            public float Annunciator_26;
            public float Annunciator_27;
            public float Annunciator_28;
            public float Annunciator_29;
            public float Annunciator_30;
            public float Annunciator_31;
            public float Annunciator_32;                        // Last annunciator

            //public float beacon;                              // LIGHT BEACON (Boolean)
        }

        // we need an enum to register the data structure above. note that if you want to store them in separate structures, you'll need to register with a separate enum value here.
        private enum RequestType
        {
            RNSFrameData,
            MBx24FrameData
        }

        private const uint WM_USER_SIMCONNECT = 0x0402;                 // User event identified by a number
        private const string MSFS_PROCESS_NAME = "FlightSimulator";     // MSFS's process name
        private const string PLUGIN_NAME = "TOGA";                      // Plugin's name

        private const int SIMCONNECT_TIMER_INTERVAL_MS = 50;            // how often to poll data from the game
        //private DispatcherTimer simConnectDispatcherTimer;              // the DispatcherTimer object used to poll data from the game

        private SimConnect simconnect;                                  // the SimConnect SDK object used to subscribe and interact with the game
        private RNSReceivedSimVars simvars;



        // Ghostkeys definitions
        [DllImport("User32.dll")] // To use with keyghosting an external app
        static extern int SetForegroundWindow(IntPtr point); // Send app to foreground and focus

        // Lists & arrays
        public List<NorAir.RNS> Radios = new List<NorAir.RNS>();   // Holds multiple RNS peripherals ( note: this TOGA version just handles one instance of each peripheral type @ index 0 )
        public List<NorAir.MBx24> MBx24s = new List<MBx24>();      // Holds multiple MBx24 peripherals ( note: this TOGA version just handles one instance of each peripheral type @ index 0 )
        public List<PeripheralProperty> PeripheralsPresent = new List<PeripheralProperty>(); // Holds peripherals present after enumeration
        public List<string> ValidIds = new List<string>();      // Used for validating IDs and sugestion of best new IDs
        public int[] JPORTButtons = new int[200];               // J-PORT buttons logic state
        public System.Windows.Forms.Label[] Connectors = new System.Windows.Forms.Label[200];  // Physical connectors labels to display


        // Environment & paths

        // Base defs path, located in "My Documents\NorAirLabs" folder
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";

        // X-Plane paths & files
        public static string XPlanePath = BasePath + @"\X-Plane"; //                            X-Plane defs path
        public static string XPlaneProfilesPath = XPlanePath + @"\Profiles"; //                 X-Plane profiles base path
        public static string XPlaneDefaultProfile = XPlanePath + @"\DefaultProfile.json"; //    X-Plane default profile

        // MSFS paths & files
        public static string MSFSPath = BasePath + @"\MSFS"; //                             MSFS defs path
        public static string MSFSProfilesPath = MSFSPath + @"\Profiles"; //                 MSFS profiles base path
        public static string MSFSDefaultProfile = MSFSPath + @"\DefaultProfile.json"; //    MSFS default profile
        public string MSFSProfileSet;                                                   //  MSFS thread-safe profile name

        // TOGA definitions files
        public string PeripheralsFileName = BasePath + ".\\Peripherals.json"; //    Declared peripherals
        public string BackLitFileName = BasePath + ".\\BackLit.json"; //            General backlit defs
        public string StylesFileName = BasePath + ".\\Styles.json"; //              Toga environment defs

        // Miscellaneous vars
        public string nl = Environment.NewLine; //  New line

        // Default style & definitions
        public Color backColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color frontColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color buttonColor = (Color)ColorTranslator.FromHtml("#252525");
        public Color buttonForecolor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public Color borderColor = (Color)ColorTranslator.FromHtml("#E8BA1B");
        public decimal borderSize = 2;
        public Color mouseOverColor = (Color)ColorTranslator.FromHtml("#E0E0E0");
        public Point ControlLocation;
        public const int BackLitColorSlots = 10;
        public BackLitColor[] BackLitColors = new BackLitColor[BackLitColorSlots];
        public Font CharFont = new Font("Verdana", 8);

        // NorAirLabs primary hardware instances & definitions
        public NorAir.OBCS CPORT = new NorAir.OBCS();   // C-PORT instance
        public NorAir.WALL WALL = new NorAir.WALL();    // Broadcast engine instance
        public NorAir.JPORT JPORT = new NorAir.JPORT(); // J-PORT instance ( joystick )
        public NorAir.TalkMode SendOnly = TalkMode.SendOnly; // Simplify life. See NorAir namespace

        // NorAirLabs flags
        bool RNSPresent = false;
        bool MBx24Present = false;
        bool InRealTime = false;

        // A HID joystick instance for reading
        public SharpDX.DirectInput.Joystick theJoystick = null;

        //                                      - Main body -


        /* Remarks to programmers:              READ ME
        
        - Terminology:

            Peripherals - All the functional hardware that connects to OBCS such as RNS or MBx24

            OBCS - "On Board Computer System". This is a computer system that manages
                communications and interfaces a serial and a joystick devices with user computer.

            RNS - "Radio & Navigation System". It is a peripheral and comprises a stack of instruments
                that connects to OBCS. It includes a COMM, a NAV, an ADF and a transponder.

            MBx24 - "Modules Bay" for 24 input/output modules. This peripheral supports up to
                24 modules like buttons and annunciators modules. It has 32 input connectors
                and 32 output connectors to attach the modules. Be aware that one module may
                physical occupy 1 slot but many inputs and/or outputs connectors
            
        - Important notes before browse the TOGA software:
        
            * Serial numbers
        
            - The first two digits of a serial number represents the type of hardware
            to handle. For example, if a serial number is "3123456789", the first two
            digits, "31", designates that it is a RNS peripheral ( Radio & Navigation System ).
            This enables a simple and quick way to access to a peripheral without having
            communication or other approaches to find out the hardware model.

            * Enumeration of peripherals
        
            - Peripheral enumeration is the registration process of a peripheral serial number
            in the OBCS. Enumerating a peripheral, the user supplies an 'ID' for communication
            purposes.
        
            * Peripherals 'ID'
        
            - The 'ID' is the "callsign" of a peripheral. After proper enumeration of one
            peripheral in the OBCS, the peripheral will ONLY be recognized by its ID number.
            This ID number is set to a default but can be changed for a more convenient one.
            Although two equal peripherals could be present, each one will be reached by its
            individual ID.
            For instance, if present the RNS "3123456789" and RNS "3122233344", the first can
            have the "callsign" ( ID ) 31 and the second 32. This shortens the communication
            time and length and simplifies the hardware handling.
        
            - Valid IDs are between 21 and 98. All others are reserved and WILL NOT BE ACCEPTED
            BY OBCS hardware and may generate errors and/or issues by enumeration.
        
            - Default IDs:
                31 - RNS peripherals
                50 - MBx24 peripherals
                35 - Autopilot ( Yes, already in testbed )
                10 - J-Port ( Reserved )
                99 - C-Port & OBCS ( Reserved )
                15 - Throttle ( Yes, again, already in testbed )
        
        */


        public Form1()
        {

            InitializeComponent();

            initFiles();

            ReadStyles();   // Get GUI styles

            SetTheme(this.Controls, backColor, frontColor); // Apply styles

            ReadPreferredBackLitColors(); // Get backlit defs.

            MakeColorBox();

            DrawPanel2(); // Draw a panel accordingly to hardware

            XPlaneFillProfiles(); // Set up X-Plane profiles

            MSFSFillProfiles(); // Set up MSFS profiles

            JoystickLabel.BackColor = backColor;
            JoystickLabel.ForeColor = backColor;
            JoystickLabel.Text = "Joystick on-line";

            Thread JPORTThread = new Thread(() => JPORTRead()); //  Thread to read J-PORT until shutdown
            JPORTThread.IsBackground = true; //                     This will run forever but skips steps while simulation is interfaced

            InitJoystick();

            init(); // Init hardware and enumeration

            if (theJoystick != null)
            {
                joystickOnLine(true);
                JPORTContinueReading = true;                        
                JPORTThread.Start();
            }
            else
            {
                joystickOnLine(false);
            }

        }

        public void initFiles()
        {
            /// <summary>
            /// Fundamental file creation
            /// </summary>
            /// Software first run creates the base directories and files needed
            /// 

            if (!Directory.Exists(BasePath))    
            {
                try
                {
                    Directory.CreateDirectory(BasePath);
                    Directory.CreateDirectory(BasePath + @"\X-Plane");
                    Directory.CreateDirectory(BasePath + @"\AeroWinx");
                    Directory.CreateDirectory(BasePath + @"\MSFS");

                    Directory.CreateDirectory(XPlaneProfilesPath);
                    File.Copy(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\DataRefs.txt", XPlanePath + @"\DataRefs.txt");

                    Directory.CreateDirectory(MSFSProfilesPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show("It is not possible to create the main folder \n\n" +
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        "\\NorAirLabs\n" + e.Message,
                    "Critical error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    this.Close();   //                                      Without proper initiation, the software fails. So, exit.
                }
            }
        }

        public void InitJoystick()
        {
            /// <summary>
            /// Detects and instantiates by Direct Input the NorAir Labs joystick
            /// </summary>
            /// If found, sets theJoystick var
            /// 

            Guid joystickGuid = Guid.NewGuid();

            DirectInput directInput = new DirectInput();

            foreach (var deviceInstance in directInput.GetDevices())
            {
                try
                {
                    var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);

                    if (joystick.Properties.InstanceName.Contains("Northern Aircraft Labs"))
                    {
                        joystickGuid = deviceInstance.InstanceGuid;
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (joystickGuid != Guid.Empty)
            {
                try
                {
                    SharpDX.DirectInput.Joystick jjoystick;
                    jjoystick = new SharpDX.DirectInput.Joystick(directInput, joystickGuid);
                    jjoystick.Properties.BufferSize = 1024; //allocate a buffer to hold the device's state
                    jjoystick.Acquire(); //do this for every joystick that's plugged in, the user may have multiple joysticks plugged in
                    theJoystick = jjoystick;
                }
                catch
                {
                    return;
                }
            }

            if (theJoystick != null)
            {
                joystickOnLine(true);
            }
            else
            {
                joystickOnLine(false);
            }
        }

        private void SetTheme(Control.ControlCollection ctrs, Color bcolor, Color fcolor)
        {
            /// <summary>
            /// Change foreground and background colors of controls
            /// It is restrited to the ones mentioned
            /// </summary>
            /// 

            ApplyTheme(ctrs, bcolor, fcolor);   // Theme settings to general objects (b.t.w., this is a recursive method).

            // Specific & fixed style settings
            string fontcopy = CharFont.FontFamily.ToString();
            Font ItalicCharFont = new Font(fontcopy, 9, FontStyle.Italic);
            NoRegisteredPeripheralsLabel.Font = ItalicCharFont;
            NoRegisteredPeripheralExplanation.BackColor = Color.SeaShell;
            NoRegisteredPeripheralExplanation.ForeColor = Color.Black;
            XPlaneNoProfilesLabel.Font = ItalicCharFont;
            MSFSNoProfilesLabel.Font = ItalicCharFont;
            NoProfilePeripheralsLabel.Font = ItalicCharFont;
            Font Header = new Font(fontcopy, 12, FontStyle.Bold);
            TitleLabel.Font = Header;
            TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            TitleLabel.BackColor = frontColor;
            TitleLabel.ForeColor = backColor;

            this.BackColor = backColor; // Tabs and main's form entities update
            this.ForeColor = frontColor;

            statusBar.BackColor = backColor;
            statusBar.ForeColor = frontColor;

            BackGroundPanel.BackColor = backColor;
            BackGroundPanel.ForeColor = frontColor;

            TABXPlaneSetup.BackColor = backColor;
            TABXPlaneSetup.ForeColor = frontColor;
            TABMSFSSetup.BackColor = backColor;
            TABMSFSSetup.ForeColor = frontColor;
            TABPeripheralsSetup.BackColor = backColor;
            TABPeripheralsSetup.ForeColor = frontColor;
            TABBackLit.BackColor = backColor;
            TABBackLit.ForeColor = frontColor;
            TABOptions.BackColor = backColor;
            TABOptions.ForeColor = frontColor;
            TOGASetup.SelectedTab.BackColor = backColor;
            TOGASetup.SelectedTab.ForeColor = frontColor;

            VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            VersionLabel.Text = System.String.Format("Ver. {0}", Version);
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
            var ctr = from controls in ctrs.OfType<Button>()    // Theme settings to buttons
                      select controls;
            foreach (var control in ctr)
            {
                control.BackColor = buttonColor;
                control.ForeColor = buttonForecolor;
                control.Font = CharFont;
                control.FlatStyle = FlatStyle.Flat;
                control.FlatAppearance.BorderColor = borderColor;
                control.FlatAppearance.BorderSize = (int)borderSize;
                control.FlatAppearance.MouseOverBackColor = mouseOverColor;
            }
        }

        void DrawPanel2()
        {
            if (panel2 == null)
            {
                return;
            }
            int marginLeft = 14;
            int marginTop = 24;
            Point p = new Point();
            Size z = new Size();
            int counter = 0;
            for (int i = 0; i != 8; i++)
            {
                for (int j = 0; j != 25; j++)
                {
                    p.X = marginLeft + j * 38;            // Instatiate input index 
                    p.Y = marginTop + i * 30;
                    z.Width = 34;
                    z.Height = 16;
                    var il = new System.Windows.Forms.Label();
                    il.Location = p;
                    il.Size = z;
                    il.Name = "IL" + i.ToString("D2");
                    il.Text = (counter + 1).ToString("D2");
                    il.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    il.BackColor = backColor;
                    il.ForeColor = frontColor;
                    il.BorderStyle = BorderStyle.FixedSingle;
                    JoyPanel.Controls.Add(il);

                    Connectors[counter] = il;
                    counter++;
                }
            }
        }

        public void JPORTRead()
        {
            /// <summary>
            /// Thread for keep reading joystick buttons logic state
            /// </summary>
            /// It only does the job if other threads aren't running
            /// 

            if (theJoystick != null)
            {

                for (int i = 0; i != 200; i++)
                {
                    Connectors[i].BackColor = backColor;
                    Connectors[i].ForeColor = frontColor;
                }

                PostMessage(CPORT.Send(JPORT.SoftReset()));

                Thread.Sleep(500);

                while (true)
                {
                    if (!JPORTContinueReading) // Continue?
                    {
                        break;
                    }
                    if (theJoystick != null)
                    {
                        if (stop)   // If other threads are not running and reading the J-PORT...
                        {
                            if (CPORT.Found)
                            {
                                try
                                {

                                    theJoystick.Poll();

                                    var theData = theJoystick.GetCurrentState();

                                    for (int i = 0; i < theData.Buttons.Count(); i++)
                                    {
                                        if (theData.Buttons[i] == false)
                                        {
                                            Connectors[i].BackColor = backColor;
                                            Connectors[i].ForeColor = frontColor;
                                        }
                                        else
                                        {
                                            Connectors[i].BackColor = frontColor;
                                            Connectors[i].ForeColor = backColor;
                                        }
                                    }
                                }
                                catch
                                {
                                    JoystickLabel.BackColor = backColor;        // Label must be changed here to avoid cross thread share of label
                                    JoystickLabel.ForeColor = backColor;
                                    theJoystick = null;
                                }
                                Thread.Sleep(100); // Wait a while to restart.
                            }
                        }
                    }
                }
            }
        }

        public void joystickOnLine(bool OnLine)
        {
            if (OnLine)
            {
                JoystickLabel.BackColor = frontColor;
                JoystickLabel.ForeColor = backColor;
                return;
            }
            JoystickLabel.BackColor = backColor;
            JoystickLabel.ForeColor = backColor;
        }


        public void FillInPeripherals()
        {
            /// <summary>
            /// Here, instances of the peripherals are added to 'PeripheralsPresent' list
            /// by hardware type, to be handled later. Also, configurations are sent to them.
            /// </summary>

            // Read registered peripherals
            Radios.Clear();
            MBx24s.Clear();
            ValidIds.Clear();
            for (int i = 21; i < 99; i++) // OBCS ONLY validates IDs from 21 to 98
            {
                ValidIds.Add(i.ToString());
            }
            int PeripheralsCount = 0;

            if (PeripheralsPresent.Count() != 0)
            {
                CPORT.Send(JPORT.SoftReset());

                Thread.Sleep(200);

                foreach (PeripheralProperty p in PeripheralsPresent)
                {
                    if (p != null)
                    {
                        string configfile = BasePath + "\\" + p.SERIALNUMBER.Trim() + ".json";

                        if (p.SERIALNUMBER.StartsWith("31"))
                        {
                            // It is a radio & navigation system. Add to Radios object list
                            Radios.Add(new NorAir.RNS(p.SERIALNUMBER, (byte)p.ID));

                            ValidIds.Remove(p.ID.ToString()); // This 'ID' is now in use. Remove from available IDs

                            PeripheralsCount++;

                            if (!File.Exists(configfile))
                            {
                                RNSForm rns = new RNSForm();

                                rns.CreateDefaultRNSConfig(p.SERIALNUMBER, p.ID, p.DESCRIPTION);
                            }
                            RNSPresent = ConfigRNS(p.SERIALNUMBER, p.ID);
                        }
                        if (p.SERIALNUMBER.StartsWith("50"))
                        {
                            // It is a Modules bay. Add to Modules bay object list
                            MBx24s.Add(new NorAir.MBx24(p.SERIALNUMBER, (byte)p.ID));

                            ValidIds.Remove(p.ID.ToString()); // This 'ID' is now in use. Remove from available IDs

                            PeripheralsCount++;

                            if (!File.Exists(configfile))
                            {
                                MBx24Form mb = new MBx24Form();
                                mb.CreateDefaultMBx24Config(p.SERIALNUMBER, p.ID, p.DESCRIPTION);
                            }
                            MBx24Present = ConfigMBx24(p.SERIALNUMBER, p.ID);
                        }
                    }
                }
            }

            if (PeripheralsCount == 0)
            {
                ShowStat("Please, add peripherals for enumeration");
                return;
            }
        }

        public void init()
        {
            /// <summary>
            /// The peripherals enumeration.
            /// The enumeration is a declaration of the peripherals present and its IDs
            /// to the OBCS.
            /// The OBCS produces a simple yet effective addresses translation table and
            /// starts the internal messages exchange at the highest speed possible. 
            /// A successful enumeration can be seen by the activity led panel in OBCS hardware.
            /// This allows users to reach peripherals via IDs in a simple and fast way during the
            /// simulation software execution.
            /// 
            /// * Remarks:
            /// 
            ///     - If a peripheral is enumerated for the first time, it can have an attributed
            ///     an ID by user choice. Here, the application manages IDs automatically.
            ///     
            ///     - If the enumeration regards an existing one, OBCS will substitute the attributed
            ///     ID to the new one supplied.
            ///     
            ///     - Enumeration may occour as many times as needed without restarts or resets.
            /// 
            ///     - Enumeration is lost on hard-resets and power shutdown.
            ///     
            /// </summary>

            if (!ReadRegisteredPeripherals())   // Are there any application saved peripherals in the list?
            {
                ShowStat("Please, add peripherals for enumeration ");
            }
            else
            {
                FillInPeripherals();

                XPlaneSetButtons(false);              // Disable some buttons
                ShowStat("Looking for OBCS...");   // Update status bar
                if (!CPORT.Found)
                {
                    CPORT.CPort.Close();
                    Find_OBCS();                    // Look for an OBCS
                }
                if (CPORT.Found)
                {
                    bool lastStop = stop;

                    if (!CPORT.Opened)
                    {
                        CPORT.Open();
                        CPORT.Flush();
                    }

                    stop = false;  // Suspend the joystick reading thread for a while

                    //  Altough TOGA iterates various instances of the same peripheral type, it only manages
                    //  the first one of each type for now.

                    if (Radios.Count != 0)          // RNSes enumeration
                    {
                        foreach (NorAir.RNS radio in Radios)    // Iterate listed RNSes 
                        {
                            bool en = CPORT.Enumerate(radio.enumerationData); // Send enumeration data
                            if (en)
                            {
                                ShowStat("Enumerated " + radio.SerialNumber.ToString());
                            }
                            else
                            {
                                ShowStat("Error: " + radio.SerialNumber.ToString() + " was not enumerated");
                                Thread.Sleep(1000); // Some time for user read the status line
                            }
                        }
                    }

                    if (MBx24s.Count != 0)          // MBx24 enumeration
                    {
                        foreach (NorAir.MBx24 MB in MBx24s) // Iterate listed MBx24es 
                        {

                            bool en = CPORT.Enumerate(MB.enumerationData); // Send enumeration data
                            if (en)
                            {
                                ShowStat("Enumerated " + MB.SerialNumber.ToString());
                            }
                            else
                            {
                                ShowStat("Error: " + MB.SerialNumber.ToString() + " was not enumerated");
                                Thread.Sleep(1000); // Some time for user read the status line
                            }

                        }
                    }
                    if (PeripheralsPresent.Count != 0)
                    {
                        InRealTime = true; // If there's peripherals, lets go alive. This will be used by config threads and backlit setup

                        FillInPeripherals();
                    }

                    for (int i = 0; i != 200; i++)  // Reset joystick buttons panel
                    {
                        if (Connectors[i] != null)
                        {
                            JPORTButtons[i] = 0;
                            Connectors[i].BackColor = backColor;
                            Connectors[i].ForeColor = frontColor;
                        }
                    }

                    stop = lastStop;

                    ShowStat("Ready");
                }
                else        // No OBCS found. Nevertheless, TOGA can run for setups and stuff
                {
                    ShowStat("OBCS not found");
                    InRealTime = false; // There's not peripherals to work with while setting up these.
                }
            }
        }

        private void XPlaneSetButtons(bool stat)  // Change the status of X-Plane connect button
        {
            if (CPORT.Found)
            {
                XPConnect.Enabled = stat;
            }
        }


        public bool ReadRegisteredPeripherals()
        {
            /// <summary>
            /// This method reads the main file of peripherals.
            /// It reads and parses the serials and id numbers
            /// and stores them in meaningful lists.
            /// </summary>

            bool returnValue = false;

            PeripheralProperty PP = new PeripheralProperty();

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            NoRegisteredPeripherals.Visible = true;
            XPlaneSetupButton.Enabled = false;

            // If file exists, read data. This file is created by adding a peripheral
            if (File.Exists(PeripheralsFileName))
            {

                PeripheralsPresent.Clear();
                XPlaneSerialList.Items.Clear();
                MSFSSerialList.Items.Clear();
                this.RegisteredPeripherals.Items.Clear();
                // Open the file and read it back.

                string lines = File.ReadAllText(PeripheralsFileName);    // Read all lines at once

                lines = verifyBrackects(lines);

                try
                {
                    PeripheralsPresent = JsonConvert.DeserializeObject<List<PeripheralProperty>>(lines);  // Deserialize 'lines'
                }
                catch
                {
                    returnValue = false;
                }

                foreach (PeripheralProperty p in PeripheralsPresent)
                {
                    this.RegisteredPeripherals.Items.Add(p.SERIALNUMBER + "," + p.DESCRIPTION);
                    XPlaneSerialList.Items.Add(p.SERIALNUMBER + "," + p.DESCRIPTION);
                    MSFSSerialList.Items.Add(p.SERIALNUMBER + "," + p.DESCRIPTION);
                    returnValue = true;
                }

            }

            if (returnValue == true)
            {
                NoRegisteredPeripherals.Visible = false;
                XPlaneSetupButton.Enabled = true;
                MSFSSetupButton.Enabled = true;
            }

            return returnValue;
        }


        public void Say(string s)
        {
            /// <summary>
            /// Show messages in a messagebox
            /// </summary>
            MessageBox.Show(s, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void ShowStat(string s)
        {
            /// <summary>
            /// Updates the status bar
            /// </summary>
            statLabel.Text = s;
        }
        private void Find_OBCS()
        {
            /// <summary>
            /// This method calls a hardware search engine to find and OBCS.
            /// This is the proper way to find an OBCS rather then try'n'fail
            /// or other methodologies.
            /// </summary>

            ShowStat("Scanning ports for OBCS...");
            CPORT.FindOBCS();    // A string is returned. See documentation for its meaning

            ShowStat("Not found. Ready");    // default label
            if (CPORT.Found)
            {                       // Open the correspondent port and set labels/buttons
                CPORT.Open();
                ShowStat("Found OBCS (" + CPORT.CPortName + ")");
                OBCSport.Text = "OBCS port: " + CPORT.CPortName;
                findOBCS.Visible = false;
            }
            else
            {
                OBCSport.Text = ""; // OBCS port: None";
            }
        }

        #region MSFS region
        /* ===================================== MS Flight Sim =====================================
         * 
         * The MSFS section deploys methods to interact with MSFS SIM and NorAirLabs hardware
         * 
         * It uses MSFS Setup form as well as hardware dependencies forms.
         * 
         * Here, one can configure the relations with input/output/joystick buttons and MSFS software
         * 
         * 
         * */



        private void Simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {

            /* =============== Conversion from SimConnect to C# data types =================
             * 
             * C# type 	            SIMCONNECT_DATATYPE
             * 
             * bool                     INT32
             * int                      INT32
             * long                     INT64
             * float                    FLOAT32
             * double                   FLOAT64
             * string                   STRING8, STRING32, STRING64, STRING128, STRING256
             *
             */

            // Register data definitions. Must have the same number and order of data definitions of the simvars structure.

            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT32, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "KOHLSMAN SETTING HG", "inHG", SIMCONNECT_DATATYPE.FLOAT32, 0, SimConnect.SIMCONNECT_UNUSED);

            if (simconnect != null) // A previous session may had set data definitions. Delete them
            {
                simconnect.ClearClientDataDefinition(DefineID.ID0);
                simconnect.ClearClientDataDefinition(DefineID.ID1);
            }

            MSFSSetup msfs = new MSFSSetup();
            msfs.currentFileName = MSFSProfilesPath + @"\" + MSFSProfileSet + @"\" + Radios[0].SerialNumber.Trim() + ".json";
            RNSMSFSDATA rnsmsfsdata = msfs.MSFSRNSLoadData(Radios[0].SerialNumber, Radios[0].Id);
            msfs.currentFileName = MSFSProfilesPath + @"\" + MSFSProfileSet + @"\" + MBx24s[0].SerialNumber.Trim() + ".json";
            MBx24MSFSDATA mbx24msfsdata = msfs.MBx24LoadData(MBx24s[0].SerialNumber, MBx24s[0].Id);

            // Setup data definitions for events that updates MSFS instruments with NorAirLabs hardware data

            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.COMMACTTOREAD.Name, rnsmsfsdata.COMMACTTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.COMMSTBTOREAD.Name, rnsmsfsdata.COMMSTBTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.NAVACTTOREAD.Name, rnsmsfsdata.NAVACTTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.NAVSTBTOREAD.Name, rnsmsfsdata.NAVSTBTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.ADFTOREAD.Name, rnsmsfsdata.ADFTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DefineID.ID0, rnsmsfsdata.XPDRTOREAD.Name, rnsmsfsdata.XPDRTOREAD.Units, Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);

            for (int i = 0; i != 32; i++)
            {
                SIMVARsConnectors[i] = (byte)255;
                MSFSConnectorsLastState[i] = (byte)0;
            }

            int ReceivedDataIndex = 0;
            // Set each field
            foreach (var r in mbx24msfsdata.REFERENCES)
            {
                string svtr = r.SIMVARTOREAD;

                if (svtr != null)
                {
                    svtr = svtr.Trim();

                    if (svtr.Length != 0)
                    {
                        simconnect.AddToDataDefinition(DefineID.ID1, svtr, "Boolean", Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
                        SIMVARsConnectors[ReceivedDataIndex] = r.CONNECTOR;
                        ReceivedDataIndex++;
                    }
                }
            }

            //simconnect.AddToDataDefinition(RequestType.MBx24FrameData, "PITOT HEAT", "Boolean", Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);


            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "LIGHT BEACON", "Boolean", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "PITOT HEAT", "Boolean", dt, 0, SimConnect.SIMCONNECT_UNUSED);

            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "COM ACTIVE FREQUENCY:1", "KHz", dt , 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "COM STANDBY FREQUENCY:1", "KHz", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "NAV ACTIVE FREQUENCY:1", "KHz", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "NAV STANDBY FREQUENCY:1", "KHz", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "ADF ACTIVE FREQUENCY:1", "KHz",   dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "TRANSPONDER CODE:1", "Hz", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "LIGHT BEACON", "Boolean", dt, 0, SimConnect.SIMCONNECT_UNUSED);
            //simconnect.AddToDataDefinition(RequestType.PerFrameData, "PITOT HEAT", "Boolean", dt, 0, SimConnect.SIMCONNECT_UNUSED);

            // Register the SimVar struct with the PerFrameData enum
            simconnect.RegisterDataDefineStruct<RNSReceivedSimVars>(DefineID.ID0);
            simconnect.RegisterDataDefineStruct<MBx24ReceivedSimVars>(DefineID.ID1);

            // Request data from MSFS
            simconnect.RequestDataOnSimObject(RequestType.RNSFrameData, DefineID.ID0, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
            simconnect.RequestDataOnSimObject(RequestType.MBx24FrameData, DefineID.ID1, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

            // Register events for MSFS
            // simconnect.MapClientEventToSimEvent(SimEventType.COM_RADIO_SET_HZ, "COM_RADIO_SET_HZ");
            // simconnect.MapClientEventToSimEvent(SimEventType.COM_RADIO_SET_HZ, SimEventType.COM_RADIO_SET_HZ.ToString());
            // simconnect.MapClientEventToSimEvent(SimEventType.KOHLSMAN_INC, "KOHLSMAN_INC");
            // simconnect.MapClientEventToSimEvent(SimEventType.KOHLSMAN_DEC, "KOHLSMAN_DEC");
        }


        private void Simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            ///<summary>
            /// When MSFS exits, this performs closing actions if needed
            ///</summary>
            ///

            stop = true;
            return;
        }


        private void Simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            ///<summary>
            /// Executes if an exception is thrown
            ///</summary>
            ///

            Debug.WriteLine($"Exception received: {data}");
        }

        private void Simconnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            /// <summary>
            /// Event to receive simvars data
            /// </summary>
            /// On new data arrival from MSFS, this method evaluates and processes it.
            /// Essentially, updates peripherals with received data
            /// 

            //if (MSFSInteractions >= MSFSMaxInteractions)
            if (!stop)
            {
                MSFSInteractions = 0;

                switch ((RequestType)data.dwRequestID)
                {
                    case RequestType.RNSFrameData:

                        simvars = (RNSReceivedSimVars)data.dwData[0];  // Cast from simconnect object to struct "simvars"

                        // Example for other field formats
                        //Debug.Write($"Altitude: {simvars.Altitude:0.00}");
                        //Debug.Write($", Barometro: {simvars.KohlsmanSettingHg:0.00}");

                        string caf = simvars.Com1ActFreq.ToString();
                        string csf = simvars.Com1StbFreq.ToString();

                        string na1 = simvars.Nav1ActFreq.ToString();
                        string ns1 = simvars.Nav1StbFreq.ToString();
                        string adf1 = simvars.ADF1ActFreq.ToString();
                        string xpdr = simvars.Transponder.ToString();

                        // To debug data
                        //Debug.Write($" - COM1: {caf}, {csf}");
                        //Debug.WriteLine($", NAV1: {na1}, {ns1}, ADF: {adf1}, XPDR: {xpdr}");
                        break;

                    case RequestType.MBx24FrameData:

                        Int64[] ReceivedData = new Int64[32];

                        MBx24ReceivedSimVars mbx24simvars = (MBx24ReceivedSimVars)data.dwData[0]; // Cast from SimConnect object to mbx24simvars

                        StructureToArray<MBx24ReceivedSimVars, Int64>(mbx24simvars, ref ReceivedData); // Convert to an array for easier handle

                        // Itinerate all 32 connectors
                        for (int i = 0; i != 32; i++)
                        {
                            if (SIMVARsConnectors[i] != (byte)255)    // If no connector is attributed, skip
                            {
                                if (MSFSStartInteractions)  // If it is the first interaction, update connectors
                                {                           // and respective last state to current state
                                    if (ReceivedData[i] != 0)   // If ON is received, set last state to ON
                                    {
                                        CPORT.Send(MBx24s[0].SetOutput(SIMVARsConnectors[i], MBx24s[0].ON));
                                        MSFSConnectorsLastState[i] = (byte)1;   // Update last state
                                    }
                                    else
                                    {
                                        CPORT.Send(MBx24s[0].SetOutput(SIMVARsConnectors[i], MBx24s[0].OFF));
                                    }
                                }
                                else
                                {
                                    if (MSFSConnectorsLastState[i] == (byte)0)  // If connector was turned OFF
                                    {
                                        if (ReceivedData[i] != 0)   // If ON is received, turn connector ON
                                        {
                                            CPORT.Send(MBx24s[0].SetOutput(SIMVARsConnectors[i], MBx24s[0].ON));

                                            MSFSConnectorsLastState[i] = (byte)1;   // Update last state
                                        }
                                    }
                                    else                                        // If connector was turned ON
                                    {
                                        if (ReceivedData[i] == 0)   // If OFF is received, turn connector OFF
                                        {
                                            CPORT.Send(MBx24s[0].SetOutput(SIMVARsConnectors[i], MBx24s[0].OFF));

                                            MSFSConnectorsLastState[i] = (byte)0;    // Update last state
                                        }
                                    }
                                }
                            }
                        }

                        MSFSStartInteractions = false;

                        break;

                    default:
                        break;
                }
            }
            //MSFSInteractions++;
        }

        public void StructureToArray<S, E>(S rStruct, ref E[] aArray) where S : struct where E : struct
        {
            GCHandle iHandle;
            try
            {
                iHandle = GCHandle.Alloc(aArray, GCHandleType.Pinned);
                Marshal.StructureToPtr<S>(rStruct, iHandle.AddrOfPinnedObject(), false);
            }
            finally
            {
                //iHandle.Free();
            }

        }

        private void MSFSConnect_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Event to connect to MSFS and start sending/receiving threads
            /// </summary>
            /// This tries to establish a valid connection with MSFS. It must
            /// but running and already in flight ( not at intro screen ).
            /// 
            /// After MSFS process is found, a SimConnect connections is created
            /// and communication is started using three threads, one to receive,
            /// one to send data and another for ghostkeys processing. ????
            /// 
            /// A flag is kept to monitor and instruct the threads when to stop and
            /// return.
            /// 
            /// Proper threads disposal should be made in order to keep the software sanity.
            /// 

            // If connection is already running

            if (MSFSConnect.Text == "Disconnect")
            {
                stop = true;                    // Issue a stop condition to threads

                try
                {
                    if (simconnect != null) // Delete session data definitions for future sessions.
                    {
                        // Dispose MSFS events
                        simconnect.OnRecvOpen -= new SimConnect.RecvOpenEventHandler(Simconnect_OnRecvOpen);
                        simconnect.OnRecvQuit -= new SimConnect.RecvQuitEventHandler(Simconnect_OnRecvQuit);
                        simconnect.OnRecvException -= new SimConnect.RecvExceptionEventHandler(Simconnect_OnRecvException);
                        simconnect.OnRecvSimobjectData -= new SimConnect.RecvSimobjectDataEventHandler(Simconnect_OnRecvSimobjectData);

                        // Delete session data definitions for future sessions.
                        simconnect.ClearClientDataDefinition(DefineID.ID0); // RNS data definition
                        simconnect.ClearClientDataDefinition(DefineID.ID1); // MBx24 data definition
                    }
                }
                catch
                {

                }

                Thread.Sleep(500);              // Some time to complete threads tasks

                // Restore form start conditions
                MSFSConnect.Text = "Connect";
                MSFSConnect.BackColor = buttonColor;
                ShowStat("Microsoft Flight Simulator disconnected");

                // Enable some buttons
                ExitButton.Enabled = true;         // Enable 'Exit'
                MSFSEditLinkedHardwareButton.Enabled = true;
                MSFSDeleteLinkedHardwareButton.Enabled = true;
                MSFSNewProfile.Enabled = true;
                MSFSProfile.Enabled = true; // MSFS profile button
                PeripheralsSetupButton.Enabled = true;
                BackLitSetupButton.Enabled = true;
                XPlaneSetupButton.Enabled = true;   // X-Plane button
                AeroWinxSetupButton.Enabled = false; // AeroWinx button
                MSFSSetupButton.Enabled = true; // MSFS button
                OptionsButton.Enabled = true;
                return;
            }

            bool MSFSFound = false;
            stop = false;

            // Proceed on trying to start a connection
            ShowStat("Looking for Microsoft Flight Simulator...");

            // Look for a MSFS session. If not found, don't loose time. Just return.
            Process p = Process.GetProcessesByName(MSFS_PROCESS_NAME).FirstOrDefault();

            if (p == null)  // If something went wrong
            {               // just return without stoping all threads
                ShowStat("Start a Microsoft Flight Simulador session first");
                Say("Microsoft Flight Simulador not found.\nStart a Microsoft Flight Simulador session first");
                return;
            }

            try
            {
                ShowStat("Microsoft Flight Simulator found. Setting up connection...");
                var handle = Process.GetCurrentProcess().MainWindowHandle;

                simconnect = new SimConnect(PLUGIN_NAME, handle, WM_USER_SIMCONNECT, null, 0);

                // register a callback for each handler:
                // called when you connect to the sim for the first time
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(Simconnect_OnRecvOpen);
                // called when the sim exits
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(Simconnect_OnRecvQuit);
                // called when there's an exception when sending us the data
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(Simconnect_OnRecvException);
                //// called every time we are sent new simvar data
                simconnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(Simconnect_OnRecvSimobjectData);

                //simconnect.MapClientEventToSimEvent(SimEventType.COM_RADIO_SET_HZ, SimEventType.COM_RADIO_SET_HZ.ToString());
                MSFSStartInteractions = true;
                MSFSFound = true;
                // start out DispatcherTimer to start polling from the game
                //simConnectDispatcherTimer.Start();

                //// toggle the connect/disconnect buttons
                //connectButton.IsEnabled = false;
                //disconnectButton.IsEnabled = true;
                ShowStat("Setting up profiled peripherals...");
            }
            catch
            {
                ShowStat("Could not establish connection to a Microsoft Flight Simulador session");
                Say("Microsoft Flight Simulador not found.\nStart a Microsoft Flight Simulador session first");
                MSFSFound = false;
            }

            if (!MSFSFound)
            {
                return;
            }

            // Connect to C-Port if not yet connected
            if (!CPORT.Opened)
            {
                CPORT.Open();
            }

            // Get the currently selected profile
            string prof = MSFSProfile.Text;

            foreach (PeripheralProperty pp in PeripheralsPresent)
            {
                if (pp != null)
                {
                    if (pp.SERIALNUMBER.StartsWith("31"))
                    {
                        // Example on how to check if a peripheral is connected physically.
                        // Temporarily instantiate the peripheral and Ping() it.
                        // If connected, it'll return its serial number.
                        // If not, it will return the error code '255' '31' '30' or null

                        RNS rns = new RNS(pp.SERIALNUMBER, (byte)pp.ID);

                        byte[] answer = CPORT.Send(rns.Ping());

                        if (answer != null)
                        {
                            if (answer[0] < 250)// if no error detected
                            {
                                RNSPresent = true;

                                CPORT.Send(rns.SoftReset()); // Soft reset RNS hardware

                                // Setup RNS hardware before MSFS defs.
                                // The backlit color, as profile dependent, will be set by one of the threads that
                                // already handles the profile.

                                ConfigRNS(Radios[0].SerialNumber, Radios[0].Id);
                            }
                        }
                    }
                    if (pp.SERIALNUMBER.StartsWith("50"))
                    {
                        // Example on how to check if a peripheral is connected physically.
                        // Temporarily instantiate the peripheral and Ping() it.
                        // If connected, it'll return its serial number.
                        // If not, it will return the error code '255' '50' '30'

                        MBx24 mbx24 = new MBx24(pp.SERIALNUMBER, (byte)pp.ID);

                        byte[] answer = CPORT.Send(mbx24.Ping());

                        if (answer != null)
                        {
                            if (answer[0] < 250) // if no error detected
                            {
                                MBx24Present = true;

                                CPORT.Send(mbx24.SoftReset()); //Soft reset MBx24 hardware

                                // SetupMBx24 hardware before MSFS defs.
                                // The backlit color, as profile dependent, will be set by one of the threads that
                                // already handles the profile.

                                ConfigMBx24(MBx24s[0].SerialNumber, MBx24s[0].Id);
                            }
                        }
                    }
                }
            }

            ShowStat("Done");

            // Form's buttons setup
            ExitButton.Enabled = false; // Disable exit without proper thread completition
            MSFSEditLinkedHardwareButton.Enabled = false;
            MSFSDeleteLinkedHardwareButton.Enabled = false;
            MSFSNewProfile.Enabled = false;
            MSFSProfile.Enabled = false;
            PeripheralsSetupButton.Enabled = false;
            BackLitSetupButton.Enabled = false;
            AeroWinxSetupButton.Enabled = false;
            MSFSSetupButton.Enabled = false;
            OptionsButton.Enabled = false;
            XPlaneSetupButton.Enabled = false;

            // Setup form - Button toggle description & color
            MSFSConnect.Text = "Disconnect";
            MSFSConnect.BackColor = Color.FromArgb(255, 0, 0);

            // Thread-safe profile name to use
            MSFSProfileSet = MSFSProfile.Text;

            // stop condition flag
            stop = false;

            // Instantiate write thread
            Thread MSFSThread = new Thread(() => SendToMSFS(simconnect, prof));
            MSFSThread.IsBackground = true;
            MSFSThread.Start();

            //// Instantiate read thread
            Thread MSFSReceive = new Thread(() => ReceiveMSFS(prof));
            MSFSReceive.IsBackground = true;

            //// Instantiate read thread
            //Thread MSFSGhostKeying = new Thread(() => MSFSGhostKeys( prof));
            //MSFSGhostKeying.IsBackground = true;

            MSFSReceivingThreadIsRunning = true;
            MSFSReceive.Start();


        }

        public void SendToMSFS(SimConnect simConnect, string prof)
        {
            /// <summary>
            /// Thread to send data to MSFS
            /// </summary>
            /// Processing data in threads allows the main thread
            /// to continue executing important actions as having
            /// a "Disconnect button", despite data being handled
            /// by events.
            /// Meanful data is sent here to MSFS as peripherals
            /// are played with, like a changed RNS frequency.
            /// 
            /// As only RNS has non-boolean data to send, a MBx24
            /// is not handled here.


            if (!RNSPresent)
            {
                return;
            }

            MSFSSendingThreadIsRunning = true;

            // RNS working vars

            // Old frequency/squawk values for comparison
            int lastnavstb = 0;
            int lastnavact = 0;
            int lastcomstb = 0;
            int lastcomact = 0;
            int lastadf = 0;
            int lastxpdr = 0;

            // Returning byte arrays
            byte[] FreqReport = new byte[32];
            byte[] squawk = new byte[32];

            int msWaitTime = 100;

            string folder = MSFSProfilesPath + @"\" + prof + @"\";  // Profile folder
            MSFSSetup msfsSetup = new MSFSSetup();                  // Instatiate MSFSSetup to use its methods & properties
            msfsSetup.currentFileName = folder + Radios[0].SerialNumber.Trim() + ".json"; // Set the MSFSSetup "currentFileName" property
            RNSMSFSDATA rnsmsfsdata = new RNSMSFSDATA();    // Create a RNSMSFSDATA instance
            rnsmsfsdata = msfsSetup.MSFSRNSLoadData(Radios[0].SerialNumber.Trim(), (int)Radios[0].Id); // Load RNS data

            // Set RNS backlit color
            SetBacklitColor(rnsmsfsdata.SERIAL, rnsmsfsdata.ID, rnsmsfsdata.BACKLITCOLOR);

            // Retrieve initial data from RNS
            FreqReport = CPORT.Send(Radios[0].FreqReport());    // Get frequencies
            squawk = CPORT.Send(Radios[0].GetSquawk());     // Get squawk code

            int navact = (int)Extract(FreqReport, RNS.Instruments.ActiveNAV);
            int navstb = (int)Extract(FreqReport, RNS.Instruments.StandByNAV);
            int COMMact = (int)Extract(FreqReport, RNS.Instruments.ActiveCOMM);
            int COMMstb = (int)Extract(FreqReport, RNS.Instruments.StandByCOMM);
            int adf = (int)Extract(FreqReport, RNS.Instruments.ADF);
            int squawkCode = (int)ExtractSquawk(squawk);

            simconnect.MapClientEventToSimEvent(rnsmsfsdata.COMMACTTOWRITE.EventIdType, rnsmsfsdata.COMMACTTOWRITE.Name);
            simconnect.MapClientEventToSimEvent(rnsmsfsdata.COMMSTBTOWRITE.EventIdType, rnsmsfsdata.COMMSTBTOWRITE.Name);
            simconnect.MapClientEventToSimEvent(rnsmsfsdata.NAVACTTOWRITE.EventIdType, rnsmsfsdata.NAVACTTOWRITE.Name);
            simconnect.MapClientEventToSimEvent(rnsmsfsdata.NAVSTBTOWRITE.EventIdType, rnsmsfsdata.NAVSTBTOWRITE.Name);
            simconnect.MapClientEventToSimEvent(rnsmsfsdata.ADFTOWRITE.EventIdType, rnsmsfsdata.ADFTOWRITE.Name);
            simconnect.MapClientEventToSimEvent(rnsmsfsdata.XPDRTOWRITE.EventIdType, rnsmsfsdata.XPDRTOWRITE.Name);

            while (!stop)
            {
                FreqReport = CPORT.Send(Radios[0].FreqReport());    // Get frequencies
                squawk = CPORT.Send(Radios[0].GetSquawk());     // Get squawk code

                navact = (int)Extract(FreqReport, RNS.Instruments.ActiveNAV);
                navstb = (int)Extract(FreqReport, RNS.Instruments.StandByNAV);
                COMMact = (int)Extract(FreqReport, RNS.Instruments.ActiveCOMM);
                COMMstb = (int)Extract(FreqReport, RNS.Instruments.StandByCOMM);
                adf = (int)Extract(FreqReport, RNS.Instruments.ADF);
                squawkCode = (int)ExtractSquawk(squawk);

                if (lastcomact != COMMact)
                {
                    lastcomact = COMMact;
                    if (rnsmsfsdata.COMMACTTOWRITE.Name.Trim().Length != 0)
                    {
                        uint value = (uint)(COMMact * rnsmsfsdata.COMMACTTOWRITE.Multiplier);
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.COMMACTTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }

                if (lastcomstb != COMMstb)
                {
                    lastcomstb = COMMstb;
                    if (rnsmsfsdata.COMMSTBTOWRITE.Name.Trim().Length != 0)
                    {
                        uint value = (uint)(COMMstb * rnsmsfsdata.COMMSTBTOWRITE.Multiplier);
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.COMMSTBTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }

                if (lastnavact != navact)
                {
                    lastnavact = navact;
                    if (rnsmsfsdata.NAVACTTOWRITE.Name.Trim().Length != 0)
                    {
                        uint value = (uint)(navact * rnsmsfsdata.NAVACTTOWRITE.Multiplier);
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.NAVACTTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }

                if (lastnavstb != navstb)
                {
                    lastnavstb = navstb;
                    if (rnsmsfsdata.NAVSTBTOWRITE.Name.Trim().Length != 0)
                    {
                        uint value = (uint)(navstb * rnsmsfsdata.NAVSTBTOWRITE.Multiplier);
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.NAVSTBTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }
                if (lastadf != adf)
                {
                    lastadf = adf;
                    if (rnsmsfsdata.ADFTOWRITE.Name.Trim().Length != 0)
                    {
                        uint value = Dec2Bcd((uint)(adf * rnsmsfsdata.ADFTOWRITE.Multiplier));
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.ADFTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }
                if (lastxpdr != squawkCode && squawkCode != -1)
                {
                    lastxpdr = squawkCode;
                    if (rnsmsfsdata.XPDRTOWRITE.Name.Trim().Length != 0)
                    {

                        uint value = Dec2Bcd((uint)squawkCode);
                        simconnect.TransmitClientEvent_EX1(SimConnect.SIMCONNECT_OBJECT_ID_USER, rnsmsfsdata.XPDRTOWRITE.EventIdType, SimPriorities.SIMCONNECT_GROUP_PRIORITY_DEFAULT, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, value, 0, 0, 0, 0);
                        continue;
                    }
                }
                Thread.Sleep(msWaitTime);
            }
            MSFSSendingThreadIsRunning = false;
        }

        public void ReceiveMSFS(string prof)
        {
            /// <summary>
            /// Thread to receive data from MSFS
            /// </summary>
            /// This thread allows main thread to continue reading buttons.
            /// It is important to allow this to have a "Disconnect" button
            /// working while connected to MSFS session.
            /// Every task is handled by simconnect created events.
            /// The exit depends only on a "stop" flag.
            /// A delay is used to avoid processing saturation.

            const int msWaitTime = 100;

            while (!stop)
            {
                try
                {
                    simconnect?.ReceiveMessage();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + ": " + e.Source);
                    // Just let thread run and try again
                }
                Thread.Sleep(msWaitTime);
            }

            MSFSReceivingThreadIsRunning = false;
        }

        public void MSFSGhostKeys(string IP, int port, string prof)
        {

        }

        public static uint Dec2Bcd(uint num)
        {
            return HornerMethod(num, 10, 0x10);
        }

        public static uint Bcd2Dec(uint num)
        {
            return HornerMethod(num, 0x10, 10);
        }

        static private uint HornerMethod(uint Num, uint Divider, uint Factor)
        {
            uint Remainder = 0, Quotient = 0, Result = 0;
            Remainder = Num % Divider;
            Quotient = Num / Divider;
            if (!(Quotient == 0 && Remainder == 0))
            {
                Result += HornerMethod(Quotient, Divider, Factor) * Factor + Remainder;
            }
            return Result;
        }

        private void MSFSNewProfile_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Creates a new profile
            /// </summary>

            MSFSAddProfile dialogForm = new MSFSAddProfile();

            DialogResult d = dialogForm.Show();

            if (d == DialogResult.OK)
            {
                MSFSFillProfiles();
            }
        }

        private void MSFSSetButtons(bool stat)  // Change the status of some form buttons
        {
            if (CPORT.Found)
            {
                MSFSConnect.Enabled = stat;
            }
        }

        public void MSFSFillProfiles()
        {
            /// <summary>
            /// Fills the profiles selection box and, if defined, sets the default profile
            /// </summary>

            MSFSProfile.Items.Clear();
            try
            {
                MSFSDefaultProfileCheckBox.Checked = false;
                string defprof = "";
                string[] profiles = Directory.GetDirectories(MSFSProfilesPath, @".");
                if (profiles.Length != 0)
                {
                    defprof = MSFSReadDefaultProfile(); // Get default profile

                    MSFSNoProfilesLabel.Visible = false; // Hide helper

                    foreach (string profile in profiles)
                    {
                        string subdir = profile.Substring(MSFSProfilesPath.Length + 1);
                        MSFSProfile.Items.Add(subdir);

                        if (defprof.Length != 0)
                        {
                            if (defprof.Trim().Equals(subdir.Trim()))
                            {
                                MSFSProfile.SelectedIndex = MSFSProfile.Items.Count - 1; // Set this profile as the default one
                                MSFSDefaultProfileCheckBox.Checked = true;
                            }
                        }
                    }

                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowStat("There was a problem retrieving profiles ( Unauthorized Access ). Check folders permitions");
            }
        }

        public string MSFSReadDefaultProfile()
        {
            /// <summary>
            /// Reads the default MSFS profile to use
            /// </summary>

            string defprofile = "";
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            if (File.Exists(MSFSDefaultProfile))
            {
                try
                {
                    // Read the file 

                    string lineRead = File.ReadAllText(MSFSDefaultProfile);

                    defprofile = lineRead.Substring(1, lineRead.Length - 2);
                }
                catch
                {

                }
            }
            Debug.WriteLine(defprofile);
            return defprofile;
        }

        private void MSFSSaveDefaultProfile()
        {
            /// <summary>
            /// Saves the default profile to use
            /// </summary>


            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            MSFSRemoveDefaultProfile();

            string line = JsonConvert.SerializeObject(MSFSProfileName.Text.Trim(), settings);

            File.WriteAllText(MSFSDefaultProfile, line);

        }

        private void MSFSRemoveDefaultProfile()
        {
            /// <summary>
            /// Deletes the default profile file when default is unset
            /// </summary>


            if (File.Exists(MSFSDefaultProfile))
            {
                File.Delete(MSFSDefaultProfile);
            }
        }

        private void MSFSDefaultProfileCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            /// <summary>
            /// Sets or removes the default profile when default check option changes
            /// </summary>

            if (MSFSDefaultProfileCheckBox.Checked)
            {
                MSFSSaveDefaultProfile();
            }

        }

        private void MSFSProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            /// <summary>
            /// Handles the selected profile
            /// </summary>

            int index = MSFSProfile.SelectedIndex;

            if (index == -1)
            {
                return;
            }

            string Profile = MSFSProfile.Text;

            if (MSFSFillHardware(Profile))
            {
                MSFSProfilePanel.Visible = true;
                MSFSProfileName.Text = Profile;
                MSFSSetButtons(true);
                string defprof = MSFSReadDefaultProfile();
                if (MSFSReadDefaultProfile().Trim().Equals(Profile))
                {
                    MSFSDefaultProfileCheckBox.Checked = true;
                }
                else
                {
                    MSFSDefaultProfileCheckBox.Checked = false;
                }
            }
        }


        public bool MSFSFillHardware(string profile)
        {
            /// <summary>
            /// Fills the hardware list box by hardware type
            /// </summary>

            MSFSProfileHardwareList.Items.Clear();
            MSFSNoProfilePeripherals.Visible = true;
            try
            {
                string profPath = MSFSProfilesPath + @"\" + profile;
                string[] profiles = Directory.GetFiles(profPath, @"*.json");
                if (profiles.Length != 0)
                {
                    MSFSNoProfilePeripherals.Visible = false;
                    foreach (string prof in profiles)
                    {
                        string serial;
                        string desc;
                        int hardwareType;
                        string fileName = prof.Substring(profPath.Length + 1);

                        string lines = File.ReadAllText(prof);    // Read all lines at once

                        lines = verifyBrackects(lines);

                        bool valid = int.TryParse(fileName.Substring(0, 2), out hardwareType);

                        if (valid)
                        {
                            switch (hardwareType)
                            {
                                case 31:
                                    MSFSSetup.RNSMSFSDATA rns = new MSFSSetup.RNSMSFSDATA();
                                    rns = JsonConvert.DeserializeObject<MSFSSetup.RNSMSFSDATA>(lines);
                                    serial = rns.SERIAL;
                                    desc = rns.DESCRIPTION;// ProfileName.Text;
                                    MSFSProfileHardwareList.Items.Add(serial.Trim() + ", " + desc);
                                    break;
                                case 50:
                                    MSFSSetup.MBx24MSFSDATA mbx24 = new MSFSSetup.MBx24MSFSDATA();
                                    mbx24 = JsonConvert.DeserializeObject<MSFSSetup.MBx24MSFSDATA>(lines);
                                    serial = mbx24.SERIAL;
                                    desc = mbx24.DESCRIPTION;// ProfileName.Text;
                                    MSFSProfileHardwareList.Items.Add(serial.Trim() + ", " + desc);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowStat("There was a problem retrieving profiles ( Unauthorized Access ). Check folders permitions");
                return false;
            }

            return true;
        }
        private void MSFSAddPeripheralToList_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Adds a peripheral to the current profile
            /// </summary>

            if (MSFSSerialList.SelectedIndex != -1)
            {
                string serial = MSFSSerialList.GetItemText(MSFSSerialList.SelectedItem);
                if (MSFSProfileHardwareList.FindString(serial.Substring(0, 10)) == -1)
                {
                    MSFSProfileHardwareList.Items.Add(serial);
                    MSFSNoProfilePeripherals.Visible = false;
                }
                else
                {
                    ShowStat("Not possible to add peripheral. Already in Profile peripherals list");
                }
            }
        }
        private void MSFSEditLinkedHardwareButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Edits profile linked hardware
            /// </summary>

            if (MSFSProfileHardwareList.SelectedIndices.Count != 0)
            {
                string HardwareSelected = MSFSProfileHardwareList.Text;

                if (HardwareSelected.Trim().Length != 0)
                {
                    string serial = HardwareSelected.Substring(0, 10);

                    PeripheralProperty pp = new PeripheralProperty();
                    pp = PeripheralsPresent.Find(pp => pp.SERIALNUMBER.Trim() == serial.Trim());

                    if (pp != null)
                    {

                        if (pp.SERIALNUMBER != null)
                        {
                            if (pp.SERIALNUMBER.Length == 0)
                            {
                                return;
                            }

                            string fileName = MSFSProfilesPath + @"\" + MSFSProfileName.Text + @"\" + pp.SERIALNUMBER + ".json";
                            MSFSSetup dialogForm = new MSFSSetup();
                            DialogResult d = dialogForm.Show(pp.SERIALNUMBER, pp.ID, backColor, frontColor, fileName, ProfileName.Text, MSFSProfileHardwareList.Text);
                            MSFSFillHardware(MSFSProfileName.Text);
                        }
                    }
                }
            }
        }
        private void MSFSProfilePeripheralSelected(object sender, EventArgs e)
        {
            MSFSEditLinkedHardwareButton.Enabled = true;
            MSFSDeleteLinkedHardwareButton.Enabled = true;
        }

        private void MSFSDeleteLinkedHardwareButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Delete linked hardware from profile
            /// </summary>

            if (MSFSProfileHardwareList.SelectedIndices.Count != 0)
            {
                string HardwareSelected = MSFSProfileHardwareList.Text;

                if (HardwareSelected.Trim().Length != 0)
                {
                    string serial = HardwareSelected.Substring(0, 10);
                    string fileName = MSFSProfilesPath + @"\" + MSFSProfileName.Text + @"\" + serial + ".json";
                    DialogResult delete = MessageBox.Show("About to delete\n" + fileName +
                        "\n\nProceed?", "Delete profile hardware file",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                    if (delete == DialogResult.Yes)
                    {
                        File.Delete(fileName);
                        MSFSFillHardware(MSFSProfileName.Text);
                    }
                }
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

        #endregion

        #region X-Plane region

        /* ====================================== X-Plane ==========================================
         *
         * The X-Plane section has methods to interact with the simulator and NorAirLabs hardware
         * 
         * It uses XPlaneSetup form as well as hardware dependencies forms.
         * 
         * Here, one can configure the relations with inputs/outputs/joystick buttons and X-Plane
         * software.
         * 
         *
         */

        public void XPConnect_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Event to connect to X-Plane and start sending/receiving threads
            /// </summary>
            /// This tries to establish a valid connection with X-Plane. It must
            /// but running and already in flight ( not at intro screen ).
            /// 
            /// After X-Plane process is found, socket connections are created
            /// and communication is started using three threads, one to receive,
            /// one to send data and another for ghostkeys processing.
            /// 
            /// A flag is kept to monitor and instruct the threads when to stop and
            /// return.
            /// 
            /// Proper threads disposal should be made in order to keep the software sanity.
            /// 


            // If connection is already running

            if (XPConnect.Text == "Disconnect")
            {
                stop = true;                    // Issue a stop condition

                Thread.Sleep(500);              // Some time to complete threads tasks

                XPConnect.Text = "Connect";          // Restore form start conditions
                XPConnect.BackColor = buttonColor;
                ShowStat("XPlane disconnected");

                // Enable some buttons
                ExitButton.Enabled = true;         // Enable 'Exit'
                EditLinkedHardwareButton.Enabled = true;
                DeleteLinkedHardwareButton.Enabled = true;
                NewProfile.Enabled = true;
                XPlaneProfile.Enabled = true; // X-Plane button
                PeripheralsSetupButton.Enabled = true;
                BackLitSetupButton.Enabled = true;
                AeroWinxSetupButton.Enabled = false; // AeroWinx
                MSFSSetupButton.Enabled = true; // MSFS button
                OptionsButton.Enabled = true;
                return;
            }

            // If trying to start a connection, proceed
            ShowStat("Looking for X-Plane...");

            // Look for a X-Plane session. If not found, don't loose time. Just return.
            if (!XPlane.Find())
            {
                ShowStat("Start XPlane and create/load/resume a flight first");
                Say("A X-Plane session was not found.\nStart XPlane and create/load/resume a flight first");
                return;
            }

            ShowStat("X-Plane found. Peripherals setup...");

            // Connect to C-Port if not yet connected
            if (!CPORT.Opened)
            {
                CPORT.Open();
            }

            // Get the currently selected profile
            string prof = XPlaneProfile.Text;

            ShowStat("Done");


            // Instantiate write thread
            Thread XPlaneThread = new Thread(() => SendToXPlane(XPlane.XPIP, XPlane.XPPort, prof));
            XPlaneThread.IsBackground = true;

            // Instantiate read thread
            Thread XPlaneReceive = new Thread(() => ReceiveXPlane(XPlane.XPIP, XPlane.XPPort, prof));
            XPlaneReceive.IsBackground = true;

            // Instantiate read thread
            Thread XPlaneGhostKeying = new Thread(() => XPlaneGhostKeys(XPlane.XPIP, XPlane.XPPort, prof));
            XPlaneGhostKeying.IsBackground = true;

            // Create and setup a connection

            // Form's buttons setup

            ExitButton.Enabled = false; // Disable exit without proper thread completition

            EditLinkedHardwareButton.Enabled = false;
            DeleteLinkedHardwareButton.Enabled = false;
            NewProfile.Enabled = false;
            XPlaneProfile.Enabled = false;
            PeripheralsSetupButton.Enabled = false;
            BackLitSetupButton.Enabled = false;
            AeroWinxSetupButton.Enabled = false;
            MSFSSetupButton.Enabled = false;
            OptionsButton.Enabled = false;

            stop = false; // Stop flag for threads

            if (XPConnect.Text == "Connect") // Test the form button using its text
            {
                if (!CPORT.Opened)
                {
                    CPORT.Open(); // Open OBCS C-Port if not already opened
                }

                // Threads start
                XPlaneThreadIsRunning = true;
                XPlaneThread.Start();

                XPlaneReceivingThreadIsRunning = true;
                XPlaneReceive.Start();

                XPlaneGhostKeying.Start();

                // Setup form - Button toggle description & color
                XPConnect.Text = "Disconnect";
                XPConnect.BackColor = Color.FromArgb(255, 0, 0);

                ShowStat("Connected to X-Plane");

                // Minimize to notification area
                Hide();
                NotifyArea.Visible = true;
                NotifyArea.BalloonTipText = "Click icon to restore TOGA window";
                NotifyArea.BalloonTipTitle = "TOGA is running here";
                NotifyArea.ShowBalloonTip(2000, "TOGA is running here", "Click icon to restore window", ToolTipIcon.Info);

                // Set focus to X-PLANE
                Process p = Process.GetProcessesByName("X-Plane").FirstOrDefault();

                if (p != null)
                {
                    IntPtr XPlaneProcessHandler = p.MainWindowHandle; // Set a pointer to XPlane main window

                    SetForegroundWindow(XPlaneProcessHandler);
                }

            }

        }

        public void XPlaneGhostKeys(string IP, int port, string prof)
        {
            /// <sumary>
            /// XPlane GhostKeys and button-to-datarefs handle thread.
            /// </sumary>

            /* This method handles ghostkeys and does buttons to datarefs management.
             * 
             * It will translate joystick buttons logic state to two actions:
             * 
             * - a keyboard stroke sequence, aka, a "ghost key" stroke
             *      and/or
             * - a X-Plane dataref.
             * 
             * 
             * 'Ghosting keys' is a procedure that routes a joystick button to a key stroke
             * sequence. It mimics a keyboard key in a joystick button. This is a software
             * feature that takes advantage of the easy joystick buttons handling of the 
             * NorAir Labs hardware. The logic state of each button is always refleted in
             * the O.S. device, making X-Plane to read it too. Also, as some simulators
             * and O.Ses. limits the maximum joystick buttons number (X-Plane 12 limits to
             * 160 buttons) this is a good alternative to enjoy the remaining free buttons.
             * 
             * Here, the J-PORT buttons are requested, decoded to an array and handled, all
             * according to references done in configurations.
             * 
             * The 'keyStrokes' array contains the direct relation between J-PORT buttons and
             * the defined keyboard sequences. The 'dataRefsToSend' array contains the datarefs
             * to handle when a J-PORT button is pressed. These arrays are managed independently.
             * 
             * Here, before the main cycle, it is a good opportunity to set and program the J-PORT
             * buttons since the relevant data is brought to view during data preparation, even
             * if previously had been done. The exposure and relations of configurations data
             * is more concentrated here. Its important to notice that X-Plane configurations
             * for peripherals are completely dependent of the peripherals setups itselfs. 
             * 
             * Also, one must be aware of what joystick setup was made inside X-Plane it self
             * to not superimpose setups. This can lead to unexpected behaviours on the
             * simulation software side.
             * 
             * Attributing a dataref AND a ghost key to the same J-PORT button may be benefit or
             * not. It is a pratical thinking done with functionality and the expected simulation
             * behaviour in mind. If user is not comfy enough with its handling, should not do it.
             * 
             * The main cycle retrieves all 200 buttons at a time per itineration. A delay is
             * implemented in order to not saturate the simulation, slowing down keystrokes
             * sending and datarefs transactions.
             * 
             * Remarks:
             * - Ghosting keys is a software feature, not a hardware supported one.
             * _ VICe buttons should not be used with the selected buttons for this feature, although
             *   may work depending on the circumstances.
             * - Not every keyboard sequences are accepted by X-Plane. There are some reserved ones.
             * - Some key combinations may not work due to O.S. restrictions, simulation software 
             * acceptance or other issues. User is encouraged to try it in order to find out which
             * work or not.
             *  
             */


            int NumberOfButtons = 200; // Just have everything

            int[] lastState = new int[NumberOfButtons]; // Array to hold buttons' last state

            string[] dataRefsToSend = new string[NumberOfButtons]; // Array to hold the dataRefs to send

            float[] dataRefsOnValues = new float[NumberOfButtons]; // Array to hold the dataRefs values
                                                                   // when logic state is "ON"

            float[] lastDataRefState = new float[NumberOfButtons]; // Keep track of dataRefs state

            string[] keyStrokes = new string[NumberOfButtons];  // Array to hold the keystrokes

            NorAirXPlane.Connect XPlane = new NorAirXPlane.Connect();   // Instantiate a connection
            NorAirXPlane.DRefs XPlaneDRefs = new NorAirXPlane.DRefs();  // Instantiate XPlaneDRefs object

            XPlane.XPIP = IP;
            XPlane.XPPort = port;

            // Connect to C-Port if not yet connected
            if (!CPORT.Opened)
            {
                CPORT.Open();
            }

            // Softreset J-PORT
            CPORT.Send(JPORT.SoftReset());

            if (RNSPresent)
            {

                // Compile the RNS to J-Port buttons number and associated keystrokes sequence.
                //
                // As RNS has just two keys available ( ADF & Transponder keys ) in definitions,
                // their setup information is assembled directly.
                //

                byte[] answer = new byte[32];

                // Declare objects to hold data
                RNSForm.RNSSettings RNSKeyDefinitions = new RNSForm.RNSSettings(); // RNSSetup settings

                XPlaneSetup.RNSXPLANEDATA xplaneRNSGhostKeys = new XPlaneSetup.RNSXPLANEDATA(); //X-Plane RNS data

                // Instantiate localy a RNS setup form to access its methods
                RNSForm rnsf = new RNSForm();

                // Instantiate localy a XPlane Setup form to access its methods
                XPlaneSetup rnsXpSetup = new XPlaneSetup();

                // Set current profile settings for X-Plane Setup methods
                rnsXpSetup.currentFileName = XPlaneProfilesPath + @"\" + prof + @"\" + Radios[0].SerialNumber.Trim() + ".json";

                // Load RNS to J-PORT key definitions
                RNSKeyDefinitions = rnsf.LoadData(Radios[0].SerialNumber, (int)Radios[0].Id);

                // Load RNS keystrokes sequences
                xplaneRNSGhostKeys = rnsXpSetup.RNSLoadData(Radios[0].SerialNumber, (int)Radios[0].Id);

                // If attributed, associate buttons and keystrokes into keyStrokes array
                if (RNSKeyDefinitions != null)
                {
                    // ------------------------------ SET BUTTONS ---------------------------
                    int ButtonNumber = 0;
                    string keySequence = "";

                    // Backlit color set
                    Color backlitcustomcolor = ColorTranslator.FromHtml(xplaneRNSGhostKeys.BACKLITCOLOR);

                    byte[] blc = NormalizeRGB((byte)backlitcustomcolor.R, (byte)backlitcustomcolor.G, (byte)backlitcustomcolor.B);

                    byte r = blc[0];    // Red component
                    byte g = blc[1];    // Green component
                    byte b = blc[2];    // Blue component
                    byte W = (byte)(255 - (byte)backlitcustomcolor.A); // Alpha channel (transparency ) is regarded as inverted in leds

                    // Apply backlit device defined color
                    answer = CPORT.Send(Radios[0].BackLit(r, g, b, W));

                    // ADF button number
                    ButtonNumber = RNSKeyDefinitions.ADFBUTTON;

                    // Set ADF button type
                    answer = CPORT.Send(JPORT.SetButtonType(RNSKeyDefinitions.ADFBUTTON, RNSKeyDefinitions.ADFBUTTONTYPE));

                    // Transponder button
                    ButtonNumber = RNSKeyDefinitions.XPDRBUTTON;

                    // Set transponder button type
                    answer = CPORT.Send(JPORT.SetButtonType(RNSKeyDefinitions.XPDRBUTTON, RNSKeyDefinitions.XPDRBUTTONTYPE));

                    // Get keystroke sequence
                    keySequence = xplaneRNSGhostKeys.ADFGHOSTBUTTON.Trim();

                    if (keySequence.Length != 0)
                    {
                        keyStrokes[ButtonNumber - 1] = keySequence;
                    }

                    // ADF button dataref
                    string AdfDataRef = xplaneRNSGhostKeys.ADFBUTTONDATAREF.Trim();

                    if (AdfDataRef != null)
                    {
                        if (AdfDataRef.Trim().Length != 0)
                        {
                            dataRefsToSend[ButtonNumber - 1] = AdfDataRef;
                        }
                    }

                    // Get keystroke sequence
                    keySequence = xplaneRNSGhostKeys.XPDRGHOSTBUTTON.Trim();

                    if (keySequence.Length != 0)
                    {
                        keyStrokes[ButtonNumber - 1] = keySequence;
                    }

                    // XPDR button dataref
                    string XpdrDataRef = xplaneRNSGhostKeys.XPDRBUTTONDATAREF.Trim();

                    if (XpdrDataRef != null)
                    {
                        if (XpdrDataRef.Trim().Length != 0)
                        {
                            dataRefsToSend[ButtonNumber - 1] = XpdrDataRef;
                        }
                    }
                }
                rnsf.Dispose();
                rnsXpSetup.Dispose();
            }

            if (MBx24Present)
            {
                // Compile the MBx24 to J-Port buttons number and associated keystrokes sequence.
                //
                // MBx24 provides 32 inputs that relates directly with 32 of the 200 J-PORT buttons.
                // Those are married here.
                //
                // Care must be taken not to mix connectors numbers with joystick buttons numbers.

                // Declare instances to hold data
                MBx24Form.MBx24Settings MBx24KeyDefinitions = new MBx24Form.MBx24Settings();
                XPlaneSetup.MBx24XPLANEDATA MBx24GhostKeys = new XPlaneSetup.MBx24XPLANEDATA();

                // Instantiate localy a MBx24 setup form to access its methods to make
                // the correspondence between MBx24 inputs and joystick buttons numbers,
                // i.e., keys definition
                MBx24Form mbx24f = new MBx24Form();

                // Instantiate localy a XPlane Setup form to access its methods to declare
                // which datarefs and/or ghostkeys are used in conjunction with each MBx24 inputs
                XPlaneSetup mbx24XpSetup = new XPlaneSetup();

                string fileName = XPlaneProfilesPath + @"\" + prof + @"\" + MBx24s[0].SerialNumber.Trim() + ".json";
                mbx24XpSetup.currentFileName = fileName;

                // Load MBx24 to J-PORT key definitions
                MBx24KeyDefinitions = mbx24f.LoadData(MBx24s[0].SerialNumber, (int)MBx24s[0].Id);

                // Load MBx24 keystrokes sequences and datarefs
                MBx24GhostKeys = mbx24XpSetup.MBx24LoadData(MBx24s[0].SerialNumber, (int)MBx24s[0].Id);

                // If attributed, associate buttons and keystrokes into keyStrokes array
                if (MBx24KeyDefinitions != null && MBx24GhostKeys != null)
                {
                    // Backlit color set
                    Color backlitcustomcolor = ColorTranslator.FromHtml(MBx24GhostKeys.BACKLITCOLOR);

                    byte[] blc = NormalizeRGB((byte)backlitcustomcolor.R, (byte)backlitcustomcolor.G, (byte)backlitcustomcolor.B);

                    byte r = blc[0];    // Red component
                    byte g = blc[1];    // Green component
                    byte b = blc[2];    // Blue component
                    byte W = (byte)(255 - (byte)backlitcustomcolor.A); // Alpha channel (transparency ) is regarded as inverted in leds

                    // Apply backlit device defined color
                    CPORT.Send(MBx24s[0].BackLit(r, g, b, W));

                    byte[] joyreference = new byte[32]; // Cross reference connectors/joystick button numbers

                    /* -------------------------- SET UP INPUTS ---------------------------*/
                    foreach (var inp in MBx24KeyDefinitions.INPUTS)
                    {
                        byte MBx24Connector = inp.CONNECTOR;
                        byte JoystickButton = inp.JOY;
                        byte MBx24VICeNumber = inp.VICE;
                        byte buttonType = inp.JOYTYPE;
                        byte[] answer = new byte[32];
                        joyreference[inp.CONNECTOR - 1] = (byte)(inp.JOY - 1);

                        // Assign to this input a joystick button number in J-PORT as defined by user
                        answer = CPORT.Send(MBx24s[0].SetButtonNumber(MBx24Connector, JoystickButton));

                        // Setup joystick button in J-PORT
                        answer = CPORT.Send(JPORT.SetButtonType(JoystickButton, buttonType));

                        // If set, define the VICe button
                        if (MBx24VICeNumber != 0)
                        {
                            // Create VICe
                            answer = CPORT.Send(MBx24s[0].CreateVICeConnector(MBx24Connector, MBx24VICeNumber));
                            // Set VICe type as the same type as its action button
                            answer = CPORT.Send(MBx24s[0].SetVICeType(MBx24VICeNumber, buttonType));
                        }

                    }

                    /* -------------------------- SET UP OUTPUTS ---------------------------*/
                    foreach (var outp in MBx24KeyDefinitions.OUTPUTS)
                    {
                        byte MBx24Connector = outp.CONNECTOR;
                        byte MBx24Flash = outp.FLASHING;
                        byte MBx24Inverted = outp.INVERTED;
                        byte[] answer = new byte[32];

                        // Set up output type
                        answer = CPORT.Send(MBx24s[0].SetOutputType(MBx24Connector, MBx24Flash, 0, MBx24Inverted));

                    }

                    /* ----------------------- SET UP GHOSTKEYS & DATAREFS ----------------------*/

                    foreach (var rf in MBx24GhostKeys.REFERENCES)
                    {
                        byte MBx24Connector = rf.CONNECTOR;
                        string MBx24DataRefToWrite = rf.DATAREFTOWRITE;
                        string keySequence = rf.GHOSTKEY;
                        float value = (float)rf.ONVALUE;

                        if (value == 0)
                        {
                            value = 1.0f;
                        }

                        if (keySequence.Length != 0)
                        {
                            keyStrokes[joyreference[MBx24Connector - 1]] = keySequence; // Ghost key set
                        }

                        if (MBx24DataRefToWrite != null)
                        {

                            dataRefsOnValues[MBx24Connector - 1] = value; // Value to send to X-Plane

                            dataRefsToSend[MBx24Connector - 1] = MBx24DataRefToWrite; // Dataref to write to
                        }

                    }
                    mbx24f.Dispose();
                    mbx24XpSetup.Dispose();
                }

            }

            // If there's no data to process in the thread, return,

            if (keyStrokes == null && dataRefsToSend == null)
            {
                return;
            }

            // Get XPLane process handle
            // This is needed as it is the target of the keystrokes

            Process p = Process.GetProcessesByName("X-Plane").FirstOrDefault();

            if (p == null)  // If something went wrong
            {               // just return without stoping all threads
                return;
            }

            IntPtr XPlaneProcessHandler = p.MainWindowHandle; // Set a pointer to XPlane main window

            for (int i = 0; i != 200; i++)
            {
                lastDataRefState[i] = 0.0f;
            }

            byte[] ans = CPORT.Send(JPORT.ReportAllButtons());  // Read an initial JPORT buttons state

            if (ans != null)
            {
                int[] JPortB = JPORT.DecodeButtons(ans);
                if (JPortB != null)
                {
                    for (int i = 0; i != NumberOfButtons; i++)
                    {
                        lastState[i] = JPortB[i];
                    }
                }
            }

            while (!stop)           // Main cycle
            {
                // Retrieve the 200 buttons, decode and store then in an array
                byte[] answer = CPORT.Send(JPORT.ReportAllButtons());

                if (answer != null)
                {
                    int[] JPortButtonsState = JPORT.DecodeButtons(answer);

                    if (JPortButtonsState != null)
                    {
                        for (int i = 0; i != NumberOfButtons; i++)
                        {
                            // Send key strokes
                            if (keyStrokes[i] != null)
                            {
                                if (keyStrokes[i].Length != 0)
                                {
                                    if (JPortButtonsState[i] == 1)
                                    {
                                        if (lastState[i] == 0) // Debounce the button
                                        {
                                            // Fire SendKey
                                            XPlane.SendKey(keyStrokes[i]);
                                            lastState[i] = 1;
                                        }
                                    }
                                    else
                                    {
                                        lastState[i] = 0; // Reset last key stroke state
                                    }
                                }
                            }

                            // Update dataRefs
                            if (dataRefsToSend[i] != null)
                            {
                                if (dataRefsToSend[i].Length > 10)
                                {
                                    if (JPortButtonsState[i] == 1)
                                    {

                                        if (lastDataRefState[i] == 0.0f)
                                        {
                                            lastDataRefState[i] = 1.0f;
                                            XPlane.SetDataRef(dataRefsToSend[i].Trim(), dataRefsOnValues[i]); // 1.0f);
                                        }

                                    }
                                    else
                                    {
                                        if (lastDataRefState[i] == dataRefsOnValues[i])
                                        {
                                            lastDataRefState[i] = 0.0f;            // The "OFF" logical state is always 0
                                            XPlane.SetDataRef(dataRefsToSend[i].Trim(), 0.0f);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

        }

        public void ReceiveXPlane(string IP, int port, string profile)
        {
            /// <summary>
            /// Setup & receive periodic data from XPlane for NorAIrLabs instruments
            /// </summary>

            /*  This method is design to manage one RNS and/or one MBx24
             * 
             *  ReceiveXPlane is a thread method to set up and capture data from XPlane periodically.
             *  Begins by filling a list of instruction to send to XPlane with the desire frequency
             *  rate and index. After, sets up  a socket to communicate and sends by this the list
             *  created.
             *  
             *  At main cycle, an itineration is made until a stop flag is set. Here it listens to
             *  the created socket port for meaningful data and updates the peripherals.
             *  
             *  Not less important, it finishes by instructing XPlane to not send more data.
             *  
             *  Although this method seems extensive, it is produced as so on purpose.
             *  This allows to fairly comment the code on various options of coding to enable
             *  understanding of the X-Plane's retrieving data process and relate to NorAirLabs
             *  SDK, taking advantage of its flexibility.
             *  
             */


            int frequency = 5;  // X-Plane data refresh times per second ( or frequency ).
                                //
                                // X-Plane will send periodicaly ( or 'frequency' times per second ) the array of data.
                                // Increase or decrease 'frequency' to tune. Higher values may slower or lag X-Plane.
                                // Very low frequencies may be not usefull.
                                // For example, setting 'frequency' to 5 equals to receive data every ~200ms.
                                // Basic code is provided in XPlaneSetup form for those who wish to implement a
                                // programmable frequency. This value should be global to all peripherals for
                                // performance.
                                //
                                // For more information, please visit X-Plane website and consult its documentation.
                                //


            int NumberOfIndexes = 40;  // Number of connectors to handle.
                                       // Assuming one RNS (4 indexes) and one MBx24 (32 indexes), there'll be 38 indexes max.

            NorAirXPlane.Connect ReceiveFromXplane = new NorAirXPlane.Connect(); // Create an instance of XPlane connect object
            NorAirXPlane.DRefs FromDataRefs = new NorAirXPlane.DRefs(); // Create an instance of DRefs class to use its methods

            // Address & port definitions
            ReceiveFromXplane.XPIP = IP;
            ReceiveFromXplane.XPPort = port;

            // Peripherals presence flags
            bool RNSReadyToReceive = RNSPresent;
            bool MBx24ReadyToReceive = MBx24Present;

            // Connect to C-Port if not yet connected
            if (!CPORT.Opened)
            {
                CPORT.Open();
            }

            // Create list of the datarefs for X-Plane send to this thread
            List<byte[]> msg = new List<byte[]>();

            // Create list to instruct X-Plane to stop send messages to this thread.
            // The difference between msg & stopMsgs is the frequency. Set a frequency
            // of 0 to inform X-Plane to stop sending more data for a specific dataref.
            List<byte[]> stopMsgs = new List<byte[]>();

            string[] RNSDataRefs = new string[6]; // For RNS there's 6 datarefs, one for each instrument display
            string[] MBx24DataRefs = new string[32]; // For MBx24 there's 32 datarefs for 32 outputs (LEDs or annunciators)

            // Read and update RNS datarefs. If none, RNSReadReceivingDataRefs() method returns null.
            if (RNSReadyToReceive)
            {
                RNSDataRefs = FromDataRefs.RNSReadReceivingDataRefs(Radios[0].SerialNumber, profile); // RNS receiving datarefs 
            }

            if (RNSDataRefs == null)
            {
                RNSReadyToReceive = false;
            }

            //if (MBx24ReadyToReceive)  // Updated already in main thread
            //{
            //    // Setup MBx24 hardware. If none, ConfigMBx24() method will return false.
            //    MBx24ReadyToReceive = ConfigMBx24(MBx24s[0].SerialNumber, (int)MBx24s[0].Id);
            //}

            if (MBx24ReadyToReceive)
            {
                // MBx24 receiving datarefs for outputs ( Annunciators led )
                MBx24DataRefs = FromDataRefs.MBx24ReadReceivingDataRefs(MBx24s[0].SerialNumber, profile);
                if (MBx24DataRefs == null)
                {
                    MBx24ReadyToReceive = false;
                }
            }

            // Is there any datarefs to read?
            if (!RNSReadyToReceive && !MBx24ReadyToReceive)
            {
                Say("Warning:\n - There are no defined dataRefs.\n\n  TOGA will continue but will not receive\nany data from X-Plane.");
                return;
            }

            // If hard-coded is preferred, here is an example on how to do it for a RNS
            //
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 0, "sim/cockpit2/radios/actuators/nav1_standby_frequency_hz"));
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 1, "sim/cockpit2/radios/actuators/nav1_frequency_hz"));
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 2, "sim/cockpit2/radios/actuators/com1_standby_frequency_hz"));
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 3, "sim/cockpit2/radios/actuators/com1_frequency_hz"));
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 4, "sim/cockpit2/radios/actuators/adf1_frequency_hz"));
            //msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 5, "sim/cockpit2/radios/actuators/transponder_code"));

            // Set msg list with RNS read values:
            // ( For better dataref tracking, the first 6 indexes [0..5] were reserved to RNS )
            if (RNSReadyToReceive)
            {
                for (int i = 0; i != RNSDataRefs.Length; i++)
                {
                    if (RNSDataRefs[i] != "")
                    {
                        msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, i, RNSDataRefs[i].Trim()));
                        stopMsgs.Add(ReceiveFromXplane.BuildRREFPacket(0, i, RNSDataRefs[i].Trim()));
                    }
                }
            }

            // Set msg list with MBx24 read values:
            // // ( For better dataref tracking, it is reserved the indexes from 6 to 37 to MBx24 )
            if (MBx24ReadyToReceive)
            {
                for (int i = 0; i != MBx24DataRefs.Length; i++)
                {
                    msg.Add(ReceiveFromXplane.BuildRREFPacket(frequency, 6 + i, MBx24DataRefs[i].Trim()));
                    stopMsgs.Add(ReceiveFromXplane.BuildRREFPacket(0, 6 + i, MBx24DataRefs[i].Trim()));
                }
            }

            // Create and setup socket
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint groupEP = new IPEndPoint(IPAddress.Parse(ReceiveFromXplane.XPIP), ReceiveFromXplane.XPPort);

            sock.ReceiveBufferSize = 2048;
            sock.ExclusiveAddressUse = false;

            if (msg.Count != 0)
            {
                // Instruct datarefs to receive to XPlane
                for (int i = 0; i < msg.Count; i++)
                {
                    try
                    {
                        int n = sock.SendTo(msg.ElementAt(i), groupEP);
                    }
                    catch // (SocketException e)
                    {
                        stop = true;
                        sock.Close();
                        return;
                    }
                }
            }


            // RNS working vars

            // Old frequency/squawk values for comparison
            int lastnavstb = 0;
            int lastnavact = 0;
            int lastcomstb = 0;
            int lastcomact = 0;
            int lastadf = 0;
            int lastxpdr = 0;

            // MBx24 working vars
            bool firstRead = true;

            // Old output values for comparison
            int[] OutputlastState = new int[32];

            for (int i = 0; i != 32; i++)
            {
                OutputlastState[i] = -1;
            }


            while (!stop)               // Main cycle
            {
                int bytesReceived = sock.Available; // Look for available data

                if (bytesReceived != 0)
                {
                    try
                    {
                        byte[] rec = new byte[bytesReceived]; // Receiving byte

                        sock.ReceiveFrom(rec, ref groupEP); // Receive

                        if (rec[0] == 'R' && rec[1] == 'R') // Validate if received data begins by "RR" as per 'RREF'
                        {
                            int byteCount = 5; // Skip 5 bytes. XPlane answer begins with 5 chars ("RREF\0")

                            int[] ValuesFromXPlane = new int[NumberOfIndexes]; // Array with the values read from XPlane to compare changes

                            // Set the default value to -1. It is important to now if the value has been
                            // filled. The user may have not inserted a dataRef in one of the fields and
                            // this is a way detected it.

                            for (int i = 0; i < NumberOfIndexes; i++)
                            {
                                ValuesFromXPlane[i] = -1;
                            }

                            while (byteCount < rec.Length) // Data extraction (4 bytes for index + 4 bytes for value)
                            {

                                if ((byteCount + 8) >= rec.Length)
                                {
                                    break;
                                }

                                // Auxiliar holding array
                                byte[] auxArray = new byte[4];

                                // Index - Extract 4 bytes
                                System.Buffer.BlockCopy(rec, byteCount, auxArray, 0, 4);

                                // Convert four bytes (xint) to int
                                int index = BitConverter.ToInt32(auxArray, 0);

                                // Point to next field
                                byteCount += 4;

                                // Field value - Extract 4 bytes
                                System.Buffer.BlockCopy(rec, byteCount, auxArray, 0, 4);

                                // Convert four bytes (float) to int
                                int value = (int)(BitConverter.ToSingle(auxArray, 0));

                                // Register values that are valid
                                if (value >= 0) // Simple check for a valid value
                                {
                                    ValuesFromXPlane[index] = value;
                                }

                                // Point to next RREF
                                byteCount += 4;

                            }

                            // New values have been captured.
                            // Now, set the respective fields.

                            //*-*-*-*-*-*-*-*-*-*-*- RNS section *-*-*-*-*-*-*-*-*

                            if (RNSReadyToReceive)
                            {
                                // Set RNS with new values.

                                if (ValuesFromXPlane[0] != -1)
                                {
                                    if (ValuesFromXPlane[0] != lastnavstb)
                                    {
                                        lastnavstb = ValuesFromXPlane[0];

                                        int i = (int)(ValuesFromXPlane[0] / 100); // Get integer numeric part
                                        int d = (ValuesFromXPlane[0] - (i * 100)) * 10; // Get fractional numeric part

                                        while (true)          // Talk to the peripheral until an valid answer is aquired
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetNAVStandByFrequency(i, d));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1) // Accomplished task
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ValuesFromXPlane[1] != -1)
                                {
                                    if (ValuesFromXPlane[1] != lastnavact)
                                    {
                                        lastnavact = ValuesFromXPlane[1];
                                        int i = (int)(ValuesFromXPlane[1] / 100);
                                        int d = (ValuesFromXPlane[1] - (i * 100)) * 10;
                                        while (true)
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetNAVActiveFrequency(i, d));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ValuesFromXPlane[2] != -1)
                                {
                                    if (ValuesFromXPlane[2] != lastcomstb)
                                    {
                                        lastcomstb = ValuesFromXPlane[2];
                                        int i = (int)(ValuesFromXPlane[2] / 100);
                                        int d = (ValuesFromXPlane[2] - (i * 100)) * 10;
                                        while (true)
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetCOMMStandByFrequency(i, d));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ValuesFromXPlane[3] != -1)
                                {
                                    if (ValuesFromXPlane[3] != lastcomact)
                                    {
                                        lastcomact = ValuesFromXPlane[3];
                                        int i = (int)(ValuesFromXPlane[3] / 100);
                                        int d = (ValuesFromXPlane[3] - (i * 100)) * 10;
                                        while (true)
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetCOMMActiveFrequency(i, d));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ValuesFromXPlane[4] != -1)
                                {
                                    if (ValuesFromXPlane[4] != lastadf)
                                    {
                                        lastadf = ValuesFromXPlane[4];
                                        while (true)
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetADFFrequency(ValuesFromXPlane[4]));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ValuesFromXPlane[5] != -1)
                                {
                                    if (ValuesFromXPlane[5] != lastxpdr)
                                    {
                                        lastxpdr = ValuesFromXPlane[5];
                                        while (true)
                                        {
                                            byte[] answer = CPORT.Send(Radios[0].SetXPDRSquawkCode(ValuesFromXPlane[5]));
                                            if (answer != null)
                                            {
                                                if (answer[2] == 1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //*-*-*-*-*-*-*-*-*-*-*- MBx24 section *-*-*-*-*-*-*-*-*

                            if (MBx24ReadyToReceive)
                            {

                                // Set MBx24 outputs to the new received values.
                                // Remember the first X-Plane index received for the MBx24 is 6.

                                byte[] outputsLogicState = new byte[32];    // Holds the translated logic states of the datarefs
                                                                            // X-Plane sends data as floats. A conversion to boolean is needed.

                                bool modified = false;

                                for (int i = 6; i != NumberOfIndexes; i++)
                                {
                                    if (ValuesFromXPlane[i] != -1)
                                    {
                                        if ((ValuesFromXPlane[i] != OutputlastState[i - 6]))    // Data changed?
                                        {
                                            modified = true;    // Signal modification

                                            byte newState = MBx24s[0].OFF; // Set a default value (OFF)

                                            if (ValuesFromXPlane[i] != 0)
                                            {
                                                newState = MBx24s[0].ON;
                                            }

                                            outputsLogicState[i-6] = newState;  // Update
                                        }
                                    }
                                }

                                if (modified)       // Are there changes regarding the last read status?
                                {
                                    byte[] bulkOutputs = new byte[4];   // Holds banks data

                                    int counter = 0;    // Bit iteration counter
                                    
                                    int bank = 0;       // Bank index

                                    for (int j = 0; j != 32; j++)
                                    {
                                        bulkOutputs[bank] >>= 1;                        // Shift right last bit ( starts @ MSB )
                                        byte t = (byte)(outputsLogicState[j] * 128);    // Push output logic state to the first left bit (MSB)
                                        bulkOutputs[bank] |= t;                         // Set the MSB bit of this bank
                                        counter++;                      // Control bit position and bank
                                        if (counter == 8)
                                        {
                                            counter = 0;
                                            bank++;
                                        }
                                    }

                                    // Send banks to MBx24 in bulk
                                    CPORT.Send(MBx24s[0].SetOutputBulk(bulkOutputs[0], bulkOutputs[1], bulkOutputs[2], bulkOutputs[3]));
                                    
                                }
                            }
                        }
                    }
                    catch // (Exception e)
                    {
                        // If an error occurs, an exception is fired. One can deal with it, for example,
                        // logging a time stamp, the source and the error message to a file or writing
                        // to the output Debug or even to the status bar.
                        // Debug.WriteLine("Receiving error: " + e.Source + "\n" + e.Message);
                    }
                }
            }

            if (stopMsgs.Count != 0)
            {
                // Instruct X-Plane to stop sending data

                for (int i = 0; i < stopMsgs.Count; i++)
                {
                    try
                    {
                        sock.SendTo(stopMsgs.ElementAt(i), groupEP);
                    }
                    catch // (SocketException e)
                    {
                        stop = true;
                        sock.Close();
                        return;
                    }
                }
            }

            sock.Close();

            XPlaneReceivingThreadIsRunning = false;

        }

        public bool PostMessage(byte[] message, int trials = 5)
        {
            /// <summary>
            /// Sends a message through C-Port until it is delivered.
            /// </summary>
            /// 
            /// It HAS NOT a time out. Just a simple trial counter.
            /// 

            while (trials != 0)          // Talk to the peripheral until an valid answer is aquired
            {
                byte[] answer = CPORT.Send(message);
                if (answer != null)
                {
                    if (answer[2] == 1) // Accomplished task
                    {
                        return true;
                    }
                }
                trials--;
            }
            return false;
        }

        public void SendToXPlane(string IP, int port, string profile)
        {
            /// <summary>
            /// This is the thread where peripherals are read and data is channeled to X-Plane.
            /// </summary>
            /// 
            /// It is here where data, like frequencies, squawk codes and others, is treated and
            /// sent to X-Plane.
            /// 
            /// The RNS and others ( like an autopilot ) are the ones with expressive data for
            /// this process. Even so, not all data may be significant ( as button states ), as it
            /// may be already managed by the J-PORT or other thread.
            /// 
            /// This way, a MBx24 peripheral, depending on hardware options, may not be suitable
            /// to be handled here. It is easier to manipulate the J-PORT data as the MBx24 is almost
            /// composed by joystick buttons and annunciators to be set.
            /// 
            /// Of course, if needed, a MBx24 or part of it may be processed here.
            /// 
            /// The method reads definition files with the datarefs to work on. Then requests relevant
            /// data to peripherals, correlate this with the datarefs and sends formated data packages
            /// to X-Plane.
            /// 

            int msWaitTime = 42; // Time to wait to start the next round of inquires ( see below )

            XPlaneThreadIsRunning = true;

            NorAirXPlane.Connect XPlane = new NorAirXPlane.Connect();   // Instantiate a connection
            NorAirXPlane.DRefs XPlaneDRefs = new NorAirXPlane.DRefs();  // Instantiate XPlaneDRefs object

            XPlane.XPPort = port;
            XPlane.XPIP = IP;

            // Connect to C-Port if not yet connected
            if (!CPORT.Opened)
            {
                CPORT.Open();
            }

            // Fetch datarefs by serial numbers
            Dictionary<string, string> RNSDataRefsToSend = XPlaneDRefs.RNSLoadData(Radios[0].SerialNumber, profile);

            if (RNSDataRefsToSend == null)
            {
                RNSPresent = false;
            }

            if (!RNSPresent && !MBx24Present)
            {
                // No field is set to write to XPlane datarefs. Stop this thread and return.
                // Anyway, leave the other threads running if user don't want to set those
                // fields. One may be using only J-PORT buttons based hardware like MBx24 and
                // may has no need to process data here. So, no threads-stop is issued at
                // present time.

                return;
            }

            ShowStat("Connected to XPlane");

            // RNS vars set

            float navact = -1.0f;
            float navstb = -1.0f;
            float COMMact = -1.0f;
            float COMMstb = -1.0f;
            float adf = -1.0f;
            float squawkCode = -1.0f;

            float lastnavact = -1.0f;
            float lastnavstb = -1.0f;
            float lastCOMMact = -1.0f;
            float lastCOMMstb = -1.0f;
            float lastadf = -1.0f;
            float lastsquawkCode = -1.0f;

            byte[] FreqReport = new byte[32];
            byte[] squawk = new byte[32];

            // MBx24 vars set

            byte[] MBx24Inputs = new byte[32];
            byte[] MBx24InputsLastState = new byte[32];

            if (RNSPresent)
            {
                // Retrieve initial data from RNS
                FreqReport = CPORT.Send(Radios[0].FreqReport());    // Get frequencies
                squawk = CPORT.Send(Radios[0].GetSquawk());     // Get squawk code

                navact = Extract(FreqReport, RNS.Instruments.ActiveNAV);
                navstb = Extract(FreqReport, RNS.Instruments.StandByNAV);
                COMMact = Extract(FreqReport, RNS.Instruments.ActiveCOMM);
                COMMstb = Extract(FreqReport, RNS.Instruments.StandByCOMM);
                adf = Extract(FreqReport, RNS.Instruments.ADF);
                squawkCode = ExtractSquawk(squawk);

                // Update X-Plane with initial values
                foreach (var RNSDataRef in RNSDataRefsToSend)
                {
                    switch (RNSDataRef.Key)
                    {
                        case "COMMACTTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, COMMact);
                                lastCOMMact = COMMact;
                                break;
                            }

                        case "COMMSTBTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, COMMstb);
                                lastCOMMstb = COMMstb;
                                break;
                            }

                        case "NAVACTTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, navact);
                                lastnavact = navact;
                                break;
                            }

                        case "NAVSTBTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, navstb);
                                lastnavstb = navstb;
                                break;
                            }

                        case "ADFTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, adf);
                                lastadf = adf;
                                break;
                            }

                        case "XPDRTOWRITE":
                            {
                                XPlane.SetDataRef(RNSDataRef.Value, squawkCode);
                                lastsquawkCode = squawkCode;
                                break;
                            }

                    }
                }
            }

            // An example of hard-coded version
            //
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/nav1_frequency_hz", navact);
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/nav1_standby_frequency_hz", navstb);
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/com1_frequency_hz", COMMact);
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/com1_standby_frequency_hz", COMMstb);
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/adf1_frequency_hz", adf);
            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/transponder_code", squawkCode);

            while (!stop)           // Main cycle
            {
                Thread.Sleep(msWaitTime);   // Due to hardware speed, a pause can be made to
                                            // not lag X-Plane by bombarding it with not needed
                                            // updates which can degrade simulation performance.
                                            // Adjusting msWaitTime for a value near to the period
                                            // of X-Plane frame rate frequency minus one is recommended.
                                            // Ex.: Frame rate: 25 ==> msWaitTime = 1000/(25-1) = ~42 ms.

                if (RNSPresent)
                {
                    // Get data from RNS
                    FreqReport = CPORT.Send(Radios[0].FreqReport());
                    squawk = CPORT.Send(Radios[0].GetSquawk());

                    if (FreqReport != null)
                    {
                        // RNS Data extraction

                        navact = Extract(FreqReport, RNS.Instruments.ActiveNAV);
                        navstb = Extract(FreqReport, RNS.Instruments.StandByNAV);
                        COMMact = Extract(FreqReport, RNS.Instruments.ActiveCOMM);
                        COMMstb = Extract(FreqReport, RNS.Instruments.StandByCOMM);
                        adf = Extract(FreqReport, RNS.Instruments.ADF);

                        // Update if changed from last reading
                        // ( Commented lines are examples of hard-coded versions )

                        if (lastnavact != navact)
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["NAVACTTOWRITE"], navact);
                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/nav1_frequency_hz", navact);
                            lastnavact = navact;
                        }

                        if (lastnavstb != navstb)
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["NAVSTBTOWRITE"], navstb);
                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/nav1_standby_frequency_hz", navstb);
                            lastnavstb = navstb;
                        }

                        if (lastCOMMact != COMMact)
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["COMMACTTOWRITE"], COMMact);
                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/com1_frequency_hz", COMMact);
                            lastCOMMact = COMMact;
                        }

                        if (lastCOMMstb != COMMstb)
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["COMMSTBTOWRITE"], COMMstb);
                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/com1_standby_frequency_hz", COMMstb);
                            lastCOMMstb = COMMstb;
                        }

                        if (lastadf != adf)
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["ADFTOWRITE"], adf);
                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/adf1_frequency_hz", adf);
                            lastadf = adf;
                        }
                    }


                    if (squawk != null)
                    {
                        // Transponder data extraction

                        squawkCode = ExtractSquawk(squawk);

                        if ((lastsquawkCode != squawkCode) && (squawkCode != -1.0f))
                        {
                            XPlane.SetDataRef(RNSDataRefsToSend["XPDRTOWRITE"], squawkCode);

                            //XPlane.SetDataRef("sim/cockpit2/radios/actuators/transponder_code", squawkCode);
                            lastsquawkCode = squawkCode;
                        }
                    }
                }
            }
            XPlaneThreadIsRunning = false;
            return;
        }


        public float Extract(byte[] frequencies, RNS.Instruments instrument)
        {
            /// <summary>
            /// Extracts & returns from FreqReport() array the portion of instrument frequency
            /// </summary>
            if (frequencies == null)
            {
                return -1.0f;
            }
            byte[] an = new byte[6];
            int index = 0;
            int len = 5;
            switch (instrument)
            {
                case RNS.Instruments.ActiveCOMM:
                    index = 2;
                    break;
                case RNS.Instruments.StandByCOMM:
                    index = 8;
                    break;
                case RNS.Instruments.ActiveNAV:
                    index = 14;
                    break;
                case RNS.Instruments.StandByNAV:
                    index = 20;
                    break;
                case RNS.Instruments.ADF:
                    index = 25;
                    break;
            }
            if (index != 0)
            {
                System.Buffer.BlockCopy(frequencies, index, an, 0, len);
                for (int i = 0; i < len; i++)
                {
                    an[i] += 48; // Converts a byte number to an understandable number to xplane
                }
            }
            try
            {
                return float.Parse(Encoding.UTF8.GetString(an));
            }
            catch // (Exception e)
            {
                // MessageBox.Show("Error, " + e.TargetSite + ", " + e.Message + "\nInstruments extract error [" + index.ToString() + "]: String:" + Encoding.UTF8.GetString(an), "Error", MessageBoxButtons.OK);
                return -1.0f;
            }
        }

        public float ExtractSquawk(byte[] response)
        {
            /// <summary>
            /// Extracts & returns squawk code from response
            /// </summary>
            if (response == null)
            {
                return -1.0f;
            }
            if (response.Length < 6)
            {
                return -1.0f;
            }
            byte[] an = new byte[4];
            System.Buffer.BlockCopy(response, 2, an, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                an[i] += 48; // Converts a byte number to its match in ASCII ( from 1 to '1' )
            }
            try
            {
                return float.Parse(Encoding.UTF8.GetString(an));
            }
            catch // (Exception e)
            {
                // MessageBox.Show("Error, " + e.TargetSite + "\nSquawk code extract error: " + e.Message, "Error", MessageBoxButtons.OK);
                return -1.0f;
            }

        }

        public bool ConfigMBx24(string serial, int id)
        {
            // A simple error trapping if peripherals report an error
            int errors = 0;

            MBx24Form mbx24 = new MBx24Form();
            MBx24Form.MBx24Settings mbx24Sets = new MBx24Form.MBx24Settings();

            mbx24Sets = mbx24.LoadData(serial, (int)id);

            if (mbx24Sets == null)
            {
                return false;
            }

            ShowStat("Loading MBx24 configuration");

            stop = false;   // Tell to the joystick reader thread to suspend for a while. We need 
                            // to setup up part of the joystick engine.

            /*---------- Config the INPUTs ( buttons, switches, etc. ) ------------*/
            foreach (MBx24Form.MBx24Input inp in mbx24Sets.INPUTS)
            {
                // Set J-PORT button number by associating a connector to a joystick button number
                if (!PostMessage(MBx24s[0].SetButtonNumber(inp.CONNECTOR, inp.JOY)))
                {
                    errors++;
                }

                // Set J-PORT button type
                if (!PostMessage(MBx24s[0].SetButtonType(inp.JOY, inp.JOYTYPE)))
                {
                    errors++;
                }
                // Set VICe connection
                if (!PostMessage(MBx24s[0].CreateVICeConnector(inp.CONNECTOR, inp.VICE)))
                {
                    errors++;
                }
                // Set VICe button type to the same as its action button type
                if (!PostMessage(MBx24s[0].SetVICeType(inp.VICE, inp.JOYTYPE)))
                {
                    errors++;
                }
            }       // End INPUTs config


            /*---------- Config the OUTPUTs ( LEDs, etc.) ------------*/
            foreach (MBx24Form.MBx24Output outp in mbx24Sets.OUTPUTS)
            {
                byte[] answer = new byte[32]; // Holds peripheral answers

                // Send annunciator config
                if (!PostMessage(MBx24s[0].SetOutputType(outp.CONNECTOR, outp.FLASHING, 1, outp.INVERTED)))
                {
                    errors++;
                }
            }

            if (errors != 0)
            {
                ShowStat("Error while setting buttons. Check USB and peripherals cable and run TOGA again.");
            }
            else
            {
                ShowStat("Done");
            }

            stop = true; // If it its running, tell the joystick reading thread to resume.

            return true;
        }


        public bool ConfigRNS(string serial, int Id)
        {

            byte id = (byte)Id;
            byte[] ans = new byte[32];
            char[] charSeparators = new char[] { ',', '.' };


            string JSonFile = BasePath + "\\" + serial + ".json";

            RNSForm.RNSSettings rnsSets = new RNSForm.RNSSettings();

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            if (!File.Exists(JSonFile))
            {
                return false;
            }

            ShowStat("Loading RNS configuration");

            string lines = File.ReadAllText(JSonFile);    // Read all lines at once

            lines = verifyBrackects(lines);

            try
            {
                rnsSets = JsonConvert.DeserializeObject<RNSForm.RNSSettings>(lines);  // Deserialize 'lines'

            }
            catch
            {
                return false;
            }

            if (rnsSets.COMMMAX.Length != 0)
            {
                string[] freqParts = rnsSets.COMMMAX.Split(charSeparators);
                int intpart = int.Parse(freqParts[0]);
                int decpart = int.Parse(freqParts[1]);
                byte[] res = Radios[0].SetCOMMMaximumFrequency(intpart, decpart);
                CPORT.Send(res);
            }
            if (rnsSets.COMMMIN.Length != 0)
            {
                string[] freqParts = rnsSets.COMMMIN.Split(charSeparators);
                int intpart = int.Parse(freqParts[0]);
                int decpart = int.Parse(freqParts[1]);
                byte[] res = Radios[0].SetCOMMMinimumFrequency(intpart, decpart);
                CPORT.Send(res);
            }
            if (rnsSets.COMMINTINC != 0)
            {
                byte[] res = Radios[0].SetCOMMIntegerIncrement(rnsSets.COMMINTINC);
                CPORT.Send(res);
            }
            if (rnsSets.COMMDECINC != 0)
            {
                byte[] res = Radios[0].SetCOMMDecimalIncrement(rnsSets.COMMDECINC);
                CPORT.Send(res);
            }


            if (rnsSets.NAVMAX.Length != 0)
            {
                string[] freqParts = rnsSets.NAVMAX.Split(charSeparators);
                int intpart = int.Parse(freqParts[0]);
                int decpart = int.Parse(freqParts[1]);
                byte[] res = Radios[0].SetNAVMaximumFrequency(intpart, decpart);
                CPORT.Send(res);
            }
            if (rnsSets.NAVMIN.Length != 0)
            {
                string[] freqParts = rnsSets.NAVMIN.Split(charSeparators);
                int intpart = int.Parse(freqParts[0]);
                int decpart = int.Parse(freqParts[1]);
                byte[] res = Radios[0].SetNAVMinimumFrequency(intpart, decpart);
                CPORT.Send(res);
            }
            if (rnsSets.NAVINTINC != 0)
            {
                byte[] res = Radios[0].SetNAVIntegerIncrement(rnsSets.NAVINTINC);
                CPORT.Send(res);
            }
            if (rnsSets.NAVDECINC != 0)
            {
                byte[] res = Radios[0].SetNAVDecimalIncrement(rnsSets.NAVDECINC);
                CPORT.Send(res);
            }

            if (rnsSets.ADFMAX > 0)
            {
                byte[] res = Radios[0].SetADFMaximumFrequency(rnsSets.ADFMAX);
                CPORT.Send(res);
            }

            if (rnsSets.ADFMIN >= 0)
            {
                byte[] res = Radios[0].SetADFMinimumFrequency(rnsSets.ADFMIN);
                CPORT.Send(res);
            }

            if (rnsSets.ADFINTINC != 0)
            {
                byte[] res = Radios[0].SetADFIntegerIncrement(rnsSets.ADFINTINC);
                CPORT.Send(res);
            }

            if (rnsSets.ADFDECINC != 0)
            {
                byte[] res = Radios[0].SetADFDecimalIncrement(rnsSets.ADFDECINC);
                CPORT.Send(res);
            }

            if (rnsSets.XPDRMAX > 0)
            {
                int intpart = rnsSets.XPDRMAX / 100;
                int decpart = rnsSets.XPDRMAX - intpart * 100;
                byte[] res = Radios[0].SetXPDRMaximumSquawk(intpart, decpart);
                CPORT.Send(res);

            }

            if (rnsSets.XPDRMIN >= 0)
            {
                int intpart = rnsSets.XPDRMIN / 100;
                int decpart = rnsSets.XPDRMIN - intpart * 100;
                byte[] res = Radios[0].SetXPDRMinimumSquawk(intpart, decpart);
                CPORT.Send(res);
            }

            if (rnsSets.XPDRINTINC != 0)
            {
                byte[] res = Radios[0].SetXPDRIntegerIncrement(rnsSets.XPDRINTINC);
                CPORT.Send(res);
            }

            if (rnsSets.XPDRDECINC != 0)
            {
                byte[] res = Radios[0].SetXPDRDecimalIncrement(rnsSets.XPDRDECINC);
                CPORT.Send(res);
            }


            ans = CPORT.Send(Radios[0].SetButtonNumber(1, rnsSets.COMMBUTTON));
            ans = CPORT.Send(Radios[0].SetButtonType(rnsSets.COMMBUTTON, rnsSets.COMMBUTTONTYPE));
            ans = CPORT.Send(Radios[0].SetButtonNumber(2, rnsSets.NAVBUTTON));
            ans = CPORT.Send(Radios[0].SetButtonType(rnsSets.NAVBUTTON, rnsSets.NAVBUTTONTYPE));
            ans = CPORT.Send(Radios[0].SetButtonNumber(3, rnsSets.ADFBUTTON));
            ans = CPORT.Send(Radios[0].SetButtonType(rnsSets.ADFBUTTON, rnsSets.ADFBUTTONTYPE));
            ans = CPORT.Send(Radios[0].SetButtonNumber(4, rnsSets.XPDRBUTTON));
            ans = CPORT.Send(Radios[0].SetButtonType(rnsSets.XPDRBUTTON, rnsSets.XPDRBUTTONTYPE));

            ShowStat("Done");
            return true;
        }

        public void XPlaneFillProfiles()
        {
            /// <summary>
            /// Fills the profiles selection box and, if defined, sets the default profile
            /// </summary>

            XPlaneProfile.Items.Clear();
            try
            {
                string defprof = "";
                string[] profiles = Directory.GetDirectories(XPlaneProfilesPath, @".");
                if (profiles.Length != 0)
                {
                    defprof = ReadDefaultProfile(); // Get default profile

                    XPlaneNoProfiles.Visible = false; // Hide helper

                    foreach (string profile in profiles)
                    {
                        string subdir = profile.Substring(XPlaneProfilesPath.Length + 1);
                        XPlaneProfile.Items.Add(subdir);

                        if (defprof.Length != 0)
                        {
                            if (defprof.Trim().Equals(subdir.Trim()))
                            {
                                XPlaneProfile.SelectedIndex = XPlaneProfile.Items.Count - 1; // Set this profile as the default one
                                DefaultProfileCheckBox.Checked = true;
                            }
                        }
                    }

                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowStat("There was a problem retrieving profiles ( Unauthorized Access ). Check folders permitions");
            }
        }

        public string ReadDefaultProfile()
        {
            /// <summary>
            /// Reads the default profile to use
            /// </summary>

            string defprofile = "";
            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            if (File.Exists(XPlaneDefaultProfile))
            {
                try
                {
                    // Read the file 

                    string lineRead = File.ReadAllText(XPlaneDefaultProfile);

                    defprofile = lineRead.Substring(1, lineRead.Length - 2);
                }
                catch
                {

                }
            }
            return defprofile;
        }

        private void SaveDefaultProfile()
        {
            /// <summary>
            /// Saves the default profile to use
            /// </summary>


            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string line = JsonConvert.SerializeObject(ProfileName.Text, settings);

            File.WriteAllText(XPlaneDefaultProfile, line);
        }

        private void RemoveDefaultProfile()
        {
            /// <summary>
            /// Deletes the default profile file when default is unset
            /// </summary>


            if (File.Exists(XPlaneDefaultProfile))
            {
                File.Delete(XPlaneDefaultProfile);
            }
        }

        private void DefaultProfileCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            /// <summary>
            /// Sets or removes the default profile when default check option changes
            /// </summary>

            if (DefaultProfileCheckBox.Checked)
            {
                SaveDefaultProfile();
            }
            else
            {
                RemoveDefaultProfile();
            }
        }

        private void NewProfile_Click_1(object sender, EventArgs e)
        {
            /// <summary>
            /// Creates a new profile
            /// </summary>

            AddProfile dialogForm = new AddProfile();

            DialogResult d = dialogForm.Show();

            if (d == DialogResult.OK)
            {
                XPlaneFillProfiles();
            }
        }

        private void XPlaneProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            /// <summary>
            /// Handles the selected profile
            /// </summary>

            int index = XPlaneProfile.SelectedIndex;

            if (index == -1)
            {
                return;
            }

            string Profile = XPlaneProfile.Text;

            if (XPlaneFillHardware(Profile))
            {
                ProfilePanel.Visible = true;
                ProfileName.Text = Profile;
                XPlaneSetButtons(true);
            }
        }


        public bool XPlaneFillHardware(string profile)
        {
            /// <summary>
            /// Fills the hardware list box by hardware type
            /// </summary>

            XPlaneProfileHardwareList.Items.Clear();
            NoProfilePeripherals.Visible = true;
            try
            {
                string profPath = XPlaneProfilesPath + @"\" + profile;
                string[] profiles = Directory.GetFiles(profPath, @"*.json");
                if (profiles.Length != 0)
                {
                    NoProfilePeripherals.Visible = false;
                    foreach (string prof in profiles)
                    {
                        string serial;
                        string desc;
                        int hardwareType;
                        string fileName = prof.Substring(profPath.Length + 1);

                        string lines = File.ReadAllText(prof);    // Read all lines at once

                        lines = verifyBrackects(lines);

                        bool valid = int.TryParse(fileName.Substring(0, 2), out hardwareType);

                        if (valid)
                        {
                            switch (hardwareType)
                            {
                                case 31:
                                    XPlaneSetup.RNSXPLANEDATA rns = new XPlaneSetup.RNSXPLANEDATA();
                                    rns = JsonConvert.DeserializeObject<XPlaneSetup.RNSXPLANEDATA>(lines);
                                    serial = rns.SERIAL;
                                    desc = rns.DESCRIPTION;// ProfileName.Text;
                                    XPlaneProfileHardwareList.Items.Add(serial.Trim() + ", " + desc);
                                    break;
                                case 50:
                                    XPlaneSetup.MBx24XPLANEDATA mbx24 = new XPlaneSetup.MBx24XPLANEDATA();
                                    mbx24 = JsonConvert.DeserializeObject<XPlaneSetup.MBx24XPLANEDATA>(lines);
                                    serial = mbx24.SERIAL;
                                    desc = mbx24.DESCRIPTION;// ProfileName.Text;
                                    XPlaneProfileHardwareList.Items.Add(serial.Trim() + ", " + desc);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowStat("There was a problem retrieving profiles ( Unauthorized Access ). Check folders permitions");
                return false;
            }

            return true;
        }

        public string verifyLineRead(string line)
        {
            string l1 = verifyBrackects(line);
            //Debug.WriteLine(l1);
            //int mark = 0;
            //if (l1[0].Equals('['))
            //{
            //    for (int i = l1.Length - 1; i != 0; i--)
            //    {
            //        if (l1[i] != ']')
            //        {
            //            mark = i;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    if (mark != l1.Length - 1)
            //    {
            //        //Debug.WriteLine(l1.Substring(0, mark));
            //        return (l1.Substring(0, mark));
            //    }
            //}
            //else
            //{
            //    for (int i = l1.Length - 1; i != 0; i--)
            //    {
            //        if (l1[i] != '}')
            //        {
            //            mark = i;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    if (mark != 0)
            //    {
            //        //Debug.WriteLine(l1.Substring(0, mark));
            //        return (l1.Substring(0, mark));
            //    }
            //}


            return l1;
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

        private void XPlaneAddPeripheralToList_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Adds a peripheral to the current profile
            /// </summary>

            if (XPlaneSerialList.SelectedIndex != -1)
            {
                string PeripheralSelected = XPlaneSerialList.GetItemText(XPlaneSerialList.SelectedItem);
                if (XPlaneProfileHardwareList.FindString(PeripheralSelected.Substring(0, 10)) == -1)
                {
                    XPlaneProfileHardwareList.Items.Add(PeripheralSelected);
                    NoProfilePeripherals.Visible = false;
                }
                else
                {
                    ShowStat("Not possible to add peripheral. Already in Profile peripherals list");
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Edits profile linked hardware
            /// </summary>

            if (XPlaneProfileHardwareList.SelectedIndices.Count != 0)
            {
                string HardwareSelected = XPlaneProfileHardwareList.Text;

                if (HardwareSelected.Trim().Length != 0)
                {
                    string serial = HardwareSelected.Substring(0, 10);

                    PeripheralProperty pp = PeripheralsPresent.Find(pp => pp.SERIALNUMBER.Trim() == serial.Trim());

                    if (pp.SERIALNUMBER == null)
                    {
                        return;
                    }

                    if (pp.SERIALNUMBER.Length == 0)
                    {
                        return;
                    }

                    string fileName = XPlaneProfilesPath + @"\" + ProfileName.Text + @"\" + pp.SERIALNUMBER + ".json";
                    XPlaneSetup dialogForm = new XPlaneSetup();
                    DialogResult d = dialogForm.Show(pp.SERIALNUMBER, pp.ID, backColor, frontColor, fileName, ProfileName.Text, XPlaneProfileHardwareList.Text);
                    XPlaneFillHardware(ProfileName.Text);
                }
            }
        }
        private void DeleteLinkedHardwareButton_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Delete linked hardware from profile
            /// </summary>

            if (XPlaneProfileHardwareList.SelectedIndices.Count != 0)
            {
                string HardwareSelected = XPlaneProfileHardwareList.Text;

                if (HardwareSelected.Trim().Length != 0)
                {
                    string serial = HardwareSelected.Substring(0, 10);
                    string fileName = XPlaneProfilesPath + @"\" + ProfileName.Text + @"\" + serial + ".json";
                    DialogResult delete = MessageBox.Show("About to delete\n" + fileName +
                        "\n\nProceed?", "Delete profile hardware file",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                    if (delete == DialogResult.Yes)
                    {
                        File.Delete(fileName);
                        XPlaneFillHardware(ProfileName.Text);
                    }
                }
            }
        }

        /*============================= END X-PLANE SECTION ========================================*/

        #endregion X-Plane

        public void ReadStyles()
        {
            /// <summary>
            /// Reads styling sets
            /// </summary>

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
                style.buttonColor = ColorTranslator.ToHtml(backColor);
                style.buttonForecolor = ColorTranslator.ToHtml(frontColor);
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
            buttonForecolor = ColorTranslator.FromHtml(style.buttonForecolor);
            borderSize = style.borderSize;
            borderColor = ColorTranslator.FromHtml(style.borderColor);
            FontName.Text = style.fontName;
            FontSize.Text = style.fontSize.ToString();
            BackgroundColorLabel.BackColor = backColor;
            ForegroundColorLabel.BackColor = frontColor;
            ButtonColorLabel.BackColor = buttonColor;
            BorderColorLabel.BackColor = ColorTranslator.FromHtml(style.borderColor);
            BorderSize.Value = style.borderSize;
            MouseOverColorLabel.BackColor = ColorTranslator.FromHtml(style.mouseoverColor);
            ButtonForegroundLabel.BackColor = buttonForecolor;
            CharFont = new Font(style.fontName, style.fontSize);
        }


        private void NotifyArea_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /// <summary>
            /// Brings back TOGA from the notification area
            /// </summary>

            Show();
            this.WindowState = FormWindowState.Normal;
            NotifyArea.Visible = false;
            this.BringToFront();
            this.Activate();
        }



        private void button4_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Add a new peripheral to registered peripherals list
            /// </summary>

            AddNewPeripheral dialogForm = new AddNewPeripheral();
            List<string> sn = new List<string>();
            foreach (var item in PeripheralsPresent)
            {
                sn.Add(item.SERIALNUMBER);
            }

            dialogForm.Show("", ValidIds, sn, backColor, frontColor);

            ReadRegisteredPeripherals();
            init();

        }

        private void button17_Click_1(object sender, EventArgs e)
        {
            /// <summary>
            /// Edit a peripheral in the peripherals list
            /// </summary>

            if (RegisteredPeripherals.SelectedIndex == -1)
            {
                ShowStat("No hardware selected.");
                return;
            }
            AddNewPeripheral dialogForm = new AddNewPeripheral();
            List<string> sn = new List<string>();
            foreach (var item in PeripheralsPresent)
            {
                sn.Add(item.SERIALNUMBER);
            }

            string serialToEdit = RegisteredPeripherals.Text.Substring(0, 10);
            SetupPeripheral.Enabled = false;
            button17.Enabled = false;

            DialogResult d = dialogForm.Show(serialToEdit, ValidIds, sn, backColor, frontColor);

            ReadRegisteredPeripherals();
            init();


        }

        /* ====================================== BackLit ========================================== */

        public class PrefColor      // A class for the preferred backlit colors
        {
            public byte Slot { get; set; }
            public byte Red { get; set; }
            public byte Green { get; set; }
            public byte Blue { get; set; }
            public byte White { get; set; }
            public bool Default { get; set; }
        }


        public void ReadPreferredBackLitColors()
        {
            /// <summary>
            /// Read the backlit preferred colors to color slots.
            /// </summary>
            /// 
            /// There can be 10 of them and can be changed by using the color dialog.
            /// 
            /// Remarks:
            /// - The RGB led colors may differ from the showed ones on the screen. This
            /// is due to the different color platforms such as between screen and RGB
            /// led lights. User may try to approach by tunning the color. This can
            /// be done on-the-fly if peripherals are already attached and turned on.
            /// After leaving the color dialog, if an OBCS is already found, the
            /// backlit color of peripherals changes to the selected color.
            /// 

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

            foreach (PrefColor c in colorsList) //  Fill the BackLitColors array
            {
                BackLitColor blc = new BackLitColor();
                blc.Red = c.Red;
                blc.Green = c.Green;
                blc.Blue = c.Blue;
                blc.Alpha = c.White;
                int sl = (int)c.Slot;
                BackLitColors[sl] = blc;
                if (c.Default)
                {
                    DefaultColor = (int)c.Slot;
                }
                count++;
                if (count >= BackLitColorSlots)
                {
                    break;
                }

            }
            // Fill radio buttons - aka, custom colors boxes - with the BackLitColors array values
            count = 0;
            var SelectedCustomColor = CustomColorsPanel.Controls.OfType<RadioButton>();
            foreach (RadioButton rb in SelectedCustomColor)
            {

                rb.BackColor = Color.FromArgb(
                    BackLitColors[count].Alpha,
                    BackLitColors[count].Red,
                    BackLitColors[count].Green,
                    BackLitColors[count].Blue);

                if (count == DefaultColor) // Shift default slot visibility
                {
                    Color c = rb.BackColor;
                    int h = (int)c.GetBrightness();

                    // Set the default slot
                    rb.Text = "Default";
                    rb.Checked = true;

                    // Swap colors to create some contrast
                    rb.ForeColor = Color.FromArgb(h, 255 - c.B, 255 - c.R, 255 - c.G);

                    // Send the default color to peripherals
                    SendBacklitColor(c);
                }

                count++;
                if (count == BackLitColorSlots)
                {
                    break;
                }
            }

        }

        private void BackLitSave_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Save customized colors to a file
            /// </summary>

            int defSlot = 0;
            int count = 0;

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            var SelectedCustomColor = CustomColorsPanel.Controls.OfType<RadioButton>();

            if (SelectedCustomColor.Count() == 0)
            {
                Say("Error on custom colors panel.\n Colors not saved.");
                return;
            }

            foreach (RadioButton rb in SelectedCustomColor) // Find out the default color or assume 0
            {

                if (rb.Checked)
                {
                    defSlot = count;
                    break;
                }
                count++;
            }

            List<PrefColor> tempList = new List<PrefColor>();

            count = 0;
            foreach (var backlitcustomcolor in SelectedCustomColor)
            {
                bool d = false;
                if (defSlot == count)
                {
                    d = true;
                }
                tempList.Add(new PrefColor
                {
                    Red = (byte)backlitcustomcolor.BackColor.R,
                    Green = (byte)backlitcustomcolor.BackColor.G,
                    Blue = (byte)backlitcustomcolor.BackColor.B,
                    White = (byte)backlitcustomcolor.BackColor.A,
                    Slot = (byte)count,
                    Default = d
                });
                count++;

            }

            string line = JsonConvert.SerializeObject(tempList, settings);

            File.WriteAllText(BackLitFileName, line);

            MessageBox.Show("Configuration saved", "Costumized colors save", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ReadPreferredBackLitColors(); // Re-set colors
        }

        public byte[] NormalizeRGB(byte r, byte g, byte b)
        {
            /// <summary>
            /// Approaches color normalization for RGBW leds
            /// </summary>

            byte[] arr = { r, g, b };
            if (r != g || r != b || g != b)
            {
                byte w = (byte)arr.Min();

                r = (byte)((r + w) / 2);
                g = (byte)((g + w) / 2);
                b = (byte)((b + w) / 2);
            }
            arr[0] = r;
            arr[1] = g;
            arr[2] = b;
            return arr;
        }

        private void MakeColorBox()
        {
            /// <summary>
            /// Construct a color gradient box to pick backlit colors
            /// </summary>
            /// 

            PicGrad.Image = new Bitmap(512, 512);

            using (Graphics gr = Graphics.FromImage(PicGrad.Image))
            {
                gr.Clear(Color.Silver);
                gr.DrawRectangle(Pens.Black, 5, 5, 100, 60);

                LinearGradientBrush br = new LinearGradientBrush(new Point(0, 255), new Point(511, 255), Color.White, Color.White);
                ColorBlend cb = new ColorBlend();
                cb.Positions = new[] { 0, 1 / 6f, 2 / 6f, 3 / 6f, 4 / 6f, 5 / 6f, 1 };
                cb.Colors = new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };
                br.InterpolationColors = cb;
                // rotate
                br.RotateTransform(0);
                // paint
                gr.FillRectangle(br, this.ClientRectangle);
                LinearGradientBrush br2 = new LinearGradientBrush(new Point(255, 0), new Point(255, 511), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 0, 0));//  PicGrad.ClientRectangle, Color.Black, Color.White, 0, false);
                gr.FillRectangle(br2, this.ClientRectangle);


            }
        }

        private void PicGrad_Click(object sender, MouseEventArgs e)
        {
            /// <summary>
            /// Picks a color from cursor position and sets to the sampled color box
            /// </summary>

            Bitmap bmp = new Bitmap(PicGrad.ClientSize.Width, PicGrad.Height);
            PicGrad.DrawToBitmap(bmp, PicGrad.ClientRectangle);
            Color color = bmp.GetPixel(e.X, e.Y);
            SampledColor.BackColor = color;
            bmp.Dispose();
            SendBacklitColor(color);
        }

        private void SendBacklitColor(Color color)
        {
            /// <summary>
            /// Set a peripherals backlit color
            /// </summary>

            if (!InRealTime)
            {
                return;
            }

            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            byte A = color.A;
            if (CPORT.CPortName != null)
            {
                if (CPORT.CPort.IsOpen)
                {
                    // Normalize colors to match as possible to RGB leds colors
                    byte[] ret = NormalizeRGB(R, G, B);

                    byte r = ret[0];    // Red component
                    byte g = ret[1];    // Green component
                    byte b = ret[2];    // Blue component
                    byte W = (byte)(255 - A); // Alpha channel (transparency ) is regarded as inverted in leds

                    if (W > 16)
                    {
                        W = 16;     // Limit alpha component
                    }

                    CPORT.Send(WALL.BackLit(r, g, b, W), SendOnly);  // Propagate to all peripherals
                    Thread.Sleep(100);
                }
                else
                {
                    InRealTime = false;
                }
            }
            else
            {
                InRealTime = false;
            }
        }

        private void SetBacklitColor(string serial, int id, string backlitcolor)
        {
            /// <summary>
            /// Set the backlit color for a specific peripheral 
            /// </summary>
            /// 

            Color color = ColorTranslator.FromHtml(backlitcolor);

            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            byte A = color.A;
            if (CPORT.CPortName != null)
            {
                if (CPORT.CPort.IsOpen)
                {
                    // Normalize colors to match as possible to RGB leds colors
                    byte[] ret = NormalizeRGB(R, G, B);

                    byte r = ret[0];    // Red component
                    byte g = ret[1];    // Green component
                    byte b = ret[2];    // Blue component
                    byte W = (byte)(255 - A); // Alpha channel (transparency ) is regarded as inverted in leds

                    if (W > 16)
                    {
                        W = 16;     // Limit alpha component
                    }
                    string hardware = serial.Substring(0, 2);

                    switch (hardware)
                    {
                        case "31":
                            RNS rs = new RNS(serial, (byte)id);
                            CPORT.Send(rs.BackLit(r, g, b, W));
                            break;

                        case "50":
                            MBx24 mb = new MBx24(serial, (byte)id);
                            CPORT.Send(mb.BackLit(r, g, b, W));
                            break;
                    }
                }

            }

        }
        private void AddSampledColor_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Move sampled color to a slot
            /// </summary>

            var SelectedCustomColor = CustomColorsPanel.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
            if (SelectedCustomColor != null)
            {
                SelectedCustomColor.BackColor = SampledColor.BackColor;
            }
            else
            {
                Say("Please, select a color and a custom color position first.");
            }
        }



        private void CustomColor_Selected(object sender, EventArgs e)
        {
            /// <summary>
            /// Send selected color to peripherals
            /// </summary>

            Color c = backColor; // Default color as Backlit color
            string name;
            var radioButtons = CustomColorsPanel.Controls.OfType<RadioButton>();

            var SelectedCustomColor = radioButtons.FirstOrDefault(n => n.Checked);

            if (SelectedCustomColor != null)
            {
                c = SelectedCustomColor.BackColor;
                name = SelectedCustomColor.Name;
                foreach (var radio in radioButtons)  // Display "Default" to selected slot
                {
                    if (radio.Checked)
                    {
                        int h = (int)c.GetBrightness();
                        radio.Text = "Default";
                        radio.ForeColor = Color.FromArgb(h, 255 - c.B, 255 - c.R, 255 - c.G); // Swap colors to create some contrast
                    }
                    else
                    {
                        radio.Text = "";
                    }
                }

                SendBacklitColor(c);

            }

        }


        /*============================= MAIN FORM BUTTON EVENTS ==============================*/


        private void button1_Click(object sender, EventArgs e)  // Exit button
        {
            /// <summary>
            /// Closes TOGA software. This triggers the closing event for shutdown procedures.
            /// </summary>

            this.Close();

        }

        private void findOBCS_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// The Find OBCS button method.
            /// </summary>

            stop = true;
            init();         // Find an OBCS and enumerate registered peripherals
            stop = false;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Broadcast hard-reset to peripherals followed by OBCS hard-reset
            /// Remarks:
            ///     - 'WALL' deploiments do NOT affects OBCS. 
            ///     - OBCS has its own hard-reset if needed
            /// </summary>

            if (CPORT.CPortName != null)
            {
                if (CPORT.CPort.IsOpen)
                {
                    ShowStat("Hard reset takes ~4 seconds. Please, wait some seconds");
                    CPORT.Send(WALL.HardReset(), SendOnly); // Broadcast a peripheral hard-reset instruction
                    Thread.Sleep(4000);
                    //
                    // NOTICE:  Peripherals may need until 4000ms to restart. Enumeration can not be made
                    //          during peripheral restart time
                    //

                    CPORT.Send(JPORT.HardReset(), SendOnly); // Hard-reset J-PORT

                    CPORT.Reset(); // Hard-reset OBCS

                    CPORT.Close(); // Close for now

                    InitJoystick();

                    init(); // Restart all over again with a new enumeration

                    if (theJoystick != null)
                    {
                        joystickOnLine(true);
                    }
                    else
                    {
                        joystickOnLine(false);
                    }
                }
                else
                {
                    ShowStat("No hardware detected");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)  // Peripherals setup selected
        {
            TitleLabel.Text = "Peripheral setup";
            TOGASetup.SelectedTab = TABPeripheralsSetup;
        }

        private void button5_Click(object sender, EventArgs e)  // X-Plane setup & run selected
        {
            TitleLabel.Text = "X-Plane setup && run";
            XPlaneFillProfiles();
            TOGASetup.SelectedTab = TABXPlaneSetup;
        }

        private void button6_Click(object sender, EventArgs e)  // RGB Backlit setup selected
        {
            TitleLabel.Text = "RGB Backlit setup";
            TOGASetup.SelectedTab = TABBackLit;
        }

        private void button4_Click_1(object sender, EventArgs e)    // Options selected
        {
            TitleLabel.Text = "TOGA software options";
            ReadStyles();
            TOGASetup.SelectedTab = TABOptions;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            TitleLabel.Text = "Microsoft Flight Sim setup && run";
            MSFSFillProfiles();
            TOGASetup.SelectedTab = TABMSFSSetup;
        }

        private void Form1_Resize(object sender, EventArgs e)   // Minimize to notification area
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                NotifyArea.Visible = true;
                NotifyArea.BalloonTipText = "Click icon to restore TOGA window";
                NotifyArea.BalloonTipTitle = "TOGA is running here";
                NotifyArea.ShowBalloonTip(2000, "TOGA is running here", "Click icon to restore TOGA window", ToolTipIcon.Info);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /// <summary>
            /// Exit TOGA software. This should be properly made for code sake
            /// </summary>

            DialogResult leaving = MessageBox.Show("TOGA is about to be closed.\nDo you want to leave the application?", "Exiting TOGA",
         MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (leaving == DialogResult.Yes)
            {

                stop = true; // Issue stop condition for threads

                JPORTContinueReading = false;   // Stop joystick thread

                // Wait X-Plane threads to finish their tasks
                while (XPlaneThreadIsRunning) { }
                while (XPlaneReceivingThreadIsRunning) { }

                // Wait MSFS threads to finish their tasks
                while (MSFSReceivingThreadIsRunning) { }
                while (MSFSSendingThreadIsRunning) { }

                // Dispose MSFS connector
                if (simconnect != null)
                {
                    simconnect.Dispose();
                    simconnect = null;
                }

                // Close OBCS C-Port
                CPORT.CPort.Close();

                // Exit
                e.Cancel = false;

            }
            else
            {
                // Cancel exit
                e.Cancel = true;
            }

        }

        private void SetupPeripheral_Click(object sender, EventArgs e)
        {
            if (RegisteredPeripherals.SelectedIndex != -1)
            {
                SetupPeripheral.Enabled = false;
                button17.Enabled = false;
                int index = RegisteredPeripherals.SelectedIndex;
                PeripheralProperty pp = PeripheralsPresent.ElementAt(index);
                int hardware = int.Parse(pp.SERIALNUMBER.Substring(0, 2));
                string serial = pp.SERIALNUMBER;
                int id = pp.ID;
                string desc = pp.DESCRIPTION;
                switch (hardware)   // Select dialog for each hardware type
                {
                    case 31:    // RNS type
                        ShowStat("RNS " + serial + " setup");
                        RNSForm rnsForm = new RNSForm();
                        this.Enabled = false;
                        DialogResult rnsDialogResult = rnsForm.Show(serial, id, desc, backColor, frontColor);
                        this.Enabled = true;
                        if (rnsDialogResult == DialogResult.Cancel)
                        {
                            ShowStat("Canceled");
                        }
                        rnsForm.Dispose();
                        init();
                        break;

                    case 50:    // MBx24 type
                        ShowStat("MBx24 " + serial + ", " + desc + " setup");
                        this.Enabled = false;
                        ConfigMBx24(serial, id);
                        MBx24Form dmbx24Form = new MBx24Form();
                        DialogResult dmbx24DialogResult = dmbx24Form.Show(serial, id, desc, backColor, frontColor, CPORT, MBx24s[0]);
                        this.Enabled = true;
                        if (dmbx24DialogResult == DialogResult.Cancel)
                        {
                            ShowStat("Canceled");
                        }
                        dmbx24Form.Dispose();
                        init();
                        break;
                }
                SetupPeripheral.Enabled = true;
                button17.Enabled = true;
            }
            else
            {
                ShowStat("Not peripheral selected");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ProfilePeripheralSelected(object sender, EventArgs e)
        {
            EditLinkedHardwareButton.Enabled = true;
            DeleteLinkedHardwareButton.Enabled = true;
        }

        private void RegisteredPeripherals_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Extract selected to choose dialog
            if (RegisteredPeripherals.SelectedIndex != -1)
            {
                SetupPeripheral.Enabled = true;
                button17.Enabled = true;
            }
            else
            {
                SetupPeripheral.Enabled = false;
                button17.Enabled = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Select background color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                BackgroundColorLabel.BackColor = ColorChooser.Color;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // Select foreground color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                ForegroundColorLabel.BackColor = ColorChooser.Color;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // Mouse over color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                MouseOverColorLabel.BackColor = ColorChooser.Color;
            }

        }

        private void button12_Click(object sender, EventArgs e)
        {
            // Border color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                BorderColorLabel.BackColor = ColorChooser.Color;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            // Buttons background color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                ButtonColorLabel.BackColor = ColorChooser.Color;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Buttons foreground color
            DialogResult d = ColorChooser.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                ButtonForegroundLabel.BackColor = ColorChooser.Color;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// Select font
            /// </summary>

            DialogResult d = FontDialog.ShowDialog(this);
            if (d == DialogResult.OK)
            {
                string fname = FontDialog.Font.Name;
                float fsize = FontDialog.Font.Size;
                FontName.Text = fname;
                FontSize.Text = fsize.ToString();
                FontName.Font = FontDialog.Font;
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            // Save options

            var settings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            Styles style = new Styles();

            style.styleName = "DarkMode";
            style.backColor = ColorTranslator.ToHtml(BackgroundColorLabel.BackColor);
            style.frontColor = ColorTranslator.ToHtml(ForegroundColorLabel.BackColor);
            style.fontName = FontName.Text;
            style.fontSize = float.Parse(FontSize.Text);
            style.borderColor = ColorTranslator.ToHtml(BorderColorLabel.BackColor);
            style.borderSize = BorderSize.Value;
            style.mouseoverColor = ColorTranslator.ToHtml(MouseOverColorLabel.BackColor);
            style.buttonColor = ColorTranslator.ToHtml(ButtonColorLabel.BackColor);
            style.buttonForecolor = ColorTranslator.ToHtml(ButtonForegroundLabel.BackColor);

            // Remove old options file
            if (File.Exists(StylesFileName))
            {
                File.Delete(StylesFileName);
            }

            // Save file

            string line = JsonConvert.SerializeObject(style, settings);

            File.WriteAllText(StylesFileName, line);

            backColor = BackgroundColorLabel.BackColor;
            frontColor = ForegroundColorLabel.BackColor;
            CharFont = new Font(style.fontName, style.fontSize);
            borderColor = BorderColorLabel.BackColor;
            borderSize = BorderSize.Value;
            mouseOverColor = MouseOverColorLabel.BackColor;
            buttonColor = ButtonColorLabel.BackColor;
            buttonForecolor = ButtonForegroundLabel.BackColor;
            SetTheme(this.Controls, backColor, frontColor);

            BackGroundPanel.BackColor = backColor;
            TABXPlaneSetup.BackColor = backColor;
            TABPeripheralsSetup.BackColor = backColor;
            TABBackLit.BackColor = backColor;
            TABOptions.BackColor = backColor;
            BackgroundColorLabel.BackColor = backColor;
            ForegroundColorLabel.BackColor = frontColor;
            BorderColorLabel.BackColor = borderColor;
            ButtonForegroundLabel.BackColor = buttonForecolor;
            BorderSize.Value = borderSize;
            MouseOverColorLabel.BackColor = mouseOverColor;
            ButtonColorLabel.BackColor = buttonColor;
            this.BackColor = backColor;
            this.ForeColor = frontColor;
            TOGASetup.SelectedTab.BackColor = backColor;
            TOGASetup.SelectedTab.ForeColor = frontColor;
            statusBar.BackColor = backColor;
            statusBar.ForeColor = frontColor;

            this.Update();
            this.Refresh();
        }
    }
}
