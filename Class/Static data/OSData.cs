using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data
{
    public class OSData
    {
        public string SerialNumberBIOS { get; set; }
        public string OS { get; set; }
        public OSData(string OS, string SerialNumberBIOS) 
        {
            this.SerialNumberBIOS = SerialNumberBIOS;
            this.OS = OS;
        }
    }
}
