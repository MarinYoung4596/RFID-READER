namespace TagReader.RFIDReader
{
    public class TagStatus
    {
        public string Epc { get; set; }             //
        public ulong  LastSeenTime { get; set; }    //
        public ulong  FirstSeenTime { get; set; }   //
        public ulong  TimeStamp { get; set; }       
        public ushort Antenna { get; set; }         //
        public double TxPower { get; set; }
        public ushort ChannelIndex { get; set; }    //
        public double Frequency { get; set; }

        public float  Rssi { get; set; }            //
        public ushort RawPhase { get; set; }        //
        public double PhaseRadian { get; set; }
        public double PhaseDegree { get; set; }
        public int    DopplerShift { get; set; }    //
        public double Velocity { get; set; }
        public int    TagSeenCount { get; set; }    //
    }
}
