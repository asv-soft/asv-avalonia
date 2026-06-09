using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class RttBoxToolTipConverter : IMultiValueConverter
{
    public static RttBoxToolTipConverter Instance { get; } = new();

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        var header = GetString(values, 0);
        var description = GetString(values, 1);

        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(header)
            ? description
            : $"{header}{Environment.NewLine}{description}";
    }

    private static string? GetString(IList<object?> values, int index)
    {
        if (values.Count <= index)
        {
            return null;
        }

        return values[index] as string;
    }
}
