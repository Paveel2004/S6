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
using Microsoft.Data.SqlClient;
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
using Server;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Windows.Shapes;
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
            Menu.Items.Add(new MenuItem { Text = "Сборки устройств", ClickHandler = Devices_Click });
            Menu.Items.Add(new MenuItem { Text = "Использование устройств", ClickHandler = Users_Click });
            Menu.Items.Add(new MenuItem { Text = "Аналитика и графики",  ClickHandler= Analytics_Click});
            Menu.Items.Add(new MenuItem { Text = "Отчёты"});
            Menu.Items.Add(new MenuItem { Text = "Настройки", ClickHandler = Settings_Click });

            Task.Run(() => UpdateListBox());
            DataBaseHelper.connectionString = DeserializeFromJsonFile<DataSettings>("data.json").connectionString;




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
       

        
        
        public void DisplayUserName()
        {
            
           /* Analytics_Users_ListBox_CPU.Items.Clear();
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
            }*/
        }
        public List<string> ExecuteQuery(string query, string connectionString)
        {
            var results = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Замените "ColumnName" на имя столбца, который вы хотите прочитать
                    results.Add(reader["ColumnName"].ToString());
                }

                reader.Close();
            }

            return results;
        }

        public void DisplayDevices()
        {
            Information_ListBox.Items.Clear();
            //для начала получить список устройств
            //Вывести в лист бокс несколько представлений

        }
        private void CreatePlot()
        {
     

        }
        public void DisplayDisk()
        {
            
        }
        public void DisplayOS()
        {
            
        }

        public void DisplayUsers()
        {
            
        }
        private void Devices_Click(object sender, RoutedEventArgs e)
        {
            // Обработчик события для первой кнопки
            //HiddenAnalytics();
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
        static T DeserializeFromJsonFile<T>(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                return System.Text.Json.JsonSerializer.DeserializeAsync<T>(fs).Result;
            }
        }

        private void ComputerName_DropDownOpened(object sender, EventArgs e)
        {
            ComputerName.Items.Clear();
            using (SqlConnection connection = new SqlConnection("Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True "))
            {
                connection.Open();
                string sqlQuery = "SELECT Имя FROM Устройтво"; // Замените на ваш SQL-запрос
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

        private void Type_DropDownOpened(object sender, EventArgs e)
        {
            Type.Items.Clear();
            using (SqlConnection connection = new SqlConnection("Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True "))
            {
                connection.Open();
                string sqlQuery = "SELECT [Тип характеристики] FROM [Типы характеристики]"; // Замените на ваш SQL-запрос
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string type = reader["Тип характеристики"].ToString();

                        Type.Items.Add(type);
                    }
                }
            }
        }

        private void Character_DropDownOpened(object sender, EventArgs e)
        {
            Character.Items.Clear();
            using (SqlConnection connection = new SqlConnection("Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True "))
            {
                connection.Open();
                string sqlQuery = $"EXECUTE ХарактерисикиТипа @Тип = '{Type.Text}'"; // Замените на ваш SQL-запрос
                SqlCommand cmd = new SqlCommand(sqlQuery, connection);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string type = reader["Название"].ToString();

                        Character.Items.Add(type);
                    }
                }
            }
        }
    }


}
