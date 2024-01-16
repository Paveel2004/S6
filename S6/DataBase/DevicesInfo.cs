using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace S6.DataBase
{
    internal struct DevicesInfo
    {
        string modelCPU;
        double temperatureCPU;
        double usageCPU;
        int numberOfCoresCPU;
        int architectureCPU;
        string IP;
        string MAC;
        double networkSpeed;
        string operatingSystem;
        int bitDepthOS;
        string systemStatus;
        string biosVersion;
        string usageRAM;
        string amountOfRAM;
        string typeOfRAM;
        string userName;

        public DevicesInfo(
          string modelCPU,
          double temperatureCPU,
          double usageCPU,
          int numberOfCoresCPU,
          int architectureCPU,
          string IP,
          string MAC,
          double networkSpeed,
          string operatingSystem,
          int bitDepthOS,
          string systemStatus,
          string biosVersion,
          string usageRAM,
          string amountOfRAM,
          string typeOfRAM,
          string userName)
        {
            this.modelCPU = modelCPU;
            this.temperatureCPU = temperatureCPU;
            this.usageCPU = usageCPU;
            this.numberOfCoresCPU = numberOfCoresCPU;
            this.architectureCPU = architectureCPU;
            this.IP = IP;
            this.MAC = MAC;
            this.networkSpeed = networkSpeed;
            this.operatingSystem = operatingSystem;
            this.bitDepthOS = bitDepthOS;
            this.systemStatus = systemStatus;
            this.biosVersion = biosVersion;
            this.usageRAM = usageRAM;
            this.amountOfRAM = amountOfRAM;
            this.typeOfRAM = typeOfRAM;
            this.userName = userName;
        }
        public override string ToString()
        {
            return $"•Модель процессора: {this.modelCPU}\n" +
                   $"•Температура процессора: {this.temperatureCPU}\n" +
                   $"•Загруженность процессора: {this.usageCPU}\n" +
                   $"•Количество ядер в процессоре: {this.numberOfCoresCPU}\n" +
                   $"•Архитектура процессора: {this.architectureCPU}\n" +
                   $"•IP Адрес: {this.IP}\n" +
                   $"•MAC Адрес: {this.MAC}\n" +
                   $"•Скорость передачи данных в сети: {this.networkSpeed}\n" +
                   $"•Операционная система: {this.operatingSystem}\n" +
                   $"•Разрядность системы: {this.bitDepthOS}\n" +
                   $"•Состояние системы: {this.systemStatus}\n" +
                   $"•Версия BIOS: {this.biosVersion}\n" +
                   $"•Загруженность оперативной памяти: {this.usageRAM}\n" +
                   $"•Объём оперативной памяти: {this.amountOfRAM}\n" +
                   $"•Тип оперативной памяти: {this.typeOfRAM}\n" +
                   $"•Имя пользователя: {this.userName}\n";
        }
    }
}
