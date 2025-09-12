using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Asv.Avalonia.Example;

public class PathToFileNameConverter : IValueConverter
{
    public static IValueConverter Instance { get; } = new PathToFileNameConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return Path.GetFileName(path);
        }

        return string.Empty;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
