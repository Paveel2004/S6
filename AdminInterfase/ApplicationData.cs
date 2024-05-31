using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminInterfase
{
    public class ApplicationData
    {
        public double Size { get; set; }
        public string Name { get; set; }
        public DateTime InstallDate { get; set; }

        public override string ToString()
        {
            return $"{Name} (Weight: {Size} MB, Installed: {InstallDate.ToShortDateString()})";
        }
    }
}
