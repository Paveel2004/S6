using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AdminInterfase.MoreWindow
{
    /// <summary>
    /// Логика взаимодействия для Control.xaml
    /// </summary>
    public partial class Control : Window
    {
        public Control()
        {
            InitializeComponent();
        }
        private static string path = @"C:\Windows\System32\drivers\etc\hosts";
        public static void BlockedWebsite(StringBuilder address)
        {
            StringBuilder record = new("127.0.0.1 ");
            record.Append(address.ToString());

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(record);
            }
        }
        public static void UnBlockedWebSite(string address)
        {
            var lines = File.ReadAllLines(path).ToList();
            lines.RemoveAll(line => line.Contains(address));
            File.WriteAllLines(path, lines);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Block_Click(object sender, RoutedEventArgs e)
        {
            BlockedWebsite(new StringBuilder(site.Text));
        }

        private void UnBlock_Click(object sender, RoutedEventArgs e)
        {
            UnBlockedWebSite(site.Text);
        }
    }
}
