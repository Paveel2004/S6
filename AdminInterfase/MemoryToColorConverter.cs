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
                return Brushes.Green;
            else if (memory >= 60 && memory < 70)
                return Brushes.Yellow;
            else if (memory >= 70 && memory < 90)
                return Brushes.Orange;
            else if (memory >= 90)
                return Brushes.Red;

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
