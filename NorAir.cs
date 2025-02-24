using System;

using System.Management;
using System.Drawing.Text;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TOGA;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using NALib;

/* Please, read this carefully:
        
            *** DISCLAIMER ***
            
            *** THE SOFTWARE AND RELATED DOCUMENTATION IS PROVIDED “AS IS”, WITHOUT WARRANTY
            *** OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
            *** OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
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
            *** This software is an open-source MIT licensed for users to enjoy and
            *** as an example for the use of our SDK.You may distribute, change and even
            *** sell as far as the Northern Aircraft Labs and GAMAN Portugal is properly
            *** referenced and MIT license followed. See https://opensource.org/license/MIT
            *** 
            *** Please, take some time to read this software and try to understand it before
            *** adapt to your needs or making any changes. It has plenty of comments to ease
            *** its reading.
            ***
            *** Also, read the documentation about SDK and NorAirFramework protocol at
            *** https://www.norairlabs.com
                        
 */

namespace NorAir
{
    /// <summary>
    /// The Northern Aircraft Labs namespace
    /// </summary>
    /// 
    /// This namespace includes the following main classes:
    /// 
    ///     OBCS class 
    ///                 |
    ///                 +- SetCPORT
    ///                 +- getAvailablePorts
    ///                 +- FindOBCS
    ///					+- Enumerate
    ///					+- ReportEnum
    ///					+- SetPort
    ///					+- Open
    ///					+- Close
    ///					+- Send
    ///					+- Flush
    ///					+- Reset
    ///					+- EnumerateToJPORT
    ///					+- JPortReportEnum
    ///
    ///		JPORT class
    ///					|
    ///					+- HardReset
    ///					+- SoftReset
    ///					+- ReportAllButtons
    ///					+- DecodeButtons
    ///					+- SetButtonType
    ///					+- SetButtonsState
    ///
    ///		WALL class
    ///					|
    ///					+- BackLit
    ///					+- BackLitLed
    ///					+- HardReset
    ///					+- SoftReset
    ///
    ///		RNS class
    ///					|
    ///					+- SetSerialNumber
    ///					+- SetId
    ///					+- ValidateProperties
    ///					+- CreateEnumerationData
    ///					+- Ping
    ///					+- HardReset
    ///					+- SoftReset
    ///					+- BackLit
    ///					+- BackLitLed
    ///					+- FreqReport
    ///					+- Extract
    ///					+- ExtractSquawk
    ///					+- SetNAVActiveFrequency
    ///					+- SetCOMMActiveFrequency
    ///					+- SetADFFrequency
    ///					+- SetNAVStandByFrequency
    ///					+- SetCOMMStandByFrequency
    ///					+- SetNAVIntegerIncrement
    ///					+- SetNAVDecimalIncrement
    ///					+- SetCOMMIntegerIncrement
    ///					+- SetCOMMDecimalIncrement
    ///					+- SetADFIntegerIncrement
    ///					+- SetADFDecimalIncrement
    ///					+- SetNAVMinimumFrequency
    ///					+- SetCOMMMinimumFrequency
    ///					+- SetADFMinimumFrequency
    ///					+- SetNAVMaximumFrequency
    ///					+- SetCOMMMaximumFrequency
    ///					+- SetADFMaximumFrequency
    ///					+- SetXPDRIntegerIncrement
    ///					+- SetXPDRDecimalIncrement
    ///					+- SetXPDRMinimumSquawk
    ///					+- SetXPDRMaximumSquawk
    ///					+- SetXPDRSquawkCode
    ///					+- GetSquawk
    ///					+- SetDigitLuminosity
    ///					+- SetButtonNumber
    ///					+- SetButtonType
    ///
    ///		MBx24 class
    ///					|
    ///					+- SetSerialNumber
    ///					+- SetId
    ///					+- ValidateProperties
    ///					+- CreateEnumerationData
    ///					+- Ping
    ///					+- SetButtonNumber
    ///					+- SetButtonType
    ///					+- SetFirstButtonNumber
    ///					+- ReportEncoders
    ///					+- SetOutput
    ///					+- ToggleOutput
    ///					+- SetOutputType

    public enum TalkMode
    {
        WaitForAnAnswer,
        SendOnly
    }


    public class OBCS
    {
        /// <summary>
        /// The On Board Computer System
        /// </summary>
        /// 
        /// The On Board Computer System is a dual-processor computer that implements
        /// a peripheral management engine, a joystick and its interface ( the J-PORT )
        /// and other relevant functionalities.
        /// 
        /// The 'OBCS' term is the abbreviation of 'On Board Computer System'.
        /// 
        /// Simple to operate, this computer system is reached by a HID COM port that
        /// makes it easily available by any computer language and operating system flavour.
        /// 
        /// Using the NorAirFrame protocol, one can send, receive, setup, etc. the OBCS
        /// itself, the J-PORT and, of course, all peripherals attached.
        /// 
        /// To function with peripheral, the OBCS has an internal translation table that
        /// matched peripherals serial numbers to an ID number. This ID must be supplied
        /// to the OBCS by an enumeration procedure where the OBCS checks for existence
        /// of the peripheral and its capabilities. This has to be executed after every
        /// power up or OBCS hard-reset. This IDs are easier and faster to manage. 
        /// IDs can be seen as alias or callsigns for a serial number.
        /// 
        /// Even the OBCS and the J-PORT have their own IDs ( 98 and 10 respectively )
        /// and these are reserved and non-enumerable.
        /// 
        /// After a sucessfull enumeration, the OBCS immediatly starts talking to the
        /// peripherals using the supplied IDs. From this moment, all peripherals
        /// are recognized by their own IDs and instruction are constructed using this
        /// IDs. This can be checked by the activity front panel leds on the OBCS case.
        /// 
        /// OBCS peripherals have IDs and first 2 digit of serial numbers from 21 to 98.
        /// ID 99 it is reserved for OBCS itself.
        /// 
        /// J-PORT peripherals have IDs and first 2 digit of serial numbers from 2 to 9
        /// and 11 to 20, being 1 and 10 reserved, so not usable.
        /// 
        /// For further reading and in-depth knowledge, please visit www.norairlabs.com.

        public SerialPort CPort = new SerialPort();
        public string CPortName;
        public bool Opened = false;
        public bool Found = false;
        public bool debug = false;


        public const byte Me = 99;
        public const byte JPort = 10;
        public const byte Ping = 6;
        public const byte ResetCommand = 2;
        public const byte ReportEnumeration = 18;


        public OBCS() // Empty Construtor
        {

        }

        public void SetCPORT(string portName)
        {
            /// <summary>
            /// Port assignment
            /// </summary>

            this.CPortName = portName;
        }




        public string FindOBCS()
        {

            /// <summary>
            /// Locate OBCS hardware
            /// </summary>
            /// 

            NALib.LookingForOBCS OBCSPort = new NALib.LookingForOBCS();

            Found = false;
            CPortName = "";

            string com = OBCSPort.FindOBCS().Trim();

            if (com != null)
            {
                if (com.Trim().Length != 0)
                {
                    Found = true;
                    CPortName = com;
                    return com;
                }
            }

            return null;

        }

        public void Show(string s, byte[] data) // For debug purpose
        {
            /// <summary>
            /// For debug purposes, if the contents of an byte array needed to be displayed.
            /// </summary>

            Debug.Write(s);
            foreach (byte b in data)
            {
                Debug.Write(b.ToString() + " ");
            }
            Debug.WriteLine("");
        }

        public bool Enumerate(byte[] data)
        {
            /// <summary>
            /// OBCS enumeration and peripheral registration
            /// </summary>

            if (this.Opened)
            {
                this.Flush(); // Flush buffers
                byte[] portAnswer = this.Send(data); // Send an enumeration data sequence
                if (portAnswer != null)
                {
                    if (portAnswer[0] != 0)
                    {
                        return true; // Sucessful enumeration
                    }
                }

            }
            return false;
        }

        public byte[] ReportEnum()
        {
            /// <summary>
            /// Reports if an enumeration is registered by the OBCS
            /// </summary>

            if (this.Opened)
            {
                byte[] EnumToken = { Me, ReportEnumeration };
                byte[] EnumData = this.Send(EnumToken);
                if (EnumData != null)
                {
                    if (EnumData[0] == 0)
                    {
                        return null;
                    }
                    int numberOfPeripheralsEnumerated = (int)EnumData[0];
                    byte[] PeripheralsEnumeratedId = new byte[numberOfPeripheralsEnumerated];
                    for (int i = 0; i < numberOfPeripheralsEnumerated; i++)
                    {
                        PeripheralsEnumeratedId[0] = EnumData[i + 1];
                    }
                    return PeripheralsEnumeratedId;
                }
                return null;
            }
            return null;
        }

        public void SetPort(string port)
        {
            /// <summary>
            /// OBCS port setup
            /// </summary>

            CPort.PortName = port;
            CPort.BaudRate = 115200;
            CPort.Parity = Parity.None;
            CPort.DataBits = 8;
            CPort.StopBits = StopBits.One;
            CPort.Handshake = Handshake.None;
            CPort.ReadTimeout = 1000;
            CPort.WriteTimeout = 1000;
            CPort.ReadBufferSize = 32;
            CPort.WriteBufferSize = 32;
        }

        public bool Open()
        {
            /// <summary>
            /// Opens CPORT
            /// </summary>

            this.Opened = false;
            if (CPortName == null)
            {
                return false;
            }

            if (CPortName.Length == 0)
            {
                return false;
            }

            CPort.Close();
            SetPort(CPortName);
            CPort.Open();
            if (CPort.IsOpen)
            {
                this.Opened = true;
                return true;
            }
            return false;
        }

        public void Close()
        {
            /// <summary>
            /// Closes CPORT
            /// </summary>

            this.Opened = false;
            CPort.Close();
        }

        public byte[] Send(byte[] data, TalkMode WaitOrNot = TalkMode.WaitForAnAnswer)
        {
            /// <summary>
            /// Sends data to CPORT. By default waits for an answer immediatly after
            /// </summary>

            if (data == null)
            {
                return null;
            }
            if (CPort.IsOpen)
            {
                int trials = 100000; // Number of read trials
                byte[] portAnswer = new byte[32];

                lock (CPort)        // For thread sake
                {
                    this.Flush();   //  Flush old data in buffers
                    if (CPort.IsOpen) // Again because 'lock' statement doesn't prevent from closing the port. Just in case...
                    {
                        CPort.Write(data, 0, data.Length);  // Send message

                        if (WaitOrNot == TalkMode.SendOnly) // If no answer is expected, return
                        {
                            return null;
                        }

                        while (trials != 0)                    // Wait for the answer 'counter' times
                        {
                            if (trials == 0)
                            {
                                return null;
                            }

                            trials--;
                            if (CPort.IsOpen) // Again because 'lock' statement doesn't prevent from closing the port. Just in case...
                            {
                                int toRead = CPort.BytesToRead; // Number of bytes in the read buffer

                                if (toRead == 32)   // Messages are always 32 bytes long
                                {

                                    CPort.Read(portAnswer, 0, toRead); // Read the answer

                                    if (debug)
                                    {
                                        Show("Read (" + toRead.ToString() + " bytes): ", portAnswer);
                                    }
                                    return portAnswer;
                                }
                            }
                            //Thread.Sleep(1);
                            //Debug.WriteLine(trials.ToString());
                        }
                    }
                }
            }
            return null;
        }

        public void Flush()
        {
            /// <summary>
            /// Flushes CPORT buffers
            /// </summary>

            if (this.Opened)
            {
                CPort.DiscardInBuffer();
                CPort.DiscardOutBuffer();
                CPort.ReadExisting();
            }
        }



        public void Reset()
        {
            /// <summary>
            /// Hardresets CPORT only, not peripherals
            /// Enumeration is lost. A new enumeration must be done.
            /// </summary>

            if (this.Opened)
            {
                byte[] resetCMD = { Me, ResetCommand };
                Send(resetCMD, TalkMode.SendOnly); // Note: CPORT is closed automatically after hardreset
                Thread.Sleep(500); // Wait 500 msecs to revive
                this.Opened = false;
            }
        }

        /*============= J-PORT Specific methods ===============*/

        public bool EnumerateToJPORT(byte[] data)
        {
            /// <summary>
            /// J-PORT enumeration and peripheral registration.
            /// Although similiar to OBCS enumeration, and for convenience, it is
            /// made available a renamed method to J-PORT to keep focus aside.
            /// </summary>

            if (this.Opened)
            {
                this.Flush(); // Flush buffers
                byte[] portAnswer = this.Send(data); // Send an enumeration data sequence
                if (portAnswer != null)
                {
                    if (portAnswer[0] != 0)
                    {
                        return true; // Sucessful enumeration
                    }
                }

            }
            return false;
        }

        public byte[] JPortReportEnum()
        {
            /// <summary>
            /// Reports if an enumeration is registered by the J-PORT
            /// </summary>

            if (this.Opened)
            {
                byte[] EnumToken = { JPort, ReportEnumeration };
                byte[] EnumData = this.Send(EnumToken);
                if (EnumData != null)
                {
                    if (EnumData[0] == 0)
                    {
                        return null;
                    }
                    int numberOfPeripheralsEnumerated = (int)EnumData[0];
                    byte[] PeripheralsEnumeratedId = new byte[numberOfPeripheralsEnumerated];
                    for (int i = 0; i < numberOfPeripheralsEnumerated; i++)
                    {
                        PeripheralsEnumeratedId[0] = EnumData[i + 1];
                    }
                    return PeripheralsEnumeratedId;
                }
                return null;
            }
            return null;
        }


    }
    public class JPORT
    {
        /// <summary>
        /// J-Port is the interface port for a 200 button, 9 axes joystick.
        /// It constitues an important device hardware integrated in the OBCS. 
        /// </summary>
        /// 
        /// As mentioned, the J-Port is a gate to an internally managed joystick
        /// as part of the On Board Computer System. It has its own processor for
        /// fast performance. As an OBCS component, the J-PORT is accessed through
        /// the OBCS. So, even if no peripheral is attached to OBCS, one can talk to
        /// the J-PORT and play arround with it.
        /// 
        /// The hardware is plug'n'play and no special drivers are required.
        /// 
        /// The J-Port is accessible by a messaging system to perform actions,
        /// setups or report usefull data. It is a quick and versatile system
        /// with a very easy way to talk to.
        /// 
        /// A button manager allows setting up button types and access to
        /// functionalities like instant full buttons report status.
        /// 
        /// Take in mind that button numbering translation, aka button reassignement,
        /// is not managed by the joystick but directly by the peripherals. Each peripheral
        /// manages its own translation map, freeing the joystick for other tasks
        /// and speed of operation.
        /// 
        /// Whenever 'J-PORT' or 'Joystick' terms are used, although different entities,
        /// may be perceive as the same ones as they're part of the same hardware.
        /// 
        /// Similar to OBCS, the J-PORT has its own enumeration table. This supports
        /// peripherals that are directly managed by the J-PORT like peripherals that
        /// use analog axes, etc.
        /// One of this peripherals are the NorAir Labs Throttle hardware set which
        /// implements a very fast feedback engine aimed directly to the J_PORT.
        /// 
        /// This enumeration mechanism is similar to the OBCS one and is made through
        /// this.
        /// 
        /// 



        // Constants for J-PORT commands

        public const byte Me = 10;
        public const byte HardResetCmd = 2;
        public const byte SoftResetCmd = 4;
        public const byte PingCmd = 6;
        public const byte InsertToEnumerationCmd = 7;
        public const byte ChangeButtonTypeCmd = 17;
        public const byte ReportAllButtonsCmd = 12;
        public const byte SetButtonsStateCmd = 64;

        public JPORT()  // An empty construtor
        {

        }

        public byte[] HardReset()
        {
            /// <summary>
            /// Hardreset to J-PORT
            /// </summary>

            byte[] toSend = new byte[2];
            toSend[0] = Me;
            toSend[1] = HardResetCmd;
            return toSend;
        }

        public byte[] SoftReset()
        {
            /// <summary>
            /// Softreset to J-PORT
            /// </summary>

            byte[] toSend = new byte[2];
            toSend[0] = Me;
            toSend[1] = SoftResetCmd;
            return toSend;
        }

        public byte[] ReportAllButtons()
        {
            /// <summary>
            /// Retrieve all buttons logical state in bulk
            /// </summary>
            /// The J-PORT returns and array of bytes with the following structure:
            /// 
            /// byte 0: '10' meaning this is an answer from J-PORT
            /// 
            /// byte 1: '12' meaning this is the 'ReportAllButtons' answer
            /// 
            /// byte 2 to byte 25: Clusters of 8 buttons in each byte. As so, the 9th button
            /// corresponds to the first bit of the byte 3. 
            /// 
            /// Bit values of 0 means 'OFF' or 'not pressed button', value of 1 means 'ON' or
            /// 'pressed button'
            /// 
            /// 

            byte[] toSend = new byte[2];
            toSend[0] = Me;
            toSend[1] = ReportAllButtonsCmd;
            return toSend;
        }

        public int[] DecodeButtons(byte[] ButtonsAsBits)
        {
            /// <summary>
            /// J-Port buttons decode.
            /// </summary>
            /// 
            /// J-PORT answers to 'ReportAllButtons' instruction with an array of bytes.
            /// Its 1st two bytes, the header, have the values of '10' and '12' respectly,
            /// meaning '10' as a J-PORT answer to the '12' instruction.
            /// 
            /// The remaining 25 bytes hold the buttons logic state in bits form. This
            /// method returns an easier to handle array of integers with the buttons
            /// logic state.

            if (ButtonsAsBits == null) // Not data was passed as argument
            {
                return null;
            }

            if (ButtonsAsBits.Length == 0) // Not a valid array
            {
                return null;
            }

            int[] buttonsState = new int[200];
            byte[] JPortData = new byte[25];

            System.Buffer.BlockCopy(ButtonsAsBits, 2, JPortData, 0, 25); // Remove header data & copy array

            for (int i = 0; i != 25; i++)
            {
                byte eightButtons = JPortData[i];

                for (int j = 0; j != 8; j++)
                {
                    if ((eightButtons & 1) == 1)
                    {
                        buttonsState[8 * i + j] = 1;
                    }
                    eightButtons = (byte)(eightButtons >> 1);
                }

            }
            return buttonsState;
        }

        public byte[] SetButtonType(byte buttonNumber, byte buttonType)
        {
            /// <summary>
            /// Changes the behaviour (type) of a button
            /// </summary>
            /// 
            /// - Important remarks:
            ///     It is advisable to read the answer to check if the requests was fulfilled.
            ///     if not, request again.
            ///     
            ///     This buttons setup is concurrent with the same setup made in a peripheral.
            ///     If in a peripheral a button as been configured or reassigned, those procedures
            ///     are valid here and, as so, not needed to be repeated in the J-PORT.
            ///     Attention must be taken if a button is first configured here and reassigned
            ///     later by a peripheral to not mix up concepts or working methods.
            ///
            ///     Button numbers must be between 1 and 200
            ///     
            ///     Button type valid options:
            ///         0-Push button
            ///         1-Toggle button
            ///         2-Logically inverted push button
            ///         3-Logically inverted toggle button
            ///         
            ///     Changes can only be made after enumeration. They reset to default after hard-reset
            ///     or on Power ON.
            ///     
            ///     Despite default buttons 1 and 2 could be reassigned and mapped, they have
            ///   internal functions, if a RNS is present, that will be performed no matter what.
            /// 

            if (buttonType > 3)
            {
                buttonType = 0; // Resets to default push button
            }
            byte[] com = new byte[4];
            com[0] = Me;
            com[1] = ChangeButtonTypeCmd;
            com[2] = buttonNumber;
            com[3] = buttonType;
            return com;
        }

        public byte[] SetButtonsState(byte[] buttonsNumbers, byte[] states)
        {
            /// <summary>
            /// Sets the logical level of a group of buttons
            /// </summary>
            ///
            /// This method changes/sets the logical state of a group of
            /// buttons.
            /// 
            /// - Important remarks:
            /// 
            /// Care should be taken when setting a button state. Because a button number can
            /// be reassigned at any time by a peripheral, that will be reflected here. So,
            /// first make any button number reassignment in peripherals and use this
            /// functionality after.
            /// 
            /// One can set buttons states to a maximum of 14 buttons at a time. if more buttons
            /// state have to be changed, repeat the command for the remaining buttons and values
            /// as much times as needed.
            /// 
            /// There is no answer for this command so Send with TalkMode.SendOnly must be used
            /// to prevent timeout errors and execution delays.
            /// 

            int numberOfButtonsToSet = buttonsNumbers.Length;

            if (numberOfButtonsToSet > 14)
            {
                return null;
            }

            if (states.Length != numberOfButtonsToSet)
            {
                return null;
            }

            byte[] toSend = new byte[numberOfButtonsToSet + 3];
            int counter = 3;

            toSend[0] = Me;
            toSend[1] = SetButtonsStateCmd;
            toSend[2] = (byte)numberOfButtonsToSet;

            for (int i = 0; i != numberOfButtonsToSet; i++)
            {
                toSend[counter] = buttonsNumbers[i];
                counter++;
                toSend[counter] = states[i];
                counter++;
            }

            return toSend;

        }


    }


    public class WALL
    {
        /// <summary>
        /// WALL - a message broadcast system
        /// </summary>

        // == IMPORTANT REMARKS ON WALL =============================
        //
        // WALL is a virtual message broadcast system that enables sending one message to all peripherals.
        // As so, it is ONLY FOR SENDING messages. DO NOT EXPECT FOR AN ANSWER. It may hang on reading
        // a possible answer, althougth a timeout error may be thrown.
        // Nevertheless, as no confirmation can be acquired programmatically, the WALL command can be issued
        // more than once to achieve the goal.
        //
        // Usage example:
        //
        //    NorAir.WALL shout = new NorAir.WALL(); // Constructor
        //    CPORT.write(shout.Backlit(0xFF,0,0,0)); // Set all backlits to RED.
        //
        //     == The use of the instruction 'CPORT.write' is the most safe procedure ==
        //
        //    As an alternative, CPORT.Send can be used but used with the following syntax,
        //    including the 'SendOnly' argument:
        //
        //    CPORT.Send(shout.Backli(0xFF,0,0,0), SendOnly ); // Just send, don't wait for an answer
        //
        //  'SendOnly' argument instructs CPORT.Send method to not wait for an answer from the peripheral.
        //

        public const byte wall = 1;
        public const byte HardResetCmd = 2;
        public const byte SoftResetCmd = 4;
        public const byte BaklitColorCmd = 41;
        public const byte BaklitLedColorCmd = 42;

        public WALL() // An empty constructor
        {

        }

        public byte[] BackLit(byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Broadcasts RGBW color features to all peripherals
            /// </summary>

            byte[] toSend = new byte[6];
            toSend[0] = wall;
            toSend[1] = BaklitColorCmd;
            toSend[2] = R;
            toSend[3] = G;
            toSend[4] = B;
            toSend[5] = W;
            return toSend;
        }

        public byte[] BackLitLed(byte Led, byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Broadcasts RGBW color features to the same led number on all peripherals
            /// </summary>

            byte[] toSend = new byte[7];
            toSend[0] = wall;
            toSend[1] = BaklitLedColorCmd;
            toSend[1] = Led;
            toSend[3] = R;
            toSend[4] = G;
            toSend[5] = B;
            toSend[6] = W;
            return toSend;
        }

        public byte[] HardReset()
        {
            /// <summary>
            /// Broadcasts hardreset to all peripherals
            /// </summary>

            byte[] toSend = new byte[2];
            toSend[0] = wall;
            toSend[1] = HardResetCmd;
            return toSend;
        }

        public byte[] SoftReset()
        {
            /// <summary>
            /// Broadcasts softreset to all peripherals
            /// </summary>

            byte[] toSend = new byte[2];
            toSend[0] = wall;
            toSend[1] = SoftResetCmd;
            return toSend;
        }
    }


    public class RNS
    {
        /// <summary>
        /// Radio & Navigation Systems
        /// </summary>

        // The Radio & Navigation Systems includes COMM, NAV, ADF radios and a transponder.
        // 
        // Radio instruments are coded for frequency setting/retrieving as such:
        // 0:  NAV
        // 1:  COMM
        // 2:  ADF
        // 
        // Care should be taken setting defaults for maximum, minimum and frequency/squawk.
        // No done properlly, may head to unexpected behaviour. In this case, a soft reset
        // restores factory values. Shutting down the equipment also resets any changes made.
        //
        // The transponder is treated separately
        // 

        public enum Instruments
        {
            ActiveCOMM,
            StandByCOMM,
            ActiveNAV,
            StandByNAV,
            ADF,
            XPDR
        }

        public string SerialNumber = null;
        public byte Id = 31;
        public byte[] enumerationData = new byte[8];

        // Constants for RNS commands
        public const byte NAV = 0;
        public const byte COMM = 1;
        public const byte ADF = 2;
        public const byte OBCS = 99;
        public const byte Joystick = 10;
        public const byte HardResetCmd = 2;
        public const byte SoftResetCmd = 4;
        public const byte PingCmd = 6;
        public const byte InsertToEnumerationCmd = 7;
        public const byte InstrumentsBrightnessCmd = 40;
        public const byte BaklitColorCmd = 41;
        public const byte BaklitLedColorCmd = 42;
        public const byte GetFrequenciesCmd = 72;
        public const byte ActiveFrequencyCmd = 20;
        public const byte StandbyFrequencyCmd = 21;
        public const byte IntegerIncrementCmd = 22;
        public const byte DecimalIncrementCmd = 23;
        public const byte MinimumFrequencyCmd = 24;
        public const byte MaximumFrequencyCmd = 25;
        public const byte XpdrIntegerIncrementCmd = 32;
        public const byte XpdrDecimalIncrementCmd = 33;
        public const byte MinimumSquawkCmd = 34;
        public const byte MaximumSquawkCmd = 35;
        public const byte SetSquawkCmd = 38;
        public const byte GetSquawkCodeCmd = 73;
        public const byte ChangeButtonNumberCmd = 80;
        public const byte ChangeButtonTypeCmd = 17;




        public RNS()
        {

        }

        public RNS(string SerialNumber, byte Id) // Constructor
        {
            if (this.ValidateProperties(SerialNumber, Id))
            {
                this.SerialNumber = SerialNumber;
                this.Id = Id;
                CreateEnumerationData();
            }
            else
            {
                throw new ArgumentException("Invalid serial number or ID");
            }
        }

        public bool SetSerialNumber(string SerialNumber)
        {
            /// <summary>
            /// Sets the RNS object serial number
            /// </summary>

            if (ValidateProperties(this.SerialNumber, this.Id))
            {
                this.SerialNumber = SerialNumber;
                CreateEnumerationData();
                return true;
            }
            return false;
        }

        public bool SetId(byte Id)
        {
            /// <summary>
            /// Sets the RNS object ID calling number
            /// </summary>

            if (Id > 20 && Id < 99)
            {
                this.Id = Id;
                CreateEnumerationData();
                return true;
            }
            return false;
        }

        public void Show(string s, byte[] data)
        {
            /// <summary>
            /// For debug purpose, to show data from byte arrays
            /// </summary>

            if (data == null)
            {
                Debug.WriteLine("No data");
                return;
            }
            Debug.Write(s);
            foreach (byte b in data)
            {
                Debug.Write(b.ToString() + " ");
            }
            Debug.WriteLine("");
        }

        public bool ValidateProperties(string serialNumber, byte Id)
        {
            /// <summary>
            /// Validates serial and ID numbers
            /// </summary>

            if (serialNumber.Length == 10)
            {
                if (serialNumber.StartsWith("31"))
                {
                    if (Double.Parse(serialNumber) != 0)
                    {
                        if (Id == 0)
                        {
                            Id = 31;
                        }
                        if (Id > 20 && Id < 99)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CreateEnumerationData()
        {
            /// <summary>
            /// Sets the RNS object enumeration data for OBCS enumeration
            /// Not evocable
            /// </summary>

            this.enumerationData[0] = OBCS;
            this.enumerationData[1] = InsertToEnumerationCmd;
            this.enumerationData[2] = (byte)(int.Parse(this.SerialNumber.Substring(0, 2)));
            this.enumerationData[3] = (byte)(int.Parse(this.SerialNumber.Substring(2, 2)));
            this.enumerationData[4] = (byte)(int.Parse(this.SerialNumber.Substring(4, 2)));
            this.enumerationData[5] = (byte)(int.Parse(this.SerialNumber.Substring(6, 2)));
            this.enumerationData[6] = (byte)(int.Parse(this.SerialNumber.Substring(8, 2)));
            if (this.Id == 0)
            {
                this.Id = 31;
            }
            this.enumerationData[7] = this.Id;
        }

        public byte[] Ping()
        {
            /// <summary>
            /// Returns the RNS object ping command
            /// </summary>

            byte[] com = { this.Id, PingCmd };
            return com;
        }

        public byte[] HardReset()
        {
            /// <summary>
            /// Returns the RNS object hard reset command
            /// </summary>

            byte[] com = { this.Id, HardResetCmd };
            return com;
        }

        public byte[] SoftReset()
        {
            /// <summary>
            /// Returns the RNS object soft reset command
            /// </summary>

            byte[] com = { this.Id, SoftResetCmd };
            return com;
        }

        public byte[] BackLit(byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Broadcasts RGBW color features to all leds
            /// </summary>

            byte[] toSend = new byte[6];
            toSend[0] = this.Id;
            toSend[1] = BaklitColorCmd;
            toSend[2] = R;
            toSend[3] = G;
            toSend[4] = B;
            toSend[5] = W;
            return toSend;
        }

        public byte[] BackLitLed(byte led, byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Broadcasts RGBW color features to one led
            /// </summary>

            byte[] toSend = new byte[7];
            toSend[0] = this.Id;
            toSend[1] = BaklitLedColorCmd;
            toSend[2] = led;
            toSend[3] = R;
            toSend[4] = G;
            toSend[5] = B;
            toSend[6] = W;
            return toSend;
        }

        public byte[] FreqReport()
        {
            /// <summary>
            /// Returns the report all frequencies at once command
            /// </summary>

            byte[] com = { (byte)this.Id, GetFrequenciesCmd };
            return com;
        }


        public byte[] SetNAVActiveFrequency(int intValue, int decValue)
        {
            /// <summary>
            /// Sets NAV active frequency command
            /// </summary>
            string i = intValue.ToString("D3");
            string d = decValue.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = ActiveFrequencyCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMActiveFrequency(int intValue, int decValue)
        {
            /// <summary>
            /// Sets COMM active frequency command
            /// </summary>
            string i = intValue.ToString("D3");
            string d = decValue.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = ActiveFrequencyCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetADFFrequency(int frequency)
        {
            /// <summary>
            /// Sets ADF frequency command
            /// </summary>
            string i = frequency.ToString("D4");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = ActiveFrequencyCmd;
            com[2] = ADF;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(i.Substring(3, 1));
            com[7] = 0;
            com[8] = 0;
            return com;
        }

        public byte[] SetNAVStandByFrequency(int intValue, int decValue)
        {
            /// <summary>
            /// Sets NAV active frequency command
            /// </summary>
            string i = intValue.ToString("D3");
            string d = decValue.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = StandbyFrequencyCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMStandByFrequency(int intValue, int decValue)
        {
            /// <summary>
            /// Sets COMM active frequency command
            /// </summary>
            string i = intValue.ToString("D3");
            string d = decValue.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = StandbyFrequencyCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetNAVIntegerIncrement(int increment)
        {
            /// <summary>
            /// Sets NAV integer increment
            /// </summary>
            string i = increment.ToString("D3");
            byte[] com = new byte[6];
            com[0] = (byte)this.Id;
            com[1] = IntegerIncrementCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            return com;
        }

        public byte[] SetNAVDecimalIncrement(int increment)
        {
            /// <summary>
            /// Sets NAV decimal increment
            /// </summary>
            string i = increment.ToString("D3");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = DecimalIncrementCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMIntegerIncrement(int increment)
        {
            /// <summary>
            /// Sets COMM integer increment
            /// </summary>
            string i = increment.ToString("D3");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = IntegerIncrementCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMDecimalIncrement(int increment)
        {
            /// <summary>
            /// Sets COMM decimal increment
            /// </summary>
            string i = increment.ToString("D3");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = DecimalIncrementCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            return com;
        }

        public byte[] SetADFIntegerIncrement(int increment)
        {
            /// <summary>
            /// Sets ADF increment for the 3rd and 4th digits
            /// </summary>
            string i = increment.ToString("D2");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = IntegerIncrementCmd;
            com[2] = ADF;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = 0;
            return com;
        }

        public byte[] SetADFDecimalIncrement(int increment)
        {
            /// <summary>
            /// Sets ADF increment for the 1st and 2nd digits
            /// </summary>
            string i = increment.ToString("D2");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = DecimalIncrementCmd;
            com[2] = ADF;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = 0;
            return com;
        }

        public byte[] SetNAVMinimumFrequency(int intFrequency, int decFrequency)
        {
            /// <summary>
            /// Sets NAV minimum frequency
            /// </summary>
            string i = intFrequency.ToString("D3");
            string d = decFrequency.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MinimumFrequencyCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMMinimumFrequency(int intFrequency, int decFrequency)
        {
            /// <summary>
            /// Sets COMM minimum frequency
            /// </summary>
            string i = intFrequency.ToString("D3");
            string d = decFrequency.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MinimumFrequencyCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetADFMinimumFrequency(int Frequency)
        {
            /// <summary>
            /// Sets ADF minimum frequency
            /// </summary>
            string i = Frequency.ToString("D4");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MinimumFrequencyCmd;
            com[2] = ADF;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(i.Substring(3, 1));
            com[7] = 0;
            com[8] = 0;
            return com;
        }

        public byte[] SetNAVMaximumFrequency(int intFrequency, int decFrequency)
        {
            /// <summary>
            /// Sets NAV maximum frequency
            /// </summary>
            string i = intFrequency.ToString("D3");
            string d = decFrequency.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MaximumFrequencyCmd;
            com[2] = NAV;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetCOMMMaximumFrequency(int intFrequency, int decFrequency)
        {
            /// <summary>
            /// Sets COMM maximum frequency
            /// </summary>
            string i = intFrequency.ToString("D3");
            string d = decFrequency.ToString("D3");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MaximumFrequencyCmd;
            com[2] = COMM;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(d.Substring(0, 1));
            com[7] = (byte)int.Parse(d.Substring(1, 1));
            com[8] = (byte)int.Parse(d.Substring(2, 1));
            return com;
        }

        public byte[] SetADFMaximumFrequency(int Frequency)
        {
            /// <summary>
            /// Sets ADF maximum frequency
            /// </summary>
            string i = Frequency.ToString("D4");
            byte[] com = new byte[9];
            com[0] = this.Id;
            com[1] = MaximumFrequencyCmd;
            com[2] = ADF;
            com[3] = (byte)int.Parse(i.Substring(0, 1));
            com[4] = (byte)int.Parse(i.Substring(1, 1));
            com[5] = (byte)int.Parse(i.Substring(2, 1));
            com[6] = (byte)int.Parse(i.Substring(3, 1));
            com[7] = 0;
            com[8] = 0;
            return com;
        }

        public byte[] SetXPDRIntegerIncrement(int increment)
        {
            /// <summary>
            /// Sets XPDR increment for the 3rd and 4th digits
            /// </summary>
            string i = increment.ToString("D2");
            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = XpdrIntegerIncrementCmd;
            com[2] = (byte)int.Parse(i.Substring(0, 1));
            com[3] = (byte)int.Parse(i.Substring(1, 1));
            return com;
        }

        public byte[] SetXPDRDecimalIncrement(int increment)
        {
            /// <summary>
            /// Sets XPDR increment for the 1st and 2nd digits
            /// </summary>
            string i = increment.ToString("D2");
            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = XpdrDecimalIncrementCmd;
            com[2] = (byte)int.Parse(i.Substring(0, 1));
            com[3] = (byte)int.Parse(i.Substring(1, 1));
            return com;
        }

        public byte[] SetXPDRMinimumSquawk(int intSquawk, int decSquawk)
        {
            /// <summary>
            /// Sets XPDR minimum value for squawk code
            /// </summary>
            string i = intSquawk.ToString("D2");
            string d = decSquawk.ToString("D2");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = MinimumSquawkCmd;
            com[2] = (byte)int.Parse(i.Substring(0, 1));
            com[3] = (byte)int.Parse(i.Substring(1, 1));
            com[4] = (byte)int.Parse(d.Substring(0, 1));
            com[5] = (byte)int.Parse(d.Substring(1, 1));
            return com;
        }

        public byte[] SetXPDRMaximumSquawk(int intSquawk, int decSquawk)
        {
            /// <summary>
            /// Sets XPDR maximum value for squawk code
            /// </summary>
            string i = intSquawk.ToString("D2");
            string d = decSquawk.ToString("D2");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = MaximumSquawkCmd;
            com[2] = (byte)int.Parse(i.Substring(0, 1));
            com[3] = (byte)int.Parse(i.Substring(1, 1));
            com[4] = (byte)int.Parse(d.Substring(0, 1));
            com[5] = (byte)int.Parse(d.Substring(1, 1));
            return com;
        }

        public byte[] SetXPDRSquawkCode(int SquawkCode)
        {
            /// <summary>
            /// Sets XPDR squawk code
            /// </summary>
            string i = SquawkCode.ToString("D4");
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = SetSquawkCmd;
            com[2] = (byte)int.Parse(i.Substring(0, 1));
            com[3] = (byte)int.Parse(i.Substring(1, 1));
            com[4] = (byte)int.Parse(i.Substring(2, 1));
            com[5] = (byte)int.Parse(i.Substring(3, 1));
            return com;
        }

        public byte[] GetSquawk()
        {
            /// <summary>
            /// Sequence to get the squawk code
            /// </summary>
            byte[] com = new byte[2];
            com[0] = this.Id;
            com[1] = GetSquawkCodeCmd;
            return com;
        }

        public byte[] SetDigitLuminosity(byte Luminosity)
        {
            /// <summary>
            /// Sets the displays brightness
            /// </summary>
            if (Luminosity > 100)
            {
                Luminosity = 100;
            }
            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = InstrumentsBrightnessCmd;
            com[2] = Luminosity;
            return com;
        }

        public byte[] SetButtonNumber(byte connector, byte newJoystickButtonNumber)
        {
            /// <summary>
            /// Changes the number of a button
            /// - Important remarks:
            ///     It is advisable to read the answer to check if the requests was fulfilled.
            ///     
            ///     The RNS uses virtual connectors as the encoders center push-buttons. They
            ///     are numbered from 1 to 4 where 1-COMM, 2-NAV, 3-ADF and 4-Transponder.
            ///     By default these are assigned to joystick buttons 1,2,3 and 4.
            ///     This funcionality provides a way to change this joystick button numbers.
            ///     So, one can assign a joystick button number between 1 and 200 to one of
            ///     the virtual connectors 1 to 4.
            ///     
            ///     Joystick button numbers assigned defaults to 1 through 4 after enumeration
            ///     or hard-reset.
            ///     
            ///     Despite default buttons 1 and 2 could be reassigned and mapped, they have
            ///   internal functions that will be performed no matter what.
            /// </summary>

            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = ChangeButtonNumberCmd;
            com[2] = connector;
            com[3] = newJoystickButtonNumber;
            return com;
        }

        public byte[] SetButtonType(byte buttonNumber, byte buttonType)
        {
            /// <summary>
            /// Changes the behaviour (type) of a button
            /// 
            /// - Important remarks:
            ///     It is advisable to read the answer to check if the requests was fulfilled.
            ///     if not, request again
            ///
            ///     Button numbers must be between 1 and 200
            ///     
            ///     Button type valid options:
            ///         0-Push button
            ///         1-Toggle button
            ///         2-Logically inverted push button
            ///         3-Logically inverted toggle button
            ///         
            ///     Changes can only be made after enumeration. They reset to default after hard-reset.
            ///     
            ///     Despite default buttons 1 and 2 could be reassigned and mapped, they have
            ///   internal functions that will be performed no matter what.
            /// </summary>
            /// 
            if (buttonType > 3)
            {
                buttonType = 0; // Resets to default push button
            }
            byte[] com = new byte[4];
            com[0] = Joystick; // This functionality is controlled by the joystick so aimed to it
            com[1] = ChangeButtonTypeCmd;
            com[2] = buttonNumber;
            com[3] = buttonType;
            return com;
        }

        bool SetupRNS(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show(fileName + " configuration file not found.\nCreating a new one", "File does not exists");

            }
            return true;
        }

    }

    public class MBx24
    {

        /// <summary>
        /// The MBx24 has two main functions - Read 32 inputs and actuate 32 outputs.
        /// Besides this, it also can read up to 8 NorAir encoders and displays messages
        /// through 4 four-digit displays. 32 VLOs implementation is available when needed.
        /// </summary>
        /// 
        /// = THE 32 INPUTS =
        /// The 32 inputs are physically tagged from input '1' to '32'. This are fixed numbers
        /// and are marked on the board. Internally, there's a translation address mechanism
        /// that allows users to attribute a joystick button number to each of this inputs.
        /// 
        /// By default, the button numbers start at 50 and end at 81 making available 32
        /// sequential range if joystick button numbers. Using the 'SetFirstButtonNumberCmd',
        /// one can move this range up or down between 1 and 169, being aware the the first
        /// 20 joystick buttons may be reserved to some special peripherals functions.
        /// 
        /// Also by default, the the input tagged in the board by '1' corresponds to button '50',
        /// by '2' corresponds to button '51', a so on. This means that, if the command
        /// 'SetFirstButtonNumberCmd' is used to move the button numbers range to begin at
        /// button '75', the input tagged in the board by '1' is now the button '75', the '2' is
        /// button '76', etc.
        /// 
        /// Although the a joystick button can be reassigned to a new number, care should be
        /// take to ensure the number is not already in use by other input or peripheral.
        /// 
        /// The joystick buttons have attributes that can be changed at will. This is managed
        /// by the J-Port, i.e., not by the peripherals. As so, this attributes may be changed
        /// with a request directly to the J-Port engine.
        /// 
        /// = THE 32 OUTPUTS =
        /// The 32 outputs are physically tagged from '1' to '32'. Contrary to inputs, these
        /// have a fixed numbering system and can not be altered. They're managed by the peripheral.
        /// 
        /// Their attributes can be changed. As these are managed locally, the instruction
        /// is aimed to the peripheral.
        /// 
        /// These attributes can be its type ( normal or flashing ) and the flashing period (as 
        /// multiples of ~1000ms).
        /// 
        /// As functions,  they can be turned ON, OFF or TOGGLED.
        /// 
        /// = THE 32 VICes =
        /// A VICe connector, as a virtual object, doesn’t exist physically. A VICe is nothing
        /// more than a firmware connector with an assigned joystick button number, created to
        /// mimic the inverted logic state of a real joystick button. This is useful when a
        /// simulation software requires two joystick buttons to turn on and off the same item:
        /// one to the ON logic state and another for the OFF logic state.
        /// 
        /// The off logic state may be created virtually as a VICe with an additional joystick
        /// button number, saving a physical connector although occupying a joystick button number.
        /// This may not be an issue when there are 199 more available.
        /// 


        public string SerialNumber = null;
        public byte Id = 31;
        public byte[] enumerationData = new byte[8];
        public bool isEnumerated = false;
        public byte OutputTypeNormal = 0;
        public byte OutputTypeFlashing = 1;
        public byte NotInverted = 0;
        public byte Inverted = 1;
        public byte PushButton = 0;
        public byte ToggleButton = 1;
        public byte InvertedPushButton = 2;
        public byte InvertedToggleButton = 3;
        public byte ON = 1;
        public byte OFF = 0;

        // Constants for MBx24 commands

        public const byte OBCS = 99;
        public const byte Joystick = 10;
        public const byte HardResetCmd = 2;
        public const byte SoftResetCmd = 4;
        public const byte PingCmd = 6;
        public const byte InsertToEnumerationCmd = 7;
        public const byte InstrumentsBrightnessCmd = 40;
        public const byte BaklitColorCmd = 41;
        public const byte BaklitLedColorCmd = 42;
        public const byte SetFirstButtonNumberCmd = 85;
        public const byte ReportEncodersCmd = 48;
        public const byte SetDigitValueCmd = 50;
        public const byte SetOutputCmd = 20;
        public const byte SetOutputBulkCmd = 21;
        public const byte ToggleOutputCmd = 25;
        public const byte SetOutputTypeCmd = 30;
        public const byte ChangeButtonNumberCmd = 80;
        public const byte ChangeButtonTypeCmd = 17;
        public const byte CreateVICeConnectorCmd = 75;
        public const byte ReportConnectorsCmd = 55;


        public MBx24()  // Default constructor
        {

        }

        public MBx24(string SerialNumber, byte Id) // Constructor
        {
            if (this.ValidateProperties(SerialNumber, Id))
            {
                this.SerialNumber = SerialNumber;
                this.Id = Id;
                CreateEnumerationData();
            }
            else
            {
                MessageBox.Show(SerialNumber.ToString() + "Invalid serial number or ID", "Error");
            }
        }

        public bool SetSerialNumber(string SerialNumber)
        {
            /// <summary>
            /// Sets the MBx24 object serial number
            /// </summary>

            if (ValidateProperties(this.SerialNumber, this.Id))
            {
                this.SerialNumber = SerialNumber;
                CreateEnumerationData();
                return true;
            }
            return false;
        }

        public bool SetId(byte Id)
        {
            /// <summary>
            /// Sets the MBx24 object ID calling number
            /// </summary>

            if (Id > 20 && Id < 99)
            {
                this.Id = Id;
                CreateEnumerationData();
                return true;
            }
            return false;
        }

        public void Show(string s, byte[] data)
        {
            /// <summary>
            /// For debug, to show data from byte arrays
            /// </summary>
            Debug.Write(s);
            foreach (byte b in data)
            {
                Debug.Write(b.ToString() + " ");
            }
            Debug.WriteLine("");
        }

        public bool ValidateProperties(string serialNumber, byte Id)
        {
            /// <summary>
            /// Validates serial and ID numbers
            /// </summary>

            if (serialNumber.Length == 10)
            {
                if (serialNumber.StartsWith("50"))
                {
                    if (Double.Parse(serialNumber) != 0)
                    {
                        if (Id == 0)
                        {
                            Id = 50;
                        }
                        if (Id > 20 && Id < 99)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CreateEnumerationData()
        {
            /// <summary>
            /// Sets the MBx24 object enumeration data for OBCS enumeration
            /// Not evocable
            /// </summary>
            this.enumerationData[0] = OBCS;
            this.enumerationData[1] = InsertToEnumerationCmd;
            this.enumerationData[2] = (byte)(int.Parse(this.SerialNumber.Substring(0, 2)));
            this.enumerationData[3] = (byte)(int.Parse(this.SerialNumber.Substring(2, 2)));
            this.enumerationData[4] = (byte)(int.Parse(this.SerialNumber.Substring(4, 2)));
            this.enumerationData[5] = (byte)(int.Parse(this.SerialNumber.Substring(6, 2)));
            this.enumerationData[6] = (byte)(int.Parse(this.SerialNumber.Substring(8, 2)));
            if (this.Id == 0)
            {
                this.Id = 50;
            }
            this.enumerationData[7] = this.Id;
        }

        public byte[] Ping()
        {
            /// <summary>
            /// Returns the MBx24 object ping command
            /// </summary>

            byte[] com = { this.Id, PingCmd };
            return com;
        }

        public byte[] HardReset()
        {
            /// <summary>
            /// Returns the MBx24 object hard reset command
            /// </summary>

            byte[] com = { this.Id, HardResetCmd };
            return com;
        }

        public byte[] SoftReset()
        {
            /// <summary>
            /// Returns the MBx24 object soft reset command
            /// </summary>

            byte[] com = { this.Id, SoftResetCmd };
            return com;
        }

        public byte[] BackLit(byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Sets RGBW color features to all leds
            /// </summary>

            byte[] toSend = new byte[6];
            toSend[0] = this.Id;
            toSend[1] = BaklitColorCmd;
            toSend[2] = R;
            toSend[3] = G;
            toSend[4] = B;
            toSend[5] = W;
            return toSend;
        }

        public byte[] BackLitLed(byte led, byte R, byte G, byte B, byte W)
        {
            /// <summary>
            /// Sets RGBW color features to one led
            /// </summary>

            byte[] toSend = new byte[7];
            toSend[0] = this.Id;
            toSend[1] = BaklitLedColorCmd;
            toSend[2] = led;
            toSend[3] = R;
            toSend[4] = G;
            toSend[5] = B;
            toSend[6] = W;
            return toSend;
        }

        /*============== INPUTS / BUTTONS =======================*/

        public byte[] SetButtonNumber(byte connector, byte newJoystickButtonNumber)
        {
            /// <summary>
            /// Changes the joystick button number assigned to a connector
            /// </summary>
            /// 
            /// A 'Button' is a joystick button input and can be numbered from 1 to 200.
            /// Each MBx24 has 32 'button connectors', known as input connectors or even
            /// "In connectors".
            /// These are numbered from 1 to 32 and each one has a correspondence to a joystick
            /// button number.
            /// 
            /// Assigned joystick buttons number defaults to "ID value" until "ID + 31"
            /// after enumeration.
            /// For example, if ID is 50, joystick buttons number from 50 to 81. This means
            /// that connector 1 is the joystick button number 50, connector 2 is the joystick
            /// button number 51 and so on until connector 32 is the joystick button 81.
            /// 
            /// - Important remarks:
            /// 
            ///     It is advisable to read the answer to check if the requests was fulfilled.
            ///     If not, review arguments and request again.
            ///     
            ///     Connectors must be between 1 and 32.
            ///     Joystick button numbers must be between 1 and 200.
            ///     
            ///     Button numbers defaults to "ID value" until "ID + 32" after enumeration.
            ///     For example, if ID is 50, defaults to joystick buttons from 50 to 81.
            ///     The function "SetFirstButtonNumber" changes this by initiating the
            ///     joystick button numbers at a desired number. See "SetFirstButtonNumber"
            ///     function.
            ///     
            ///     It is advisable one of the two actions:
            ///     
            ///     a) Assuming defaults, one should ID a MBx24 as 50, 82, 114, etc.,
            ///        making the joystick numbers consecutive from number 50. Of course,
            ///        these may begin at any other available ID number and added 32 to 
            ///        the number of the next MBx24 ID and so on.
            ///        
            ///     b) After enumeration, re-assign all MBx24 buttons to whatever needed
            ///        by taking care that no button number repetitions occurs, if this
            ///        is not desired.
            ///     
            ///     Although not advisable, MBx24 and the joystick manager supports multiple
            ///     calls for the same joystick button number. One may have the same button
            ///     number assign in two different peripherals. This enables duplication of
            ///     buttons in order to have the same button functionality across spreaded
            ///     physical inputs (buttons) which, in many cases, may be usefull.
            ///     For example, an aviation 'Push To Talk' (aka PTT) for both Captain and
            ///     First Officer may be assign this way. The priority goes to the pressed
            ///     state ( ON-state ) of one of the buttons. These should be used with
            ///     physical push-button types only to avoid unexpected results.
            ///     
            ///     Despite default buttons 1 and 2 could be reassigned and mapped, they have
            ///     internal RNS functions that will be performed no matter what.
            ///   


            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = ChangeButtonNumberCmd;
            com[2] = connector;
            com[3] = newJoystickButtonNumber;
            return com;
        }

        public byte[] SetButtonType(byte buttonNumber, byte buttonType)
        {
            /// <summary>
            /// Changes the behaviour (type) of a button
            /// 
            /// - Important remarks:
            /// 
            ///     It is advisable to read the answer to check if the requests was fulfilled.
            ///     if not, request again.
            ///
            ///     Button numbers must be between 1 and 200
            ///     
            ///     Button type valid options:
            ///         0-Push button
            ///         1-Toggle button
            ///         2-Logically inverted push button
            ///         3-Logically inverted toggle button
            ///         
            ///     Changes can only be made after enumeration. They reset to default after hard-reset.
            ///     
            ///     Despite default buttons 1 and 2 could be reassigned and mapped, they have
            ///   internal RNS functions that will be performed no matter what.
            /// </summary>
            /// 
            if (buttonType > 3)
            {
                buttonType = 0; // Resets to default push button
            }
            byte[] com = new byte[4];
            com[0] = Joystick; // This functionality is controlled by the joystick so aimed to it
            com[1] = ChangeButtonTypeCmd;
            com[2] = buttonNumber;
            com[3] = buttonType;
            return com;
        }

        public byte[] SetFirstButtonNumber(byte newButton)
        {
            /// <sumary>
            /// Redefines the first button number for this peripheral, i.e.,
            /// defines a new buttons number range for this peripheral beginning
            /// at 'newButton' value. After this, all buttons of this peripheral will be
            /// from 'newButton' number to 'newButton' + 31 number.
            /// This should be done after enumeration and before buttons definitions.
            /// Redefining a peripheral buttons range does not transfer definitions to
            /// the new range (such as type, logical state, etc ).
            /// The easiest way to reset buttons number to the defaults is hard-reseting
            /// the peripheral(s) and J-Port.
            /// </sumary>
            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = SetFirstButtonNumberCmd;
            com[2] = newButton;
            return com;
        }

        public byte[] ReportEncoders()
        {
            /// <summary>
            /// Reports all the 8 possible encoders fitted in a MBx24.
            /// It is returned the accumulated count of step, positive or negative,
            /// in a signed byte array. As answers are always in byte type, cast
            /// has to be made from two consecutive bytes to an integer to keep
            /// the counter sign.
            /// 
            /// After sending the report, these counters return immediately to 0.
            /// 
            /// Remarks:
            /// 
            /// Although its rare to happen, if no report is requested for a long 
            /// period ( more that 5000ms ) and the encoders counter over rolls the
            /// maximum or minimum counting steps, it will be automatically reseted
            /// to 0, so those steps are lost. To prevent this, request a report within
            /// usefull time, if encoders are present, in order to make the simulation
            /// updated and not lose encoder steps.
            /// 
            /// </summary>
            /// 
            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = ReportEncodersCmd;
            return com;
        }

        public byte[] CreateVICeConnector(byte connector, byte joystickButtonNumber)
        {
            /// <summary>
            /// Creates or destroys a VICe connector
            /// </summary>
            /// A VICe is a virtual connector with an assigned joystick button number,
            /// created to mimic the inverted logic state of an already assigned real
            /// joystick button to a physical connector.
            /// 
            /// Care should be taken to choose a new and unused joystick button number.
            /// On the other hand, the connector must be the same as the one which is to
            /// be inverted. See NorAIr FrameWork docs for in-depth instructions and examples.
            /// 

            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = CreateVICeConnectorCmd;
            com[2] = connector;
            com[3] = joystickButtonNumber;
            return com;
        }

        public byte[] SetVICeType(byte joystickButtonNumber, byte buttonType)
        {
            /// <summary>
            /// Sets the VICe connector type
            /// </summary>
            /// After creation, a VICe connector is now seen as a joystick
            /// button. This way, it should be declare how it will work.
            /// 
            /// It can be set as any other joystick button:
            /// 
            ///     joystickButtonNumber must be between 1 and 200
            ///     
            ///     buttonType valid options:
            ///         0-Push button
            ///         1-Toggle button
            ///         2-Logically inverted push button
            ///         3-Logically inverted toggle button
            ///         
            ///     Changes can only be made after enumeration. They reset to default after hard-reset.
            ///     
            ///     It is advisable to set a VICe as the same type as its action button
            ///     

            byte VICeButtonType;

            if (buttonType > 3)
            {
                buttonType = 0; // Resets to default push button
            }

            switch (buttonType) // Set the VICe connector in special cases
            {
                case 0:
                    VICeButtonType = 0; // If action button is set to push-button, set VICe as normal push-button
                    break;              // and it will be inverted naturally

                case 1:
                    VICeButtonType = 3; // If action button is set to toggle, set VICe as an inverted toggle
                    break;              // This must be done in the case of a push-button is set to behave as
                                        // a toggle button.
                                        // When the toggle mode is activated, the VICe must stay OFF and vice-versa.

                case 2:
                    VICeButtonType = 2; // If action button is set to inverted push-button, set VICe as the same
                    break;              // type to work properly

                case 3:
                    VICeButtonType = 1; // See 'case 1:'
                    break;

                default:
                    VICeButtonType = 0;
                    break;

            }

            byte[] com = new byte[4];
            com[0] = Joystick; // This functionality is controlled by the joystick so aimed to it
            com[1] = ChangeButtonTypeCmd;
            com[2] = joystickButtonNumber;
            com[3] = VICeButtonType;
            return com;

        }

        public byte[] ReportConnectors()
        {
            /// <summary>
            /// Returns the input connectors logic states
            /// </summary>
            /// Remarks:
            /// To clear out ideas, this instructions reports ONLY connectors logic states.
            /// It does NOT return joystick button states. This is useful to watch what
            /// connectors are active and find out to what connector an input module, like
            /// a switch, is plugged to.
            /// 
            /// See NorAIr FrameWork docs for in-depth instructions and examples.
            /// 

            byte[] com = new byte[2];
            com[0] = this.Id;
            com[1] = ReportConnectorsCmd;
            return com;
        }

        public int[] DecodeConnectors(byte[] ConnectorsAsBits)
        {
            /// <summary>
            /// MBx24 connectors decode.
            /// </summary>
            /// 
            /// MBx24 answers to 'ReportConnectors' instruction with an array of bytes.
            /// Its 1st two bytes, the header, have the values of ID and '55' respectly.
            /// 
            /// The remaining 4 bytes hold the connectors logic state in bits form. This
            /// method returns an easier to handle array of integers with the connectors
            /// logic state.

            if (ConnectorsAsBits == null) // Not data was passed as argument
            {
                return null;
            }

            if (ConnectorsAsBits.Length == 0) // Not a valid array
            {
                return null;
            }

            int[] connectorsState = new int[32];
            byte[] ConnectorsData = new byte[4];

            System.Buffer.BlockCopy(ConnectorsAsBits, 2, ConnectorsData, 0, 4); // Remove header data & copy array

            for (int i = 0; i != 4; i++)
            {
                byte eightConnectors = ConnectorsData[i];

                for (int j = 0; j != 8; j++)
                {
                    if ((eightConnectors & 128) == 128)
                    {
                        connectorsState[8 * i + j] = 1;
                    }
                    eightConnectors = (byte)(eightConnectors << 1);
                }

            }
            return connectorsState;
        }


        /*============== OUTPUTS =======================*/

        public byte[] SetOutput(byte outputNumber, byte state)
        {
            /// <summary>
            /// Sets an output to a state (ON / OFF). State can be 1 or 0.
            /// This command turns ON or OFF the output 'buttonNumber' number. 
            /// </summary>

            byte[] com = new byte[4];
            com[0] = this.Id;
            com[1] = SetOutputCmd;
            com[2] = outputNumber;
            com[3] = state;
            return com;
        }

        public byte[] SetOutputBulk(byte bank1, byte bank2, byte bank3, byte bank4)
        {
            /// <summary>
            /// Sets the MBx24 outputs in bulk, i.e., all at a time, to ON and OFF
            /// individually.
            /// It uses 4 logic state banks, represented by four bytes.
            /// Each byte represents 8 connectors:
            /// bank1 = connectors from 1 to 8
            /// bank2 = connectors from 9 to 16
            /// bank3 = connectors from 17 to 24
            /// bank4 = connectors from 25 to 32
            /// 
            /// The left most bit (the Most Significant Bit or MBS) correlates to the 
            /// first connector of its bank
            /// 
            /// | Bit           | MSB 7 |   6   |     5 |     4 |     3 |     2 |     1 | LSB 0 |
            /// |---------------|-------|-------|-------|-------|-------|-------|-------|-------|
            /// | Connector #   |   1   |   2   |   3   |   4   |   5   |   6   |   7   |   8   |
            /// 
            /// </summary>

            byte[] toSend = new byte[6];
            toSend[0] = this.Id;
            toSend[1] = SetOutputBulkCmd;
            toSend[2] = bank1;
            toSend[3] = bank2;
            toSend[4] = bank3;
            toSend[5] = bank4;
            return toSend;
        }

        public byte[] ToggleOutput(byte outputNumber)
        {

            /// <summary>
            /// Inverts an output logical state.
            /// This command toggles the output logical state. 
            /// </summary>

            byte[] com = new byte[3];
            com[0] = this.Id;
            com[1] = ToggleOutputCmd;
            com[2] = outputNumber;
            return com;
        }

        public byte[] SetOutputType(byte outputNumber, byte outputType, byte flashingPeriod = 0, byte inverted = 0)
        {
            /// <summary>
            /// Sets an output attribute type. In other words, program an output.
            /// 
            /// The attribute outputType can be programmed to 0-normal mode or 1-flashing mode.
            /// 
            /// 
            /// If flashing mode is selected, a flashing period can be set to a multiple
            /// of 1000ms. For example, if flashing period is set to 2, when turned on, the
            /// output will stay on for 1000ms and off for another 1000ms. Minimum flashing
            /// period is 1 (1000ms) and must be an positive integer value from 1 to 255.
            /// 
            /// Also, the output can be logically inverted, i.e., when instructed to turn ON,
            /// it will turn OFF and vice-versa.
            /// </summary>

            byte[] com = new byte[6];
            com[0] = this.Id;
            com[1] = SetOutputTypeCmd;
            com[2] = outputNumber;
            com[3] = outputType;
            com[4] = flashingPeriod;
            com[5] = inverted;
            return com;
        }
    }
}
