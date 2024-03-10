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

namespace Server
{
    internal class Program
    {
        static async Task Main()
        {
            Task.Run(() => StartServer(9993, HendleClientNetwork));
            Task.Run(() => StartServer(9986, HendleClientCPU));
            Console.ReadLine();
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
                        DataBaseHelper.Query($"EXECUTE ДобавитьПроцессор @BIOS = '{cpuData.SerialNumberBIOS}', @Модель = '{i.Model}', @Архитектура = '{i.Architecture}';");
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