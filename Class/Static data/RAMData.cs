using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Static_data
{
    public class RAMData
    {
        public string Type { get; set; }
        public ulong Volume { get; set; }
        public int Speed { get; set; }
        public RAMData(string Type, ulong Volume, int Speed)
        { 
            this.Volume = Volume;
            this.Speed = Speed;
            this.Type = Type;
        }
    }
}
