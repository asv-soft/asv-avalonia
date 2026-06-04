using System.Collections;
using System.Globalization;
using Asv.Common;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public class ValidationErrorsToStringConverter : IValueConverter
{
    public static ValidationErrorsToStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var separator = parameter?.ToString() ?? Environment.NewLine;
        var errors = EnumerateErrors(value)
            .Select(GetErrorText)
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var text = string.Join(separator, errors);

        return string.IsNullOrWhiteSpace(text) ? null : text;
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

    private static IEnumerable<object?> EnumerateErrors(object? value)
    {
        if (value is null)
        {
            yield break;
        }

        if (value is string)
        {
            yield return value;
            yield break;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }

            yield break;
        }

        yield return value;
    }

    private static string? GetErrorText(object? error)
    {
        return error switch
        {
            null => null,
            LocalizedException { LocalizedMessage: { Length: > 0 } localizedMessage } =>
                localizedMessage,
            Exception { Message: { Length: > 0 } message } => message,
            _ => error.ToString(),
        };
    }
}
