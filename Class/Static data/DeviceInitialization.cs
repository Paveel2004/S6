using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data
{
    public class DeviceInitialization
    {
        public string SerialNumberBIOS {  get; set; }
        public string ComputerName {get; set; }

        public DeviceInitialization(string SerialNumberBIOS, string ComputerName) {
            this.SerialNumberBIOS = SerialNumberBIOS;
            this.ComputerName = ComputerName;
        }
    }
}
