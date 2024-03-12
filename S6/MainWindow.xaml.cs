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
        string connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
        public MainWindow()
        {

            InitializeComponent();
            Menu.Items.Add(new MenuItem { Text = "Сборки устройств", ClickHandler = Devices_Click });
            Menu.Items.Add(new MenuItem { Text = "Использование устройств", ClickHandler = Usage_Click });
            Menu.Items.Add(new MenuItem { Text = "Отчёты"});
            Menu.Items.Add(new MenuItem { Text = "Настройки", ClickHandler = Settings_Click });
            string connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
            Task.Run(() => UpdateListBox());
            DataBaseHelper.connectionString = DeserializeFromJsonFile<DataSettings>("data.json").connectionString;

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
        private void Devices_Click(object sender, RoutedEventArgs e)
        {
            // Обработчик события для первой кнопки
            //HiddenAnalytics();
            //updateListBoxDelegate = new (DisplayDevices);
            Type.Visibility = Visibility.Hidden;
            Character.Visibility = Visibility.Hidden;
            Text1.Visibility = Visibility.Hidden;
            Text2.Visibility = Visibility.Hidden;
            Date.Visibility = Visibility.Hidden;
            prosmotr.Visibility = Visibility.Visible;
            pokazat.Visibility = Visibility.Hidden;
            delete.Visibility = Visibility.Visible;



        }
        private void Usage_Click(object sender, RoutedEventArgs e)
        {
            Type.Visibility = Visibility.Visible;
            Character.Visibility = Visibility.Visible;
            Text1.Visibility = Visibility.Visible;
            Text2.Visibility = Visibility.Visible;
            Date.Visibility = Visibility.Visible;
            prosmotr.Visibility = Visibility.Hidden;
            pokazat.Visibility = Visibility.Visible;
            delete.Visibility = Visibility.Hidden;


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

        private void Type_DropDownOpened(object sender, EventArgs e)
        {
            Type.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT [Тип характеристики] FROM [Типы характеристики]";
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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = $"EXECUTE ХарактерисикиТипа @Тип = '{Type.Text}'"; 
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

        private void pokazat_Click(object sender, RoutedEventArgs e)
        {

            string query = $"EXECUTE ИспользованиеПредставление @ТипХарактеристики = '{Type.Text}', @Характеристика = '{Character.Text}', @ИмяКомпьютера = '{ComputerName.Text}', @НачальнаяДата = '{StartData.SelectedDate}', @КонечнаДата = '{EndData.SelectedDate}'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                DataTable reversedTable = dataTable.Clone();

                for (int i = dataTable.Rows.Count - 1; i >= 0; i--)
                {
                    reversedTable.ImportRow(dataTable.Rows[i]);
                }

                // Устанавливаем новую таблицу в качестве источника данных
                dataGrid.ItemsSource = reversedTable.DefaultView;
            }


        }

        private void prosmotr_Click(object sender, RoutedEventArgs e)
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

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            DataBaseHelper.Query($"DELETE FROM Устройтво WHERE Имя = '{ComputerName.Text}'", connectionString);
        }
    }


}
