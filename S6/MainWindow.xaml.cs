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
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Win32;
using System.Windows.Media.Converters;

namespace S6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Settings settings = new Settings();
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
            Menu.Items.Add(new MenuItem { Text = "Окна", ClickHandler = Window_Click });
            Menu.Items.Add(new MenuItem { Text = "Сейчас", ClickHandler = Now });
            Menu.Items.Add(new MenuItem { Text = "Выгрузить данные", ClickHandler = DataExport });
            Menu.Items.Add(new MenuItem { Text = "Настройки", ClickHandler = Settings_Click });
            string connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";
            //Task.Run(() => UpdateListBox());
            Thread _thread;
            _thread = new Thread(UpdateData);
            _thread.IsBackground = true;
            _thread.Start();
         
            DataBaseHelper.connectionString = DeserializeFromJsonFile<DataSettings>("data.json").connectionString;
            //ShowDataOnGraph();

        }
        public void ShowDataOnGraph()
        {
            var plotModel = new PlotModel { Title = "Данные" };

            // Ось времени
            plotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "HH:mm:ss" });

            // Ось данных
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            // Серия данных
            var series = new LineSeries();
            plotModel.Series.Add(series);

            string connectionString = "Data Source = DESKTOP-LVEJL0B\\SQLEXPRESS;Initial Catalog=S6;Integrated Security=true;TrustServerCertificate=True ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand($"SELECT [Дата/Время], Значение FROM ИспользованиеУстройстваВЦелом WHERE [Тип характеристики] = '{Type.Text}' AND [Имя компьютера] = '{ComputerName.Text}' AND Название = '{Character.Text}' ORDER BY [Дата/Время] DESC;", connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime dateTime = reader.GetDateTime(0);
                         double value = double.Parse(reader.GetString(1));

                        // Добавляем точку на график
                        series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dateTime), value));
                    }
                }
            }

            this.Ethernet.Model = plotModel;
        }

        public void DataExport(object sender, RoutedEventArgs e)
        {
            // Создаем диалог выбора файла
           SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;

                // Выгрузка данных


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand($"SELECT [Дата/Время], [Тип характеристики], Значение\r\n\tFROM ИспользованиеУстройстваВЦелом\r\n\tWHERE [Тип характеристики] = 'Клавиатура' AND [Имя компьютера] = '{ComputerName.Text}' AND Название = 'Символ' ORDER BY [Дата/Время] DESC;", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        using (StreamWriter writer = new StreamWriter(fileName))
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    writer.Write(reader.GetValue(i));

                                    if (i < reader.FieldCount - 1)
                                        writer.Write("\t");
                                }

                                writer.WriteLine();
                            }
                        }
                    }
                }

                MessageBox.Show("Данные успешно экспортированы в файл " + fileName);
            }
        }

        private double Now(string Type, string Character)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("АктуальноеЗначение", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.AddWithValue("@ТипХарактеристики", Type);
                    command.Parameters.AddWithValue("@Характеристика", Character);
                    command.Parameters.AddWithValue("@ИмяПК", $"{ComputerName.Text}");

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // Предполагается, что у вас есть TextBox с именем textBox1
                           return double.Parse(reader[2].ToString());
                        }
                    }

                    reader.Close();
                }

                connection.Close();
            }
            return 0;
        }
        private void UpdateData()
        {
            while (true)
            {
                // Обновление данных
                Application.Current.Dispatcher.Invoke(() =>
                {

                    ValueRAM.Content = Now("ОЗУ", "Загруженность");
                    ValueFreeSpace.Content = Now("Диск", "Свободное место");
                    ValueUsageCPU.Content = Now("Процессор", "Загруженность");
                    ValueTemperatureCPU.Content = Now("Процессор", "Температура");
                    ValueEthernrtSpeed.Content = Now("Ethernet", "Скорость");
                    //ShowDataOnGraph();

            });

                // Задержка перед следующим обновлением
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
        private void VisibilityWindow()
        {
            listBox.Visibility = Visibility.Visible;
            ComputerName.Visibility = Visibility.Visible;
        }
        private void Window_Click(object sender, EventArgs e)
        {
            HiddenAllInterfaseItems();
            VisibilityWindow();
            string query = $"SELECT Использование.[Серийный номер BIOS], Значение, " +
                $"[Дата/Время], Название, [Тип характеристики], Устройтво.Имя AS \"Имя компьютера\"" +
                $" FROM [Использование]\r\nJOIN [Динамические характеристики] ON [ID Характеристики " +
                $"динамической] = [Динамические характеристики].ID \r\nJOIN [Типы характеристики] ON [ID Тип" +
                $"а зарактеристики] = [Типы характеристики].ID\r\nJOIN Устройтво ON Устройтво.[Серийный номер BIOS" +
                $"] = Использование.[Серийный номер BIOS] WHERE [ID Характеристики динамической] = 1008 AND Имя = '{ComputerName.Text}' " +
                $"AND [Дата/Время] = '{DateTime.Now}'";

            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectionString);
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            listBox.Items.Clear();
            foreach (DataRow row in dataTable.Rows)
            {
                listBox.Items.Add($"{row["Значение"]}");
            }
            listBox.Visibility = Visibility.Visible;
        }
        private void Settings_Click(object sender, EventArgs e) => settings.ShowDialog();
        private void HiddenAllInterfaseItems()
        {
            Type.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Hidden;
            pokazat.Visibility = Visibility.Hidden;
            Text1.Visibility = Visibility.Hidden;
            Text2.Visibility = Visibility.Hidden;
            delete.Visibility = Visibility.Hidden;
            dataGrid.Visibility = Visibility.Hidden;
            Character.Visibility = Visibility.Hidden;
            Date.Visibility = Visibility.Hidden;
            prosmotr.Visibility = Visibility.Hidden;
            TextNow1.Visibility = Visibility.Hidden;
            TextNow2.Visibility = Visibility.Hidden;
            TextNow3.Visibility = Visibility.Hidden;
            TextNow4.Visibility = Visibility.Hidden;
            TextNow5.Visibility = Visibility.Hidden;
            ValueRAM.Visibility = Visibility.Hidden;            
            ValueUsageCPU.Visibility = Visibility.Hidden;
            ValueTemperatureCPU.Visibility = Visibility.Hidden;
            ValueFreeSpace.Visibility = Visibility.Hidden;
            ValueEthernrtSpeed.Visibility = Visibility.Hidden;
            Ethernet.Visibility = Visibility.Hidden;

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



        private void Now(object sender, RoutedEventArgs e)
        {
            HiddenAllInterfaseItems();
            VisibilityNow();
            Ethernet.Visibility = Visibility.Visible;

        }
        private void VisibilityNow()
        {
            TextNow1.Visibility = Visibility.Visible;
            TextNow2.Visibility = Visibility.Visible;
            TextNow3.Visibility = Visibility.Visible;
            TextNow4.Visibility = Visibility.Visible;
            TextNow5.Visibility = Visibility.Visible;
            ValueRAM.Visibility = Visibility.Visible;
            ValueUsageCPU.Visibility = Visibility.Visible;
            ValueTemperatureCPU.Visibility = Visibility.Visible;
            ValueFreeSpace.Visibility = Visibility.Visible;
            ValueEthernrtSpeed.Visibility = Visibility.Visible;

            Type.Visibility = Visibility.Visible;
            Character.Visibility = Visibility.Visible;
            Text1.Visibility = Visibility.Visible;
            Text2.Visibility = Visibility.Visible;
            Date.Visibility = Visibility.Visible;

        }
        public void VisibilityDevices()
        {
            prosmotr.Visibility = Visibility.Visible;
            delete.Visibility = Visibility.Visible;
            dataGrid.Visibility = Visibility.Visible;
        }
        public void VisibilityUsage()
        {
            Type.Visibility = Visibility.Visible;
            Character.Visibility = Visibility.Visible;
            Text1.Visibility = Visibility.Visible;
            Text2.Visibility = Visibility.Visible;
            Date.Visibility = Visibility.Visible;
            pokazat.Visibility = Visibility.Visible;
            listBox.Visibility = Visibility.Visible;
        }
        private void Devices_Click(object sender, RoutedEventArgs e)
        {
            HiddenAllInterfaseItems();
            VisibilityDevices();
        }
        private void Usage_Click(object sender, RoutedEventArgs e)
        {
            HiddenAllInterfaseItems();
            VisibilityUsage();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
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
        public void FillComboBoxFromProcedure(ComboBox comboBox, string characteristic, string type)
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

                listBox.Items.Clear();
                foreach (DataRow row in dataTable.Rows)
                {
                    listBox.Items.Add($"{row["Дата/Время"]}  —  {row["Значение"]}");
                }
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

        private void ComputerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }


}
