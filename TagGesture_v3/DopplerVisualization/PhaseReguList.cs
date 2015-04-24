using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DopplerVisualization
{
    class PhaseReguList
    {
        private List<PlotTagInfo> list;

        public PhaseReguList()
        {
            list = new List<PlotTagInfo>();
        }

        public void Insert(PlotTagInfo tag)
        {
            list.Add(tag);
        }

        public void UpdateData(PlotTagInfo tag)
        {
            int count = list.Count();
            for (int i = 0; i < count - 1; i++)
            {
                list[i] = list[i + 1];
            }
            list[count - 1] = tag;
        }

        public float CalculatePointValue()
        {
            int tagsCount = list.Count();

            PlotTagInfo tag1 = list[tagsCount-1];
            PlotTagInfo tag2 = list[tagsCount-2];
            double subMillisecond = tag1.FirstSeenTime.Subtract(tag2.FirstSeenTime).TotalMilliseconds;
           // Console.WriteLine(tag1.FirstSeenTime+"--"+tag2.FirstSeenTime+"---"+subMillisecond);
            
            double value = (tag1.AnglePhase - tag2.AnglePhase)/subMillisecond * 10e3;
           // Console.WriteLine("Value-------" + value);
            return (float)value;
        }
    }
}
