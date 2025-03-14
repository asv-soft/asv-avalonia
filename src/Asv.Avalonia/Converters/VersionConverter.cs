using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia
{
    public class VersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string version)
            {
                int plusIndex = version.IndexOf('+');
                if (plusIndex >= 0)
                {
                    return version.Substring(0, plusIndex);
                }
                
                return version;
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}