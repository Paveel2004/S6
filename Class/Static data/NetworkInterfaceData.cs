using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data
{
    public class NetworkInterfaceData
    {
        public string Name {  get; set; }
        public string Type { get; set; }
        public string MACAdress { get; set; }
        public NetworkInterfaceData(string Name, string  Type, string MACAdress){ 
            this.Type = Type;
            this.MACAdress = MACAdress;
            this.Name = Name;
        }
    }
}
