using System;
using System.Collections.Generic;
using System.Text;

namespace Dataobjects
{
    public class ChartData
    {
        public List<string> SentDTTM { get; set; }
        public List<double> Latency { get; set; }
        public List<double> AckTimeDifference { get; set; }
    }
}
