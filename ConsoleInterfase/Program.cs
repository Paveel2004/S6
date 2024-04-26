using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleInterfase
{
    internal class Program
    {
        public static void SendMessage(string serverAddress, int port, string message)
        {
            try
            {
                // Создаем TcpClient и подключаемся к серверу
                using TcpClient client = new TcpClient(serverAddress, port);
                Console.WriteLine($"Подключено к серверу на порту {port}...");

                // Получаем поток для обмена данными с сервером
                using NetworkStream stream = client.GetStream();

                // Отправляем сообщение серверу
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Отправлено сообщение: {message}");

                // Читаем ответ от сервера
                data = new byte[999999];
                int bytesRead = stream.Read(data, 0, data.Length);
                string response = Encoding.UTF8.GetString(data, 0, bytesRead);
                Console.WriteLine($"Ответ от сервера: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async void BroadcastMessage(string message, string address, int port)
        {
            var brodcastAddress = IPAddress.Parse(address);
            using var udpSender = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpSender.SendAsync(data, new IPEndPoint(brodcastAddress, port));
            await Task.Delay(1000);            
        }                
        
        static async void StartServer(int port, Action<TcpClient> handleClient, IPAddress localAddr)
        {
            TcpListener server = null;
            try
            {
                server = new TcpListener(localAddr, port);
                server.Start();
                //Console.WriteLine($"Сервер запущен на порту {port}. Ожидание подключений...");
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    //Console.WriteLine("Подключен клмент!");
                    Task.Run(() => handleClient(client));
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
        static void HendleClient(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[999999];
                int bytesRead;

                // Читаем данные из потока
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);

                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");

                    Console.WriteLine(message);
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
        static void Main(string[] args)
        {
            int port = 2222;
            IPAddress localAddr = IPAddress.Parse("192.168.1.52");
            Task.Run(() => StartServer(port, HendleClient, localAddr));
            string IP;
            while (true)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "Help":
                        Console.WriteLine("\r\n getTemperatureCPU\r\n            getUsageCPU\r\n            getUsageRAM\r\n            getUser\r\n            getUsers\r\n            getSpeedEthernet\r\n            getTraffic [Network Interfase Name]\r\n            getNetworkInterfases  \r\n            getProcess \r\n            closeProcess [Name]\r\n            getKeye");
                    break;
                    
                    case "getAll":
                        BroadcastMessage(command, "224.0.0.252",11000);
                        break;
                    case "getNetworkInterfases":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getSpeedEthernet":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getTemperatureCPU":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getUsageCPU":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getTraffic [Network Interfase Name]":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getApplications":
                        IP = Console.ReadLine();
                        SendMessage(IP, 1111, command);
                        break;
                    case "getProcesses":
                        break;
                }
               
            }
           
            Console.ReadKey();
            /*
            getTemperatureCPU
            getUsageCPU
            getUsageRAM
            getUser
            getUsers
            getSpeedEthernet
            getTraffic [Network Interfase Name]
            getNetworkInterfases  
            getProcess 
            closeProcess [Name]
            getKeye
            */

        }
    }
}