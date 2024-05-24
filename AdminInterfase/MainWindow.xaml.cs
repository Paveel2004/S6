using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Text.RegularExpressions;
using AdminInterfase.MoreWindow;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using System.Management;
using DocumentFormat.OpenXml.AdditionalCharacteristics;
using GlobalClass;
using Server;
using GlobalClass.Static_data;

namespace AdminInterfase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    

    public partial class MainWindow : Window
    {

        private bool isMenuVisible = true;
        private string BroadcastAddress = "224.0.0.252";
        private int BroadcastPort = 11000;
        private int localPort = 2222;
        private int localPort2 = 3333;
        private IPAddress localAddr = IPAddress.Parse(ManagerIP.GetIPAddress());
        static string connectionString = "Server=192.168.1.143\\SQLEXPRESS; Database=S6Server; User Id=Name; Password=12345QWERTasdfg; TrustServerCertificate=true";
        public MainWindow()
        {
            InitializeComponent();
            Menu.Items.Add(new MenuItem { Text = "Онлайн", ClickHandler = Online_Click});
            Menu.Items.Add(new MenuItem { Text = "Контроль"});
            Menu.Items.Add(new MenuItem { Text = "Компьютеры", ClickHandler = Computers_Click });
            Menu.Items.Add(new MenuItem { Text = "Приложения", ClickHandler = Apps_Click });
            Menu.Items.Add(new MenuItem { Text = "Настройки",});
            Task.Run(() => StartServer(localPort, client => HandleClient(client, onlineComputersListBox), localAddr));
            //Task.Run(() => StartServer(localPort2, client => HandleDB(client), localAddr));
        }
       
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.Show_Details(sender);
        }
        
        
        private void App_Click(object sender, RoutedEventArgs e)
        {
            //ListBoxItemButtonHandler.ShowApps(sender, "getApplications", "Приложения");
        }
        public void Process_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.ShowProcess(sender);
        }
        private void SetComputersVisibility()
        {
            onlineComputersListBox.Visibility = Visibility.Hidden;
            allComputersListBox.Visibility = Visibility.Visible;
        }
        private void SetOnlineVisibility()
        {
            onlineComputersListBox.Visibility = Visibility.Visible;
            allComputersListBox.Visibility = Visibility.Hidden;
        }
        public void Apps_Click(object sender, RoutedEventArgs e)
        {
            FillUsersListBox(UsersListBox);
        }
        public void Computers_Click(object sender, RoutedEventArgs e)
        {
            //GetBuild();
            SetComputersVisibility();
            onlineComputersListBox.Items.Clear();
            LoadData();
        }
     
        private List<string> GetBiosSerialNumbers()
        {
            List<string> serialNumbers = new List<string>();

            string query = "SELECT [Серийный номер BIOS] FROM [Устройтво]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string serialNumber = reader["Серийный номер BIOS"].ToString();
                        serialNumbers.Add(serialNumber);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении серийных номеров BIOS: " + ex.Message);
                }
            }

            return serialNumbers;
        }
        private string GetComputerName(string serialNumberBIOS)
        {
            string computerName = string.Empty;

            // Строка запроса, которую не нужно менять
            string query = $"SELECT Имя FROM Устройтво WHERE [Серийный номер BIOS] = '{serialNumberBIOS}'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            computerName = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при получении имени компьютера: " + ex.Message);
                    }
                }
            }

            return computerName;
        }


        private void LoadData()
        {
            string query = "ПолучитьЗначениеХарактеристики";
            List<DeviceCharacteristics> devices = new List<DeviceCharacteristics>();

            // Получение списка всех устройств из таблицы "Устройство"
            List<string> biosSerialNumbers = GetBiosSerialNumbers();

            if (biosSerialNumbers.Count == 0)
            {
                MessageBox.Show("Нет доступных устройств.");
                return;
            }

            // Для каждого устройства извлекаем данные характеристик и добавляем их в список
            foreach (string biosSerialNumber in biosSerialNumbers)
            {
                DeviceCharacteristics deviceCharacteristics = new DeviceCharacteristics();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var parameters = new List<(string Type, string Attribute, Action<string> Setter)>
            {
                ("Процессор", "Модель", value => deviceCharacteristics.ProcessorModel = value),
                ("Процессор", "Архитектура", value => deviceCharacteristics.ProcessorArchitecture = value),
                ("Процессор", "К-во ядер", value => deviceCharacteristics.ProcessorCores = value),
                ("ОЗУ", "Объём", value => deviceCharacteristics.RAMSize = value),
                ("ОЗУ", "Частота", value => deviceCharacteristics.RAMFrequency = value),
                ("ОЗУ", "Тип", value => deviceCharacteristics.RAMType = value),
                ("Графический процессор", "Модель", value => deviceCharacteristics.GPUModel = value),
                ("ОС", "Операционная система", value => deviceCharacteristics.OS = value),
                ("ОС", "Версия", value => deviceCharacteristics.OSVersion = value),
                ("ОС", "Разрядность", value => deviceCharacteristics.OSArchitecture = value),
                ("Диск", "Объём", value => deviceCharacteristics.TotalSpaceDisk = value),
            };

                    foreach (var param in parameters)
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@ТипХарактеристики", param.Type);
                            command.Parameters.AddWithValue("@Характеристика", param.Attribute);
                            command.Parameters.AddWithValue("@BIOS", biosSerialNumber);

                            try
                            {
                                SqlDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    param.Setter(reader["Значение"].ToString());
                                }
                                reader.Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                            }
                        }
                    }
                }
                deviceCharacteristics.ComputerName = GetComputerName(biosSerialNumber);
                devices.Add(deviceCharacteristics);
            }

            // Устанавливаем список устройств в качестве источника данных для ListBox
            allComputersListBox.ItemsSource = devices;
        }

        public class Characteristic
        {
            public string Type { get; set; }
            public string Attribute { get; set; }
            public string Value { get; set; }
        }
        public class ApplicationData
        {
            public int Weight { get; set; }
            public string Name { get; set; }
            public DateTime InstallDate { get; set; }

            public override string ToString()
            {
                return $"{Name} (Weight: {Weight} MB, Installed: {InstallDate.ToShortDateString()})";
            }
        }

        public void Control_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.OpenControlWindow(sender);
        }
        private Dictionary<string, string> usersDictionary = new Dictionary<string, string>();
        private List<ApplicationData> GetApplicationsForUser(string userSid)
        {
            List<ApplicationData> userApplications = new List<ApplicationData>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "EXEC ПриложенияПользователя @SIDПользователя";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SIDПользователя", userSid);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var app = new ApplicationData
                            {
                                Weight = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                InstallDate = reader.GetDateTime(2)
                            };
                            userApplications.Add(app);
                        }
                    }
                }
            }

            return userApplications;
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedIndex != -1)
            {
                string selectedUserName = UsersListBox.SelectedItem.ToString();

                // Получаем SID выбранного пользователя из словаря
                string selectedUserSid = usersDictionary[selectedUserName];

                // Вызываем процедуру ПриложенияПользователя с передачей SID
                List<ApplicationData> userApplications = GetApplicationsForUser(selectedUserSid);

                // Заполняем второй ListBox результатами процедуры
                FillApplicationsListBox(userApplications);
            }
        }

        private void FillApplicationsListBox(List<ApplicationData> applications)
        {
            ApplicationsListBox.Items.Clear();

            foreach (var application in applications)
            {
                ApplicationsListBox.Items.Add(application);
            }
        }

        public void FillUsersListBox(ListBox listBox)
        {
            listBox.Items.Clear();
            usersDictionary.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT [Имя], [Идентификатор безопасности] FROM Пользователи";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            string sid = reader.GetString(1);

                            listBox.Items.Add(name);
                            usersDictionary[name] = sid;
                        }
                    }
                }
            }
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
        static void HandleClient(TcpClient tcpClient, ListBox listBox)
        {
            try
            {
                using NetworkStream stream = tcpClient.GetStream();
                byte[] data = new byte[999999 * 100];
                int bytesRead;
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");

                    // Обновляем listBox в UI потоке
                    Application.Current.Dispatcher.Invoke(() => listBox.Items.Add(new ListBoxInfo { Text = TextHelper.DictionaryToText(message), Buttons = new ObservableCollection<string> { "Кнопка 1", "Кнопка 2", "Кнопка 3" } }));

                    stream.Write(response, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                tcpClient.Close();
            }
        }
      /*  static void HandleDB(TcpClient tcpClient)
        {            
            try
            {
                DataBaseHelper.connectionString = connectionString;
                using NetworkStream stream = tcpClient.GetStream();
                byte[] data = new byte[999999*100];
                int bytesRead;
                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    byte[] response = Encoding.UTF8.GetBytes("Сообщение получено");

                    DeviceCharacteristics sborka = JsonConvert.DeserializeObject<DeviceCharacteristics>(message);
                    DataBaseHelper.Query($"\tDELETE FROM Устройтво WHERE [Серийный номер BIOS] = '{sborka.SerialNumberBIOS}'");
                    DataBaseHelper.Query($"INSERT INTO Устройтво VALUES ('{sborka.SerialNumberBIOS}','{sborka.ComputerName}')");
                    DataBaseHelper.Query($"EXECUTE ДобавитьВСборку @ТипХарактеристики = 'Диск', @Характеристика = 'Объём', @СерийныйНомерBIOS = '{sborka.SerialNumberBIOS}', @Значение =  '{sborka.TotalSpaceDisk}'");
                    DataBaseHelper.Query($"EXECUTE ДобавитьПроцессор @BIOS = '{sborka.SerialNumberBIOS}', @Модель = '{sborka.ProcessorModel}', @Архитектура = '{sborka.ProcessorArchitecture}', @КоличествоЯдер = '{sborka.ProcessorCores}' ");
                    DataBaseHelper.Query($"EXECUTE ДобавитьОЗУ @BIOS = '{sborka.SerialNumberBIOS}', @Объём = '{sborka.RAMSize}', @Частота = '{sborka.RAMFrequency}', @Тип = '{sborka.RAMType}'");
                    DataBaseHelper.Query($"EXECUTE ДобавитьВСборку @ТипХарактеристики = 'ОС', @Характеристика = 'Операционная система', @СерийныйНомерBIOS = '{sborka.SerialNumberBIOS}', @Значение =  '{sborka.OS}'");
                    DataBaseHelper.Query($"EXECUTE ДобавитьВСборку @ТипХарактеристики = 'ОС', @Характеристика = 'Версия', @СерийныйНомерBIOS = '{sborka.SerialNumberBIOS}', @Значение =  '{sborka.OSVersion}'");
                    DataBaseHelper.Query($"EXECUTE ДобавитьВСборку @ТипХарактеристики = 'ОС', @Характеристика = 'Разрядность', @СерийныйНомерBIOS = '{sborka.SerialNumberBIOS}', @Значение =  '{sborka.OSArchitecture}'");
                    DataBaseHelper.Query($"EXECUTE ДобавитьВСборку @ТипХарактеристики = 'Графический процессор', @Характеристика = 'Модель', @СерийныйНомерBIOS = '{sborka.SerialNumberBIOS}', @Значение = '{sborka.GPUModel}'");
                    stream.Write(response, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                tcpClient.Close();
            }
        }*/
        static void getApp()
        {

        }
        public void GetAll()
        {
            onlineComputersListBox.Items.Clear();
            MessageSender.BroadcastMessage("getAll", BroadcastAddress, BroadcastPort);

            
        }
/*        public void GetBuild()
        {
     
            MessageSender.BroadcastMessage("getBuild", BroadcastAddress, BroadcastPort);

        }*/
        private void Online_Click(object sender, RoutedEventArgs e)
        {
            SetOnlineVisibility();
            GetAll();
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)((Button)sender).DataContext;
            menuItem.ClickHandler?.Invoke(sender, e);
        }
       
        private void Menu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ToggleMenuVisibility(onlineComputersListBox);
                ToggleMenuVisibility(allComputersListBox);
            }
        }
        private void ToggleMenuVisibility(ListBox listBox)
        {
            DoubleAnimation animation = new DoubleAnimation();
            if (Menu.Margin.Left >= 0) // Если Menu видимо, то анимируем его влево (скрываем)
            {
                animation.To = -Menu.ActualWidth;
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(40, 40, 30, 30), TimeSpan.FromSeconds(0.3)));
          
            }
            else // Иначе анимируем вправо (показываем)
            {
                animation.To = 0;
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(220, 40, 30, 30), TimeSpan.FromSeconds(0.3)));
            }
            animation.Duration = TimeSpan.FromSeconds(0.3);
            Menu.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(Menu.Margin, new Thickness(animation.To.Value, 0, 0, 0), animation.Duration));
            // Используем сравнение с 0, чтобы понять, видимо ли меню
            isMenuVisible = Math.Abs(Menu.Margin.Left) < double.Epsilon;
        }
        private void App_LocalDB(object sender, RoutedEventArgs e)
        {
            AppWindow app = new AppWindow();
            app.Show();
        }
        //////////////
        ///
        ObservableCollection<string> items;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var searchText = textBox.Text.ToLower();
            //listBox.ItemsSource = items.Where(item => item.ToLower().Contains(searchText));
        }
        

    }


}
