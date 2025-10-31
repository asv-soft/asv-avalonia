using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia.Example;

public class ConfirmationStatusToBoolConverter : IValueConverter
{
    public static IValueConverter Instance { get; } = new ConfirmationStatusToBoolConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ConfirmationStatus status)
        {
            return status switch
            {
                ConfirmationStatus.Yes => true,
                ConfirmationStatus.No => false,
                _ => null,
            };
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
        return value switch
        {
            true => ConfirmationStatus.Yes,
            false => ConfirmationStatus.No,
            _ => ConfirmationStatus.Undefined,
        };
    }
}
