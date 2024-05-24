using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass
{
    public class DeviceCharacteristics
    {
        public string ComputerName { get; set; }
        public string ProcessorModel { get; set; }
        public string ProcessorArchitecture { get; set; }
        public string ProcessorCores { get; set; }
        public string RAMSize { get; set; }
        public string RAMFrequency { get; set; }
        public string RAMType { get; set; }
        public string GPUModel { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }
        public string OSArchitecture { get; set; }
        public string TotalSpaceDisk { get; set; }
        public string SerialNumberBIOS { get; set; }
    }
}
