////////////////////////////////////////////////////////////////////////////////
//
//    Tag Gesture
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Impinj.OctaneSdk;
using DopplerVisualization;



namespace TagGesture
{
    class Program
    {
        // Create an instance of the ImpinjReader class.
        static ImpinjReader reader = new ImpinjReader();
        //新建一个dataTable
        static DataTable DataDS = new DataTable();

        static string filePath = @"C:\Users\MY\Desktop\20160106\pos1\";
        static string fileName = "antennaPos2_freq924375power325_Tag1Pos1plus32.csv";

        static CsvStreamWriter CsvWriter = new CsvStreamWriter(filePath + fileName);
        static double txPowerValue = 0;
        static ushort antennaPort = 1;
        static FormRealTime DSForm;
        static Hashtable TagsEPC = new Hashtable();
        static List<String> TagNames = new List<String>();

        static void InitTagsEPC()
        {
            TagsEPC.Add("CCCC 0004", 0);
            TagsEPC.Add("FFFF FFFF FFFF FFFF FFFF 2F1A", 1);
            TagsEPC.Add("FFFF FFFF FFFF FFFF FFFF 0006", 2);
            TagsEPC.Add("2FFF 32ED", 3);


            TagNames.Add("CCCC 0004");
            TagNames.Add("FFFF FFFF FFFF FFFF FFFF 2F1A");
            TagNames.Add("FFFF FFFF FFFF FFFF FFFF 0006");
            TagNames.Add("2FFF 32ED");
        }


        static void Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("Before------------!!!");
                initDataRow();
                InitTagsEPC();

                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                System.Console.WriteLine("Before Connect!!!");
                reader.Connect(SolutionConstants.ReaderHostname);
                System.Console.WriteLine("Connect!!!");
                // 获取当前默认设置
                // Get the reader features to determine if the 
                // reader supports a fixed-frequency table.
                FeatureSet features = reader.QueryFeatureSet();

                // Get the default settings
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();
                reportSetting(settings);

                // Use antenna #2
                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(2).IsEnabled = false;
                //settings.Antennas.EnableAll();
                settings.Antennas.GetAntenna(2).TxPowerInDbm = 25;

                // use antenna #1
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                ////settings.Antennas.EnableAll();
                settings.Antennas.GetAntenna(1).TxPowerInDbm = 32.5;

                //settings.Antennas.GetAntenna(1).RxSensitivityInDbm = -55;

                // settings.Antennas.GetAntenna(2).RxSensitivityInDbm = -55;


                //settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                //settings.SearchMode = SearchMode.DualTarget;
                //settings.Session = 2;
                // ReaderMode must be set to DenseReaderM4 or DenseReaderM8.
                settings.ReaderMode = ReaderMode.DenseReaderM4;

                // 每读取一个tag就report
                // Send a tag report for every tag read.
                settings.Report.Mode = ReportMode.Individual;

                filterTags(settings);

                if (0 != fixFrequency(features, settings))
                {
                    applicactionClose();
                }

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported;
                // Read with fix tx power
                singleRead();

                // multiple tx power read
                // txPowerRead(features, settings);

            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Console.WriteLine("Octane SDK exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
            }
        }

        static void singleRead()
        {

            reader.Start();
            DSForm = new FormRealTime(TagNames);
            Application.Run(DSForm);

            // Start reading.


            //Thread.Sleep(20000); //收集31s数据

            // Wait for the user to press enter.
            //Console.WriteLine("Press enter to exit.");
            //Console.ReadLine();


            // Stop reading.
            reader.Stop();

            applicactionClose();
        }

        static void txPowerRead(FeatureSet features, Settings settings)
        {
            foreach (TxPowerTableEntry tx in features.TxPowers)
            {
                // Set the transmit power (in dBm).
                Console.WriteLine("Setting Tx Power to {0} dBm", tx.Dbm);
                settings.Antennas.GetAntenna(antennaPort).TxPowerInDbm = tx.Dbm;
                txPowerValue = tx.Dbm;

                // Apply the new transmit power settings.
                reader.ApplySettings(settings);

                // Start the reader.
                reader.Start();

                // Wait
                Thread.Sleep(1000);

                // Stop the reader.
                reader.Stop();
            }
            applicactionClose();
        }

        static void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                //Console.WriteLine("EPC : {0} Doppler Frequency (Hz) : {1} Current Frequecy: {2}  PeakRSSI ：{3}  PhaseAngle : {4} PhaseDegree :{5}",
                //                    tag.Epc, tag.RfDopplerFrequency.ToString("0.00"), tag.ChannelInMhz, tag.PeakRssiInDbm, tag.PhaseAngleInRadians, ((tag.PhaseAngleInRadians) / Math.PI) * 180);

                //if (stock.Contains(tag.Epc.ToString()))
                //{
                double phaseAngle;
                //if (tag.PhaseAngleInRadians < Math.PI)
                //{
                //    phaseAngle = tag.PhaseAngleInRadians + Math.PI;
                //}
                //else
                //{
                //    phaseAngle = tag.PhaseAngleInRadians;
                //}
                phaseAngle = tag.PhaseAngleInRadians;
                Console.WriteLine("EPC : {0}  RSS ：{1} ",
                                        tag.Epc, tag.PeakRssiInDbm);


                DataRow row = DataDS.NewRow();
                //给列赋值
                DateTime dt = DateTime.Now;

                row["EPC"] = tag.Epc.ToString();
                row["Doppler Shift"] = tag.RfDopplerFrequency.ToString("0.00");
                row["Time"] = tag.FirstSeenTime.ToString();
                row["Antenna"] = tag.AntennaPortNumber;
                row["Tx Power"] = txPowerValue;
                row["Current Frequency"] = tag.ChannelInMhz.ToString();
                row["PeakRSSI"] = tag.PeakRssiInDbm.ToString();
                row["Phase Angle"] = phaseAngle;//tag.PhaseAngleInRadians.ToString();
                row["Phase"] = ((tag.PhaseAngleInRadians) / Math.PI) * 180;

                //把有值的列添加到表
                DataDS.Rows.Add(row);
                //DSForm.updateTaginfo(row);
                DSForm.updateTaginfo((int)TagsEPC[tag.Epc.ToString()], (float)(tag.RfDopplerFrequency));
                //DSForm.updateTaginfo((int)TagsEPC[tag.Epc.ToString()], (float)phaseAngle, tag.FirstSeenTime.LocalDateTime);
            }
        }


        static public void initDataRow()
        {
            // 创建表中的列
            DataDS.Columns.Add("EPC");
            DataDS.Columns.Add("Doppler Shift");
            DataDS.Columns.Add("Time");
            DataDS.Columns.Add("Antenna");
            DataDS.Columns.Add("Tx Power");
            DataDS.Columns.Add("Current Frequency");
            DataDS.Columns.Add("PeakRSSI");
            DataDS.Columns.Add("Phase Angle");
            DataDS.Columns.Add("Phase");

            // 初始化列名
            DataRow row = DataDS.NewRow();
            row["EPC"] = "EPC";
            row["Doppler Shift"] = "DopplerShift(Hz)";
            row["Time"] = "Time";
            row["Antenna"] = "Antenna";
            row["Tx Power"] = "TxPower";
            row["Current Frequency"] = "Frequency(MHz)";
            row["PeakRSSI"] = "RSS(dbm)";
            row["Phase Angle"] = "PhaseAngle(Radian)";
            row["Phase"] = "PhaseAngle(Degree)";
            DataDS.Rows.Add(row);
        }

        static public void reportSetting(Settings settings)
        {
            // Tell the reader to include the
            // RF doppler frequency in all tag reports. 
            settings.Report.IncludeDopplerFrequency = true;

            // 允许输出
            settings.Report.IncludeChannel = true;
            settings.Report.IncludePeakRssi = true;
            settings.Report.IncludePhaseAngle = true;
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeAntennaPortNumber = true;
        }

        static public void applicactionClose()
        {

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
            // 写CSV文件
            CsvWriter.AddData(DataDS, 1);
            CsvWriter.Save();


            // Disconnect from the reader.
            reader.Disconnect();
            Console.WriteLine(DataDS.Rows.Count);
            Console.ReadLine();


        }

        static public void filterTags(Settings settings)
        {
            // Setup a tag filter.
            // Only the tags that match this filter will respond.
            // First, setup tag filter #1.
            // We want to apply the filter to the EPC memory bank.
            settings.Filters.TagFilter1.MemoryBank = MemoryBank.Epc;
            // Start matching at the third word (bit 32), since the 
            // first two words of the EPC memory bank are the
            // CRC and control bits. BitPointers.Epc is a helper
            // enumeration you can use, so you don't have to remember this.
            settings.Filters.TagFilter1.BitPointer = BitPointers.Epc;
            // Only match tags with EPCs that start with "3008"
            //settings.Filters.TagFilter1.TagMask = "FFFFFFFFFFFFFFFFE";
            // This filter is 16 bits long (one word).
            //settings.Filters.TagFilter1.BitCount = 68;

            settings.Filters.TagFilter1.TagMask = "FFFFFFFFFFFFFFFFFFFF"; //AAAAAAAAAAAAAAAAAAAAE
            // This filter is 16 bits long (one word).
            settings.Filters.TagFilter1.BitCount = 80;

            settings.Filters.TagFilter2.MemoryBank = MemoryBank.Epc;
            // Start matching at the third word (bit 32), since the 
            // first two words of the EPC memory bank are the
            // CRC and control bits. BitPointers.Epc is a helper
            // enumeration you can use, so you don't have to remember this.
            settings.Filters.TagFilter2.BitPointer = BitPointers.Epc;

            // Only match tags with EPCs that start with "3008"
            settings.Filters.TagFilter2.TagMask = "E2009027";
            // This filter is 16 bits long (one word).
            settings.Filters.TagFilter2.BitCount = 32;

            // Set the filter mode.
            // Both filters must match.
            settings.Filters.Mode = TagFilterMode.None;

        }

        static public int fixFrequency(FeatureSet features, Settings settings)
        {
            // Specify the transmit frequencies to use.
            // Make sure your reader supports this and
            // that the frequencies are valid for your region.
            List<double> freqList = new List<double>();

            if (!features.IsHoppingRegion)
            {
                //freqList.Add(921.625);
                //freqList.Add(921.875);
                //freqList.Add(922.125);
                //freqList.Add(922.375);
                //freqList.Add(922.625);
                //freqList.Add(922.875);
                //freqList.Add(923.125);
                //freqList.Add(923.375);
                //freqList.Add(923.625);
                //freqList.Add(923.875);
                //freqList.Add(924.125);
                freqList.Add(924.375);
                // 其他符合标准的频率值
                // 921.625;921.875;922.125;922.375;922.625;922.875;923.125;923.375;923.625;923.875;924.125;924.375;
                settings.TxFrequenciesInMhz = freqList;
                return 0;
            }
            else
            {
                Console.WriteLine("This reader does not allow the transmit frequencies to be specified.");
                return -1;
            }
        }



    }
}
