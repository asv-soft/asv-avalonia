using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class EnumToBooleanConverter : IValueConverter
{
    public static EnumToBooleanConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string str && value != null)
        {
            if (Enum.TryParse(value.GetType(), str, out var result))
            {
                parameter = result;
            }
        }

        return value?.Equals(parameter);
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
    }
}
