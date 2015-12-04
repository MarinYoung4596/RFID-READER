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
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Data;

using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.Impinj;
using Org.LLRP.LTK.LLRPV1.DataType;
using log4net;

using SimpleLLRPSample;

namespace SimpleLLRPSample
{
    public class Program
    {
        private static int reportCount = 0;
        private static int eventCount = 0;
        private static int directionCount = 0;
        private static UInt32 modelNumber = 0;

        // state data collected to use in the velocity algorithm
        private static String currentEpcData;
        private static UInt16 currentAntennaID;
        private static UInt16 currentChannelIndex;
        private static UInt16 currentRfPhase;
        private static UInt64 currentReadTime;
        private static Int16 currentPeakRSSI;
        private static Int16 currentDopplerFrequency;

        // state data collected to use in the velocity algorithm
        private static String lastEpcData;
        private static UInt16 lastAntennaID;
        private static UInt16 lastChannelIndex;
        private static UInt16 lastRfPhase;
        private static UInt64 lastReadTime;

        private static double filteredVelocity;

        // use these factors to reduce computation overload
        // convert rf-phase ([0, 4096]) to phase-angle (Radian, [0, 2*pi]), namely res = (currentRfPhase / 4096) * 2 * Math.PI
        private static double convert2radian = 2 * Math.PI / 4096.0;
        // convert rf-phase ([0, 4096]) to phase (degree, [0, 360]), namely res = (currentRfPhase / 4096) * 360;
        private static double convert2degree = 360.0 / 4096.0;


        public static DataTable data;
        public static LLRPClient reader;
        private static Reader.ReaderParameters reader_para;


        #region Saving Data
        public static void InitializeDataRow()
        {
            // Create columns in data table
            data.Columns.Add("EPC");
            data.Columns.Add("Time");
            data.Columns.Add("Antenna");
            data.Columns.Add("Tx Power");
            data.Columns.Add("Current Frequency");
            data.Columns.Add("PeakRSSI");
            data.Columns.Add("Phase Angle");
            data.Columns.Add("Phase");
            data.Columns.Add("Doppler Shift");
            data.Columns.Add("Velocity");

            // Initialize Column Name
            DataRow row = data.NewRow();
            row["EPC"] = "EPC";
            row["Time"] = "Timestamp";
            row["Antenna"] = "Antenna";
            row["Tx Power"] = "TxPower";
            row["Current Frequency"] = "Frequency(MHz)";
            row["PeakRSSI"] = "RSS(dbm)";
            row["Phase Angle"] = "PhaseAngle(Radian)";
            row["Phase"] = "PhaseAngle(Degree)";
            row["Doppler Shift"] = "DopplerShift(Hz)";
            row["Velocity"] = "Velocity";
            data.Rows.Add(row);
        }


        public static void AppendRowToDatatable()
        {
            DataRow row = data.NewRow();

            row["EPC"] = currentEpcData;
            row["Time"] = currentReadTime;
            row["Antenna"] = currentAntennaID;
            // !!!
            row["TX Power"] = (ushort)(61 - reader_para.Attenuation * 4);
            // [920.63 : 0.25 : 924.38], 16 different channels
            row["Current Frequency"] = 920.63 + (reader_para.ChannelIndex - 1) * 0.25;
            row["PeakRSSI"] = currentPeakRSSI / 100;
            row["Phase Angle"] = currentRfPhase * convert2radian;   //(currentRfPhase / 4096) * 2 * Math.PI;
            row["Phase"] = currentRfPhase * convert2degree;         //(currentRfPhase / 4096) * 360;
            row["Doppler Shift"] = currentDopplerFrequency;
            row["Velocity"] = filteredVelocity;

            data.Rows.Add(row);
        }


        public static void SaveData(CsvStreamWriter CsvWriter)
        {
            CsvWriter.AddData(data, 1);
            CsvWriter.Save();
        }
        #endregion


        #region Output Settings (REPORT/LOGGING)
        // Simple Handler for receiving the tag reports from the reader
        public static void reader_OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg)
        {
            // Report could be empty
            if (msg.TagReportData != null)
            {
                // Loop through and print out each tag
                for (int i = 0; i < msg.TagReportData.Length; i++)
                {
                    reportCount++;

                    // just write out the EPC as a hex string for now. It is guaranteed to be
                    // in all LLRP reports regardless of default configuration
                    string data = "EPC: ";
                    string velData;
                    if (msg.TagReportData[i].EPCParameter[0].GetType() == typeof(PARAM_EPC_96))
                    {
                        currentEpcData = ((PARAM_EPC_96)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                        data += currentEpcData;
                    }
                    else
                    {
                        currentEpcData = ((PARAM_EPCData)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                        data += currentEpcData;
                    }

                    velData = data;

                    // collect some data for velocity calculation
                    // NOTE: these could be NULL, so we should check
                    if (msg.TagReportData[i].AntennaID != null)
                    {
                        currentAntennaID = msg.TagReportData[i].AntennaID.AntennaID;
                        data += " ant: " + currentAntennaID.ToString();
                    }

                    if (msg.TagReportData[i].ChannelIndex != null)
                    {
                        currentChannelIndex = msg.TagReportData[i].ChannelIndex.ChannelIndex;
                        data += " ch: " + currentChannelIndex.ToString();
                    }

                    if (msg.TagReportData[i].FirstSeenTimestampUTC != null)
                    {
                        currentReadTime = msg.TagReportData[i].FirstSeenTimestampUTC.Microseconds;
                        data += " time: " + currentReadTime.ToString();
                    }

                    if (msg.TagReportData[i].Custom != null)
                    {
                        for (int x = 0; x < msg.TagReportData[i].Custom.Length; x++)
                        {
                            // try to make a tag direction report out of it
                            Object param = msg.TagReportData[i].Custom[x];
                            if (param is PARAM_ImpinjRFPhaseAngle)
                            {
                                currentRfPhase = ((PARAM_ImpinjRFPhaseAngle)param).PhaseAngle;
                                data += " Phase: " + currentRfPhase.ToString();
                            }
                            else if (param is PARAM_ImpinjPeakRSSI)
                            {
                                currentPeakRSSI = ((PARAM_ImpinjPeakRSSI)param).RSSI;
                                data += " RSSI: " + currentPeakRSSI.ToString();
                            }
                            else if (param is PARAM_ImpinjRFDopplerFrequency)
                            {
                                currentDopplerFrequency = ((PARAM_ImpinjRFDopplerFrequency)param).DopplerFrequency;
                                data += " DF: " + currentDopplerFrequency.ToString();
                            }

                            // LOGGING
                            // LogHelper.WriteLog(typeof(Program), currentRfPhase.ToString());
                            // AppendRowToDatatable();
                        }
                        AppendRowToDatatable();
                    }

                    // estimate the velocity and print a filtered version
                    double velocity;
                    if (calculateVelocity(out velocity))
                    {
                        directionCount++;
                        /* keep a filtered value. Use a 1 pole IIR here for simplicity */
                        filteredVelocity = (6 * filteredVelocity + 4 * velocity) / 10.0;

                        if (filteredVelocity > 0.25)
                            velData += "---->";
                        else if (filteredVelocity < -0.25)
                            velData += "<----";
                        else
                            velData += "     ";

                        //velData += " Velocity: " + filteredVelocity.ToString("F2");
                        data += " Velocity: " + filteredVelocity.ToString();
                    }

                    Console.WriteLine(data);
                    //Console.WriteLine(velData);

                } // end for
            } // end if
        }

        public static bool calculateVelocity(out double velocity)
        {
            bool retVal = false;
            velocity = 0;

            /* you have to have two samples from the same EPC on the same
             * antenna and channel.  NOTE: this is just a simple example.
             * More sophisticated apps would create a database with
             * this information per-EPC */
            if ((lastEpcData == currentEpcData) &&
                (lastAntennaID == currentAntennaID) &&
                (lastChannelIndex == currentChannelIndex) &&
                (lastReadTime < currentReadTime))
            {
                /* positive velocity is moving towards the antenna */
                double phaseChangeDegrees = (((double)currentRfPhase - (double)lastRfPhase) * 360.0) / 4096.0;
                double timeChangeUsec = (double)(currentReadTime - lastReadTime);

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
                    ** 0.32786885245901635 meters. The formula below eports meters per second.
                    ** Note that 360 degrees equals only 1/2 a wavelength of motion because
                    ** we are computing the round trip phase change.
                    **
                    **  phaseChange (degrees)   1/2 wavelength     0.327 meter      1000000 usec
                    **  --------------------- * -------------- * ---------------- * ------------
                    **  timeChange (usec)       360 degrees       1  wavelength      1 second
                    **
                    ** which should net out to estimated tag velocity in meters/second */

                    velocity = ((phaseChangeDegrees * 0.5 * 0.327868852 * 1000000) / (360 * timeChangeUsec));

                    retVal = true;
                }

            }

            // save the current sample as the alst sample
            lastReadTime = currentReadTime;
            lastEpcData = currentEpcData;
            lastRfPhase = currentRfPhase;
            lastAntennaID = currentAntennaID;
            lastChannelIndex = currentChannelIndex;

            return retVal;
        }

        // Simple Handler for receiving the reader events from the reader
        public static void reader_OnReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            // Events could be empty
            if (msg.ReaderEventNotificationData == null) return;

            // Just write out the LTK-XML for now
            eventCount++;

            // speedway readers always report UTC timestamp
            UNION_Timestamp t = msg.ReaderEventNotificationData.Timestamp;
            PARAM_UTCTimestamp ut = (PARAM_UTCTimestamp)t[0];
            double millis = (ut.Microseconds + 500) / 1000;

            // LLRP reports time in microseconds relative to the Unix Epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = epoch.AddMilliseconds(millis);

            Console.WriteLine("======\tReader Event " + eventCount.ToString() + "\t======\t" + now.ToString("O"));

            // this is how you would look for individual events of interest
            // Here I just dump the event
            if (msg.ReaderEventNotificationData.AISpecEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AISpecEvent.ToString());
            if (msg.ReaderEventNotificationData.AntennaEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AntennaEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionAttemptEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionAttemptEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionCloseEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionCloseEvent.ToString());
            if (msg.ReaderEventNotificationData.GPIEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.GPIEvent.ToString());
            if (msg.ReaderEventNotificationData.HoppingEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.HoppingEvent.ToString());
            if (msg.ReaderEventNotificationData.ReaderExceptionEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReaderExceptionEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent.ToString());
            if (msg.ReaderEventNotificationData.ROSpecEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ROSpecEvent.ToString());
        }
        #endregion


        #region RoSpec Settings
        public static void Delete_RoSpec()
        {
            Console.Write("Delete RoSpec ----- ");
            MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
            msg.ROSpecID = 1111;
            MSG_ERROR_MESSAGE msg_err;

            MSG_DELETE_ROSPEC_RESPONSE rsp = reader.DELETE_ROSPEC(msg, out msg_err, 2000);

            if (rsp != null)// Success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)// Error
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else// Timeout
            {
                Console.WriteLine("DELETE_ROSPEC Command Timeout Error.");
                reader.Close();
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
            MSG_CUSTOM_MESSAGE cust_rsp = reader.CUSTOM_MESSAGE(imp_msg, out msg_err, 8000);
            MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE msg_rsp = cust_rsp as MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE;

            if (msg_rsp != null)    // Success
            {
                if (msg_rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(msg_rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)   // Error
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else    // Time out
            {
                Console.WriteLine("Enable Extensions Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Add_RoSpec()
        {
            Console.Write("Adding RoSpec ----- ");
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC msg = new MSG_ADD_ROSPEC();

            // Reader Operation Spec (ROSpec)
            msg.ROSpec = new PARAM_ROSpec();
            // ROSpec must be disabled by default
            msg.ROSpec.CurrentState = ENUM_ROSpecState.Disabled;
            // The ROSpec ID can be set to any number
            // You must use the same ID when enabling this ROSpec
            msg.ROSpec.ROSpecID = 1111;

            // ROBoundarySpec
            // Specifies the start and stop triggers for the ROSpec
            msg.ROSpec.ROBoundarySpec = new PARAM_ROBoundarySpec();
            // Immediate start trigger
            // The reader will start reading tags as soon as the ROSpec is enabled
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Immediate;
            // No stop trigger. Keep reading tags until the ROSpec is disabled.
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;

            // Antenna Inventory Spec (AISpec)
            // Specifies which antennas and protocol to use
            msg.ROSpec.SpecParameter = new UNION_SpecParameter();

            // Antenna inventory operation
            PARAM_AISpec aiSpec = new PARAM_AISpec();
            aiSpec.AntennaIDs = new UInt16Array();
            // Enable all antennas
            for (ushort i = 1; i <= reader_para.AntennaID.Length; ++i )
            {
                if (reader_para.AntennaID[i - 1])
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
            // Send a report for every tag read
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

            MSG_ADD_ROSPEC_RESPONSE rsp = reader.ADD_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                reader.Close();
            }
            else
            {
                Console.WriteLine("ADD_ROSPEC Command Timed out\n");
                reader.Close();
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
                    reader.Close();
                    return;
                }
                Console.Write("OK!\n");
            }
            catch
            {
                Console.WriteLine("Unable to convert to valid XML");
                reader.Close();
                return;
            }

            Console.Write("\tAdding RoSpec ----- ");
            // covert to the proper message type
            MSG_ADD_ROSPEC msg = (MSG_ADD_ROSPEC)obj;

            // Communicate that message to the reader
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC_RESPONSE rsp = reader.ADD_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("ADD_ROSPEC Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Enable_RoSpec()
        {
            Console.Write("Enabling RoSpec ----- ");
            MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the ROSpec we created above
            MSG_ENABLE_ROSPEC_RESPONSE rsp = reader.ENABLE_ROSPEC(msg, out msg_err, 2000);
            if (rsp != null)//success
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)//error
            {
                Console.WriteLine("ERROR!\n\t");
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else//time out
            {
                Console.WriteLine("ENABLE_ROSPEC Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Start_RoSpec()
        {
            Console.Write("Starting RoSpec ----- ");
            MSG_START_ROSPEC msg = new MSG_START_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the RoSpec we created above
            MSG_START_ROSPEC_RESPONSE rsp = reader.START_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
                Console.WriteLine(rsp.LLRPStatus.ToString());
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("START_ROSPEC Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Stop_RoSpec()
        {
            Console.Write("Stopping RoSpec ----- ");
            MSG_STOP_ROSPEC msg = new MSG_STOP_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = 1111; // this better match the RoSpec we created above
            MSG_STOP_ROSPEC_RESPONSE rsp = reader.STOP_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("STOP_ROSPEC Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void Get_RoSpec()
        {
            Console.Write("Getting RoSpec ----- ");
            MSG_GET_ROSPECS msg = new MSG_GET_ROSPECS();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE rsp = reader.GET_ROSPECS(msg, out msg_err, 3000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET_ROSPEC Command Timed out\n");
                reader.Close();
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
                    reader.Close();
                    return;
                }
            }
            catch
            {
                Console.WriteLine("Unable to convert to valid XML");
                reader.Close();
                return;
            }

            // Communicate that message to the reader
            MSG_SET_READER_CONFIG msg = (MSG_SET_READER_CONFIG)obj;
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = reader.SET_READER_CONFIG(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void SetReaderConfig()
        {
            Console.Write("Set Reader Configuration ----- ");

            ushort numAntennaToSet = 0;
            for (ushort i = 0; i < reader_para.AntennaID.Length; ++i)
            {
                if (reader_para.AntennaID[i])
                    ++numAntennaToSet;
            }

            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
            msg.AccessReportSpec = new PARAM_AccessReportSpec();
            msg.AccessReportSpec.AccessReportTrigger = ENUM_AccessReportTriggerType.End_Of_AccessSpec;

            PARAM_C1G2InventoryCommand cmd = new PARAM_C1G2InventoryCommand();
            cmd.C1G2RFControl = new PARAM_C1G2RFControl();
            cmd.C1G2RFControl.ModeIndex = reader_para.ModeIndex;
            cmd.C1G2RFControl.Tari = 0;
            cmd.C1G2SingulationControl = new PARAM_C1G2SingulationControl();
            cmd.C1G2SingulationControl.Session = new TwoBits(0);
            cmd.C1G2SingulationControl.TagPopulation = reader_para.TagPopulation;
            cmd.C1G2SingulationControl.TagTransitTime = reader_para.TagTransitTime;
            cmd.TagInventoryStateAware = false;

            msg.AntennaConfiguration = new PARAM_AntennaConfiguration[numAntennaToSet];
            for (ushort i = 0; i < numAntennaToSet; ++i)
            {
                msg.AntennaConfiguration[i] = new PARAM_AntennaConfiguration();
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();
                msg.AntennaConfiguration[i].AirProtocolInventoryCommandSettings.Add(cmd);
                msg.AntennaConfiguration[i].AntennaID = (ushort)(i + 1);
                msg.AntennaConfiguration[i].RFReceiver = new PARAM_RFReceiver();
                msg.AntennaConfiguration[i].RFReceiver.ReceiverSensitivity = reader_para.ReaderSensitivity;
                msg.AntennaConfiguration[i].RFTransmitter = new PARAM_RFTransmitter();
                msg.AntennaConfiguration[i].RFTransmitter.ChannelIndex = reader_para.ChannelIndex;
                msg.AntennaConfiguration[i].RFTransmitter.HopTableID = reader_para.HopTableIndex;
                msg.AntennaConfiguration[i].RFTransmitter.TransmitPower = (ushort)(61 - reader_para.Attenuation * 4);
            }


            // NOT SUPPORT FOR AntennaProperties
            //msg.AntennaProperties = new PARAM_AntennaProperties[1];
            //msg.AntennaProperties[0] = new PARAM_AntennaProperties();
            //msg.AntennaProperties[0].AntennaConnected = true;
            //msg.AntennaProperties[0].AntennaGain = 0;
            //msg.AntennaProperties[0].AntennaID = 1;

            msg.EventsAndReports = new PARAM_EventsAndReports();
            msg.EventsAndReports.HoldEventsAndReportsUponReconnect = false;

            msg.KeepaliveSpec = new PARAM_KeepaliveSpec();
            msg.KeepaliveSpec.KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Null;
            msg.KeepaliveSpec.PeriodicTriggerValue = reader_para.PeriodicTriggerValue;

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
            c1g2mem.EnableCRC = false;
            c1g2mem.EnablePCBits = false;
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
            MSG_SET_READER_CONFIG_RESPONSE rsp = reader.SET_READER_CONFIG(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void ResetReaderToFactoryDefault()
        {
            Console.Write("Factory Default the Reader ----- ");

            // factory default the reader
            MSG_SET_READER_CONFIG msg_cfg = new MSG_SET_READER_CONFIG();
            MSG_ERROR_MESSAGE msg_err;

            msg_cfg.ResetToFactoryDefault = true;
            msg_cfg.MSG_ID = 2; //this doesn't need to bet set as the library will default

            //if SET_READER_CONFIG affects antennas it could take several seconds to return
            MSG_SET_READER_CONFIG_RESPONSE rsp_cfg = reader.SET_READER_CONFIG(msg_cfg, out msg_err, 12000);

            if (rsp_cfg != null) // success
            {
                if (rsp_cfg.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp_cfg.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null) // error
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else // time out
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void GetReaderCapabilities()
        {
            Console.Write("Getting Reader Capabilities ----- ");

            MSG_GET_READER_CAPABILITIES cap = new MSG_GET_READER_CAPABILITIES();
            cap.MSG_ID = 2; // not this doesn't need to bet set as the library will default
            cap.RequestedData = ENUM_GetReaderCapabilitiesRequestedData.All;

            //Send the custom message and wait for 8 seconds
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CAPABILITIES_RESPONSE msg_rsp = reader.GET_READER_CAPABILITIES(cap, out msg_err, 8000);

            if (msg_rsp != null)
            {
                if (msg_rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(msg_rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET reader Capabilities Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }

            // Get the reader model number
            PARAM_GeneralDeviceCapabilities dev_cap = msg_rsp.GeneralDeviceCapabilities;

            // Check to make sure the model number matches and that this device
            // is an impinj reader (deviceManufacturerName == 25882)
            if ((dev_cap != null) &&
                (dev_cap.DeviceManufacturerName == 25882))
            {
                modelNumber = dev_cap.ModelName;
            }
            else
            {
                Console.WriteLine("Could not determine reader model number\n");
                reader.Close();
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
            MSG_GET_READER_CONFIG_RESPONSE rsp = reader.GET_READER_CONFIG(msg, out msg_err, 3000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    Console.WriteLine(rsp.LLRPStatus.ErrorDescription.ToString());
                    reader.Close();
                    System.Environment.Exit(1);
                }
                Console.WriteLine("OK!\n");
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                System.Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("GET_READER_CONFIG Command Timed out\n");
                reader.Close();
                System.Environment.Exit(1);
            }
        }


        public static void ConnectTo(string ip)
        {
            Console.Write("Connecting To Reader ----- ");

            ENUM_ConnectionAttemptStatusType status;

            //Open the reader connection.  Timeout after 5 seconds
            bool ret = reader.Open(ip, 5000, out status);

            //Ensure that the open succeeded and that the reader
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
            reader.OnReaderEventNotification += new delegateReaderEventNotification(reader_OnReaderEventNotification);
            reader.OnRoAccessReportReceived += new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }


        public static void reader_CleanSubscription()
        {
            Console.WriteLine("Closing...\n");
            reader.Close();
            reader.OnReaderEventNotification -= new delegateReaderEventNotification(reader_OnReaderEventNotification);
            reader.OnRoAccessReportReceived -= new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }


        public static void setReader_PARM()
        {
            /*
             * Set Channel Index, 16 in total, which represents frequency (MHz). Namely,
             * 1: 920.63; 2: 920.88; ...... ; 16: 924.38
             */
            reader_para.ChannelIndex = 1;
            reader_para.Attenuation = 0;
            reader_para.ModeIndex = 1000;
            reader_para.HopTableIndex = 0;
            reader_para.PeriodicTriggerValue = 0;
            reader_para.TagPopulation = 32;
            reader_para.TagTransitTime = 0;
            reader_para.ReaderSensitivity = 1;
            // each value in the array map to Antenna 1, Antenna 2, Antenna 3, Antenna 4, respectively.
            reader_para.AntennaID = new bool[] { true, true, false, false };
        }

        #endregion


        public static void InitializeConfiguration()
        {
            Console.WriteLine("Initializing...\n");

            // Create an instance of LLRP reader client.
            reader = new LLRPClient();
            // Create an DataTable to save the current state of Tag
            data = new DataTable();
            // Set Reader Config in Default Way.
            reader_para = new Reader.ReaderParameters();

            //Impinj Best Practice! Always Install the Impinj extensions
            Impinj_Installer.Install();
        }



        public static void Main()
        {
            string IPAddress = "169.254.1.1";
            string fpath = @"C:\Users\MY\Desktop\";
            if (!Directory.Exists(fpath))
                Directory.CreateDirectory(fpath);

            DateTime dt = DateTime.Now;
            string strCurrentTime = dt.ToString("yyyyMMdd_HHmmss");
            string fname = strCurrentTime + ".csv";

            // set data collection time (s)
            UInt16 sustainTime = 5;

            Console.WriteLine("Impinj C# LTK.NET RFID Application DocSample4 reader ----- " + IPAddress + "\n");

            InitializeConfiguration();
            InitializeDataRow();
            setReader_PARM();
            ConnectTo(IPAddress);

            //subscribe to client event notification and ro access report
            reader_AddSubscription();
            ResetReaderToFactoryDefault();
            GetReaderCapabilities();
            Enable_Impinj_Extensions();
            //SetReaderConfig_WithXML();
            SetReaderConfig();

            //Add_RoSpec_WithXML();
            Add_RoSpec();
            Enable_RoSpec();
            //Start_RoSpec();


            // wait around to collect some data (UNIT: ms)
            Thread.Sleep(sustainTime * 1000);

            Stop_RoSpec();
            ResetReaderToFactoryDefault();

            Console.WriteLine("  Calculated " + directionCount + " Velocity Estimates.");
            Console.WriteLine("  Received " + reportCount + " Tag Reports.");
            Console.WriteLine("  Received " + eventCount + " Events.");

            // clean up the reader
            reader_CleanSubscription();

            // save data into csv file.
            Console.WriteLine("Saving data...");
            Console.WriteLine(fpath + fname);
            CsvStreamWriter CsvWriter = new CsvStreamWriter(fpath + fname);
            SaveData(CsvWriter);

            Console.WriteLine("DONE!\n");
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey(true);
            //Thread.Sleep(5000);
        }
    }
}
