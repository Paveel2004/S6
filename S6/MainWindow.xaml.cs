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
using OxyPlot.Wpf;
using OxyPlot;
using OxyPlot.Series;
using System.Data;
using OxyPlot.Axes;
using System.Globalization;

namespace S6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isAnalytigsVisible = false;
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
            Menu.Items.Add(new MenuItem { Text = "Аналитика и графики",  ClickHandler= Analytics_Click});
            Menu.Items.Add(new MenuItem { Text = "Настройки", ClickHandler = Settings_Click });

            Task.Run(() => UpdateListBox());


        }
        private void HiddenAnalytics()
        {
            Analytics_and_graphs.Visibility = Visibility.Hidden;
            isAnalytigsVisible = false;
        }
        private void Analytics_Click(object sender, EventArgs e)
        {
            isAnalytigsVisible = !isAnalytigsVisible;
            if (isAnalytigsVisible ) 
                Analytics_and_graphs.Visibility = Visibility.Visible;
            else
                Analytics_and_graphs.Visibility = Visibility.Hidden;
            DisplayUserName();
            CreatePlot();
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
            HiddenAnalytics();
            updateListBoxDelegate = new (DisplayDisk);
        }
        private void OS_Click(object sender, RoutedEventArgs e)
        {
            HiddenAnalytics();
            updateListBoxDelegate = new (DisplayOS);

        }

        //private ConcurrentQueue<string> messagesQueue = new ConcurrentQueue<string>();
       
        static void Query(string query, string connecrionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connecrionString))
            {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    connection.Close();
            }
        }
        
        
        public void DisplayUserName()
        {
            Analytics_Users_ListBox_CPU.Items.Clear();
            using(SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using(SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM UserName",connection))
                    {
                        using(SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Analytics_Users_ListBox_CPU.Items.Add(reader["Имя пользователя"].ToString());
                                Analytics_Users_ListBox_RAM.Items.Add(reader["Имя пользователя"].ToString());
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
        private void CreatePlot()
        {
            var RAM = new List<DataPoint>();

            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM RAM_History", connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double load = Convert.ToDouble(reader["Загруженность"]);
                                string dateTimeString = reader["Дата и время"].ToString();
                                DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null);
                                RAM.Add(new DataPoint(DateTimeAxis.ToDouble(dateTime), load));
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
            var plotModeRAM = new PlotModel { Title = "Оперативная память" };
            var lineRAM = new LineSeries
            {
                Title = "Нагрузка",
                ItemsSource = RAM

            };
            plotModeRAM.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",

            });
            plotModeRAM.Series.Add(lineRAM);
            OxyRAM.Model = plotModeRAM;





            var dataPointsUsage = new List<DataPoint>();
            var dataPointsTemp = new List<DataPoint>();
            using (SQLiteConnection connection = new SQLiteConnection(DataBaseHelper.connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM CPU_History",connection))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double temp = Convert.ToDouble(reader["Температура"]);
                                double load = Convert.ToDouble(reader["Загруженность"]);
                                string dateTimeString = reader["Дата и время"].ToString();
                                DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null);
                                dataPointsUsage.Add(new DataPoint(DateTimeAxis.ToDouble(dateTime), load));
                                dataPointsTemp.Add(new DataPoint(DateTimeAxis.ToDouble(dateTime), temp));
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
            var lineSeries = new LineSeries
            {
                Title = "Нагрузка",
                ItemsSource = dataPointsUsage

            };
            var lineTemps = new LineSeries
            {
                Title = "Температура",
                ItemsSource = dataPointsTemp

            };

            var plotModel = new PlotModel{ Title = "Процессор" };
            plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                
            });
            plotModel.Series.Add(lineSeries);
            plotModel.Series.Add(lineTemps);
            OxyCPU.Model = plotModel;
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
            HiddenAnalytics();
            updateListBoxDelegate = new (DisplayDevices);
        }
        private void Users_Click(object sender, RoutedEventArgs e)
        {
            HiddenAnalytics();
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

        private void Analytics_Users_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(Analytics_Users_ListBox_CPU.SelectedValue.ToString());
        }
    }

}
