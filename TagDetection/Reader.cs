using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SimpleLLRPSample;

namespace SimpleLLRPSample
{
    public class Reader
    {
        public class ReaderParameters
        {
            private ushort attenuation;

            public ushort Attenuation
            {
                get { return attenuation; }
                set { attenuation = value; }
            }


            private ushort modeIndex;

            public ushort ModeIndex
            {
                get { return modeIndex; }
                set { modeIndex = value; }
            }


            private ushort tagPopulation;

            public ushort TagPopulation
            {
                get { return tagPopulation; }
                set { tagPopulation = value; }
            }


            private ushort tagTransitTime;

            public ushort TagTransitTime
            {
                get { return tagTransitTime; }
                set { tagTransitTime = value; }
            }


            private bool[] antennaID;

            public bool[] AntennaID
            {
                get { return antennaID; }
                set { antennaID = value; }
            }


            private ushort readerSensitivity;

            public ushort ReaderSensitivity
            {
                get { return readerSensitivity; }
                set { readerSensitivity = value; }
            }


            private ushort channelIndex;

            public ushort ChannelIndex
            {
                get { return channelIndex; }
                set { channelIndex = value; }
            }


            private ushort hopTableIndex;

            public ushort HopTableIndex
            {
                get { return hopTableIndex; }
                set { hopTableIndex = value; }
            }


            private ushort periodicTriggerValue;

            public ushort PeriodicTriggerValue
            {
                get { return periodicTriggerValue; }
                set { periodicTriggerValue = value; }
            }


            public ReaderParameters()
            {
                attenuation = 0;
                modeIndex = 2;
                tagPopulation = 1;
                tagTransitTime = 0;
                antennaID = new bool[] { true, false, false, false }; // Support Multiple Antennas. R420, R220
                readerSensitivity = 1;
                channelIndex = 1;
                hopTableIndex = 1;
                periodicTriggerValue = 0;
            }


            public void setReaderParameters(ReaderParameters config)
            {
                attenuation = config.attenuation;
                modeIndex = config.modeIndex;
                tagPopulation = config.tagPopulation;
                tagTransitTime = config.tagTransitTime;
                antennaID = config.antennaID;
                readerSensitivity = config.readerSensitivity;
                channelIndex = config.channelIndex;
                hopTableIndex = config.hopTableIndex;
                periodicTriggerValue = config.periodicTriggerValue;
            }
        } // end ReaderParameters
    } // end Reader
} // end namespace
