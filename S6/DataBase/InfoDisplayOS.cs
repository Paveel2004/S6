using S6.JsonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace S6.DataBase
{
    internal class InfoDisplayOS : OSInfo
    {
        public string User {  get; set; }
        public InfoDisplayOS(string OS, int Architecture, string SerialNumber, int NumberOfUsers, string SystemState, string VersionOS, string User)
        {
            this.OS = OS;
            this.Architecture = Architecture;
            this.SerialNumber = SerialNumber;
            this.NumberOfUsers = NumberOfUsers;
            this.SystemState = SystemState;
            this.VersionOS = VersionOS;
            this.User = User;

        }
        public override string ToString()
        {
            return $"•Операционная система {this.OS}\n" +
                $"•Разрядность {this.Architecture}\n" +
                $"•Серийный номер {this.SerialNumber}\n" +
                $"•Количество пользователей {this.NumberOfUsers}\n" +
                $"•Состояние {this.SystemState}\n" +
                $"•Версия ОС {this.VersionOS}\n" +
                $"•Последний пользователь {this.User}";
        }


    }
}
