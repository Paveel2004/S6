using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S6.DataBase
{
    internal struct UsersInfo
    {

        string userName;
        string status;
        string operatingSystem;
        string serialNumberOfTheOS;


        public UsersInfo(string userName="", string status = "", string operatingSystem = "", string serialNumberOfTheOS = "")
        {
            this.userName = userName;
            this.status = status;  
            this.operatingSystem = operatingSystem;
            this.serialNumberOfTheOS = serialNumberOfTheOS;

        }
        public override string ToString()
        {
            return $"Имя пользователя: {this.userName}\n• Статус {this.status}\n• Операционная система {this.operatingSystem}\n• Серийный номер системы {this.serialNumberOfTheOS}\n";
        }
    }
}
