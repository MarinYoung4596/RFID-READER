using System.Collections.Generic;

namespace TagReader.RFIDReader
{
    public class TagInfos
    {
        #region Class @Key Defination
        public class Key
        {
            public string Epc { get; set; }
            public ushort Antenna { get; set; }
            public ushort ChannalIndex { get; set; }

            public Key(string epc, ushort antenna, ushort channal)
            {
                Epc = epc;
                Antenna = antenna;
                ChannalIndex = channal;
            }

            public override int GetHashCode()
            {
                const int prime = 37;
                var result = 17;
                result = result*prime + Epc.GetHashCode();
                result = result*prime + Antenna.GetHashCode();
                result = result*prime + ChannalIndex.GetHashCode();
                return result;
            }

            public override bool Equals(object obj)
            {
                if (null == obj)
                    return false;
                if (obj.GetType() != this.GetType())
                    return false;

                return obj.GetHashCode() == this.GetHashCode();
            }
        }
        #endregion


        #region Class @Value Defination
        public class Value
        {
            public List<float> RssiList;
            public List<double> PhaseList; // Radian
            public List<double> DopplerShifList;
            public int ReadNums;

            public Value()
            {
                RssiList = new List<float>();
                PhaseList = new List<double>();
                DopplerShifList = new List<double>();
                ReadNums = 0;
            }
        }//
        #endregion

    }//
}
