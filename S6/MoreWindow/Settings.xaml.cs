using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace S6.MoreWindow
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            try
            {
                ConnectionString.Text = DeserializeFromJsonFile<DataSettings>("data.json").connectionString;
                IP.Text = DeserializeFromJsonFile<DataSettings>("data.json").serverAddress.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла конфигурации: " + ex.Message);
            }
        }
        static void SerializeToJsonFile<T>(T obj, string filePath)
        {
            using (FileStream fs = File.Create(filePath))
            {
                JsonSerializer.SerializeAsync(fs, obj, new JsonSerializerOptions { WriteIndented = true }).Wait();
            }
        }

        static T DeserializeFromJsonFile<T>(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                return JsonSerializer.DeserializeAsync<T>(fs).Result;
            }
        }
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {

            DataSettings data = new DataSettings
            {
                serverAddress = IP.Text ?? " ",
                connectionString = ConnectionString.Text ?? " ",
            };

            try
            {
                SerializeToJsonFile(data, "data.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении файла конфигурации: " + ex.Message);
            }
        }
    }
}
