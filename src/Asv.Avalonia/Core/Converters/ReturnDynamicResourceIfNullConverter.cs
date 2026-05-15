using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Asv.Avalonia;

public class ReturnDynamicResourceIfNullConverter : IValueConverter
{
    public static ReturnDynamicResourceIfNullConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter != null)
        {
            return value ?? new DynamicResourceExtension(parameter);
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
