using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class ZeroDoubleIfFalseConverter : IValueConverter
{
    public static readonly ZeroDoubleIfFalseConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is false)
        {
            return 0.0;
        }
        
        // parse parameter as double
        if (parameter is double d)
        {
            return d;
        }
        return double.MaxValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}