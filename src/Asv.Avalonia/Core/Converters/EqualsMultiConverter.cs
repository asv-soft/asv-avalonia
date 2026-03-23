using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class EqualsMultiConverter : IMultiValueConverter
{
    public static IMultiValueConverter Instance { get; } = new EqualsMultiConverter();

    public object Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (values.Count < 2)
        {
            return false;
        }

        return Equals(values[0], values[1]);
    }
}
