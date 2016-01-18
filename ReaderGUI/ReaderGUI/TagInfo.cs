namespace ReaderGUI
{
    public class TagInfo
    {
        public string Epc { get; set; }
        public string TimeStamp { get; set; }
        public ushort Antenna { get; set; }
        public float TxPower { get; set; }
        public ushort ChannelIndex { get; set; }
        public double Frequency { get; set; }
        public int Rssi { get; set; }
        public double PhaseInRadian { get; set; }
        public double PhaseInDegree { get; set; }
        public int DopplerShift { get; set; }
        public double Velocity { get; set; }


  
    }
}
