using S6.JsonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S6.DataBase
{
    internal class DiskInfoDisplay : DiskInfo
    {
        public string СurrentUser { get; set; }
        public string OperatingSystem { get; set; } 

        public DiskInfoDisplay(string deviceID, ulong size, ulong freeSpace, string CurrendUser, string OperatingSystem)
        { 
            this.DeviceID = deviceID;
            this.Size = size;
            this.FreeSpace = freeSpace;
            this.СurrentUser = CurrendUser;
            this.OperatingSystem = OperatingSystem;
        }
        public override string ToString()
        {
            return $"•Имя диска: {this.DeviceID}\n" +
                $"•Объём диска: {this.Size}\n" +
                $"•Свободное место: {this.FreeSpace}\n" +
                $"•Текущий пользователь: {this.СurrentUser}\n" +
                $"•Операционная система: {this.OperatingSystem}";
        }
    }
}
