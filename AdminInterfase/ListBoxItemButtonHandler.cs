using AdminInterfase.MoreWindow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Text.RegularExpressions;
using AdminInterfase.MoreWindow;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using System.Management;
using GlobalClass;

namespace AdminInterfase
{
    internal static class ListBoxItemButtonHandler
    {


        public static void Show_Details(object sender)
        {
            // Получение кнопки, которую нажали
            Button button = sender as Button;

            // Получение StackPanel, содержащего кнопки
            StackPanel buttonPanel = button.Parent as StackPanel;
            StackPanel buttons = buttonPanel.Children.OfType<StackPanel>().First();

            // Получение TextBlock с дополнительным текстом

            TextBlock additionalText = buttonPanel.Children.OfType<TextBlock>().First(t => t.Name == "AdditionalText");
            TextBlock usersList = buttonPanel.Children.OfType<TextBlock>().First(t => t.Name == "UsersList");

            var item = button.DataContext;

            // Преобразуем DataContext в нужный тип
            var myItem = item as ListBoxInfo; // замените MyItemType на тип вашего элемента

            // Изменение видимости кнопок и дополнительного текста
            if (buttons.Visibility == Visibility.Collapsed)
            {
                buttons.Visibility = Visibility.Visible;
                additionalText.Visibility = Visibility.Visible;
                usersList.Visibility = Visibility.Visible;

                foreach (string ip in FindIPAddresses(myItem.Text))
                {
                    additionalText.Text = TextHelper.DictionaryToText(MessageSender.SendMessage(ip, 1111, "AdditionalInformation"));
                    usersList.Text = $"Пользователи: {TextHelper.DeserializeJsonArrayAndReturnElements(MessageSender.SendMessage(ip, 1111, "getUsers"))} \n";
                }

                // Установка значения для дополнительного текста
                button.Content = "Скрыть";
            }
            else
            {
                buttons.Visibility = Visibility.Collapsed;
                additionalText.Visibility = Visibility.Collapsed;
                usersList.Visibility = Visibility.Collapsed;
                button.Content = "Подробнее";
            }
        }
        public static void OpenControlWindow(object sender)
        {
            var button = (Button)sender;
            var item = button.DataContext;
            var myItem = item as ListBoxInfo;
            var ipAddresses = FindIPAddresses(myItem.Text);

            foreach (string ip in ipAddresses)
            {
                MoreWindow.Control control = new MoreWindow.Control(ip);
                control.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                control.Owner = Application.Current.MainWindow;
                control.ShowDialog();
            }
        }
        public static IEnumerable<string> FindIPAddresses(string text)
        {
            var regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            var matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
        public static void ShowProcess(object sender)
        {
            var button = (Button)sender;

            // DataContext кнопки содержит данные элемента
            var item = button.DataContext;

            // Преобразуем DataContext в нужный тип
            var myItem = item as ListBoxInfo;

            // Регулярное выражение для поиска IP-адресов
            var ipAddresses = FindIPAddresses(myItem.Text);

            foreach (string ip in ipAddresses)
            {
                ProcessWindow process = new(JsonConvert.DeserializeObject<List<ProcessInfo>>(MessageSender.SendMessage(ip, 1111, "getProcesses")), ip);
                process.Show();
            }

        }
        public static void ShowApps(object sender, string command, string title)
        {
            // Получаем кнопку, на которую нажали
            var button = (Button)sender;

            // DataContext кнопки содержит данные элемента
            var item = button.DataContext;

            // Преобразуем DataContext в нужный тип
            var myItem = item as ListBoxInfo;

            // Регулярное выражение для поиска IP-адресов
            var ipAddresses = FindIPAddresses(myItem.Text);
            
            // Выводим все найденные IP-адреса
            foreach (string ip in ipAddresses)
            {
                Apps app = new(JsonConvert.DeserializeObject<List<string>>(MessageSender.SendMessage(ip, 1111, command)), title,ip  );
                app.Show();
            }
        }
    }
}
