﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S6.JsonClass
{
    internal class SystemInfo
    {
        public CPUInfo CPU { get; set; }
        public OSInfo OS { get; set; }
        public BIOSInfo BIOS { get; set; }
        public UserInfo USER { get; set; }
        public NetworkInfo NETWORK { get; set; }
    }
}
