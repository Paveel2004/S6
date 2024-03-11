using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data
{
    public class DiskData
    {
        public string SerialNumberBIOS { get; set; }
        public double TotalSpace { get; set; }
        public DiskData(double TotalSpace, string SerialNumberBIOS) 
        { 
            this.TotalSpace = TotalSpace;
            this.SerialNumberBIOS = SerialNumberBIOS;
        }
    }
}
