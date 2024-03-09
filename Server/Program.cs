using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using Data_collection;
using System.Net.Mail;
using GlobalClass;
using System.Net.NetworkInformation;

namespace Server
{
    internal class Program
    {
        static async Task Main()
        {
            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                int port = 1111;

                server = new TcpListener(localAddr, port);

                server.Start();

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    // Ожидаем входящее подключение
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Подключен клиент!");

                    // Обрабатываем подключенного клиента в отдельном потоке
                    Task.Run(() => HandleClient(client));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server?.Stop();
            }
        }   

        static void HandleClient(TcpClient tcpClient)
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
                        Console.WriteLine($"{ ni.Name}\n { ni.Type}\n{ ni.MACAdress}");

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