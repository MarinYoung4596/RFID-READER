
namespace TagReader
{
    public class ReaderSettings
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


        public ReaderSettings()
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
        } // end Reader constructor
    } // end Reader
} // end namespace
