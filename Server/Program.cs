using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
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

                    SystemInfo systemInfo = JsonConvert.DeserializeObject<SystemInfo>(message);
                    
                    DataBaseHelper.connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
                    DataBaseHelper.Query($"\r\nEXECUTE ДобавитьУстройство @BIOS='{systemInfo.BIOS.SerialNumber}', @МодельПроцессора = '{systemInfo.CPU.Name}', @АрхитектураПроцессора = '{systemInfo.CPU.Architecture}', @ТипОЗУ='{systemInfo.RAM.RamType}', @ЧастотаОЗУ='{systemInfo.RAM.ConfiguredClockSpeed}', @ОбъёмОЗУ='{systemInfo.RAM.TotalPhisicalMemory}', @ОперационнаСистема='{systemInfo.OS.OS}', \r\n@РазрядностьОперационнойСистемы='{systemInfo.OS.Architecture}', @ВерсияОперационнойСистепмы = '{systemInfo.OS.VersionOS}', @МодельВидеокарты = '{systemInfo.VideoCard.Model}', @ФизическийMAC = '' ");
                    // Отправляем подтверждение клиенту
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

    }
}