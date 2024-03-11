using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    public class UsageEthernet
    {
        public string SerialNumberBIOS { get; set; }
        public double Speed { get; set; }
        public DateTime DateTime { get; }
        public UsageEthernet(double Speed, string SerialNumberBIOS) { 
            this.SerialNumberBIOS = SerialNumberBIOS;
            this.Speed = Speed;
            DateTime = DateTime.Now;

        }
    }
}
