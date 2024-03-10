using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace GlobalClass.Static_data
{
    public class CPUData
    {
        public string Model { get; set;}
        public string Architecture { get; set;}
        public CPUData(string Model, string Architecture)
        {
            this.Model = Model;
            this.Architecture = Architecture;
        }
    }
}
