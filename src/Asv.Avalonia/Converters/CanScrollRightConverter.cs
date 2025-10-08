using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class CanScrollRightConverter : IMultiValueConverter
{
    public static CanScrollRightConverter Instance { get; } = new();

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (
            values.Count == 3
            && values[0] is double offsetX
            && values[1] is double viewportWidth
            && values[2] is double extentWidth
        )
        {
            // Проверка: можно ли скроллить вправо
            return offsetX + viewportWidth < extentWidth;
        }
        return false;
    }
}
