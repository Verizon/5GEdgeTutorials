using System;
using System.Globalization;
using Humanizer;
using Xamarin.Forms;

namespace Wavelength.Converters
{
    public class DateTimeOffsetToHumanizerString
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.Humanize();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}