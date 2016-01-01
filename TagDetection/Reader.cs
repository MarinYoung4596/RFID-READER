using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SimpleLLRPSample;

// ReSharper disable once CheckNamespace
namespace SimpleLLRPSample
{
    public class Reader
    {
        public class ReaderParameters
        {
            public double TransmitPower { get; set; }
            public ushort ModeIndex { get; set; }
            public ushort TagPopulation { get; set; }
            public ushort TagTransitTime { get; set; }
            public bool[] AntennaId { get; set; }
            public ushort ReaderSensitivity { get; set; }
            public ushort ChannelIndex { get; set; }
            public ushort HopTableIndex { get; set; }
            public ushort PeriodicTriggerValue { get; set; }


            public ReaderParameters()
            {
                TransmitPower = 32.5; // dbm
                ModeIndex = 2;
                TagPopulation = 1;
                TagTransitTime = 0;
                AntennaId = new bool[] { true, false, false, false }; // Support Multiple Antennas. R420, R220
                ReaderSensitivity = 1;
                ChannelIndex = 1;
                HopTableIndex = 1;
                PeriodicTriggerValue = 0;
            } // end ReaderParameters constructor

        } // end ReaderParameters
    } // end Reader
} // end namespace
