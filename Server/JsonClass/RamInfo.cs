using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class RamInfo
    {

       public string  RamType { get; set; }
       public string RamUsage { get; set; }
       public string TotalPhisicalMemory {  get; set; }
       public int ConfiguredClockSpeed { get; set; }
       public int Type {  get; set; }


    }
}
