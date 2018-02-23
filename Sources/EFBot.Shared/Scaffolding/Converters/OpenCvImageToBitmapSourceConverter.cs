using System;
using System.Globalization;
using System.Windows.Data;
using Emgu.CV;

namespace EFBot.Shared.Scaffolding.Converters
{
    public sealed class OpenCvImageToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = value as IImage;
            return img?.ToBitmapSource();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}