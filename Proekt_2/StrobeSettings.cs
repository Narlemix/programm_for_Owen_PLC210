using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proekt_2
{
    public class StrobeSettings
    {
        public int FrequencyHz { get; set; }
        public int DutyCyclePercent { get; set; }
        public ushort Pin { get; set; }
    }
}