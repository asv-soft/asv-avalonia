using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class CoalesceConverter : IMultiValueConverter
{
    public static CoalesceConverter Instance { get; } = new();

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return values.FirstOrDefault(v => v != null && v != BindingOperations.DoNothing);
    }
}
