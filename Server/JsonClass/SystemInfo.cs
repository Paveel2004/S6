using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class SystemInfo
    {
        public CPUInfo CPU { get; set; }
        public OSInfo OS { get; set; }
        public BIOSInfo BIOS { get; set; }
        public UserInfo USER { get; set; }
        public NetworkInfo NETWORK { get; set; }
        public RamInfo RAM { get; set; }
        public List<DiskInfo> DISK { get; set; }

    }
}
