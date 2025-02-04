using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class StringJoinConverter : IValueConverter
{
    public static StringJoinConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string[] arr)
        {
            return string.Join(parameter?.ToString() ?? ", ", arr);
        }

        return value;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
