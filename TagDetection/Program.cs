/*
 * TagDetection based on Impinj LLRP Tool Kit (LTK)
 *
 * MarinYoung@163.com
 * Last Modified: 2015/11/26
 * Reference:
 * [1]  https://support.impinj.com/hc/en-us/articles/202756168-Hello-LLRP-Low-Level-Reader-Protocol
 * [2]  https://support.impinj.com/hc/en-us/articles/202756348-Get-Low-Level-Reader-Data-with-LLRP
 * [3]  https://support.impinj.com/hc/en-us/articles/204383817-Latest-firmware-utilities-and-development-libraries-for-Impinj-readers-and-gateways
 * [4]  https://github.com/pengyuzhang/Blink/tree/master/ReaderLibrary
 * [5]  Speedway revolution reader application note: Low Level User Data Support
        https://support.impinj.com/hc/en-us/articles/202755318-Application-Note-Low-Level-User-Data-Support
 * [6]  https://support.impinj.com/hc/en-us/articles/202756368-Optimizing-Tag-Throughput-Using-ReaderMode
 */

using System;
using System.Threading;
using System.IO;
using System.Data;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.Impinj;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace SimpleLLRPSample
{
    public class Program
    {
        private static int _reportCount = 0;
        private static int _eventCount = 0;
        private static int _directionCount = 0;

        // state Data collected to use in the velocity algorithm
        private static string _currentEpcData;
        private static ushort _currentAntennaId;
        private static ushort _currentChannelIndex;
        private static ushort _currentRfPhase;
        private static ulong _currentReadTime;
        private static short _currentPeakRssi;
        private static short _currentDopplerFrequency;

        // state Data collected to use in the velocity algorithm
        private static string _lastEpcData;
        private static ushort _lastAntennaId;
        private static ushort _lastChannelIndex;
        private static ushort _lastRfPhase;
        private static ulong _lastReadTime;

        private static double _filteredVelocity;

        // use these factors to reduce computation overload
        // convert rf-phase ([0, 4096]) to phase-angle (Radian, [0, 2*pi]), namely res = (_currentRfPhase / 4096) * 2 * Math.PI
        public static double Convert2Radian = 2 * Math.PI / 4096.0;
        // convert rf-phase ([0, 4096]) to phase (degree, [0, 360]), namely res = (_currentRfPhase / 4096) * 360;
        public static double Convert2Degree = 360.0 / 4096.0;


        public static DataTable Data;
        public static LLRPClient Reader;
        private static Reader.ReaderParameters _readerPara;


        // if LOG_TO_FILE = true, output to log, else, output to console
        private static readonly bool LOG_TO_FILE = true;


        #region Saving Data
        public static void InitializeDataRow()
        {
            // Create columns in Data table
            Data.Columns.Add("EPC");
            Data.Columns.Add("Time");
            Data.Columns.Add("Antenna");
            Data.Columns.Add("Tx Power");
            Data.Columns.Add("Current Frequency");
            Data.Columns.Add("PeakRSSI");
            Data.Columns.Add("Phase Angle");
            Data.Columns.Add("Phase");
            Data.Columns.Add("Doppler Shift");
            Data.Columns.Add("Velocity");

            // Initialize Column Name
            DataRow row = Data.NewRow();
            row["EPC"]               = "EPC";
            row["Doppler Shift"]     = "DopplerShift(Hz)";
            row["Time"]              = "Timestamp";
            row["Antenna"]           = "Antenna";
            row["Tx Power"]          = "TxPower(mW)";
            row["Current Frequency"] = "Frequency(MHz)";
            row["PeakRSSI"]          = "RSS(dbm)";
            row["Phase Angle"]       = "PhaseAngle(Radian)";
            row["Phase"]             = "PhaseAngle(Degree)";
            row["Velocity"]          = "Velocity";
            Data.Rows.Add(row);
        }


        public static void AppendRowToDatatable()
        {
            DataRow row = Data.NewRow();

            row["EPC"] = _currentEpcData;
            row["Time"] = _currentReadTime;
            row["Antenna"] = _currentAntennaId;
            row["TX Power"] = _readerPara.TransmitPower; // Power(dbm) [10 : 0.25 : 32.5], 90 different values.
            row["Current Frequency"] = 920.625 + (_readerPara.ChannelIndex - 1) * 0.25; // Frequency(MHz)[920.625 : 0.25 : 924.375], 16 different channels
            row["PeakRSSI"] = _currentPeakRssi / 100;
            row["Phase Angle"] = _currentRfPhase * Convert2Radian;   //(_currentRfPhase / 4096) * 2 * Math.PI;
            row["Phase"] = _currentRfPhase * Convert2Degree;         //(_currentRfPhase / 4096) * 360;
            row["Doppler Shift"] = _currentDopplerFrequency;
            row["Velocity"] = _filteredVelocity;

            Data.Rows.Add(row);
        }


        public static void SaveData(CsvStreamWriter csvWriter)
        {
            csvWriter.AddData(Data, 1);
            csvWriter.Save();
        }
        #endregion


        #region Output Settings (REPORT/LOGGING)
        // Simple Handler for receiving the tag reports from the Reader
        public static void reader_OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg)
        {
            // Report could be empty
            if (msg.TagReportData != null)
            {
                // Loop through and print out each tag
                for (int i = 0; i < msg.TagReportData.Length; i++)
                {
                    _reportCount++;

                    // just write out the EPC as a hex string for now. It is guaranteed to be
                    // in all LLRP reports regardless of default configuration
                    string data = "EPC: ";
                    string velData;
                    if (msg.TagReportData[i].EPCParameter[0].GetType() == typeof(PARAM_EPC_96))
                    {
                        _currentEpcData = ((PARAM_EPC_96)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                        data += _currentEpcData;
                    }
                    else
                    {
                        _currentEpcData = ((PARAM_EPCData)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                        data += _currentEpcData;
                    }

                    velData = data;

                    // collect some Data for velocity calculation
                    // NOTE: these could be NULL, so we should check
                    if (msg.TagReportData[i].AntennaID != null)
                    {
                        _currentAntennaId = msg.TagReportData[i].AntennaID.AntennaID;
                        data += " ant: " + _currentAntennaId.ToString();
                    }

                    if (msg.TagReportData[i].ChannelIndex != null)
                    {
                        _currentChannelIndex = msg.TagReportData[i].ChannelIndex.ChannelIndex;
                        data += " ch: " + _currentChannelIndex.ToString();
                    }

                    if (msg.TagReportData[i].FirstSeenTimestampUTC != null)
                    {
                        _currentReadTime = msg.TagReportData[i].FirstSeenTimestampUTC.Microseconds;
                        //data += " time: " + _currentReadTime.ToString();
                    }
                    
                    if (msg.TagReportData[i].Custom != null)
                    {
                        for (int x = 0; x < msg.TagReportData[i].Custom.Length; x++)
                        {
                            // try to make a tag direction report out of it
                            Object param = msg.TagReportData[i].Custom[x];
                            if (param is PARAM_ImpinjRFPhaseAngle)
                            {
                                _currentRfPhase = ((PARAM_ImpinjRFPhaseAngle)param).PhaseAngle;
                                data += " Phase: " + _currentRfPhase.ToString();
                            }
                            else if (param is PARAM_ImpinjPeakRSSI)
                            {
                                _currentPeakRssi = ((PARAM_ImpinjPeakRSSI)param).RSSI;
                                data += " RSSI: " + _currentPeakRssi.ToString();
                            }
                            else if (param is PARAM_ImpinjRFDopplerFrequency)
                            {
                                _currentDopplerFrequency = ((PARAM_ImpinjRFDopplerFrequency)param).DopplerFrequency;
                                data += " DF: " + _currentDopplerFrequency.ToString();
                            }

                            // LOGGING
                            // LogHelper.WriteLog(typeof(Program), _currentRfPhase.ToString());
                            // AppendRowToDatatable();
                        }
                        AppendRowToDatatable();
                    }

                    // estimate the velocity and print a filtered version
                    double velocity;
                    if (CalculateVelocity(out velocity))
                    {
                        _directionCount++;
                        /* keep a filtered value. Use a 1 pole IIR here for simplicity */
                        _filteredVelocity = (6 * _filteredVelocity + 4 * velocity) / 10.0;

                        if (_filteredVelocity > 0.25)
                            velData += "---->";
                        else if (_filteredVelocity < -0.25)
                            velData += "<----";
                        else
                            velData += "     ";

                        //velData += " Velocity: " + _filteredVelocity.ToString("F2");
                        //data += " Velocity: " + _filteredVelocity.ToString();
                    }

                    Console.WriteLine(data);

                } // end for
            } // end if
        }

        public static bool CalculateVelocity(out double velocity)
        {
            bool retVal = false;
            velocity = 0;

            /* you have to have two samples from the same EPC on the same
             * antenna and channel.  NOTE: this is just a simple example.
             * More sophisticated apps would create a database with
             * this information per-EPC */
            if ((_lastEpcData == _currentEpcData) &&
                (_lastAntennaId == _currentAntennaId) &&
                (_lastChannelIndex == _currentChannelIndex) &&
                (_lastReadTime < _currentReadTime))
            {
                /* positive velocity is moving towards the antenna */
                double phaseChangeDegrees = (((double)_currentRfPhase - (double)_lastRfPhase) * 360.0) / 4096.0;
                double timeChangeUsec = (double)(_currentReadTime - _lastReadTime);

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
                    ==> velocity = delta_d / delta_t = 
                    */
                    double waveLength = 300000000 / (920.63 + (_readerPara.ChannelIndex-1)*0.25);
                    velocity = ((phaseChangeDegrees * 0.5 * 0.327868852 * 1000000) / (360 * timeChangeUsec));

                    retVal = true;
                }

            }

            // save the current sample as the alst sample
            _lastReadTime = _currentReadTime;
            _lastEpcData = _currentEpcData;
            _lastRfPhase = _currentRfPhase;
            _lastAntennaId = _currentAntennaId;
            _lastChannelIndex = _currentChannelIndex;

            return retVal;
        }

        // Simple Handler for receiving the Reader events from the Reader
        public static void reader_OnReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            // Events could be empty
            if (msg.ReaderEventNotificationData == null) return;

            // Just write out the LTK-XML for now
            _eventCount++;

            // speedway readers always report UTC timestamp
            UNION_Timestamp t = msg.ReaderEventNotificationData.Timestamp;
            PARAM_UTCTimestamp ut = (PARAM_UTCTimestamp)t[0];
            double millis = (ut.Microseconds + 500) / 1000;

            // LLRP reports time in microseconds relative to the Unix Epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = epoch.AddMilliseconds(millis);


            string message = "======\tReader Event " + _eventCount.ToString() + "\t======\n" + now.ToString("O");
            if (LOG_TO_FILE)
                LogHelper.WriteLog(typeof(Program), message);
            else
                Console.WriteLine(message);

            // this is how you would look for individual events of interest
            // Here I just dump the event
            if (msg.ReaderEventNotificationData.AISpecEvent != null)
            {
                message = msg.ReaderEventNotificationData.AISpecEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.AntennaEvent != null)
            {
                message = msg.ReaderEventNotificationData.AntennaEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ConnectionAttemptEvent != null)
            {
                message = msg.ReaderEventNotificationData.ConnectionAttemptEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ConnectionCloseEvent != null)
            {
                message =  msg.ReaderEventNotificationData.ConnectionCloseEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.GPIEvent != null)
            {
                message = msg.ReaderEventNotificationData.GPIEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.HoppingEvent != null)
            {
                message = msg.ReaderEventNotificationData.HoppingEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ReaderExceptionEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReaderExceptionEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent != null)
            {
                message = msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
            if (msg.ReaderEventNotificationData.ROSpecEvent != null)
            {
                message = msg.ReaderEventNotificationData.ROSpecEvent.ToString();
                if (LOG_TO_FILE)
                    LogHelper.WriteLog(typeof(Program), message);
                else
                    Console.WriteLine(message);
            }
        }
        #endregion


        #region RoSpec Settings
        public static void Delete_RoSpec()
        {
            Console.Write("Delete RoSpec ----- ");
            MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
            msg.ROSpecID = 1111;
            MSG_ERROR_MESSAGE msg_err;

            MSG_DELETE_ROSPEC_RESPONSE rsp = Reader.DELETE_ROSPEC(msg, out msg_err, 2000);

            if (rsp != null)// Success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)// Error
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else// Timeout
            {
                Console.WriteLine("DELETE_ROSPEC Command Timeout Error.");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Enable_Impinj_Extensions()
        {
            Console.Write("Enabling Impinj Extensions ------ ");

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
                    Console.WriteLine(msg_rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)   // Error
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else    // Time out
            {
                Console.WriteLine("Enable Extensions Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Add_RoSpec()
        {
            Console.Write("Adding RoSpec ----- ");
            MSG_ERROR_MESSAGE msg_err;
            // ROBoundarySpec
            // Specifies the start and stop triggers for the ROSpec
            // Immediate start trigger
            // The Reader will start reading tags as soon as the ROSpec is enabled
            // No stop trigger. Keep reading tags until the ROSpec is disabled.
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
            for (ushort i = 1; i <= _readerPara.AntennaId.Length; ++i )
            {
                if (_readerPara.AntennaId[i - 1])
                    aiSpec.AntennaIDs.Add(i);
            }
            // No AISpec stop trigger. It stops when the ROSpec stops.
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
            // Add 1)RF Phase Angle;  2)Peak RSSI  3)RF Doppler Frequency  fields to the report
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
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
            }
            else
            {
                Console.WriteLine("ADD_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        static void Add_RoSpec_WithXML()
        {
            Console.Write("Adding RoSpec from XML file -----\n");

            Org.LLRP.LTK.LLRPV1.DataType.Message obj;
            ENUM_LLRP_MSG_TYPE msg_type;

            // read the XML from a file and validate its an ADD_ROSPEC
            Console.Write("\tLoading XML file ----- ");
            try
            {
                FileStream fs = new FileStream(@"..\..\addRoSpec.xml", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd();
                fs.Close();

                LLRPXmlParser.ParseXMLToLLRPMessage(s, out obj, out msg_type);

                if (obj == null || msg_type != ENUM_LLRP_MSG_TYPE.ADD_ROSPEC)
                {
                    Console.WriteLine("Could not extract message from XML");
                    Reader.Close();
                    return;
                }
                Console.Write("OK!\n");
            }
            catch
            {
                Console.WriteLine("Unable to convert to valid XML");
                Reader.Close();
                return;
            }

            Console.Write("\tAdding RoSpec ----- ");
            // covert to the proper message type
            MSG_ADD_ROSPEC msg = (MSG_ADD_ROSPEC)obj;

            // Communicate that message to the Reader
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC_RESPONSE rsp = Reader.ADD_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("ADD_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Enable_RoSpec()
        {
            Console.Write("Enabling RoSpec ----- ");
            MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the ROSpec we created above
            MSG_ENABLE_ROSPEC_RESPONSE rsp = Reader.ENABLE_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)//success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)//error
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else//time out
            {
                Console.WriteLine("ENABLE_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Start_RoSpec()
        {
            Console.Write("Starting RoSpec ----- ");
            MSG_START_ROSPEC msg = new MSG_START_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the RoSpec we created above
            MSG_START_ROSPEC_RESPONSE rsp = Reader.START_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
                Console.WriteLine(rsp.LLRPStatus.ToString());
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("START_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Stop_RoSpec()
        {
            Console.Write("Stopping RoSpec ----- ");
            MSG_STOP_ROSPEC msg = new MSG_STOP_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the RoSpec we created above
            MSG_STOP_ROSPEC_RESPONSE rsp = Reader.STOP_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("STOP_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Get_RoSpec()
        {
            Console.Write("Getting RoSpec ----- ");
            MSG_GET_ROSPECS msg = new MSG_GET_ROSPECS();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE rsp = Reader.GET_ROSPECS(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET_ROSPEC Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }
        #endregion


        #region Reader Configuration
        public static void SetReaderConfig_WithXML()
        {
            Console.Write("Adding SET_READER_CONFIG from XML file ----- ");

            Org.LLRP.LTK.LLRPV1.DataType.Message obj;
            ENUM_LLRP_MSG_TYPE msg_type;

            // read the XML from a file and validate its an ADD_ROSPEC
            try
            {
                FileStream fs = new FileStream(@"..\..\setReaderConfig.xml", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd();
                fs.Close();

                LLRPXmlParser.ParseXMLToLLRPMessage(s, out obj, out msg_type);

                if (obj == null || msg_type != ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG)
                {
                    Console.WriteLine("Could not extract message from XML");
                    Reader.Close();
                    return;
                }
            }
            catch
            {
                Console.WriteLine("Unable to convert to valid XML");
                Reader.Close();
                return;
            }

            // Communicate that message to the Reader
            MSG_SET_READER_CONFIG msg = (MSG_SET_READER_CONFIG)obj;
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = Reader.SET_READER_CONFIG(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void SetReaderConfig()
        {
            Console.Write("Set Reader Configuration ----- ");

            ushort numAntennaToSet = 0;
            for (ushort i = 0; i < _readerPara.AntennaId.Length; ++i)
            {
                if (_readerPara.AntennaId[i])
                    ++numAntennaToSet;
            }

            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
            msg.AccessReportSpec = new PARAM_AccessReportSpec();
            msg.AccessReportSpec.AccessReportTrigger = ENUM_AccessReportTriggerType.End_Of_AccessSpec;

            PARAM_C1G2InventoryCommand cmd = new PARAM_C1G2InventoryCommand();
            cmd.C1G2RFControl = new PARAM_C1G2RFControl();

            cmd.C1G2RFControl.ModeIndex = 1000;//_readerPara.ModeIndex;
            cmd.C1G2RFControl.Tari = 10;
            cmd.C1G2SingulationControl = new PARAM_C1G2SingulationControl();
            cmd.C1G2SingulationControl.Session = new TwoBits(0);
            cmd.C1G2SingulationControl.TagPopulation = _readerPara.TagPopulation;
            cmd.C1G2SingulationControl.TagTransitTime = _readerPara.TagTransitTime;
            cmd.TagInventoryStateAware = false;

            msg.AntennaConfiguration = new PARAM_AntennaConfiguration[numAntennaToSet];
            for (ushort i = 0; i < numAntennaToSet; ++i)
            {
                msg.AntennaConfiguration[i] = new PARAM_AntennaConfiguration();
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings.Add(cmd);
                msg.AntennaConfiguration[i].AntennaID = (ushort)(i + 1);
                msg.AntennaConfiguration[i].RFReceiver = new PARAM_RFReceiver();
                msg.AntennaConfiguration[i].RFReceiver.ReceiverSensitivity = _readerPara.ReaderSensitivity;
                msg.AntennaConfiguration[i].RFTransmitter = new PARAM_RFTransmitter();
                msg.AntennaConfiguration[i].RFTransmitter.ChannelIndex = _readerPara.ChannelIndex;
                msg.AntennaConfiguration[i].RFTransmitter.HopTableID = _readerPara.HopTableIndex;
                msg.AntennaConfiguration[i].RFTransmitter.TransmitPower = (ushort)(1 + (_readerPara.TransmitPower-10)/0.25);
            }


            /* 
             * possible values:
             * Dual_Target;     Dual_Target_with_BtoASelect;
             * No_Target;       Reader_Selected;
             * Single_Target;   Single_Target_BtoA;         Single_Target_With_Suppression
             */
            PARAM_ImpinjInventorySearchMode impinjSearchMod = new PARAM_ImpinjInventorySearchMode();
            impinjSearchMod.InventorySearchMode = ENUM_ImpinjInventorySearchType.Single_Target;

            
            PARAM_ImpinjLowDutyCycle impinjDutyCycle = new PARAM_ImpinjLowDutyCycle();
            impinjDutyCycle.LowDutyCycleMode = ENUM_ImpinjLowDutyCycleMode.Disabled;
            impinjDutyCycle.EmptyFieldTimeout = 2000;
            impinjDutyCycle.FieldPingInterval = 200;


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
            msg.KeepaliveSpec.PeriodicTriggerValue = _readerPara.PeriodicTriggerValue;

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
            PARAM_C1G2EPCMemorySelector c1g2mem = new PARAM_C1G2EPCMemorySelector();
            c1g2mem.EnableCRC = false; // CRC
            c1g2mem.EnablePCBits = false; // PC
            msg.ROReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector.Add(c1g2mem);
            msg.ROReportSpec.TagReportContentSelector.EnableAccessSpecID = false;
            msg.ROReportSpec.TagReportContentSelector.EnableAntennaID = true;
            msg.ROReportSpec.TagReportContentSelector.EnableChannelIndex = true;
            msg.ROReportSpec.TagReportContentSelector.EnableFirstSeenTimestamp = true;
            msg.ROReportSpec.TagReportContentSelector.EnableInventoryParameterSpecID = false;
            msg.ROReportSpec.TagReportContentSelector.EnableLastSeenTimestamp = false;
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
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void ResetReaderToFactoryDefault()
        {
            Console.Write("Factory Default the Reader ----- ");

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
                    Console.WriteLine(rsp_cfg.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null) // error
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else // time out
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void GetReaderCapabilities()
        {
            Console.Write("Getting Reader Capabilities ----- ");

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
                    Console.WriteLine(msg_rsp.LLRPStatus.StatusCode.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET Reader Capabilities Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
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
                Console.WriteLine("Could not determine Reader model number\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void GetReaderConfig()
        {
            Console.Write("Get Reader Config ----- ");

            MSG_GET_READER_CONFIG msg = new MSG_GET_READER_CONFIG();
            msg.AntennaID = 1;
            msg.GPIPortNum = 0;
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CONFIG_RESPONSE rsp = Reader.GET_READER_CONFIG(msg, out msg_err, 2000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    Reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                Reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET_READER_CONFIG Command Timed out\n");
                Reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void ConnectTo(string ip)
        {
            Console.Write("Connecting To Reader ----- ");

            ENUM_ConnectionAttemptStatusType status;

            //Open the Reader connection.  Timeout after 5 seconds
            bool ret = Reader.Open(ip, 2000, out status);

            //Ensure that the open succeeded and that the Reader
            // returned the correct connection status result
            if (!ret || status != ENUM_ConnectionAttemptStatusType.Success)
            {
                Console.WriteLine(status.ToString());
                Console.WriteLine("Failed to Connect to Reader \n");
                return;
            }
            Console.WriteLine("OK!\n");
        }


        public static void reader_AddSubscription()
        {
            Console.WriteLine("Adding Event Handlers...\n");
            Reader.OnReaderEventNotification += new delegateReaderEventNotification(reader_OnReaderEventNotification);
            Reader.OnRoAccessReportReceived += new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }


        public static void reader_CleanSubscription()
        {
            Console.WriteLine("Closing...\n");
            Reader.Close();
            Reader.OnReaderEventNotification -= new delegateReaderEventNotification(reader_OnReaderEventNotification);
            Reader.OnRoAccessReportReceived -= new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }

        public static void InitializeConfiguration()
        {
            Console.WriteLine("Initializing...\n");

            // Create an instance of LLRP Reader client.
            Reader = new LLRPClient();
            // Create an DataTable to save the current state of Tag
            Data = new DataTable();
            // Set Reader Config in Default Way.
            _readerPara = new Reader.ReaderParameters();

            //Impinj Best Practice! Always Install the Impinj extensions
            Impinj_Installer.Install();
        }

        public static void setReader_PARM()
        {
            if (_readerPara == null) return;
            /*
             * Set Channel Index, 16 in total, which represents different frequency (MHz). Namely,
             * [1: 920.38]
             * [2: 920.88]
             * ...... 
             * [16: 924.38]
            */
            _readerPara.ChannelIndex = 16;
            _readerPara.TransmitPower = 32.5; // range from 10 dbm to 32.5 dbm , 0.25, 90 values in total
           /*
            * ModeIndex  NAME                  SENSITIVITY     INTERFERENCE TOLERANCE
            *   0        Max Throughput        good            poor
            *   1        Hybrid                good            good
            *   2        Dense Reader (M=4)    better          excellent
            *   3        Dense Reader (M=8)    best            excellent
            *   4*       MaxMiller             better          good
            *   * MaxMiller is not available in all regions
            */
            _readerPara.ModeIndex = 1000;
            _readerPara.HopTableIndex = 0;
            _readerPara.PeriodicTriggerValue = 0;
            _readerPara.TagPopulation = 2;
            _readerPara.TagTransitTime = 0;
            _readerPara.ReaderSensitivity = 1;
            // each value in the array map to Antenna 1, Antenna 2, Antenna 3, Antenna 4, respectively.
            _readerPara.AntennaId = new bool[] { true, false, false, false };
        }
        #endregion

        public static void Main()
        {
            const string ipAddress = "169.254.1.1";
            const string fpath = @"C:\Users\Marin\OneDrive\Documents\DATA\20160127\test\";
            if (!Directory.Exists(fpath))
                Directory.CreateDirectory(fpath);

            DateTime dt = DateTime.Now;
            string strCurrentTime = dt.ToString("MMddHHmmss");
            string fname = "r420_curr_LLRP10.22_"+strCurrentTime + ".csv";

            // set Data collection time (s)
            const ushort sustainTime = 20;

            Console.WriteLine("Impinj C# LTK.NET RFID Application DocSample4 Reader ----- " + ipAddress + "\n");

            InitializeConfiguration();
            InitializeDataRow();
            setReader_PARM();
            ConnectTo(ipAddress);

            //subscribe to client event notification and ro access report
            reader_AddSubscription();
            ResetReaderToFactoryDefault();
            GetReaderCapabilities();
            Enable_Impinj_Extensions();
            
            SetReaderConfig();  //SetReaderConfig_WithXML();
            Add_RoSpec();       //Add_RoSpec_WithXML();
            Enable_RoSpec();
            //Start_RoSpec();


            // wait around to collect some Data (UNIT: ms)
            Thread.Sleep(sustainTime * 1000);

            Stop_RoSpec();
            ResetReaderToFactoryDefault();

            Console.WriteLine("  Calculated {0} Velocity Estimates.", _directionCount);
            Console.WriteLine("  Received {0} Tag Reports.", _reportCount);
            Console.WriteLine("  Received {0} Events.", _eventCount);

            // clean up the Reader
            reader_CleanSubscription();

            // save Data into csv file.
            Console.WriteLine("Saving Data...");
            Console.WriteLine(fpath + fname);
            CsvStreamWriter csvWriter = new CsvStreamWriter(fpath + fname);
            SaveData(csvWriter);

            Console.WriteLine("DONE!\n");
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey(true);
            //Thread.Sleep(5000);
        }
    }
}
