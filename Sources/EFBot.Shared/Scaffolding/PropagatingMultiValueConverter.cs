using System;
using System.Globalization;
using System.Windows.Data;

namespace EFBot.Shared.Scaffolding
{
    public sealed class PropagatingMultiValueConverter : IMultiValueConverter
    {
        public static PropagatingMultiValueConverter Instance = new PropagatingMultiValueConverter();
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}