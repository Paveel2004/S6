using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class DiskInfo
    {
        public string DeviceID { get; set; }
        public ulong Size { get; set; }
        public ulong FreeSpace { get; set; }
        public string VolumeName { get; set; }
    }
}
