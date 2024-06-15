using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdminInterfase
{
    internal static class MessageSender
    {
        public static string SendMessage(string serverAddress, int port, string message)
        {
            try
            {
                // Создаем TcpClient и подключаемся к серверу
                using TcpClient client = new TcpClient(serverAddress, port);
                // Получаем поток для обмена данными с сервером
                using NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                // Читаем ответ от сервера
                data = new byte[999999];
                int bytesRead = stream.Read(data, 0, data.Length);
                string response = Encoding.UTF8.GetString(data, 0, bytesRead);
                return response;
            }
            catch (Exception ex)
            {return "";}

        }
        public static async void BroadcastMessage(string message, string address, int port)
        {
            var brodcastAddress = IPAddress.Parse(address);
            using var udpSender = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpSender.SendAsync(data, new IPEndPoint(brodcastAddress, port));
            await Task.Delay(1000);
        }
    }
}
