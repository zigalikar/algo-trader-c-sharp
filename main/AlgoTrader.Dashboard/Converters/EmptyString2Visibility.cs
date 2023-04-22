using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace AlgoTrader.Dashboard.Converters
{
    public class EmptyString2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && string.IsNullOrWhiteSpace(str) == false)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
