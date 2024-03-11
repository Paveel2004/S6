using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    public class UsageDisk
    {
        public string SerialNumberBIOS { get; set; }
        public double FreeSpace { get; set; }
        public DateTime DateTime {get;set;}
        public UsageDisk(double FreeSpace, string SerialNumberBIOS) {
            this.SerialNumberBIOS = SerialNumberBIOS;
            this.FreeSpace = FreeSpace;
            DateTime = DateTime.Now;

        }

    }
}
