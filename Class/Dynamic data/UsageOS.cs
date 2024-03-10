using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    public class UsageOS
    {
        public string CurrentUser { get; set; }
        public string Status { get; set; }
        public DateTime DateTime { get; }
        public string SerialNumberBIOS { get; set; }
        public UsageOS(string CurrentUser, string Status, string SerialNumberBIOS) { 
            DateTime = DateTime.Now;
            this.CurrentUser = CurrentUser;
            this.Status = Status;
            this.SerialNumberBIOS = SerialNumberBIOS;
        }
    }
}
