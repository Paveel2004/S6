using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass
{
    public class DeviceData<T>
    {
        public string SerialNumberBIOS { get; set; }
        public List<T> Data { get; set; }
    }
}
