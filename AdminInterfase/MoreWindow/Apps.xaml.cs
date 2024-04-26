using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class Apps : Window
    {
        public Apps(List<string> app,string name)
        {
            InitializeComponent();
            this.Title = name;
            listBox.ItemsSource = app;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

   
        }



    }
}
