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
        private IPAddress localAddr = IPAddress.Parse("192.168.1.52");
        public MainWindow()
        {
            InitializeComponent();
            Menu.Items.Add(new MenuItem { Text = "Онлайн", ClickHandler = Monitoring_Click});
            Menu.Items.Add(new MenuItem { Text = "Контроль"});
            Menu.Items.Add(new MenuItem { Text = "Компьютеры", ClickHandler = Computers_Click });
            Menu.Items.Add(new MenuItem { Text = "Отчёты"});
            Menu.Items.Add(new MenuItem { Text = "Настройки"});
            Task.Run(() => StartServer(localPort, client => HandleClient(client, listBox), localAddr));

        }
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            // Получение кнопки, которую нажали
            Button button = sender as Button;

            // Получение StackPanel, содержащего кнопки
            StackPanel buttonPanel = button.Parent as StackPanel;
            StackPanel buttons = buttonPanel.Children.OfType<StackPanel>().First();

            // Получение TextBlock с дополнительным текстом
            TextBlock additionalText = buttonPanel.Children.OfType<TextBlock>().First(t => t.Name == "AdditionalText");

            var item = button.DataContext;

            // Преобразуем DataContext в нужный тип
            var myItem = item as ListBoxInfo; // замените MyItemType на тип вашего элемента

            // Регулярное выражение для поиска IP-адресов
            var regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            // Ищем все IP-адреса в тексте
            var matches = regex.Matches(myItem.Text);

            // Изменение видимости кнопок и дополнительного текста
            if (buttons.Visibility == Visibility.Collapsed)
            {
                buttons.Visibility = Visibility.Visible;
                additionalText.Visibility = Visibility.Visible;

                foreach (Match match in matches)
                {

                    additionalText.Text = SendMessage(match.Value, 1111, "AdditionalInformation");
                }
                 // Установка значения для дополнительного текста
                button.Content = "Скрыть";
            }
            else
            {
                buttons.Visibility = Visibility.Collapsed;
                additionalText.Visibility = Visibility.Collapsed;
                button.Content = "Подробнее";
            }
        }

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
            { return ""; }
            
        }
        private void ShowApps(object sender, string command, string title)
        {
            // Получаем кнопку, на которую нажали
            var button = (Button)sender;

            // DataContext кнопки содержит данные элемента
            var item = button.DataContext;

            // Преобразуем DataContext в нужный тип
            var myItem = item as ListBoxInfo; // замените MyItemType на тип вашего элемента

            // Регулярное выражение для поиска IP-адресов
            var regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            // Ищем все IP-адреса в тексте
            var matches = regex.Matches(myItem.Text);

            // Выводим все найденные IP-адреса
            foreach (Match match in matches)
            {
                Apps app = new(JsonConvert.DeserializeObject<List<string>>(SendMessage(match.Value, 1111, command)), title);
                app.Show();
            }
        }
        private void App_Click(object sender, RoutedEventArgs e)
        {
            ShowApps(sender, "getApplications", "Приложения");
        }
        public void Process_Click(object sender, RoutedEventArgs e)
        {
            ShowApps(sender, "getProcesses", "Процессы");
        }
        public void Computers_Click(object sender, RoutedEventArgs e)
        {

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
                    Application.Current.Dispatcher.Invoke(() => listBox.Items.Add(new ListBoxInfo { Text = message, Buttons = new ObservableCollection<string> { "Кнопка 1", "Кнопка 2", "Кнопка 3" } }));
                    
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
            BroadcastMessage("getAll", BroadcastAddress, BroadcastPort);
            
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
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(40, 30, 30, 40), TimeSpan.FromSeconds(0.3)));
          
            }
            else // Иначе анимируем вправо (показываем)
            {
                animation.To = 0;
                listBox.BeginAnimation(ListBox.MarginProperty, new ThicknessAnimation(listBox.Margin, new Thickness(220, 30, 30, 40), TimeSpan.FromSeconds(0.3)));
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
            var searchText = textBox.Text.ToLower();
            listBox.ItemsSource = items.Where(item => item.ToLower().Contains(searchText));
        }

    }

}
