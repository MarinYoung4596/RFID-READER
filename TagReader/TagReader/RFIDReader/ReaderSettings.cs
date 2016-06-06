
namespace TagReader.RFIDReader
{
    public class ReaderSettings
    {
        public string Ip { get; set; }
        public double TransmitPower { get; set; }
        public ushort ModeIndex { get; set; }
        public ushort TagPopulation { get; set; }
        public ushort TagTransitTime { get; set; }
        public bool[] AntennaId { get; set; }
        public ushort Tari { get; set; }
        public ushort ReaderSensitivity { get; set; }
        public ushort ChannelIndex { get; set; }
        public ushort HopTableIndex { get; set; }
        public ushort PeriodicTriggerValue { get; set; }


        public ReaderSettings()
        {
            Ip = "192.168.1.222";
            TransmitPower = 32.5;
            ModeIndex = 2;
            TagPopulation = 1;
            TagTransitTime = 0;
            AntennaId = new[] { true, false, false, false };
            Tari = 10;
            ReaderSensitivity = 1;
            ChannelIndex = 1;
            HopTableIndex = 1;
            PeriodicTriggerValue = 0;
        } // end Reader constructor
    } // end Reader
} // end namespace
