using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace S6
{
    internal class MenuItem
    {
        public string Text { get; set; }
        public RoutedEventHandler ClickHandler { get; set; }
    }
}
