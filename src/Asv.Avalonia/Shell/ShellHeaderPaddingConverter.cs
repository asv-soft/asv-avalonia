using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public sealed class ShellHeaderPaddingConverter : IMultiValueConverter
{
    public static IMultiValueConverter Instance { get; } = new ShellHeaderPaddingConverter();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var extraWidth = values.Count == 2 && values[1] is int count && count > 0
            && values[0] is double width && double.IsFinite(width) ? Math.Max(0, width) + 8 : 0;
        return new Thickness(100, 0, 210 + extraWidth, 0);
    }
}
