/*
 * TagDetection based on Impinj LLRP Tool Kit (LTK)
 *
 * MarinYoung@163.com
 * Last Modified: 2016/6/3
 * Reference:
 * [1]  https://support.impinj.com/hc/en-us/articles/202756168-Hello-LLRP-Low-Level-Reader-Protocol
 * [2]  https://support.impinj.com/hc/en-us/articles/202756348-Get-Low-Level-Reader-Data-with-LLRP
 * [3]  https://support.impinj.com/hc/en-us/articles/204383817-Latest-firmware-utilities-and-development-libraries-for-Impinj-readers-and-gateways
 * [4]  https://github.com/pengyuzhang/Blink/tree/master/ReaderLibrary
 * [5]  Speedway revolution reader application note: Low Level User Data Support
        https://support.impinj.com/hc/en-us/articles/202755318-Application-Note-Low-Level-User-Data-Support
 * [6]  https://support.impinj.com/hc/en-us/articles/202756368-Optimizing-Tag-Throughput-Using-ReaderMode
 * [7]  https://support.impinj.com/hc/en-us/articles/202756568-EPC-Gen2-Tag-Filtering
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.Impinj;
using Org.LLRP.LTK.LLRPV1.DataType;
using TagReader.Properties;

namespace TagReader.RFIDReader
{
    public class ReaderWrapper
    {
        public static int TotalReport;
        public static int TotalEvent;

        public static bool LogToFile = false;
        public static double Convert2Radian = 2 * Math.PI / 4096.0;        
        public static double Convert2Degree = 360.0 / 4096.0;

        public static DataTable Data;
        public static LLRPClient Reader;
        public static ReaderSettings ReaderParameters;
        public static FormTagReader MainForm;


        public class MyException : ApplicationException
        {
            public MyException(string message) : base(message) { }

            public override string Message => base.Message;
        };


        public static void Initialize_Configuration()
        {
            WriteMessage(Resources.Initialize);

            // Create an instance of LLRP Reader client.
            Reader = new LLRPClient();
            // Create an DataTable to save the current state of Tag
            Data = new DataTable();
            // Set Reader Config in Default Way.
            ReaderParameters = new ReaderSettings();

            //Impinj Best Practice! Always Install the Impinj extensions
            Impinj_Installer.Install();
        }


        public static void setReader_PARM()
        {
            if (ReaderParameters == null) return;
            ReaderParameters.Ip = "192.168.1.222";
            /*
            ** Set Channel Index, 16 in total, which represents different frequency (MHz). Namely,
            ** [1: 920.375]
            ** [2: 920.875]
            ** ...... 
            ** [16: 924.375]
            */
            ReaderParameters.ChannelIndex = 16;
            // Power(dbm) [10 : 0.25 : 32.5], 90 different values in total
            ReaderParameters.TransmitPower = 32.5;
            /*
            ** ModeIndex  NAME                  SENSITIVITY     INTERFERENCE TOLERANCE
            **   0        Max Throughput        good            poor
            **   1        Hybrid                good            good
            **   2        Dense Reader (M=4)    better          excellent
            **   3        Dense Reader (M=8)    best            excellent
            **   4*       MaxMiller             better          good
            **   1000     Auto set Dense Reader
            **   1001     Auto set Single Reader
            **   * MaxMiller is not available in all regions
            */
            ReaderParameters.ModeIndex = 1000;
            // ?
            ReaderParameters.HopTableIndex = 0;
            // ?
            ReaderParameters.PeriodicTriggerValue = 0;
            
            ReaderParameters.TagPopulation = 32;
            ReaderParameters.Tari = 10;
            // ?
            ReaderParameters.TagTransitTime = 0;
            // ?
            ReaderParameters.ReaderSensitivity = 1;
            // each value in the array map to Antenna 1, Antenna 2, Antenna 3, Antenna 4, respectively.
            ReaderParameters.AntennaId = new[] { true, false, false, false };
        }


        #region Saving Data
        public static void Initialize_DataRow()
        {
            // Create columns in Data table
            Data.Columns.Add("EPC");
            Data.Columns.Add("TimeStamp");
            Data.Columns.Add("Antenna");
            Data.Columns.Add("TxPower");
            Data.Columns.Add("ChannelIndex");
            Data.Columns.Add("Frequency");
            Data.Columns.Add("PeakRSSI");
            Data.Columns.Add("PhaseRadian");
            Data.Columns.Add("PhaseAngle");
            Data.Columns.Add("DopplerShift");
            Data.Columns.Add("Velocity");

            // Initialize Column Name
            DataRow row = Data.NewRow();
            row["EPC"] = "EPC";
            row["DopplerShift"] = "Doppler Shift(Hz)";
            row["TimeStamp"] = "Timestamp";
            row["Antenna"] = "Antenna";
            row["TxPower"] = "TxPower(mW)";
            row["ChannelIndex"] = "Channel Index";
            row["Frequency"] = "Frequency(MHz)";
            row["PeakRSSI"] = "RSS(dBm)";
            row["PhaseRadian"] = "Phase Angle(Radian)";
            row["PhaseAngle"] = "Phase(Degree)";
            row["Velocity"] = "Velocity";
            Data.Rows.Add(row);
        }


        public static void UpdateTagStatus(ref TagStatus currTagStatus)
        {
            currTagStatus.Rssi = currTagStatus.Rssi / 100;
            currTagStatus.PhaseRadian = currTagStatus.RawPhase * Convert2Radian;
            currTagStatus.PhaseDegree = currTagStatus.RawPhase * Convert2Degree;
            currTagStatus.Frequency = 920.625 + (currTagStatus.ChannelIndex - 1) * 0.25;
        }


        public static void AppendRowToDatatable(ref TagStatus reportedTagStatus)
        {
            DataRow row = Data.NewRow();

            row["EPC"] = reportedTagStatus.Epc;
            row["TimeStamp"] = reportedTagStatus.TimeStamp;
            row["Antenna"] = reportedTagStatus.Antenna;
            row["TXPower"] = ReaderParameters.TransmitPower;
            row["ChannelIndex"] = reportedTagStatus.ChannelIndex;
            row["Frequency"] = reportedTagStatus.Frequency;
            row["PeakRSSI"] = reportedTagStatus.Rssi;
            row["PhaseRadian"] = reportedTagStatus.PhaseRadian;
            row["PhaseAngle"] = reportedTagStatus.PhaseDegree;
            row["DopplerShift"] = reportedTagStatus.DopplerShift;
            row["Velocity"] = reportedTagStatus.Velocity;

            Data.Rows.Add(row);
        }


        public static void SaveData(CsvStreamWriter csvWriter)
        {
            csvWriter.AddData(Data, 1);
            csvWriter.Save();
        }


        public static void WriteMessage(string message)
        {
            MainForm.Invoke(method: new Action(() =>
            {
                MainForm.UpdateStatusBar_Message(ref message);
            }));
            
            if (LogToFile)
            {
                LogHelper.WriteLog(typeof(ReaderWrapper), message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
        #endregion


        #region Output Settings (REPORT/LOGGING)

        public class PhaseAndTime
        {
            public ulong Timestamp { get; set; }
            public double PhaseDegree { get; set; }
            public int ReportCount { get; set; }
            public double Velocity { get; set; }

            public PhaseAndTime(ulong time, double phase, ushort count)
            {
                Timestamp = time;
                PhaseDegree = phase;
                ReportCount = count;
                Velocity = 0;
            }
        }

        private static Dictionary<TagInfos.Key, PhaseAndTime> ReportHistory = new Dictionary<TagInfos.Key, PhaseAndTime>();

        public static bool CalculateVelocity(out double velocity, ref TagStatus currTagStatus)
        {
            var epc = currTagStatus.Epc;
            var antenna = currTagStatus.Antenna;
            var channel = currTagStatus.ChannelIndex;
            var key = new TagInfos.Key(epc, antenna, channel);

            bool retVal = false;
            velocity = 0;

            /* you have to have two samples from the same EPC on the same
             * antenna and channel.  NOTE: this is just a simple example.
             * More sophisticated apps would create a database with
             * this information per-EPC */
            if (ReportHistory.ContainsKey(key))
            {
                /* positive velocity is moving towards the antenna */
                var timeChangeUsec = (currTagStatus.TimeStamp - ReportHistory[key].Timestamp);
                var phaseChangeDegrees = currTagStatus.PhaseDegree - ReportHistory[key].PhaseDegree;

                /* always wrap the phase to between -180 and 180 */
                while (phaseChangeDegrees < -180)
                    phaseChangeDegrees += 360;
                while (phaseChangeDegrees > 180)
                    phaseChangeDegrees -= 360;

                /* if our phase changes close to 180 degrees, you can see we
                ** have an ambiguity of whether the phase advanced or retarded by
                ** 180 degrees (or slightly over). There is no way to tell unless
                ** you use more advanced techniques with multiple channels.  So just
                ** ignore any samples where phase change is > 90 */
                if (Math.Abs((int)phaseChangeDegrees) <= 90)
                {
                    /* We can divide these two to get degrees/usec, but it would be more
                    ** convenient to have this in a common unit like meters/second.
                    ** Here's a straightforward conversion.  NOTE: to be exact here, we
                    ** should use the channel index to find the channel frequency/wavelength.
                    ** For now, I'll just assume the wavelength corresponds to mid-band at
                    ** 0.32786885245901635 meters. The formula below reports meters per second.
                    ** Note that 360 degrees equals only 1/2 a wavelength of motion because
                    ** we are computing the round trip phase change.
                    **
                    **  phaseChange (degrees)   1/2 wavelength     0.327 meter      1000000 usec
                    **  --------------------- * -------------- * ---------------- * ------------
                    **  timeChange (usec)       360 degrees       1  wavelength      1 second
                    **
                    ** which should net out to estimated tag velocity in meters/second */

                    /*
                        phase1 + 2*k1*pi = 2*pi*(2*d1/wavelength)       (1)
                        phase2 + 2*k2*pi = 2*pi*(2*d2/wavelength)       (2)
                    (1) - (2):
                        delta_phase + 2*pi*delta_k = 2*pi*(2*delta_d/wavelength)
                    ==> delta_d = (wavelength/(4*pi)) * (delta_phase + 2*pi*delta_k)
                    ==> velocity = delta_d/delta_t = 
                    */
                    //double waveLength = 300000000 / currTagStatus.Frequency;
                    velocity = ((phaseChangeDegrees * 0.5 * 0.327868852 * 1000000) / (360 * timeChangeUsec));

                    retVal = true;
                }
                ReportHistory[key].Timestamp = currTagStatus.TimeStamp;
                ReportHistory[key].PhaseDegree = currTagStatus.PhaseDegree;
            }
            else
            {
                ReportHistory.Add(key, new PhaseAndTime(currTagStatus.TimeStamp, currTagStatus.PhaseDegree, 1));
            }
            return retVal;
        }


        /// <summary>
        /// Simple Handler for receiving the tag reports from the Reader
        /// </summary>
        /// <param name="msg"></param>
        public static void reader_OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg)
        {
            // Report could be empty
            if (msg.TagReportData == null) return;

            var currTagStatus = new TagStatus();
            // Loop through and print out each tag
            foreach (PARAM_TagReportData t in msg.TagReportData)
            {
                TotalReport++;

                MainForm.Invoke(method: new Action(() =>
                {
                    MainForm.UpdateStatusBar_Report();
                }));
                

                // just write out the EPC as a hex string for now. It is guaranteed to be
                // in all LLRP reports regardless of default configuration
                var data = "EPC: ";
                TagInfos.Key key = null;

                if (t.EPCParameter[0].GetType() == typeof(PARAM_EPC_96))
                {
                    currTagStatus.Epc = ((PARAM_EPC_96)t.EPCParameter[0]).EPC.ToHexString();
                }
                else
                {
                    currTagStatus.Epc = ((PARAM_EPCData)t.EPCParameter[0]).EPC.ToHexString();
                }
                data += currTagStatus.Epc;

                // collect some Data for velocity calculation
                // NOTE: these could be NULL, so we should check
                if (t.AntennaID != null)
                {
                    currTagStatus.Antenna = t.AntennaID.AntennaID;
                    data += "\tant: " + currTagStatus.Antenna;
                }
                if (t.ChannelIndex != null)
                {
                    currTagStatus.ChannelIndex = t.ChannelIndex.ChannelIndex;
                    data += "\tch: " + currTagStatus.ChannelIndex;
                }
                if (t.FirstSeenTimestampUTC != null)
                {
                    currTagStatus.TimeStamp = t.FirstSeenTimestampUTC.Microseconds;
                    data += "\ttime: " + currTagStatus.TimeStamp.ToString();
                }
                if (t.LastSeenTimestampUTC != null)
                {
                    currTagStatus.LastSeenTime = t.LastSeenTimestampUTC.Microseconds;
                }
                if (t.TagSeenCount != null)
                {
                    key = new TagInfos.Key(currTagStatus.Epc, currTagStatus.Antenna, currTagStatus.ChannelIndex);
                    if (ReportHistory.ContainsKey(key))
                    {
                        ReportHistory[key].ReportCount += 1;
                    }
                    else
                    {
                        ReportHistory.Add(key, new PhaseAndTime(currTagStatus.TimeStamp, currTagStatus.PhaseDegree, 1));
                    }
                    currTagStatus.TagSeenCount = ReportHistory[key].ReportCount;
                }

                if (t.Custom == null) continue;
                for (var x = 0; x < t.Custom.Length; x++)
                {
                    // try to make a tag direction report out of it
                    Object param = t.Custom[x];
                    if (param is PARAM_ImpinjRFPhaseAngle)
                    {
                        currTagStatus.RawPhase = ((PARAM_ImpinjRFPhaseAngle)param).PhaseAngle;
                        data += "\tPhase: " + currTagStatus.RawPhase;
                    }
                    else if (param is PARAM_ImpinjPeakRSSI)
                    {
                        currTagStatus.Rssi = ((PARAM_ImpinjPeakRSSI)param).RSSI;
                        data += "\tRSSI: " + currTagStatus.Rssi;
                    }
                    else if (param is PARAM_ImpinjRFDopplerFrequency)
                    {
                        currTagStatus.DopplerShift = ((PARAM_ImpinjRFDopplerFrequency)param).DopplerFrequency;
                        data += "\tDoppler: " + currTagStatus.DopplerShift;
                    }
                }
                UpdateTagStatus(ref currTagStatus);

                // estimate the velocity and print a filtered version
                double velocity;
                if (CalculateVelocity(out velocity, ref currTagStatus))
                {
                    // keep a filtered value. Use a 1 pole IIR here for simplicity 
                    ReportHistory[key].Velocity = (6 * ReportHistory[key].Velocity + 4 * velocity) / 10.0;

                    var v = ReportHistory[key].Velocity;
                    if (v > 0.25)
                        data += "---->";
                    else if (v < -0.25)
                        data += "<----";
                    else
                        data += "     ";
                }
                currTagStatus.Velocity = velocity;

                // save data
                AppendRowToDatatable(ref currTagStatus);

                // pass to Form
                try
                {
                    MainForm.Invoke(method: new Action(() =>
                    {
                        MainForm.UpdateChart(ref currTagStatus);
                        MainForm.AddListItem(ref currTagStatus);
                    }));
                }
                catch
                {
                    // ignored
                }
                //WriteMessage(data);
            } // end of foreach
        }


        /// <summary>
        /// Simple Handler for receiving the Reader events from the Reader
        /// </summary>
        /// <param name="msg"></param>
        public static void reader_OnReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            // Events could be empty
            if (msg.ReaderEventNotificationData == null) return;

            // Just write out the LTK-XML for now
            TotalEvent++;

            MainForm.Invoke(method: new Action(() =>
            {
                MainForm.UpdateStatusBar_Event();
            }));
            

            // speedway readers always report UTC timestamp
            UNION_Timestamp t = msg.ReaderEventNotificationData.Timestamp;
            PARAM_UTCTimestamp ut = (PARAM_UTCTimestamp)t[0];
            Debug.Assert(ut != null, "ut != null");
            double millis = (ut.Microseconds + 500) / 1000;

            // LLRP reports time in microseconds relative to the Unix Epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = epoch.AddMilliseconds(millis);

            var message = "======\tReader Event " + TotalEvent + "\t======\n" + now.ToString("O");
            WriteMessage(message);

            // this is how you would look for individual events of interest
            // Here I just dump the event
            if (msg.ReaderEventNotificationData.AISpecEvent != null)
            {
                message = msg.ReaderEventNotificationData.AISpecEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.AntennaEvent != null)
            {
                message = msg.ReaderEventNotificationData.AntennaEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ConnectionAttemptEvent != null)
            {
                message = msg.ReaderEventNotificationData.ConnectionAttemptEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ConnectionCloseEvent != null)
            {
                message = msg.ReaderEventNotificationData.ConnectionCloseEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.GPIEvent != null)
            {
                message = msg.ReaderEventNotificationData.GPIEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.HoppingEvent != null)
            {
                message = msg.ReaderEventNotificationData.HoppingEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ReaderExceptionEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReaderExceptionEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent.ToString();
                WriteMessage(message);
            }
            if (msg.ReaderEventNotificationData.ROSpecEvent != null)
            {
                message = msg.ReaderEventNotificationData.ROSpecEvent.ToString();
                WriteMessage(message);
            }
        }
        #endregion


        #region RoSpec Settings
        /// <summary>
        /// Deletes the ROSpec at the Reader corresponding to ROSpecID passed in this message.
        /// </summary>
        public static void Delete_RoSpec()
        {
            WriteMessage("Deleting RoSpec ----- ");
            MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
            msg.ROSpecID = 1111;
            MSG_ERROR_MESSAGE msg_err;

            MSG_DELETE_ROSPEC_RESPONSE rsp = Reader.DELETE_ROSPEC(msg, out msg_err, 2000);

            if (rsp != null)// Success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)// Error
            {
                WriteMessage(msg_err.ToString());
            }
            else// Timeout
            {
                WriteMessage("DELETE_ROSPEC Command Timed out!");
            }
        }


        public static void Enable_Impinj_Extensions()
        {
            WriteMessage("Enabling Impinj Extensions ------ ");

            MSG_ERROR_MESSAGE msg_err;
            MSG_IMPINJ_ENABLE_EXTENSIONS imp_msg = new MSG_IMPINJ_ENABLE_EXTENSIONS();

            imp_msg.MSG_ID = 1; // not this doesn't need to bet set as the library will default

            //Send the custom message and wait for 8 seconds
            MSG_CUSTOM_MESSAGE cust_rsp = Reader.CUSTOM_MESSAGE(imp_msg, out msg_err, 8000);
            MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE msg_rsp = cust_rsp as MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE;

            if (msg_rsp != null)    // Success
            {
                if (msg_rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(msg_rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)   // Error
            {
                WriteMessage(msg_err.ToString());
            }
            else    // Time out
            {
                WriteMessage("Enable Extensions Command Timed out\n");
            }
        }


        /// <summary>
        /// Communicates the information of a ROSpec to the Reader.
        /// </summary>
        public static void Add_RoSpec()
        {
            WriteMessage("Adding RoSpec ----- ");
            MSG_ERROR_MESSAGE msg_err;
            // ROBoundarySpec
            // Specifies the Start and Stop triggers for the ROSpec
            // Immediate Start trigger
            // The Reader will Start reading tags as soon as the ROSpec is enabled
            // No Stop trigger. Keep reading tags until the ROSpec is disabled.
            MSG_ADD_ROSPEC msg = new MSG_ADD_ROSPEC();
            msg.ROSpec = new PARAM_ROSpec();
            msg.ROSpec.CurrentState = ENUM_ROSpecState.Disabled;
            msg.ROSpec.ROSpecID = 1111;
            msg.ROSpec.ROBoundarySpec = new PARAM_ROBoundarySpec();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Immediate;
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;

            // Antenna Inventory Spec (AISpec)
            // Specifies which antennas and protocol to use
            msg.ROSpec.SpecParameter = new UNION_SpecParameter();

            // Antenna inventory operation
            PARAM_AISpec aiSpec = new PARAM_AISpec();
            aiSpec.AntennaIDs = new UInt16Array();
            // Enable all antennas
            for (ushort i = 0; i < ReaderParameters.AntennaId.Length; ++i)
            {
                if (ReaderParameters.AntennaId[i])
                    aiSpec.AntennaIDs.Add(Convert.ToUInt16(i+1));
            }
            // No AISpec Stop trigger. It stops when the ROSpec stops.
            aiSpec.AISpecStopTrigger = new PARAM_AISpecStopTrigger();
            aiSpec.AISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null;

            aiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
            aiSpec.InventoryParameterSpec[0] = new PARAM_InventoryParameterSpec();
            aiSpec.InventoryParameterSpec[0].InventoryParameterSpecID = 1234;
            aiSpec.InventoryParameterSpec[0].ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2;

            msg.ROSpec.SpecParameter.Add(aiSpec);

            // create a new ROReportSpec
            // N: Unsigned Short Integer. This is the number of TagReportData Parameters used in ROReportTrigger = 1 and 2.
            // If N = 0, there is no limit on the number of TagReportData Parameters.
            // This field SHALL be ignored when ROReportTrigger = 0.
            msg.ROSpec.ROReportSpec = new PARAM_ROReportSpec();
            msg.ROSpec.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
            msg.ROSpec.ROReportSpec.N = 1;
            msg.ROSpec.ROReportSpec.TagReportContentSelector = new PARAM_TagReportContentSelector();
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableAccessSpecID = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableAntennaID = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableChannelIndex = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableFirstSeenTimestamp = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableInventoryParameterSpecID = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableLastSeenTimestamp = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnablePeakRSSI = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableROSpecID = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableSpecIndex = true;
            msg.ROSpec.ROReportSpec.TagReportContentSelector.EnableTagSeenCount = true;

            // Send a report for every tag read
            // Add 1)RF Phase Angle;  2)Peak RSSI  3)RF Doppler Frequency
            PARAM_ImpinjTagReportContentSelector contentSelector = new PARAM_ImpinjTagReportContentSelector();

            contentSelector.ImpinjEnableSerializedTID = new PARAM_ImpinjEnableSerializedTID();
            contentSelector.ImpinjEnableSerializedTID.SerializedTIDMode = ENUM_ImpinjSerializedTIDMode.Enabled;
            contentSelector.ImpinjEnableRFPhaseAngle = new PARAM_ImpinjEnableRFPhaseAngle();
            contentSelector.ImpinjEnableRFPhaseAngle.RFPhaseAngleMode = ENUM_ImpinjRFPhaseAngleMode.Enabled;
            contentSelector.ImpinjEnablePeakRSSI = new PARAM_ImpinjEnablePeakRSSI();
            contentSelector.ImpinjEnablePeakRSSI.PeakRSSIMode = ENUM_ImpinjPeakRSSIMode.Enabled;
            contentSelector.ImpinjEnableRFDopplerFrequency = new PARAM_ImpinjEnableRFDopplerFrequency();
            contentSelector.ImpinjEnableRFDopplerFrequency.RFDopplerFrequencyMode = ENUM_ImpinjRFDopplerFrequencyMode.Enabled;

            // These are custom fields, so they get added this way
            msg.ROSpec.ROReportSpec.Custom.Add(contentSelector);

            MSG_ADD_ROSPEC_RESPONSE rsp = Reader.ADD_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(Resources.ERROR);
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("ADD_ROSPEC Command Timed out");
            }
        }


        static void Add_RoSpec_WithXML()
        {
            WriteMessage("Adding RoSpec from XML file -----");

            Org.LLRP.LTK.LLRPV1.DataType.Message obj = new MSG_ADD_ROSPEC();
            ENUM_LLRP_MSG_TYPE msg_type;

            // read the XML from a file and validate its an ADD_ROSPEC
            WriteMessage("\tLoading XML file ----- ");
            try
            {
                FileStream fs = new FileStream(@"..\..\addRoSpec.xml", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd();
                fs.Close();

                LLRPXmlParser.ParseXMLToLLRPMessage(s, out obj, out msg_type);

                if (obj == null || msg_type != ENUM_LLRP_MSG_TYPE.ADD_ROSPEC)
                {
                    WriteMessage("Could not extract message from XML");
                }
                Console.Write(Resources.OK);
            }
            catch
            {
                WriteMessage("Unable to convert From valid XML");
            }

            WriteMessage("\tAdding RoSpec ----- ");
            // covert to the proper message type
            MSG_ADD_ROSPEC msg = (MSG_ADD_ROSPEC)obj;

            // Communicate that message to the Reader
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC_RESPONSE rsp = Reader.ADD_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(Resources.ERROR);
                WriteMessage(msg_err.ToString());
                //Reader.Close();
                //Environment.Exit(1);
            }
            else
            {
                WriteMessage("ADD_ROSPEC Command Timed out\n");
            }
        }


        /// <summary>
        /// This message is issued by the Client to the Reader. Upon receiving the message, 
        /// the Reader moves the ROSpec corresponding to the ROSpecID passed in this message 
        /// from the disabled to the inactive state.
        /// </summary>
        public static void Enable_RoSpec()
        {
            WriteMessage("Enabling RoSpec ----- ");
            MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the ROSpec we created above
            MSG_ENABLE_ROSPEC_RESPONSE rsp = Reader.ENABLE_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)//success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)//error
            {
                WriteMessage(Resources.ERROR);
                WriteMessage(msg_err.ToString());
            }
            else//time out
            {
                WriteMessage("ENABLE_ROSPEC Command Timed out\n");
            }
        }


        /// <summary>
        /// Upon receiving the message, the Reader starts the ROSpec corresponding to ROSpecID passed 
        /// in this message, if the ROSpec is in the enabled state.
        /// </summary>
        public static void Start_RoSpec()
        {
            WriteMessage("Starting RoSpec ----- ");
            MSG_START_ROSPEC msg = new MSG_START_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111;
            MSG_START_ROSPEC_RESPONSE rsp = Reader.START_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
                WriteMessage(rsp.LLRPStatus.ToString());
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("START_ROSPEC Command Timed out\n");
            }
        }


        /// <summary>
        /// If the Reader was currently executing the ROSpec corresponding to the ROSpecID, 
        /// and the Reader was able to stop executing that ROSpec, then the success code is returned. 
        /// </summary>
        public static void Stop_RoSpec()
        {
            WriteMessage("Stopping RoSpec ----- ");
            MSG_STOP_ROSPEC msg = new MSG_STOP_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111;
            MSG_STOP_ROSPEC_RESPONSE rsp = Reader.STOP_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("STOP_ROSPEC Command Timed out\n");
            }
        }


        /// <summary>
        /// Requests to the Reader to retrieve all the ROSpecs that have been configured at the Reader.
        /// </summary>
        public static void Get_RoSpec()
        {
            WriteMessage("Getting RoSpec ----- ");
            MSG_GET_ROSPECS msg = new MSG_GET_ROSPECS();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE rsp = Reader.GET_ROSPECS(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("GET_ROSPEC Command Timed out\n");
            }
        }
        #endregion


        #region Reader Configuration
        public static void SetReaderConfig_WithXML()
        {
            WriteMessage("SET_READER_CONFIG from XML file ----- ");

            Org.LLRP.LTK.LLRPV1.DataType.Message obj = new MSG_SET_READER_CONFIG();

            // read the XML from a file and validate its an ADD_ROSPEC
            try
            {
                FileStream fs = new FileStream(@"..\..\setReaderConfig.xml", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd();
                fs.Close();

                ENUM_LLRP_MSG_TYPE msg_type;
                LLRPXmlParser.ParseXMLToLLRPMessage(s, out obj, out msg_type);

                if (obj == null || msg_type != ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG)
                {
                    WriteMessage("Could not extract message from XML");
                }
            }
            catch
            {
                WriteMessage("Unable to convert to valid XML");
            }

            // Communicate that message to the Reader
            MSG_SET_READER_CONFIG msg = (MSG_SET_READER_CONFIG)obj;
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = Reader.SET_READER_CONFIG(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                    WriteMessage(rsp.LLRPStatus.ErrorDescription.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("SET_READER_CONFIG Command Timed out\n");
            }
        }


        public static void SetReaderConfig()
        {
            WriteMessage("Set Reader Configuration ----- ");

            ushort numAntennaToSet = 0;
            for (ushort i = 0; i < ReaderParameters.AntennaId.Length; ++i)
            {
                if (ReaderParameters.AntennaId[i])
                    ++numAntennaToSet;
            }

            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG
            {
                AccessReportSpec = new PARAM_AccessReportSpec
                {
                    AccessReportTrigger = ENUM_AccessReportTriggerType.End_Of_AccessSpec
                }
            };

            // https://support.impinj.com/hc/en-us/articles/202756568-EPC-Gen2-Tag-Filtering
            ///// create the inventory command
            PARAM_C1G2InventoryCommand cmd = new PARAM_C1G2InventoryCommand();

            /* set up tag filter; Marin, 2016.5.25
            cmd.C1G2Filter = new PARAM_C1G2Filter[1];
            cmd.C1G2Filter[0] = new PARAM_C1G2Filter();
            cmd.C1G2Filter[0].C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask();
            // Filter on EPC (memory bank #1)
            cmd.C1G2Filter[0].C1G2TagInventoryMask.MB = new TwoBits(1);
            // Start filtering at the address 0x20 (the Start of the third word).
            // The first two words of the EPC are the checksum and Protocol Control bits.
            cmd.C1G2Filter[0].C1G2TagInventoryMask.Pointer = 0x20;
            cmd.C1G2Filter[0].C1G2TagInventoryMask.TagMask = LLRPBitArray.FromHexString("78B5");
            */

            ///// set RF Control parameters
            cmd.C1G2RFControl = new PARAM_C1G2RFControl();
            cmd.C1G2RFControl.ModeIndex = ReaderParameters.ModeIndex;
            cmd.C1G2RFControl.Tari = ReaderParameters.Tari;

            ///// set the session
            cmd.C1G2SingulationControl = new PARAM_C1G2SingulationControl();
            cmd.C1G2SingulationControl.Session = new TwoBits(1);
            cmd.C1G2SingulationControl.TagPopulation = ReaderParameters.TagPopulation;
            cmd.C1G2SingulationControl.TagTransitTime = ReaderParameters.TagTransitTime;
            cmd.TagInventoryStateAware = false;

            PARAM_ImpinjInventorySearchMode impinjSearchMod = new PARAM_ImpinjInventorySearchMode();
            impinjSearchMod.InventorySearchMode = ENUM_ImpinjInventorySearchType.Dual_Target;

            /*
            // Duty Cycle
            PARAM_ImpinjLowDutyCycle impinjDutyCycle = new PARAM_ImpinjLowDutyCycle();
            impinjDutyCycle.LowDutyCycleMode = ENUM_ImpinjLowDutyCycleMode.Disabled;
            impinjDutyCycle.EmptyFieldTimeout = 2000;
            impinjDutyCycle.FieldPingInterval = 200;
            */

            cmd.Custom.Add(impinjSearchMod);

            msg.AntennaConfiguration = new PARAM_AntennaConfiguration[numAntennaToSet];
            for (ushort i = 0; i < numAntennaToSet; ++i)
            {
                msg.AntennaConfiguration[i] = new PARAM_AntennaConfiguration();
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();
                // Add the inventory command to the AI spec.
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings.Add(cmd);
                msg.AntennaConfiguration[i].AntennaID = (ushort)(i + 1);
                msg.AntennaConfiguration[i].RFReceiver = new PARAM_RFReceiver();
                // Receive sensitivity.
                msg.AntennaConfiguration[i].RFReceiver.ReceiverSensitivity = ReaderParameters.ReaderSensitivity;
                msg.AntennaConfiguration[i].RFTransmitter = new PARAM_RFTransmitter();
                // Frequency
                msg.AntennaConfiguration[i].RFTransmitter.ChannelIndex = ReaderParameters.ChannelIndex;
                // ?
                msg.AntennaConfiguration[i].RFTransmitter.HopTableID = ReaderParameters.HopTableIndex;
                // TxPower
                msg.AntennaConfiguration[i].RFTransmitter.TransmitPower = (ushort)(1 + (ReaderParameters.TransmitPower - 10) / 0.25);
            }


            //NOT SUPPORT FOR AntennaProperties
            //msg.AntennaProperties = new PARAM_AntennaProperties[1];
            //msg.AntennaProperties[0] = new PARAM_AntennaProperties();
            //msg.AntennaProperties[0].AntennaConnected = true;
            //msg.AntennaProperties[0].AntennaGain = 0;
            //msg.AntennaProperties[0].AntennaID = 1;

            msg.EventsAndReports = new PARAM_EventsAndReports();
            msg.EventsAndReports.HoldEventsAndReportsUponReconnect = false;

            msg.KeepaliveSpec = new PARAM_KeepaliveSpec();
            msg.KeepaliveSpec.KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Null;
            msg.KeepaliveSpec.PeriodicTriggerValue = ReaderParameters.PeriodicTriggerValue;

            msg.ReaderEventNotificationSpec = new PARAM_ReaderEventNotificationSpec();
            msg.ReaderEventNotificationSpec.EventNotificationState = new PARAM_EventNotificationState[5];

            msg.ReaderEventNotificationSpec.EventNotificationState[0] = new PARAM_EventNotificationState();
            msg.ReaderEventNotificationSpec.EventNotificationState[0].EventType = ENUM_NotificationEventType.AISpec_Event;
            msg.ReaderEventNotificationSpec.EventNotificationState[0].NotificationState = false;

            msg.ReaderEventNotificationSpec.EventNotificationState[1] = new PARAM_EventNotificationState();
            msg.ReaderEventNotificationSpec.EventNotificationState[1].EventType = ENUM_NotificationEventType.Antenna_Event;
            msg.ReaderEventNotificationSpec.EventNotificationState[1].NotificationState = true;

            msg.ReaderEventNotificationSpec.EventNotificationState[2] = new PARAM_EventNotificationState();
            msg.ReaderEventNotificationSpec.EventNotificationState[2].EventType = ENUM_NotificationEventType.GPI_Event;
            msg.ReaderEventNotificationSpec.EventNotificationState[2].NotificationState = false;

            msg.ReaderEventNotificationSpec.EventNotificationState[3] = new PARAM_EventNotificationState();
            msg.ReaderEventNotificationSpec.EventNotificationState[3].EventType = ENUM_NotificationEventType.Reader_Exception_Event;
            msg.ReaderEventNotificationSpec.EventNotificationState[3].NotificationState = true;

            msg.ReaderEventNotificationSpec.EventNotificationState[4] = new PARAM_EventNotificationState();
            msg.ReaderEventNotificationSpec.EventNotificationState[4].EventType = ENUM_NotificationEventType.RFSurvey_Event;
            msg.ReaderEventNotificationSpec.EventNotificationState[4].NotificationState = true;


            msg.ROReportSpec = new PARAM_ROReportSpec();
            msg.ROReportSpec.N = 1;
            msg.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_AISpec;
            msg.ROReportSpec.TagReportContentSelector = new PARAM_TagReportContentSelector();
            msg.ROReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector = new UNION_AirProtocolEPCMemorySelector();

            PARAM_C1G2EPCMemorySelector c1G2Mem = new PARAM_C1G2EPCMemorySelector();
            c1G2Mem.EnableCRC = false;
            c1G2Mem.EnablePCBits = false;
            msg.ROReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector.Add(c1G2Mem);

            msg.ROReportSpec.TagReportContentSelector.EnableAccessSpecID = false;
            msg.ROReportSpec.TagReportContentSelector.EnableAntennaID = true;
            msg.ROReportSpec.TagReportContentSelector.EnableChannelIndex = true;
            msg.ROReportSpec.TagReportContentSelector.EnableFirstSeenTimestamp = true;
            msg.ROReportSpec.TagReportContentSelector.EnableInventoryParameterSpecID = false;
            msg.ROReportSpec.TagReportContentSelector.EnableLastSeenTimestamp = true;
            msg.ROReportSpec.TagReportContentSelector.EnablePeakRSSI = true;
            msg.ROReportSpec.TagReportContentSelector.EnableROSpecID = false;
            msg.ROReportSpec.TagReportContentSelector.EnableSpecIndex = false;
            msg.ROReportSpec.TagReportContentSelector.EnableTagSeenCount = true;

            msg.ResetToFactoryDefault = false;


            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = Reader.SET_READER_CONFIG(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                    WriteMessage(rsp.LLRPStatus.ErrorDescription.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("SET_READER_CONFIG Command Timed out\n");
            }
        }


        public static void ResetReaderToFactoryDefault()
        {
            WriteMessage("Factory Default the Reader ----- ");

            // factory default the Reader
            MSG_SET_READER_CONFIG msg_cfg = new MSG_SET_READER_CONFIG();
            MSG_ERROR_MESSAGE msg_err;

            msg_cfg.ResetToFactoryDefault = true;
            msg_cfg.MSG_ID = 2; //this doesn't need to bet set as the library will default

            //if SET_READER_CONFIG affects antennas it could take several seconds to return
            MSG_SET_READER_CONFIG_RESPONSE rsp_cfg = Reader.SET_READER_CONFIG(msg_cfg, out msg_err, 2000);

            if (rsp_cfg != null) // success
            {
                if (rsp_cfg.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp_cfg.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null) // error
            {
                WriteMessage(msg_err.ToString());
            }
            else // time out
            {
                WriteMessage("SET_READER_CONFIG Command Timed out\n");
            }
        }


        public static void GetReaderCapabilities()
        {
            WriteMessage("Getting Reader Capabilities ----- ");

            MSG_GET_READER_CAPABILITIES cap = new MSG_GET_READER_CAPABILITIES
            {
                MSG_ID = 2,// not this doesn't need to bet set as the library will default
                RequestedData = ENUM_GetReaderCapabilitiesRequestedData.All
            };

            //Send the custom message and wait for 8 seconds
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CAPABILITIES_RESPONSE msg_rsp = Reader.GET_READER_CAPABILITIES(cap, out msg_err, 2000);

            if (msg_rsp != null)
            {
                if (msg_rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(msg_rsp.LLRPStatus.StatusCode.ToString());
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());;
            }
            else
            {
                WriteMessage("GET Reader Capabilities Command Timed out\n");
            }

            // Get the Reader model number
            PARAM_GeneralDeviceCapabilities dev_cap = msg_rsp.GeneralDeviceCapabilities;

            // Check to make sure the model number matches and that this device
            // is an impinj Reader (deviceManufacturerName == 25882)
            if ((dev_cap != null) &&
                (dev_cap.DeviceManufacturerName == 25882))
            {
            }
            else
            {
                WriteMessage("Could not determine Reader model number\n");
            }
        }


        public static void GetReaderConfig()
        {
            WriteMessage("Get Reader Config ----- ");

            MSG_GET_READER_CONFIG msg = new MSG_GET_READER_CONFIG();
            msg.AntennaID = 1;
            msg.GPIPortNum = 0;
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CONFIG_RESPONSE rsp = Reader.GET_READER_CONFIG(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    WriteMessage(rsp.LLRPStatus.StatusCode.ToString());
                    WriteMessage(rsp.LLRPStatus.ErrorDescription);
                }
                WriteMessage(Resources.OK);
            }
            else if (msg_err != null)
            {
                WriteMessage(msg_err.ToString());
            }
            else
            {
                WriteMessage("GET_READER_CONFIG Command Timed out\n");
            }
        }


        public static bool ConnectToReader()
        {
            WriteMessage("Connecting To Reader ----- ");

            ENUM_ConnectionAttemptStatusType status;

            //Open the Reader connection.  Timeout after 5 seconds
            bool ret = Reader.Open(ReaderParameters.Ip, 5000, out status);
            ReaderWrapper.Initialize_DataRow();

            //Ensure that the open succeeded and that the Reader
            // returned the correct connection status result
            if (!ret || status != ENUM_ConnectionAttemptStatusType.Success)
            {
                WriteMessage(status.ToString());
                WriteMessage("Failed to Connect to Reader \n");
                return false;
            }
            WriteMessage(Resources.OK);
            return true;
        }


        public static void reader_AddSubscription()
        {
            WriteMessage("Adding Event Handlers...");
            Reader.OnReaderEventNotification += new delegateReaderEventNotification(reader_OnReaderEventNotification);
            Reader.OnRoAccessReportReceived += new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }


        public static void reader_CleanSubscription()
        {
            WriteMessage("Closing...");
            Reader.Close();
            Reader.OnReaderEventNotification -= new delegateReaderEventNotification(reader_OnReaderEventNotification);
            Reader.OnRoAccessReportReceived -= new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }
        #endregion

        public static void Start()
        {
            reader_AddSubscription();
            ReaderWrapper.ResetReaderToFactoryDefault();
            //ReaderWrapper.GetReaderCapabilities();
            ReaderWrapper.Enable_Impinj_Extensions();

            ReaderWrapper.SetReaderConfig(); //SetReaderConfig_WithXML();
            ReaderWrapper.Add_RoSpec(); //Add_RoSpec_WithXML();
            ReaderWrapper.Enable_RoSpec();
        }

        public static void Stop()
        {
            ReaderWrapper.Stop_RoSpec();
            ReaderWrapper.ResetReaderToFactoryDefault();

            // clean up the Reader
            ReaderWrapper.reader_CleanSubscription();
        }
    } // end of ReaderWrapper
}