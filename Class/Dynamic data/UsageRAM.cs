using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    public class UsageRAM
    {
        public string SerialNumberBIOS { get; set; }
        public double Workload { get; set; }
        public DateTime DateTime { get; }
        public UsageRAM(double Workload, string SerialNumberBIOS)
        {
            this.Workload = Workload;
            DateTime = DateTime.Now;
            this.SerialNumberBIOS = SerialNumberBIOS;
          
        }
    }
}
