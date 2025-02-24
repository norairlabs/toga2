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
using static NorAirXPlane.Connect;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NorAirMSFS
{
    /// <summary>
    /// The Northern Aircraft Labs namespace for Microsoft Flight Simulator
    /// </summary>
    /// 

    public class Connect
    {

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

        public class RNSRacius
        {
            public int X_1000 = 1000;
            public int X_100 = 100;
            public int X_10 = 10;
            public int X_1 = 1;
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
            public SimEventType EventIDType { get; set; }
            public int Multiplier { get; set; }

        }

        public enum GroupType
        {
            AutoPilot,
            BreakLanding,
            Electric,
            Engine,
            Fuel,
            Heli,
            Radios,
            Systems,
            Others
        }
        public class SimVarEntity
        {
            public GroupType GroupType { get; set; }
            public string SimVar { get; set; }
            public string Tag { get; set; }

        }

        // Path for datarefs defs ( Usually 'My Documents' folder )
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NorAirLabs";

        // MSFS paths & files
        public static string MSFSPath = BasePath + @"\MSFS"; //                             MSFS defs path
        public static string MSFSProfilesPath = MSFSPath + @"\Profiles"; //                 MSFS profiles base path
        public static string MSFSDefaultProfile = MSFSPath + @"\DefaultProfile.json"; //    MSFS default profile

        // TOGA definitions files
        public string PeripheralsFileName = BasePath + ".\\Peripherals.json"; //    Declared peripherals
        public string BackLitFileName = BasePath + ".\\BackLit.json"; //            General backlit defs
        public string StylesFileName = BasePath + ".\\Styles.json"; //              Toga environment defs

        public string filename;

        public List<SimVarEntity> RetrieveSimVarsGroups()
        {
            List<SimVarEntity> simVars = new List<SimVarEntity>();

            try
            {
                filename = @"D:\C#\TOGA\SIMVARS_AUTOPILOT.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.AutoPilot, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_BRAKELANDING.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.BreakLanding, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_ELECTRIC.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Electric, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_ENGINE.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Engine, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_FUEL.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Fuel, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_HELI.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Heli, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_RADIOS.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Radios, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_SYSTEMS.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string[] sc = new string[2];
                                sc = line.Split(new char[] { ',' });
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Systems, SimVar = sc[0], Tag = sc[1] });
                            }

                        }
                    }
                }
                filename = @"D:\C#\TOGA\SIMVARS_OTHERS.txt";
                if (File.Exists(filename))
                {

                    string lines = File.ReadAllText(filename);
                    if (lines.Length != 0)
                    {
                        List<string> l = lines.Split('\n').ToList();
                        foreach (string line in l)
                        {
                            if (line.Length > 3)
                            {
                                string ll = string.Concat(line.Where(c => Char.IsLetterOrDigit(c) || c.Equals(' ') || c.Equals(':')));
                                simVars.Add(new SimVarEntity { GroupType = GroupType.Others, SimVar = ll, Tag = "" });
                            }

                        }
                    }
                }
            }
            catch // (Exception e)
            {
                Say("One SIMVARS file was malformed:\n" + filename);
            }


            if (simVars.Count != 0)
            {
                return simVars;
            }
            return null;

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
