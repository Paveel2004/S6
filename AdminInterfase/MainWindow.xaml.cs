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
        private IPAddress localAddr = IPAddress.Parse(ManagerIP.GetIPAddress());
        string connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
        public MainWindow()
        {
            InitializeComponent();
            Menu.Items.Add(new MenuItem { Text = "Онлайн", ClickHandler = Monitoring_Click});
            Menu.Items.Add(new MenuItem { Text = "Контроль"});
            Menu.Items.Add(new MenuItem { Text = "Компьютеры", ClickHandler = Computers_Click });
            Menu.Items.Add(new MenuItem { Text = "Настройки",});
            Task.Run(() => StartServer(localPort, client => HandleClient(client, listBox), localAddr));

        }
       
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.Show_Details(sender);
        }
      

        private void App_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.ShowApps(sender, "getApplications", "Приложения");
        }
        public void Process_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.ShowProcess(sender);
        }
        public void Computers_Click(object sender, RoutedEventArgs e)
        {

            listBox.Items.Clear();
            LoadData();
        }
        public class DeviceCharacteristics
        {
            public string ProcessorModel { get; set; }
            public string ProcessorArchitecture { get; set; }
            public string ProcessorCores { get; set; }
            public string RAMSize { get; set; }
            public string RAMFrequency { get; set; }
            public string RAMType { get; set; }
            public string GPUModel { get; set; }
            public string OS { get; set; }
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
                ("ОС", "Операционная система", value => deviceCharacteristics.OS = value)
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

                devices.Add(deviceCharacteristics);
            }

            // Устанавливаем список устройств в качестве источника данных для ListBox
            listBox2.ItemsSource = devices;
        }



        public class Characteristic
        {
            public string Type { get; set; }
            public string Attribute { get; set; }
            public string Value { get; set; }
        }

        public void Control_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.OpenControlWindow(sender);
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
                byte[] data = new byte[999999*100];
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
        public void GetAll()
        {
            listBox.Items.Clear();
            MessageSender.BroadcastMessage("getAll", BroadcastAddress, BroadcastPort);

            
        }
        private void Monitoring_Click(object sender, RoutedEventArgs e)
        {
                       
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
                ToggleMenuVisibility();
            }
        }
        private void ToggleMenuVisibility()
        {
            DoubleAnimation animation = new DoubleAnimation();
            if (Menu.Margin.Left >= 0) // Если Menu видимо, то анимируем его влево (скрываем)
            {
                animation.To = -Menu.ActualWidth;
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(40, 40, 30, 40), TimeSpan.FromSeconds(0.3)));
          
            }
            else // Иначе анимируем вправо (показываем)
            {
                animation.To = 0;
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(220,40, 30, 40), TimeSpan.FromSeconds(0.3)));
            }
            animation.Duration = TimeSpan.FromSeconds(0.3);
            Menu.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(Menu.Margin, new Thickness(animation.To.Value, 0, 0, 0), animation.Duration));
            // Используем сравнение с 0, чтобы понять, видимо ли меню
            isMenuVisible = Math.Abs(Menu.Margin.Left) < double.Epsilon;
        }

        //////////////
        ///
        ObservableCollection<string> items;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
         //   var searchText = textBox.Text.ToLower();
            //listBox.ItemsSource = items.Where(item => item.ToLower().Contains(searchText));
        }
                

    }

}
