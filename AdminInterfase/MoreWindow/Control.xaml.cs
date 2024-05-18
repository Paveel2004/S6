using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private string computerAdress;
        public Control(string computerAddress)
        {
            this.computerAdress = computerAddress;
            InitializeComponent();
        }
        private static string path = @"C:\Windows\System32\drivers\etc\hosts";
        


        
        private void Block_Click(object sender, RoutedEventArgs e)
        {
            MessageSender.SendMessage(computerAdress, 1111, $"blockSite [{site.Text}]");
        }

        private void UnBlock_Click(object sender, RoutedEventArgs e)
        {
            MessageSender.SendMessage(computerAdress, 1111, $"unBlockSite [{site.Text}]");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
