using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    internal class UsageCPU
    {
        public double Temperature { get; set; }
        public double Workload { get; set; }
        private DateTime DateTime { get; }
        public UsageCPU(int Temperature, int Workload)
        {
            this.Temperature = Temperature;
            this.Workload = Workload;
            DateTime = DateTime.Now;
        }
    }
}
