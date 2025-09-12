using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia.Example;

public class StringToHotKeyConverter : IValueConverter
{
    public static IValueConverter Instance { get; } = new StringToHotKeyConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            return HotKeyInfo.Parse(s);
        }

        return null;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is HotKeyInfo hk)
        {
            return hk.ToString();
        }

        return null;
    }
}
