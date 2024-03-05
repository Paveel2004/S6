using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class OSInfo
    {
        public string OS { get; set; }
        public int Architecture { get; set; }
        public string SerialNumber { get; set; }
        public int NumberOfUsers { get; set; }
        public string SystemState { get; set; }
        public string VersionOS { get; set; }
    }
}
