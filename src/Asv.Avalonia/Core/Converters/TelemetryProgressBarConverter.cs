using System.Globalization;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public enum TelemetryProgressBarConverterMode
{
    IsVisible,
    IsIndeterminate,
    Value,
}

public sealed class TelemetryProgressBarConverter : IValueConverter
{
    public static TelemetryProgressBarConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var mode = GetMode(parameter);
        var progress = GetProgress(value);

        return mode switch
        {
            TelemetryProgressBarConverterMode.IsVisible => !double.IsNaN(progress),
            TelemetryProgressBarConverterMode.IsIndeterminate => double.IsInfinity(progress),
            TelemetryProgressBarConverterMode.Value => double.IsFinite(progress) ? progress : 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null),
        };
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

    private static TelemetryProgressBarConverterMode GetMode(object? parameter)
    {
        if (parameter is TelemetryProgressBarConverterMode mode)
        {
            return mode;
        }

        if (
            parameter is string text
            && Enum.TryParse<TelemetryProgressBarConverterMode>(text, true, out mode)
        )
        {
            return mode;
        }

        throw new InvalidOperationException(
            $"{nameof(TelemetryProgressBarConverter)} requires a {nameof(TelemetryProgressBarConverterMode)} parameter."
        );
    }

    private static double GetProgress(object? value)
    {
        if (value is double progress)
        {
            return progress;
        }

        if (value is null)
        {
            return double.NaN;
        }

        throw new InvalidOperationException(
            $"Cannot convert value of type {value.GetType()} to progress."
        );
    }
}
