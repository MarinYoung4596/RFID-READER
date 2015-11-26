using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DopplerVisualization
{
    class PlotTagInfo
    {
        private int tagIdx;
        private float anglePhase;
        private DateTime firstSeenTime;

        public int TagIdx
        {
            get { return tagIdx; }
            set { tagIdx = value; }
        }
        public float AnglePhase
        {
            get { return anglePhase; }
            set { anglePhase = value; }
        }
        public DateTime FirstSeenTime
        {
            get { return firstSeenTime; }
            set { firstSeenTime = value; }
        }


        public PlotTagInfo(int _tagIdx, float _anglePhase, DateTime _firstSeenTime)
        {
            tagIdx = _tagIdx;
            anglePhase = _anglePhase;
            firstSeenTime = _firstSeenTime;
        }


    }
}
