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
        }
        static void SerializeToJsonFile<T>(T obj, string filePath)
        {
            using (FileStream fs = File.Create(filePath))
            {
                JsonSerializer.SerializeAsync(fs, obj, new JsonSerializerOptions { WriteIndented = true }).Wait();

            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            var data = new
            {
                port = int.Parse(PORT.Text),
                serverAddress = IP.Text
            };

            SerializeToJsonFile(data, "data.json");
        }
    }
}
