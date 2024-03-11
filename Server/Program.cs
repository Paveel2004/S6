using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using Data_collection;
using System.Net.Mail;
using System.Net.NetworkInformation;
using GlobalClass.Static_data;
using GlobalClass;
using System;
using GlobalClass.Dynamic_data;

namespace Server
{
    internal class Program
    {
        static async Task Main()
        {
            Task.Run(() => StartServer(9930, HendleClientNetwork));
            Task.Run(() => StartServer(9860, HendleClientCPU));
            Task.Run(() => StartServer(9790, HendleClientRAM));
            Task.Run(() => StartServer(9720, HendleClientUsageRAM));
            Task.Run(() => StartServer(9650, HendleClientUsageOS));
            Task.Run(() => StartServer(9580, HendleClientUsageCPU));
            Console.ReadLine();
        }
        static void HendleClientUsageCPU(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

                    var cpu = JsonConvert.DeserializeObject<UsageCPU>(message);

                    DataBaseHelper.Query($"EXECUTE ДобавитьИспользованиеПроцессора @BIOS = '{cpu.SerialNumberBIOS}', @Температура = '{cpu.Temperature}', @Загруженность = '{cpu.Workload}', @ДатаВремя = '{cpu.DateTime}'");

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static void HendleClientUsageOS(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

                    var os = JsonConvert.DeserializeObject<UsageOS>(message);

                   DataBaseHelper.Query($"EXECUTE ДобавитьИспользованиеОС @BIOS = '{os.SerialNumberBIOS}', \t@Статус = '{os.Status}', @ТекущийПользователь = '{os.CurrentUser}', @ДатаВремя = '{os.DateTime}'");

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static void HendleClientUsageRAM(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

                    var ram = JsonConvert.DeserializeObject<UsageRAM>(message);

                        DataBaseHelper.Query($"EXECUTE ДобавитьИспользование @ТипХарактеристики = 'ОЗУ', @Характеристика = 'Загруженность', @СерийныйНомерBIOS = '{ram.SerialNumberBIOS}', @Значение = '{ram.Workload}', @ДатаВремя = '{ram.DateTime}'");

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static void HendleClientRAM(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

                    DeviceData<RAMData> ramData = JsonHelper.DeserializeDeviceData<RAMData>(message);
                    foreach (RAMData i in ramData.Data)
                    {
                        DataBaseHelper.Query($"EXECUTE ДобавитьОЗУ @BIOS = '{ramData.SerialNumberBIOS}', @Объём = '{i.Volume}', @Частота = '{i.Speed}', @Тип = '{i.Type}';");
                    }

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static void HendleClientCPU(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

                    DeviceData<CPUData> cpuData = JsonHelper.DeserializeDeviceData<CPUData>(message);
                    foreach (CPUData i in cpuData.Data) 
                    {
                        DataBaseHelper.Query($"EXECUTE ДобавитьПроцессор @BIOS = '{cpuData.SerialNumberBIOS}', @Модель = '{i.Model}', @Архитектура = '{i.Architecture}', @КоличествоЯдер = '{i.NumberOfCores}' ");

                    }

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static void HendleClientNetwork(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[5000];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
                    DeviceData<NetworkInterfaceData> networkData = JsonHelper.DeserializeDeviceData<NetworkInterfaceData>(message);
                    foreach (NetworkInterfaceData ni in networkData.Data)
                    {
                        DataBaseHelper.Query($"EXECUTE ДобавитьСетевойИнтерфейс @BIOS = '{networkData.SerialNumberBIOS}', @ФизическийMAC = '{ni.MACAdress}', @Имя = '{ni.Name}', @Тип = '{ni.Type}'");
                    }
                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                   
                    stream.Write(response, 0, response.Length);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        static async void StartServer(int port, Action<TcpClient> handleClient)
        {
            TcpListener server = null;
            try
            {
                // Указываем IP-адрес и порт, на котором будет слушать сервер
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // Создаем TcpListener
                server = new TcpListener(localAddr, port);

                // Начинаем прослушивание клиентов
                server.Start();

                Console.WriteLine($"Сервер запущен на порту {port}. Ожидание подключений...");

                while (true)
                {
                    // Ожидаем входящее подключение
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Подключен клиент!");
                    // Обрабатываем подключенного клиента в отдельном потоке
                    Task.Run(() => handleClient(client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Завершаем прослушивание клиентов при выходе из цикла
                server?.Stop();
            }
        }


    }
}