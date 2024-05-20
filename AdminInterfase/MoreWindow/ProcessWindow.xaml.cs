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
using Microsoft.Win32;
using System.Runtime.Intrinsics.Arm;
using GlobalClass;
using OfficeOpenXml;
using System.IO;

namespace AdminInterfase
{
    /// <summary>
    /// Логика взаимодействия для ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        private double totalRAM;
        private string userName;
        private string computerName;
        private string ip;
        private class ThreadArgs
        {
            public ListBox ListBox { get; set; }
            public string Ip { get; set; }
            public TextBox SearchBox { get; set; }
            public ComboBox SortOrder { get; set; }
        }
        
        public ProcessWindow(List<ProcessInfo> processInfoList, string ip)
        {
            InitializeComponent();
            SetList(processInfoList, listBox);
            items = new ObservableCollection<ProcessInfo>(processInfoList);

            Thread thread = new Thread(new ParameterizedThreadStart(UpDate));
            thread.IsBackground = true;
            thread.Start(new ThreadArgs { ListBox = listBox, Ip = ip, SearchBox = searchBox,  SortOrder = sortOrder });

            this.totalRAM = double.Parse(MessageSender.SendMessage(ip, 1111, "getTotalRAM"));
            this.userName = MessageSender.SendMessage(ip, 1111, "getUserName");
            this.computerName = MessageSender.SendMessage(ip, 1111, "getComputerName");
            this.ip = ip;
        }

        static ObservableCollection<ProcessInfo> items;


        public static void SetList(List<ProcessInfo> processInfoList, ListBox listBox)
        {
            listBox.Dispatcher.Invoke(() =>
            {
                listBox.ItemsSource = processInfoList;
                items = new ObservableCollection<ProcessInfo>(processInfoList);
            });
            
        }
        // Обработчик событий для кнопки "Окна"
        private void WindowsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Фильтрация списка процессов, где title != "—"
            typeFilter = "Окна";
        }

        // Обработчик событий для кнопки "Фоновые"
        private void BackgroundRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            typeFilter = "Фоновые";
        }
        private static string typeFilter = "Все";
        // Обработчик событий для кнопки "Все"
        private void AllRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Показ всех процессов
            typeFilter = "Все";
        }
        private static void SetTypeFilter(string type, ListBox listBox, TextBox searchBox, ComboBox sortOrder)
        {
            var searchText = searchBox.Text.ToLower();
            IEnumerable<ProcessInfo> filteredItems;

            switch (type)
            {
                case "Окна":
                    filteredItems = items.Where(item => item.WindowTitle != "—" && (item.ProcessName.ToLower().Contains(searchText) || item.WindowTitle.ToLower().Contains(searchText)));
                    break;
                case "Все":
                    filteredItems = items.Where(item => item.ProcessName.ToLower().Contains(searchText) || item.WindowTitle.ToLower().Contains(searchText));
                    break;
                case "Фоновые":
                    filteredItems = items.Where(item => item.WindowTitle == "—" && (item.ProcessName.ToLower().Contains(searchText) || item.WindowTitle.ToLower().Contains(searchText)));
                    break;
                default:
                    filteredItems = items;
                    break;
            }

            // Сортировка в зависимости от выбранного значения в ComboBox
            switch (sortOrder.SelectedIndex)
            {
                case 0: // Сначала более затратные
                    filteredItems = filteredItems.OrderByDescending(item => item.MemoryUsageMB);
                    break;
                case 1: // Сначала менее затратные
                    filteredItems = filteredItems.OrderBy(item => item.MemoryUsageMB);
                    break;
            }

            listBox.ItemsSource = filteredItems;
        }


        /*private void ExportButton_Click(object sender, RoutedEventArgs e)
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
                    foreach (ProcessInfo item in listBox.Items)
                    {
                        doc.InsertParagraph(item.ToString());
                    }

                    // Сохраняем документ
                    doc.Save();
                }

                MessageBox.Show("Документ успешно сохранен!");
            }
        }
*/
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Workbook|*.xlsx";
            saveFileDialog.Title = "Сохранить документ Excel";

            // Если пользователь выбрал файл для сохранения
            if (saveFileDialog.ShowDialog() == true)
            {
                // Создаем новый документ Excel
                using (var pck = new ExcelPackage())
                {
                    var ws = pck.Workbook.Worksheets.Add("Процессы");

                    // Добавляем информацию о компьютере, пользователе, IP-адресе и времени создания отчёта
                    ws.Cells["A1"].Value = "Имя компьютера:";
                    ws.Cells["B1"].Value = computerName;

                    ws.Cells["A2"].Value = "Имя пользователя:";
                    ws.Cells["B2"].Value = userName;

                    ws.Cells["A3"].Value = "IP адрес:";
                    ws.Cells["B3"].Value = ip;

                    ws.Cells["A4"].Value = "Время создания отчёта:";
                    ws.Cells["B4"].Value = DateTime.Now.ToString();

                    // Добавляем заголовки
                    ws.Cells[6, 1].Value = "Процесс";
                    ws.Cells[6, 2].Value = "Название окна";
                    ws.Cells[6, 3].Value = "Оперативная память (MB)";

                    // Форматируем заголовки
                    using (var range = ws.Cells[6, 1, 6, 3])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Добавляем данные из ListBox в документ Excel
                    int row = 7;
                    foreach (ProcessInfo item in listBox.Items)
                    {
                        ws.Cells[row, 1].Value = item.ProcessName;
                        ws.Cells[row, 2].Value = item.WindowTitle;
                        ws.Cells[row, 3].Value = item.MemoryUsageMB;

                        // Определение цвета ячейки в зависимости от использования оперативной памяти
                        double memoryUsagePercentage = (item.MemoryUsageMB / totalRAM) * 100;

                        if (memoryUsagePercentage <= 1.5)
                        {
                            ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                        }
                        else if (memoryUsagePercentage <= 6.25)
                        {
                            ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        else if (memoryUsagePercentage <= 15)
                        {
                            ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                        }
                        else
                        {
                            ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                        }

                        row++;
                    }

                    // Авторазмер колонок
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    // Установка рамки для всего диапазона данных
                    var dataRange = ws.Cells[6, 1, row - 1, 3];
                    dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    // Сохраняем документ
                    var fileInfo = new FileInfo(saveFileDialog.FileName);
                    pck.SaveAs(fileInfo);
                }

                MessageBox.Show("Документ Excel успешно сохранен!");
            }
        }






        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
          
        }

        static void UpDate(object obj)
        {
            ThreadArgs args = obj as ThreadArgs;
            ListBox listBox = args.ListBox;
            TextBox searchBox = args.SearchBox;
            ComboBox sortOrder = args.SortOrder;
            string ip = args.Ip;
            while (true)
            {
                var processInfoList = JsonConvert.DeserializeObject<List<ProcessInfo>>(MessageSender.SendMessage(ip, 1111, "getProcesses"));
                listBox.Dispatcher.Invoke(() =>
                {

                    SetList(processInfoList, listBox);
                    SetTypeFilter(typeFilter, listBox, searchBox, sortOrder);

                });
                Thread.Sleep(1000 - 7);
            }
        }
    }

}
