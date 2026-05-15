using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia
{
    public class VersionConverter : IValueConverter
    {
        private string _versionPart = string.Empty;

        public object? Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            if (value is not string version)
            {
                return value;
            }

            var plusIndex = version.IndexOf('+');
            return plusIndex >= 0 ? version[..plusIndex] : version;
        }

        public object? ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture
        )
        {
            if (value is string version)
            {
                return version + _versionPart;
            }

            return value;
        }
    }
}
