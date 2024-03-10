using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalClass.Dynamic_data
{
    internal class UsageOS
    {
        public string CurrentUser { get; set; }
        public string Status { get; set; }
        private DateTime DateTime { get; }
        public UsageOS(string CurrentUser, string Status) { 
            DateTime = DateTime.Now;
            this.CurrentUser = CurrentUser;
            this.Status = Status;
        }
    }
}
