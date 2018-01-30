using System;
using System.Globalization;
using System.Windows.Data;

namespace LarkatorGUI
{
    public class AddIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse((string)parameter, out var b) && value is double a)
                return a + b;

            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
