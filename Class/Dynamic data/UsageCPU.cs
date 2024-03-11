using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    public class UsageCPU
    {
        public string SerialNumberBIOS { get; set; }
        public double Temperature { get; set; }
        public double Workload { get; set; }
        public DateTime DateTime { get; }
        public UsageCPU(double Temperature, double Workload, string serialNumberBIOS)
        {
            this.Temperature = Temperature;
            this.Workload = Workload;
            DateTime = DateTime.Now;
            SerialNumberBIOS = serialNumberBIOS;
        }
    }
}
