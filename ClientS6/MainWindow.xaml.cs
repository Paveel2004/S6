using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using System.Dynamic;
using System.Diagnostics;

namespace ClientS6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        static void SerializeToJsonFile<T>(T obj, string filePath)
        {
            using(FileStream fs = File.Create(filePath))
            {
                JsonSerializer.SerializeAsync(fs, obj, new JsonSerializerOptions { WriteIndented = true }).Wait();

            }
        }

        private void Connection_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string jsonFilePath = @"C:\Users\ASUS\source\repos\ClientS6\ClientS6\bin\Debug\net6.0-windows\data.json",
                    exeDataCollectionPath = @"C:\Users\ASUS\source\repos\Data collection\Data collection\bin\Debug\net6.0\Data collection.exe";
                Process process = new Process();


                var data = new
                {
                    serverAddress = IP.Text
                };

                SerializeToJsonFile(data, jsonFilePath);
                process.StartInfo.FileName = exeDataCollectionPath;
                process.Start();
            
            }

            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }
        }
    }
}