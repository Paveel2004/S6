using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class CPUInfo
    {
        public string Architecture { get; set; }
        public string Name { get; set; }
        public int CoreCount { get; set; }
        public float Temperature { get; set; }
        public string SerialNumber { get; set; }
        public float CpuUsage { get; set; }
    }
}
