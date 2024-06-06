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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using DocumentFormat.OpenXml.Vml;

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
        private string connectionString = "Server=WIN-5CLMGM4LR48\\SQLEXPRESS; Database=Server; User Id=Name; Password=12345QWERTasdfg; TrustServerCertificate=true";
        public PlotModel PlotModel { get; private set; }

        private void LoadDataAndPlot(string sid, DateTime selectedDate)
        {
            
            DateTime startDate = selectedDate.Date;
            DateTime endDate = selectedDate.Date.AddDays(1).AddTicks(-1);

            var data = GetDataFromDatabase(sid, startDate, endDate);

            if (data.Count == 0)
            {
                MessageBox.Show("Нет данных за выбранную дату.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PlotModel = new PlotModel { Title = "Оперативная память" };
            var xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Title = "Дата" };
            PlotModel.Axes.Add(xAxis);
            var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Значение" };
            PlotModel.Axes.Add(yAxis);
            var lineSeries = new LineSeries
            {
                Title = "RAM Usage",
                MarkerType = MarkerType.Circle
            };

            foreach (var item in data)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Value));
            }

            PlotModel.Series.Add(lineSeries);
            RamUsage.Model = PlotModel;
        }

        private List<UsageData> GetDataFromDatabase(string user, DateTime startDate, DateTime endDate)
        {
            var data = new List<UsageData>();

            string storedProcedure = "UserUsageRAM";

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@User", user);
                    command.Parameters.AddWithValue("@StartData", startDate);
                    command.Parameters.AddWithValue("@EndData", endDate);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new UsageData
                            {
                                Date = reader.GetDateTime(reader.GetOrdinal("Дата/Время")),
                                Value = reader.GetDouble(reader.GetOrdinal("Значение"))
                            });
                        }
                    }
                }
            }

            return data;
        }


        public MainWindow()
        {
            InitializeComponent();
           

            /*            FillComboBoxFromProcedure(NameOS, "Операционная система", "ОС");
                        FillComboBoxFromProcedure(VersionOS, "Версия", "ОС");
                        FillComboBoxFromProcedure(ArchitectureOS, "Разрядность", "ОС");

                        FillComboBoxFromProcedure(ModelCPU,"Модель","Процессор");
                        FillComboBoxFromProcedure(ArchitectureCPU, "Архитектура", "Процессор");
                        FillComboBoxFromProcedure(CoreCPU, "К-во ядер", "Процессор");


                        FillComboBoxFromProcedure(TotalSpaseRAM, "Объём", "ОЗУ");
                        FillComboBoxFromProcedure(SpeedRAM, "Частота", "ОЗУ");
                        FillComboBoxFromProcedure(TypeRam, "Тип", "ОЗУ");
            */
            LoadProcessorModels();
            DataContext = this;
           
            Task.Run(() => StartServer(localPort, client => HandleClient(client, onlineComputersListBox), localAddr));
            //Task.Run(() => StartServer(localPort2, client => HandleDB(client), localAddr));
        }
        private List<string> GetProcessorModels()
        {
            List<string> processorModels = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM vw_ProcessorModels", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string model = reader["Модель"].ToString();
                            processorModels.Add(model);
                        }
                    }
                }
            }

            return processorModels;
        }
        private List<string> GetFilter(string view, string col)
        {
            List<string> processorModels = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand($"SELECT * FROM {view}", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string model = reader[col].ToString();
                            processorModels.Add(model);
                        }
                    }
                }
            }

            return processorModels;
        }
        public List<string> GetFilteredSerialNumbers(string osName = null, string osArchitecture = null,
                                               string osVersion = null, string processorModel = null,
                                               string processorManufacturer = null, string processorArchitecture = null,
                                               string? logicalProcessors = null, string? cores = null,
                                               string videoModel = null, string graphicsProcessor = null,
                                               string videoManufacturer = null, string? videoMemorySize = null,
                                               string ramPlacement = null, string? ramSize = null,
                                               string ramType = null, string? ramSpeed = null,
                                               string driveType = null, string? drivePool = null)
        {

            List<string> serialNumbers = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetFilteredDevices", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Добавление параметров
                    command.Parameters.AddWithValue("@osName", osName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@osArchitecture", osArchitecture ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@osVersion", osVersion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorModel", processorModel ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorManufacturer", processorManufacturer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorArchitecture", processorArchitecture ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@logicalProcessors", logicalProcessors ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@cores", cores ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoModel", videoModel ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@graphicsProcessor", graphicsProcessor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoManufacturer", videoManufacturer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoMemorySize", videoMemorySize ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramPlacement", ramPlacement ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramSize", ramSize ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramType", ramType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramSpeed", ramSpeed ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@driveType", driveType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@drivePool", drivePool ?? (object)DBNull.Value);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        // Обработка результатов запроса
                        while (reader.Read())
                        {
                            string serialNumber = reader["Серийный номер BIOS"].ToString();
                            serialNumbers.Add(serialNumber);
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибок
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }

            return serialNumbers;
        }

   /*     private void CallStoredProc(string osName, string osArchitecture, string osVersion, string processorModel,
                                 string processorManufacturer, string processorArchitecture, int? logicalProcessors,
                                 int? cores, string videoModel, string graphicsProcessor, string videoManufacturer,
                                 long? videoMemorySize, string ramPlacement, float? ramSize, string ramType,
                                 int? ramSpeed, string driveType, bool? drivePool)
        {
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_GetFilteredDevices", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Добавление параметров
                    command.Parameters.AddWithValue("@osName", osName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@osArchitecture", osArchitecture ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@osVersion", osVersion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorModel", processorModel ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorManufacturer", processorManufacturer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@processorArchitecture", processorArchitecture ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@logicalProcessors", logicalProcessors ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@cores", cores ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoModel", videoModel ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@graphicsProcessor", graphicsProcessor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoManufacturer", videoManufacturer ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@videoMemorySize", videoMemorySize ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramPlacement", ramPlacement ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramSize", ramSize ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramType", ramType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ramSpeed", ramSpeed ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@driveType", driveType ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@drivePool", drivePool ?? (object)DBNull.Value);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        // Обработка результатов запроса
                        while (reader.Read())
                        {
                            // Ваш код для обработки результатов
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибок
                    }
                }
            }
        }*/

        private void LoadProcessorModels()
        {        
            ModelCPU.ItemsSource = GetFilter("vw_ProcessorModels", "Модель");
            ArchitectureCPU.ItemsSource = GetFilter("vw_ProcessorArchitectures","Архитектура");
            CoreCPU.ItemsSource = GetFilter("vw_ProcessorCoreCounts", "Количество ядер");
            GPUmanufacturers.ItemsSource = GetFilter("vw_ProcessorManufacturers", "Производитель");
            LP.ItemsSource = GetFilter("vw_ProcessorLogicalProcessorCounts", "Количество логичских процессов");

         
            GPU.ItemsSource = GetFilter("vw_VideoAdapterGPUs", "Графический процессор");
            memoryVideo.ItemsSource = GetFilter("vw_VideoAdapterMemorySizes", "Объём памяти");
            videoManufactur.ItemsSource = GetFilter("vw_VideoAdapterManufacturers", "Производитель");
            ModelVideo.ItemsSource = GetFilter("vw_VideoAdapterModels", "Модель");
         
            NameOS.ItemsSource = GetFilter("vw_OSNames", "Название");




            ArchitectureOS.ItemsSource = GetFilter("vw_OSArchitectures", "Архитектура");
            VersionOS.ItemsSource = GetFilter("vw_OSVersions", "Версия");

            TotalSpaseRAM.ItemsSource = GetFilter("vw_RamSizes", "Объём");
            SpeedRAM.ItemsSource = GetFilter("vw_RamFrequencies", "Частота");
            TypeRam.ItemsSource = GetFilter("vw_RamTypes", "Тип");
            Manufacturer.ItemsSource = GetFilter("vw_RamManufacturers", "Производитель");

            DiskType.ItemsSource = GetFilter("vw_DiskTypes", "Тип");
            Pool.ItemsSource = GetFilter("vw_DiskPools", "Пул");
        }
      


     
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.Show_Details(sender);
        }
        private void ToggleMenu_Checked(object sender, RoutedEventArgs e)
        {
            // Запускаем анимацию для отображения меню
            var slideDownStoryboard = Resources["SlideDown"] as Storyboard;
            slideDownStoryboard?.Begin();
        }

        private void ToggleMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            // Запускаем анимацию для скрытия меню
            var slideUpStoryboard = Resources["SlideUp"] as Storyboard;
            slideUpStoryboard?.Begin();
        }
/*      public void FillComboBoxFromProcedure(ComboBox comboBox, string characteristic, string type)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("ПараметрыДляФильтра", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Характеристика", characteristic);
                    command.Parameters.AddWithValue("@ТипХарактеристики", type);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Очищаем комбобокс перед добавлением новых элементов
                    comboBox.Items.Clear();

                    // Добавляем значения из столбца "Значение" в комбобокс
                    foreach (DataRow row in dataTable.Rows)
                    {
                        comboBox.Items.Add(row["Значение"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }*/

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
            Apps.Visibility = Visibility.Hidden;
            Usage.Visibility = Visibility.Hidden;
        }
        private void SetOnlineVisibility()
        {
            onlineComputersListBox.Visibility = Visibility.Visible;
            allComputersListBox.Visibility = Visibility.Hidden;
            Apps.Visibility = Visibility.Hidden;
            Usage.Visibility = Visibility.Hidden;
        }
        private void SetUsageVisibility()
        {
            onlineComputersListBox.Visibility = Visibility.Hidden;        
            Apps.Visibility = Visibility.Hidden;         
            allComputersListBox.Visibility = Visibility.Hidden;
            Apps.Visibility = Visibility.Hidden;
            onlineComputersListBox.Visibility = Visibility.Hidden;
            allComputersListBox.Visibility = Visibility.Hidden;
            Usage.Visibility = Visibility.Visible;
            Apps.Visibility = Visibility.Visible;//////////
        }
        private void AppsVisibility()
        {
            onlineComputersListBox.Visibility = Visibility.Hidden;
            allComputersListBox.Visibility = Visibility.Hidden;
            Apps.Visibility = Visibility.Visible;
            Usage.Visibility = Visibility.Hidden;
        }
        public void Apps_Click(object sender, RoutedEventArgs e)
        {
            AppsVisibility();
            FillUsersAppListBox(UsersListBox);
        }
        
        public void Computers_Click(object sender, RoutedEventArgs e)
        {
          //  allComputersListBox.Items.Clear();
            //GetBuild();
            SetComputersVisibility();
       
            allComputersListBox.Items.Clear();
            LoadAllComputerInfo();
            ////////////////
            ///


        }
        private void LoadAllComputerInfo()
        {
            string osName = NameOS.SelectedItem?.ToString();
            string osArchitecture = ArchitectureOS.SelectedItem?.ToString();
            string osVersion = VersionOS.SelectedItem?.ToString();
            string processorModel = ModelCPU.SelectedItem?.ToString();
            string processorManufacturer = GPUmanufacturers.SelectedItem?.ToString();
            string processorArchitecture = ArchitectureCPU.SelectedItem?.ToString();
            string? logicalProcessors = LP.SelectedItem?.ToString();
            string? cores = CoreCPU.SelectedItem?.ToString();
            string videoModel = ModelVideo.SelectedItem?.ToString();
            string graphicsProcessor = GPU.SelectedItem?.ToString();
            string videoManufacturer = videoManufactur.SelectedItem?.ToString();
            string? videoMemorySize = memoryVideo.SelectedItem?.ToString();
            string ramPlacement = null; // Не уверен, откуда брать это значение
            string? ramSize = TotalSpaseRAM.SelectedItem?.ToString();
            string ramType = TypeRam.SelectedItem?.ToString();
            string? ramSpeed = SpeedRAM.SelectedItem?.ToString();
            string driveType = DiskType.SelectedItem?.ToString();
            string? drivePool = Pool.SelectedItem?.ToString();
            List<string> biosSerialNumbers = GetFilteredSerialNumbers(
     osName, osArchitecture, osVersion, processorModel, processorManufacturer,
     processorArchitecture, logicalProcessors, cores, videoModel, graphicsProcessor,
     videoManufacturer, videoMemorySize, ramPlacement, ramSize, ramType,
     ramSpeed, driveType, drivePool);

            foreach (var biosSerialNumber in biosSerialNumbers)
            {
                LoadComputerInfo(biosSerialNumber);
            }
        }


        private List<string> GetAllBiosSerialNumbers()
        {
            List<string> biosSerialNumbers = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT [Серийный номер BIOS] FROM Устройство", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["Серийный номер BIOS"] != DBNull.Value)
                            {
                                biosSerialNumbers.Add(reader["Серийный номер BIOS"].ToString());
                            }
                        }
                    }
                }
            }

            return biosSerialNumbers;
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

        public void Control_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItemButtonHandler.OpenControlWindow(sender);
        }
        private Dictionary<string, string> usersDictionary = new Dictionary<string, string>();
        private List<ApplicationData> GetApplicationsForUser(string userSid)
        {//Так2
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
                            userApplications.Add(new ApplicationData
                            {
                                Name = reader.GetString(reader.GetOrdinal("Название")),
                                Size = reader.GetDouble(reader.GetOrdinal("Вес")),
                                InstallDate = reader.GetDateTime(reader.GetOrdinal("Дата установки"))
                            });

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
                List<ApplicationData> userApplications = GetApplicationsForUser(selectedUserSid);//Так1
                
                // Заполняем второй ListBox результатами процедуры
                FillApplicationsListBox(userApplications);
                try
                {
                    LoadDataAndPlot(selectedUserSid, RAMDate.SelectedDate.Value);
                }
                catch {}
         


            }
        }
        private void UsersUsageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedIndex != -1)
            {
                string selectedUserName = UsersListBox.SelectedItem.ToString();

                // Получаем SID выбранного пользователя из словаря
                string selectedUserSid = usersDictionary[selectedUserName];

                // Вызываем процедуру ПриложенияПользователя с передачей SID                

                // Заполняем второй ListBox результатами процедуры
            

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

        public void FillUsersAppListBox(ListBox listBox)
        {
            listBox.Items.Clear();
            usersDictionary.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM ПользователиСПриложениями";

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
        public void FillUsersUsageListBox(ListBox listBox,DateTime Date)
        {           
            
                listBox.Items.Clear();
                usersDictionary.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"EXEC UsersUsageRamDateList @Date='{Date}'";

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
              /*  ToggleMenuVisibility(onlineComputersListBox);
                ToggleMenuVisibility(allComputersListBox);*/
            }
        }
       /* private void ToggleMenuVisibility(ListBox listBox)
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
        }*/
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
        private void LoadComputerInfo(string bios)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                List<ProcessorInfo> processorInfos = GetProcessorInfos(bios, connection);
                List<VideoAdapterInfo> videoAdapterInfos = GetVideoAdapterInfos(bios, connection);
                List<DriveInfo> driveInfos = GetDriveInfos(bios, connection);
                List<RamInfo> ramInfos = GetRamInfos(bios, connection);
                // Создаем объект ViewModel и добавляем его в список элементов ListBox
                allComputersListBox.Items.Add(new HardwareInfoViewModel(processorInfos, videoAdapterInfos, driveInfos, ramInfos));
            }
        }
        private List<DriveInfo> GetDriveInfos(string biosSerialNumber, SqlConnection connection)
        {
            List<DriveInfo> driveInfos = new List<DriveInfo>();

            using (SqlCommand command = new SqlCommand("GetDriveInfo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BIOS", biosSerialNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DriveInfo driveInfo = new DriveInfo
                        {
                            Model = reader["Модель"] != DBNull.Value ? reader["Модель"].ToString() : "",
                            Size = reader["Объём"] != DBNull.Value ? Convert.ToDouble(reader["Объём"]) : 0.0,
                            Type = reader["Тип"] != DBNull.Value ? reader["Тип"].ToString() : "",
                            Pool = reader["Пул"] != DBNull.Value && Convert.ToBoolean(reader["Пул"]),
                            Layout = reader["Разметка"] != DBNull.Value ? reader["Разметка"].ToString() : ""
                        };

                        driveInfos.Add(driveInfo);
                    }
                }
            }

            return driveInfos;
        }

        private List<RamInfo> GetRamInfos(string biosSerialNumber, SqlConnection connection)
        {
            List<RamInfo> ramInfos = new List<RamInfo>();

            using (SqlCommand command = new SqlCommand("GetRAMInfo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BIOS", biosSerialNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RamInfo ramInfo = new RamInfo
                        {
                            Type = reader["Тип"] != DBNull.Value ? reader["Тип"].ToString() : "",
                            Size = reader["Объём"] != DBNull.Value ? Convert.ToDouble(reader["Объём"]) : 0.0,
                            Speed = reader["Частота"] != DBNull.Value ? Convert.ToInt32(reader["Частота"].ToString()) : 0,
                            Manufacturer = reader["Производитель"] != DBNull.Value ? reader["Производитель"].ToString() : "",
                            Lot = reader["Размещение"] != DBNull.Value ? reader["Размещение"].ToString() : "",

                        };

                        ramInfos.Add(ramInfo);
                    }
                }
            }

            return ramInfos;
        }

        private void RAMDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FillUsersUsageListBox(UsersListBox,RAMDate.SelectedDate.Value);
        }

        private List<ProcessorInfo> GetProcessorInfos(string biosSerialNumber, SqlConnection connection)
        {
            List<ProcessorInfo> processorInfos = new List<ProcessorInfo>();

            using (SqlCommand command = new SqlCommand("GetProcessorInfo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BIOS", biosSerialNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProcessorInfo processorInfo = new ProcessorInfo
                        {
                            Model = reader["Модель"] != DBNull.Value ? reader["Модель"].ToString() : "",
                            Architecture = reader["Архитектура"] != DBNull.Value ? reader["Архитектура"].ToString() : "",
                            LogicalProcessors = reader["Количество логичских процессов"] != DBNull.Value ? Convert.ToInt32(reader["Количество логичских процессов"]) : 0,
                            Cores = reader["Количество ядер"] != DBNull.Value ? Convert.ToInt32(reader["Количество ядер"]) : 0,
                            Manufacturer = reader["Производитель"] != DBNull.Value ? reader["Производитель"].ToString() : "",
                            Frequency = reader["Частота"] != DBNull.Value ? Convert.ToDouble(reader["Частота"]) : 0
                        };

                        processorInfos.Add(processorInfo);
                    }
                }
            }

            return processorInfos;
        }

        private List<VideoAdapterInfo> GetVideoAdapterInfos(string biosSerialNumber, SqlConnection connection)
        {
            List<VideoAdapterInfo> videoAdapterInfos = new List<VideoAdapterInfo>();

            using (SqlCommand command = new SqlCommand("GetVideoAdapter", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BIOS", biosSerialNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        VideoAdapterInfo videoAdapterInfo = new VideoAdapterInfo
                        {
                            Model = reader["Модель"] != DBNull.Value ? reader["Модель"].ToString() : "",
                            GraphicsProcessor = reader["Графический процессор"] != DBNull.Value ? reader["Графический процессор"].ToString() : "",
                            MemorySize = reader["Объём памяти"] != DBNull.Value ? Convert.ToInt64(reader["Объём памяти"]) : 0,
                            Manufacturer = reader["Производитель"] != DBNull.Value ? reader["Производитель"].ToString() : ""
                        };

                        videoAdapterInfos.Add(videoAdapterInfo);
                    }
                }
            }

            return videoAdapterInfos;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
          

        }

        private void Usage_Click(object sender, MouseButtonEventArgs e)
        {           
            //UsersListBox.Items.Clear();
            SetUsageVisibility();
        }
    }

    // Классы моделей для хранения информации о процессоре и видеоадаптере
    public class ProcessorInfo
    {
        public string Model { get; set; }
        public string Architecture { get; set; }
        public int LogicalProcessors { get; set; }
        public int Cores { get; set; }
        public string Manufacturer { get; set; }
        public double Frequency { get; set; }
    }
    public class UsageData
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
    public class RamInfo
    {
        public string Type { get; set; }
        public string Lot { get; set; }
        public int Speed { get; set; }
        public double Size { get; set; }
        public string Manufacturer { get; set; }
    }
    public class DriveInfo
    {
        public double Size { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public bool Pool { get; set; }
        public string Layout { get; set; }
    }

    public class VideoAdapterInfo
    {
        public string Model { get; set; }
        public string GraphicsProcessor { get; set; }
        public long MemorySize { get; set; }
        public string Manufacturer { get; set; }
    }


    // Класс ViewModel для отображения информации о процессоре и видеоадаптере
    public class HardwareInfoViewModel
    {
        public ObservableCollection<KeyValuePair<string, string>> ProcessorInfoTexts { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> VideoAdapterInfoTexts { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> DriveInfoTexts { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> RamInfoTexts { get; set; }

        public HardwareInfoViewModel(List<ProcessorInfo> processorInfos, List<VideoAdapterInfo> videoAdapterInfos, List<DriveInfo> driveInfos, List<RamInfo> ramInfos)
        {
            ProcessorInfoTexts = new ObservableCollection<KeyValuePair<string, string>>();
            VideoAdapterInfoTexts = new ObservableCollection<KeyValuePair<string, string>>();
            DriveInfoTexts = new ObservableCollection<KeyValuePair<string, string>>();
            RamInfoTexts = new ObservableCollection<KeyValuePair<string, string>>();

            foreach (var ramInfo in ramInfos)
            {
                RamInfoTexts.Add(new KeyValuePair<string, string>("Тип:", ramInfo.Type));
                RamInfoTexts.Add(new KeyValuePair<string, string>("Объём:", ramInfo.Size.ToString()));
                RamInfoTexts.Add(new KeyValuePair<string, string>("Частота:", ramInfo.Speed.ToString()));
                RamInfoTexts.Add(new KeyValuePair<string, string>("Размещение:", ramInfo.Lot));
                RamInfoTexts.Add(new KeyValuePair<string, string>("Производитель:", ramInfo.Manufacturer));
                DriveInfoTexts.Add(new KeyValuePair<string, string>("", "")); // Отступ между элементами
            }



            foreach (var driveInfo in driveInfos)
            {
                DriveInfoTexts.Add(new KeyValuePair<string, string>("Модель:", driveInfo.Model));
                DriveInfoTexts.Add(new KeyValuePair<string, string>("Пул:", driveInfo.Pool ? "Да" : "Нет"));
                DriveInfoTexts.Add(new KeyValuePair<string, string>("Тип:", driveInfo.Type));
                DriveInfoTexts.Add(new KeyValuePair<string, string>("Разметка:", driveInfo.Layout));
                DriveInfoTexts.Add(new KeyValuePair<string, string>("", "")); // Отступ между элементами
            }

            foreach (var processorInfo in processorInfos)
            {
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Модель:", processorInfo.Model));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Архитектура:", processorInfo.Architecture));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Логические процессы:", processorInfo.LogicalProcessors.ToString()));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Ядра:", processorInfo.Cores.ToString()));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Производитель:", processorInfo.Manufacturer));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("Частота:", processorInfo.Frequency.ToString()));
                ProcessorInfoTexts.Add(new KeyValuePair<string, string>("", "")); // Отступ между элементами
            }

            foreach (var videoAdapterInfo in videoAdapterInfos)
            {
                VideoAdapterInfoTexts.Add(new KeyValuePair<string, string>("Модель:", videoAdapterInfo.Model));
                VideoAdapterInfoTexts.Add(new KeyValuePair<string, string>("Графический процессор:", videoAdapterInfo.GraphicsProcessor));
                VideoAdapterInfoTexts.Add(new KeyValuePair<string, string>("Объем памяти:", videoAdapterInfo.MemorySize.ToString()));
                VideoAdapterInfoTexts.Add(new KeyValuePair<string, string>("Производитель:", videoAdapterInfo.Manufacturer));
                VideoAdapterInfoTexts.Add(new KeyValuePair<string, string>("", "")); // Отступ между элементами
            }
        }

    }


}
