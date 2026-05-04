using System.Globalization;
using Avalonia.Data.Converters;
using DotNext.Patterns;

namespace Asv.Avalonia;

public class DoubleTrueIfNanConverter : ISingleton<DoubleTrueIfNanConverter>, IValueConverter
{
    public static DoubleTrueIfNanConverter Instance { get; } = new DoubleTrueIfNanConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return double.IsNaN(d) ? true : false;
        }

        if (value is null)
        {
            return false;
        }

        throw new InvalidOperationException(
            $"Cannot convert value of type {value.GetType()} to boolean."
        );
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
