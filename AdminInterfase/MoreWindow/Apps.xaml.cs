using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Xceed.Words.NET;

namespace AdminInterfase.MoreWindow
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class Apps : Window
    {
        static ObservableCollection<string> items;
        public Apps(List<string> app,string name)
        {
            InitializeComponent();
            SetList(app, listBox);
            items = new ObservableCollection<string>(app);
            if (name == "Процессы")
            {
                Thread thread = new Thread(new ParameterizedThreadStart(UpDate));
                thread.IsBackground = true;
                thread.Start(listBox);
            }

        }
        public static void SetList(List<string> app, ListBox listBox)
        {
            listBox.Dispatcher.Invoke(() =>
            {
                listBox.ItemsSource = app;
                items = new ObservableCollection<string>(app);
            });
        }
        static void UpDate(object obj)
        {
            ListBox listBox = obj as ListBox;
            while (true)
            {
                var app = JsonConvert.DeserializeObject<List<string>>(MessageSender.SendMessage(ManagerIP.GetIPAddress(), 1111, "getProcesses"));
                listBox.Dispatcher.Invoke(() =>
                {
                    SetList(app, listBox);
                });
                Thread.Sleep(1000 - 7);
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = searchBox.Text.ToLower();
            listBox.ItemsSource = items.Where(item => item.ToLower().Contains(searchText));
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Word Document|*.docx";
            saveFileDialog.Title = "Сохранить документ Word";

            // Если пользователь выбрал файл для сохранения
            if (saveFileDialog.ShowDialog() == true)
            {
                // Создаем новый документ Word
                using (var doc = DocX.Create(saveFileDialog.FileName))
                {
                    // Добавляем данные из ListBox в документ Word
                    foreach (var item in listBox.Items)
                    {
                        doc.InsertParagraph(item.ToString());
                    }

                    // Сохраняем документ
                    doc.Save();
                }

                MessageBox.Show("Документ успешно сохранен!");
            }
        }





    }
}
