using System;
using System.Windows.Data;
using System.Globalization;

namespace AlgoTrader.Dashboard.Converters
{
    public class Date2AxisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime && targetType == typeof(double))
                return ((DateTime) value).Ticks / 10000000000.0;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
