using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SimpleLLRPSample;

namespace SimpleLLRPSample
{
    class TagInfo
    {
        private string EPC;

        public string _EPC
        {
            get { return EPC; }
            set { EPC = value; }
        }


        private string timeStamp;

        public string TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }


        private ushort antenna;

        public ushort Antenna
        {
            get { return antenna; }
            set { antenna = value; }
        }


        private float txPower;

        public float TxPower
        {
            get { return txPower; }
            set { txPower = value; }
        }


        private ushort channelIndex;

        public ushort ChannelIndex
        {
            get { return channelIndex; }
            set { channelIndex = value; }
        }


        private double frequency;

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }


        private int rssi;

        public int Rssi
        {
            get { return rssi; }
            set { rssi = value; }
        }


        private double phaseInRadian;

        public double PhaseInRadian
        {
            get { return phaseInRadian; }
            set { phaseInRadian = value; }
        }


        private double phaseInDegree;

        public double PhaseInDegree
        {
            get { return phaseInDegree; }
            set { phaseInDegree = value; }
        }


        private int dopplerShift;

        public int DopplerShift
        {
            get { return dopplerShift; }
            set { dopplerShift = value; }
        }


        private double velocity;

        public double Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }


        // constructor
        public TagInfo(
            string _EPC,
            string _TimeStamp,
            ushort _Antenna,
            ushort _ChannelIndex,
            int _RSSI,
            int _RawPhase,
            int _DopperShift,
            double _Velocity)
        {
            EPC = _EPC;
            timeStamp = _TimeStamp;
            antenna = _Antenna;
            channelIndex = _ChannelIndex;
            frequency = 920.63 + (_ChannelIndex - 1) * 0.25;
            rssi = _RSSI;
            phaseInDegree = _RawPhase * Program.Convert2Degree;
            phaseInRadian = _RawPhase * Program.Convert2Radian;
            dopplerShift = _DopperShift;
            velocity = _Velocity;
        }
    }
}
