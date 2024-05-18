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

namespace AdminInterfase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static string GetIPAddress()
        {
            string query = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection queryCollection = searcher.Get();

            foreach (ManagementObject m in queryCollection)
            {
                // Получаем массив IP-адресов (может быть несколько)
                string[] ipAddresses = m["IPAddress"] as string[];

                // Выбираем первый не-null и не-пустой IP-адрес
                if (ipAddresses != null && ipAddresses.Length > 0 && !string.IsNullOrEmpty(ipAddresses[0]))
                {
                    return ipAddresses[0];
                }
            }

            // Если не удалось получить IP-адрес, возвращаем "N/A" или другое значение по вашему усмотрению
            return "N/A";
        }
        private bool isMenuVisible = true;
        private string BroadcastAddress = "224.0.0.252";
        private int BroadcastPort = 11000;
        private int localPort = 2222;
        private IPAddress localAddr = IPAddress.Parse(GetIPAddress());
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
            ListBoxItemButtonHandler.ShowApps(sender, "getProcesses", "Процессы");
        }
        public void Computers_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Visible;
            ComputerName.Visibility = Visibility.Visible;
            listBox.Items.Clear();
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
            dataGrid.Visibility = Visibility.Hidden;
            ComputerName.Visibility = Visibility.Hidden;                
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
               private void ComputerName_DropDownOpened(object sender, EventArgs e)
       {
           ComputerName.Items.Clear();
           using (SqlConnection connection = new SqlConnection(connectionString))
           {
               connection.Open();
               string sqlQuery = "SELECT Имя FROM Устройтво";
               SqlCommand cmd = new SqlCommand(sqlQuery, connection);

               using (SqlDataReader reader = cmd.ExecuteReader())
               {
                   while (reader.Read())
                   {
                       string name = reader["Имя"].ToString();

                       ComputerName.Items.Add(name);
                   }
               }
           }
       }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string query = $" EXECUTE ДанныеОУстройстве @Имя = '{ComputerName.Text}'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {


                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                dataGrid.ItemsSource = dataTable.DefaultView;

            }
        }

    }

}
