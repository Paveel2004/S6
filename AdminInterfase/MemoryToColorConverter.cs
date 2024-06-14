using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
namespace AdminInterfase
{
    public class MemoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double memory = (double)value;
            if (memory < 50)
                return new SolidColorBrush(Color.FromRgb(0, 100, 0)); // DarkGreen
            else if (memory >= 60 && memory < 70)
                return new SolidColorBrush(Color.FromRgb(139, 139, 0)); // DarkYellow
            else if (memory >= 70 && memory < 90)
                return new SolidColorBrush(Color.FromRgb(255, 140, 0)); // DarkOrange
            else if (memory >= 90)
                return new SolidColorBrush(Color.FromRgb(139, 0, 0)); // DarkRed

            return Brushes.Black;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
