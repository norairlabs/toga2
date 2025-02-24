using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TOGA;
using static System.Windows.Forms.LinkLabel;
using static TOGA.Form1;

namespace NorAirXPlane
{
    /// <summary>
    /// The Northern Aircraft Labs namespace for XPlane 11
    /// </summary>

    public class Connect
    {
        // Send keystrokes (SendKey) definitions

        [DllImport("User32.dll")] // To use with keyghosting an external app
        static extern int SetForegroundWindow(IntPtr point); // Send app to foreground and focus

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        public class DRefs
        {
            public byte id;
            public string dataRef;
            public string target;
            public string origin;

        }


        // Socket defs
        public string XPIP; // IP
        public int XPPort = 49000; // Port
        public string MultiCastAddress = "239.255.1.1";
        public string AlternateAddress = "127.0.0.1";

        // Path for datarefs defs ( Usually 'My Documents' folder )
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";

        public string JSonPath = BasePath;

        // List for DRefs
        public List<DRefs> dataRefsList = new List<DRefs>();

        public static byte[] BuildDREFPacket(string dataRef, float value)
        {
            /// <summary>
            /// Packs a structure of a DataRef for writing data to XPlane
            /// </summary>

            byte[] bytes = new byte[509];
            System.Buffer.BlockCopy(UTF8Encoding.UTF8.GetBytes("DREF\0"), 0, bytes, 0, 5);
            System.Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, 5, 4);
            System.Buffer.BlockCopy(UTF8Encoding.UTF8.GetBytes(dataRef), 0, bytes, 9, dataRef.Length);
            return bytes;
        }

        public byte[] BuildRREFPacket(int frequency, int index, string dataRef)
        {
            /// <summary>
            /// Packs a structure of a DataRef for reading periodically from XPlane
            /// </summary>

            byte[] bytes = new byte[413];
            System.Buffer.BlockCopy(UTF8Encoding.UTF8.GetBytes("RREF"), 0, bytes, 0, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(frequency), 0, bytes, 5, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(index), 0, bytes, 9, 4);
            System.Buffer.BlockCopy(UTF8Encoding.UTF8.GetBytes(dataRef), 0, bytes, 13, dataRef.Length);
            return bytes;
        }

        public bool SetDataRef(string dataRef, float value)
        {
            /// <summary>
            /// Sends a dataRef value to a XPlane's dataref
            /// </summary>
            if (this.XPIP == null || this.XPPort == 0)
            {
                return false;
            }

            bool success = true;
            byte[] msg = BuildDREFPacket(dataRef.Trim(), value);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint groupEP = new IPEndPoint(IPAddress.Parse(XPIP), XPPort);
            sock.ExclusiveAddressUse = false;
            try
            {
                sock.SendTo(msg, groupEP);
            }
            catch // (SocketException e)
            {
                // If needed, use 'e.message' to log errors
                //Debug.WriteLine("Setting dataRef: " + e.Message);
                success = false;
            }
            finally
            {
                sock.Close();
            }
            sock.Close();
            return success;
        }


        public void sendAlert(string line1, string line2 = "", string line3 = "", string line4 = "")
        {
            /// <summary>
            /// Sends a ALERT to XPlane. This will be shown in a small window.
            /// 
            /// CAUTION:
            /// This stalls XPLane communication while displaying the window message. Use with caution.
            /// 
            /// </summary>

            if (line1.Length == 0)
            {
                return;
            }
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint groupEP = new IPEndPoint(IPAddress.Parse(XPIP), XPPort);
            sock.ExclusiveAddressUse = false;
            try
            {
                string head = "ALRT";
                string final = head + '\0' + line1 + '\n' + line2 + '\n' + line3 + '\n' + line4;
                byte[] msg = new byte[965];
                for (int i = 0; i < final.Length; i++)
                {
                    msg[i] = (byte)final[i];
                }

                sock.SendTo(msg, groupEP);
            }
            catch // (SocketException e)
            {
                // If needed, use 'e.message' to log errors
                //Debug.WriteLine("Sending alert: " + e.Message);
                sock.Close();
            }
            finally
            {
                sock.Close();
            }

            sock.Close();
        }

        public bool Find()
        {
            /// <summary>
            /// Looks for a XPlane instance by joining a XPlane broadcast
            /// </summary>

            /*
                 From XPlane documentation:
                 struct
                 {
                    uchar beacon_major_version;    // 1 at the time of X-Plane 10.40, 11.55
                    uchar beacon_minor_version;    // 1 at the time of X-Plane 10.40, 2 for 11.55
                    xint application_host_id;      // 1 for X-Plane, 2 for PlaneMaker
                    xint version_number;           // 104014 is X-Plane 10.40b14, 115501 is 11.55r2
                    uint role;                     // 1 for master, 2 for extern visual, 3 for IOS
                    ushort port;                   // port number X-Plane is listening on
                    xchr    computer_name[500];    // the hostname of the computer
                    ushort  raknet_port;           // port number the X-Plane Raknet clinet is listening on
                 }
            */

            bool XPlanePresent = false; // Value to return

            // look for an available network.
            // X-Plane could not be broadcasting since there is no network.
            // Also, if X-Plane is running in other computer in the same network,
            // TOGA can detect and connect to its session.

            bool isThereANetwork = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

            if (isThereANetwork)
            {

                int listenPort = 49707; // Port to listen to

                // Set the multicast address 
                IPAddress multicastGroupAddress = IPAddress.Parse(MultiCastAddress);

                // Create a socket, bind and join to multicast 
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Create an endpoint
                EndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

                // Bind
                sock.Bind(groupEP);

                // Join the multicast
                MulticastOption mcastOption = new MulticastOption(multicastGroupAddress);
                sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);

                try
                {
                    while (true)
                    {
                        byte[] bytes = new byte[100]; // Holding data array
                        sock.ReceiveTimeout = 3000; // Set a considerable timeout to ensure reception
                        sock.ReceiveFrom(bytes, ref groupEP);

                        // Extract meaningful data
                        string header = Encoding.UTF8.GetString(bytes).Substring(0, 4);
                        string data = Encoding.UTF8.GetString(bytes).Substring(5, 21);

                        if (header == "BECN") // Check for the right packet header
                        {
                            XPPort = bytes[19] + bytes[20] * 256; ; // Get and set port property
                            XPIP = groupEP.ToString().Split(':')[0]; // Get IP data and set IP property
                            sock.Close();
                            XPlanePresent = true;
                            break;
                        }
                    }
                }
                catch // (SocketException e)
                {
                    // If needed, use 'e.message' to log errors
                    // Debug.WriteLine("Finding X-Plane: " + e.Message);
                    sock.Close();
                }
                finally
                {
                    sock.Close();
                }

                sock.Close();
                if (XPlanePresent)
                {
                    return XPlanePresent;  // Only returns if X-Plane is detected.
                                           // If not detected, try next solution
                }

            }

            // X-Plane was not detected in the network ( it is not broadcasting its IP/port).
            // Trying a local connection with default values.

            // Get XPLane process handle
            // If there is no X-Plane process, do not proceed.

            Process p = Process.GetProcessesByName("X-Plane").FirstOrDefault();

            if (p == null)  // There's no X-Plane process. Not proceeding
            {
                MessageBox.Show("Please, start X-PLane software first in this machine\n" +
                    " or in a networked one.", "X-Plane not running", MessageBoxButtons.OK);
                return false;
            }

            XPIP = "127.0.0.1";
            XPPort = 49000;

            // Set and send a dataRef to test if it is in a flight ( Thank You, X-PLane team! )

            if (this.SetDataRef("sim/test/test_float/", (float)1))
            {
                XPlanePresent = true;
            }

            return XPlanePresent;

        }

       

        public bool SendKey(string keyCombination)
        {
            string[] s = keyCombination.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

            int NumberOfKeytrokes = s.Length;

            IntPtr p = Process.GetProcessesByName("X-Plane")[0].MainWindowHandle;

            if (p == IntPtr.Zero)
            {
                return false;
            }

            if (NumberOfKeytrokes != 0)
            {
                int counter = 0;
                Input[] inps = new Input[NumberOfKeytrokes * 2];

                for (int i = 0; i < NumberOfKeytrokes; i++)
                {
                    Input inp = new Input();

                    ushort k;

                    if (s[i] == Keys.Control.ToString())
                    {
                        k = 0x11; // CTRL
                    }
                    else
                    {
                        if (s[i] == Keys.Shift.ToString())
                        {
                            k = 0xA0; // Shift
                        }
                        else
                        {
                            if (s[i] == Keys.Alt.ToString())
                            {
                                k = 0x12; // Alt
                            }
                            else
                            {
                                k = (ushort)s[i][0];
                            }
                        }
                    }

                    inp.type = (int)InputType.Keyboard;
                    inp.u.ki.wVk = k;
                    inp.u.ki.wScan = 0;
                    inp.u.ki.dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Unicode);
                    inp.u.ki.dwExtraInfo = GetMessageExtraInfo();

                    inps[counter] = inp;
                    counter++;
                }

                for (int i = NumberOfKeytrokes - 1; i > -1; i--)
                {
                    Input inp = new Input();

                    ushort k;

                    if (s[i] == Keys.Control.ToString())
                    {
                        k = 0x11; // CTRL
                    }
                    else
                    {
                        if (s[i] == Keys.Shift.ToString())
                        {
                            k = 0x10; // Shift
                        }
                        else
                        {
                            if (s[i] == Keys.Alt.ToString())
                            {
                                k = 0x12; // Alt
                            }
                            else
                            {
                                k = (ushort)s[i][0];
                            }
                        }
                    }

                    inp.type = (int)InputType.Keyboard;
                    inp.u.ki.wVk = k;
                    inp.u.ki.wScan = 0;
                    inp.u.ki.dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Unicode);
                    inp.u.ki.dwExtraInfo = GetMessageExtraInfo();

                    inps[counter] = inp;
                    counter++;
                }

                SetForegroundWindow(p);

                SendInput((uint)inps.Length, inps, Marshal.SizeOf(typeof(Input)));

            }
            else
            {
                return false;
            }
            return true;
        }

    }

    public class DRefs
    {

        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NorAirLabs\X-Plane\Profiles\";
        public string nl = Environment.NewLine;

        public string[] RNSReadReceivingDataRefs(string sn, string prof)
        {
            /// <summary>
            /// Reads and returns the meaningful datarefs to update RNS displays
            /// Setup of this fields is made in TOGA software at X-Plane main tab
            /// for RNS hardware serial numbers.
            /// </summary>

            string JSonFile = BasePath + prof + @"\" + sn.Trim() + ".json";

            // Array of returning RNS datarefs
            string[] RNSDataRefs = new string[6];

            // X-Plane RNS data instantiation ( see XPlaneSetup.cs )
            XPlaneSetup.RNSXPLANEDATA xplanedata = new XPlaneSetup.RNSXPLANEDATA();

            // Instantiate localy a XPlane Setup form to access its methods
            XPlaneSetup rnsXpSetup = new XPlaneSetup();

            // Set current profile settings for X-Plane Setup methods
            rnsXpSetup.currentProfile = prof;
            rnsXpSetup.currentFileName = JSonFile;

            xplanedata = rnsXpSetup.RNSLoadData(sn.Trim(), 0);

            RNSDataRefs[0] = xplanedata.NAVSTBTOREAD;
            RNSDataRefs[1] = xplanedata.NAVACTTOREAD;
            RNSDataRefs[2] = xplanedata.COMMSTBTOREAD;
            RNSDataRefs[3] = xplanedata.COMMACTTOREAD;
            RNSDataRefs[4] = xplanedata.ADFTOREAD;
            RNSDataRefs[5] = xplanedata.XPDRTOREAD;

            // Make null all invalid dataRefs
            for (int i = 0; i < RNSDataRefs.Length; i++)
            {
                if (RNSDataRefs[i].Length < 10)
                {
                    RNSDataRefs[i] = "";
                }
            }
            return RNSDataRefs;
        }

        public string[] MBx24ReadReceivingDataRefs(string sn,string prof)
        {
            /// <summary>
            /// Reads and returns the meaningful datarefs to update MBx24 annunciators
            /// Setup of this fields is made in X-Plane main tab for MBx24 hardware
            /// serial numbers
            /// </summary>

            // Array of returning MBx24 datarefs
            string[] MBx24DataRefs = new string[32];

            MBx24Form mbx24f = new MBx24Form();

            string JSonFile = BasePath + prof + @"\" + sn.Trim() + ".json";

            if (!File.Exists(JSonFile))
            {
                return null;
            }

            // X-Plane MBx24 data instantiation ( see XPlaneSetup.cs )
            XPlaneSetup.MBx24XPLANEDATA xplanedata = new XPlaneSetup.MBx24XPLANEDATA();

            // Instantiate localy a XPlane Setup form to access its methods
            XPlaneSetup mbx24XpSetup = new XPlaneSetup();

            // Set current profile settings for X-Plane Setup methods
            mbx24XpSetup.currentProfile = prof;
            mbx24XpSetup.currentFileName = JSonFile;

            xplanedata = mbx24XpSetup.MBx24LoadData(sn.Trim(), 0);

            if (xplanedata== null)
            {
                return null;
            }

            foreach (var reference in xplanedata.REFERENCES)
            {
                if (reference != null)
                {
                    MBx24DataRefs[(int)reference.CONNECTOR-1] = reference.DATAREFTOREAD;
                }
            }

            // Make empty all invalid dataRefs
            for (int i = 0; i < MBx24DataRefs.Length; i++)
            {
                if (MBx24DataRefs[i].Length < 10)
                {
                    MBx24DataRefs[i] = "";
                }
            }

            return MBx24DataRefs;

        }

        public Dictionary<string, string> RNSLoadData(string sn, string prof)
        {
            /// <summary>
            /// Loads the RNS profile datarefs to write to X-Plane.
            /// Returns a dictionary of datarefs correspondences or null
            /// </summary>

            Dictionary<string, string> DataRefsToReturn = new Dictionary<string, string>();

            string JSonFile = BasePath + prof + @"\" + sn.Trim() + ".json";

            // Array of returning RNS datarefs
            string[] RNSDataRefs = new string[6];

            // X-Plane RNS data instantiation ( see XPlaneSetup.cs )
            XPlaneSetup.RNSXPLANEDATA xplanedata = new XPlaneSetup.RNSXPLANEDATA();

            // Instantiate localy a XPlane Setup form to access its methods
            XPlaneSetup rnsXpSetup = new XPlaneSetup();

            // Set current profile settings for X-Plane Setup methods
            rnsXpSetup.currentProfile = prof;
            rnsXpSetup.currentFileName = JSonFile;

            xplanedata = rnsXpSetup.RNSLoadData(sn.Trim(), 0);

            if (xplanedata == null)
            {
                return null;
            }

            DataRefsToReturn.Add("ADFGHOSTBUTTON",xplanedata.ADFGHOSTBUTTON);
            DataRefsToReturn.Add("XPDRGHOSTBUTTON",xplanedata.XPDRGHOSTBUTTON);
            DataRefsToReturn.Add("COMMACTTOWRITE",xplanedata.COMMACTTOWRITE);
            DataRefsToReturn.Add("COMMSTBTOWRITE",xplanedata.COMMSTBTOWRITE);
            DataRefsToReturn.Add("NAVACTTOWRITE",xplanedata.NAVACTTOWRITE);
            DataRefsToReturn.Add("NAVSTBTOWRITE",xplanedata.NAVSTBTOWRITE);
            DataRefsToReturn.Add("ADFTOWRITE",xplanedata.ADFTOWRITE);
            DataRefsToReturn.Add("XPDRTOWRITE",xplanedata.XPDRTOWRITE);

            return DataRefsToReturn;
        }

    }
}
