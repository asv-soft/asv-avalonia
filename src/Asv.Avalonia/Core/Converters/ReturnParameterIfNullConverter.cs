using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class ReturnParameterIfNullConverter : IValueConverter
{
    public static ReturnParameterIfNullConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value ?? parameter;
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
