using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.Concurrent;
using System.Data.SQLite;
using S6.DataBase;
using S6.JsonClass;
using S6.MoreWindow;
using Newtonsoft.Json;
using System.Security.AccessControl;
using System.IO;


namespace S6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isMenuVisible = true;
        private delegate void UpdateListBoxDelegate();

        private UpdateListBoxDelegate updateListBoxDelegate;
        private object lockObject = new object();
        public MainWindow()
        {

            InitializeComponent();
            Menu.Items.Add(new MenuItem { Text = "Устройства в сети", ClickHandler = Devices_Click });
            Menu.Items.Add(new MenuItem { Text = "Пользователи", ClickHandler = Users_Click });
            Menu.Items.Add(new MenuItem { Text = "Операционная система", ClickHandler = OS_Click });
            Menu.Items.Add(new MenuItem { Text = "Дисковое пространство", ClickHandler = Disk_Click });
            Menu.Items.Add(new MenuItem { Text = "Аналитика и графики" });
            Menu.Items.Add(new MenuItem { Text = "Настройки", ClickHandler = Settings_Click });

            Task.Run(() => UpdateListBox());
            StartServer();


        }
        private void Settings_Click (object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }
        private async Task UpdateListBox()
        {
            while (true)
            {
                if (updateListBoxDelegate != null)
                {
                    await Task.Run(() => Application.Current.Dispatcher.Invoke(() => updateListBoxDelegate()));
                }
                await Task.Delay(100);
            }
        }
        private void Disk_Click(object sender, RoutedEventArgs e)
        {

            updateListBoxDelegate = new (DisplayDisk);
        }
        private void OS_Click(object sender, RoutedEventArgs e)
        {

            updateListBoxDelegate = new (DisplayOS);

        }


        //private ConcurrentQueue<string> messagesQueue = new ConcurrentQueue<string>();
        static async Task StartServer()
        {
            TcpListener server = null;
            try
            {
                // Указываем IP-адрес и порт, на котором будет слушать сервер
                /* IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                 int port = 1111;
 */
                string jsonFilePath = @"C:\Users\ASUS\source\repos\ClientS6\S6\bin\Debug\net6.0-windows\data.json";
                string jsonContent = File.ReadAllText(jsonFilePath);

                Server adress = JsonConvert.DeserializeObject<Server>(jsonContent);


                IPAddress localAddr = IPAddress.Parse(adress.serverAddress);
                int port = adress.port;
                // Создаем TcpListener
                server = new TcpListener(localAddr, port);

                // Начинаем прослушивание клиентов
                server.Start();


                while (true)
                {
                    // Ожидаем входящее подключение
                    TcpClient client = await server.AcceptTcpClientAsync();
                    //MessageBox.Show("Подключен клиент!");
                    
                    // Обрабатываем подключенного клиента в отдельном потоке
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                // Завершаем прослушивание клиентов при выходе из цикла
                server?.Stop();
            }
        }
        static void Query(string query, string connecrionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connecrionString))
            {
               // try
               // {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                //}
             /*   catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }*/
            }
        }
        static async void HandleClient(TcpClient tcpClient)
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
                    Query($"INSERT OR REPLACE INTO Процессор ([Серийный номер], Модель, Загруженность, [Количество ядер], Архитектура, Температура) VALUES ('{systemInfo.CPU.SerialNumber}','{systemInfo.CPU.Name}',{systemInfo.CPU.CpuUsage},{systemInfo.CPU.CoreCount},'{systemInfo.CPU.Architecture}',{systemInfo.CPU.Temperature})", DataBaseHelper.connectionString);
                    Query($"INSERT OR REPLACE INTO Система ([Операционная система], Разрядность, [Серийный номер], [Количество пользователей], Состояние, [Версия ОС], [Текущий пользователь]) " +
            $"VALUES ('{systemInfo.OS.OS}',{systemInfo.OS.Architecture},'{systemInfo.OS.SerialNumber}',{systemInfo.OS.NumberOfUsers},'{systemInfo.OS.SystemState}','{systemInfo.OS.VersionOS}','{systemInfo.USER.UserName}')", DataBaseHelper.connectionString);

                    Query($"INSERT OR REPLACE INTO  Пользователи (SID, [Имя пользователя], Статус, [Серийный номер системы]) VALUES ('{systemInfo.USER.UserSID}','{systemInfo.USER.UserName}','{systemInfo.USER.UserState}','{systemInfo.OS.SerialNumber}')",DataBaseHelper.connectionString);
                    Query($"INSERT OR REPLACE INTO Сеть (IP,MAC,[Ethernet speed]) VALUES ('{systemInfo.NETWORK.IP}','{systemInfo.NETWORK.MAC}',{systemInfo.NETWORK.EthernetSpeed})", DataBaseHelper.connectionString);
                    Query($"INSERT OR REPLACE INTO [Оперативная память] ([Тип памяти], Загруженность, Объём, BIOS) VALUES ('{systemInfo.RAM.RamType}',{systemInfo.RAM.RamUsage},{systemInfo.RAM.TotalPhisicalMemory},'{systemInfo.BIOS.SerialNumber}')",DataBaseHelper.connectionString); 
                    Query($"INSERT OR REPLACE INTO BIOS ([Версия BIOS],[Серийный номер]) VALUES ('{systemInfo.BIOS.BiosVeesion}','{systemInfo.BIOS.SerialNumber}')",DataBaseHelper.connectionString);
                    Query($"INSERT OR REPLACE INTO Устройство (Процессор, [Оперативная память], BIOS, Система, [MAC Адрес]) VALUES ('{systemInfo.CPU.SerialNumber}','{systemInfo.BIOS.SerialNumber}','{systemInfo.BIOS.SerialNumber}','{systemInfo.OS.SerialNumber}','{systemInfo.NETWORK.MAC}')", DataBaseHelper.connectionString);
                    for (int i = 0; i < systemInfo.DISK.Count; i++)
                    {
                       
                        var disk = systemInfo.DISK[i];

                        try
                        {
                            Query($"INSERT OR REPLACE INTO Диски ([Имя диска], [Объём диска], [Свободное место], [Серийный номер системы]) " +
                                    $"VALUES ('{disk.DeviceID}', {disk.Size}, {disk.FreeSpace}, '{systemInfo.OS.SerialNumber}')", DataBaseHelper.connectionString);
                        }
                        catch (Exception ex)
                        {

                            Query($"UPDATE Диски\r\nSET [Объём диска] = {disk.Size}, [Свободное место] = {disk.FreeSpace}\r\n" +
                                    $"WHERE [Имя диска] = '{disk.DeviceID}' AND [Серийный номер системы] = '{systemInfo.OS.SerialNumber}';\r\n",
                                    DataBaseHelper.connectionString);
                           
                        }
                    }

                    // Отправляем подтверждение клиенту 
                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");
                    stream.Write(response, 0, response.Length);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Закрываем соединение при завершении работы с клиентом
                tcpClient.Close();
            }
        }
        

        public void DisplayDevices()
        {
            Information_ListBox.Items.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM MainInfo",connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DevicesInfo devices = new DevicesInfo(
                                    reader["Модель процессора"].ToString(),
                                    double.Parse(reader["Температура процессора"].ToString()), // Преобразование в double
                                    double.Parse(reader["Загруженность процессора"].ToString()), // Преобразование в double
                                    int.Parse(reader["Количество ядер в процессоре"].ToString()), // Преобразование в int
                                    int.Parse(reader["Архитектура процессора"].ToString()), // Преобразование в int
                                    reader["IP Адрес"].ToString(),
                                    reader["MAC Адрес"].ToString(),
                                    double.Parse(reader["Скорость Ethernet"].ToString()), // Преобразование в double
                                    reader["Операционная система"].ToString(),
                                    int.Parse(reader["Разрядность системы"].ToString()), // Преобразование в int
                                    reader["Состояние системы"].ToString(),
                                    reader["Версия BIOS"].ToString(),
                                    reader["Загруженность оперативной памяти"].ToString(),
                                    reader["Объём оперативной памяти"].ToString(), // Преобразование в string
                                    reader["Тип оперативной памяти"].ToString(), // Преобразование в string
                                    reader["Текущий пользователь"].ToString());
                                Information_ListBox.Items.Add(devices);
                            }
                            
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void DisplayDisk()
        {
            Information_ListBox.Items.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM DiskIndo", connection)) 
                    { 
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DiskInfoDisplay disk = new(
                                    reader["Имя диска"].ToString(),
                                    ulong.Parse(reader["Объём диска"].ToString()),
                                    ulong.Parse(reader["Свободное место"].ToString()),
                                    reader["Текущий пользователь"].ToString(),
                                    reader["Операционная система"].ToString());
                                Information_ListBox.Items.Add(disk);
                            }
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
        public void DisplayOS()
        {
            Information_ListBox.Items.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Система", connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                InfoDisplayOS os = new (
                                      reader["Операционная система"].ToString(),
                                      int.Parse(reader["Разрядность"].ToString()),
                                      reader["Серийный номер"].ToString(),
                                      int.Parse(reader["Количество пользователей"].ToString()),
                                      reader["Состояние"].ToString(),
                                      reader["Версия ОС"].ToString(),
                                      reader["Текущий пользователь"].ToString()
                                      );
                                Information_ListBox.Items.Add(os);
                            }
                        }
                    }
                    connection.Close();
                }
                catch (Exception ex)
                { 
                    MessageBox.Show(ex.Message, "Error");   
                }
            }
        }

        public void DisplayUsers()
        {
            Information_ListBox.Items.Clear();
            // Обработчик события для второй кнопки
            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM USERS", connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UsersInfo users = new UsersInfo(reader["SID"].ToString(), reader["Имя пользователя"].ToString(), reader["Статус"].ToString(), reader["Операционная система"].ToString(), reader["Серийный номер ОС"].ToString());
                                Information_ListBox.Items.Add(users);
                            }
                        }
                    }
                    connection.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void Devices_Click(object sender, RoutedEventArgs e)
        {
            // Обработчик события для первой кнопки

            updateListBoxDelegate = new (DisplayDevices);
        }
        private void Users_Click(object sender, RoutedEventArgs e)
        {

            updateListBoxDelegate = new (DisplayUsers);

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Ваш обработчик события
            var menuItem = (MenuItem)((Button)sender).DataContext;
            menuItem.ClickHandler?.Invoke(sender, e);
        }
        private void ToggeleMenuVisibility()
        {
            DoubleAnimation animation = new DoubleAnimation();
            if (Menu.Margin.Left >= 0) // Если Menu видимо, то анимируем его влево (скрываем)
            {
                animation.To = -Menu.ActualWidth;
            }
            else // Иначе анимируем вправо (показываем)
            {
                animation.To = 0;
            }
            animation.Duration = TimeSpan.FromSeconds(0.3);
            Menu.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(Menu.Margin, new Thickness(animation.To.Value, 0, 0, 0), animation.Duration));

            // Используем сравнение с 0, чтобы понять, видимо ли меню
            isMenuVisible = Math.Abs(Menu.Margin.Left) < double.Epsilon;
        }

        private void Menu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ToggeleMenuVisibility();
            }
        }
    }

}
