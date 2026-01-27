using System.Composition;
using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(TimeSpanBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class HmsTimeSpanUnit() : UnitItemBase(1)
{
    public const string Id = $"{TimeSpanBase.Id}.hour-minute-second";

    public override string UnitItemId => Id;
    public override string Name => RS.HourMinuteSecond_UnitItem_Name;
    public override string Description => RS.HourMinuteSecond_TimeSpan_Description;
    public override string Symbol => RS.HourMinuteSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    private const NumberStyles TimeStyle =
        NumberStyles.AllowDecimalPoint
        | NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite;

    public override bool IsValid(string? value) => TryParse(value, out _);

    public override ValidationResult ValidateValue(string? value)
    {
        try
        {
            Parse(value, CultureInfo.InvariantCulture);
            return ValidationResult.Success;
        }
        catch (ValidationException ex)
        {
            return new UnitException(ex.Message, ex.LocalizedMessage);
        }
    }

    public override double Parse(string? value)
    {
        TryParse(value, out var timeSpan);
        return timeSpan.TotalSeconds;
    }

    public override string Print(double value, string? format = null)
    {
        var timeSpan = TimeSpan.FromSeconds(value);
        var formattedTime = Format(timeSpan, CultureInfo.InvariantCulture);
        return formattedTime;
    }

    public override string PrintWithUnits(double value, string? format = null)
    {
        var timeSpan = TimeSpan.FromSeconds(value);
        var formattedTime = Format(timeSpan, CultureInfo.InvariantCulture);

        return $"{formattedTime} {Symbol}";
    }

    private static string Format(TimeSpan timeSpan, CultureInfo culture)
    {
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;

        var totalHours = (int)timeSpan.TotalHours;
        var minutes = Math.Abs(timeSpan.Minutes);
        var seconds = Math.Abs(timeSpan.Seconds);
        var milliseconds = Math.Abs(timeSpan.Milliseconds);

        var formattedSeconds = seconds.ToString("D2", culture);

        if (milliseconds > 0)
        {
            var formattedMs = milliseconds.ToString("000", culture).TrimEnd('0');
            if (formattedMs.Length > 0)
            {
                formattedSeconds += decimalSeparator + formattedMs;
            }
        }

        return $"{totalHours}{timeSeparator}{minutes.ToString("D2", culture)}{timeSeparator}{formattedSeconds}";
    }

    private static bool TryParse(string? value, out TimeSpan timeSpan)
    {
        try
        {
            timeSpan = Parse(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception)
        {
            timeSpan = TimeSpan.Zero;
            return false;
        }
    }

    private static TimeSpan Parse(string? value, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw IsNullOrWhiteSpaceValidationException.Instance;
        }

        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        var parts = value.Split(timeSeparator);

        if (parts.Length != 3)
        {
            throw new ValidationException(
                $"Invalid format. Expected 'hh{timeSeparator}mm{timeSeparator}ss'. Got: '{value}'",
                string.Format(RS.HourMinuteSecond_UnitItem_InvalidFormatException, timeSeparator)
            );
        }

        if (
            string.IsNullOrWhiteSpace(parts[0])
            || string.IsNullOrWhiteSpace(parts[1])
            || string.IsNullOrWhiteSpace(parts[2])
        )
        {
            throw new ValidationException(
                "All time components (hours, minutes, seconds) must be specified",
                RS.HourMinuteSecond_UnitItem_UnspecifiedTimeException
            );
        }

        if (
            !double.TryParse(parts[0], TimeStyle, culture, out var hours)
            || !double.TryParse(parts[1], TimeStyle, culture, out var minutes)
            || !double.TryParse(parts[2], TimeStyle, culture, out var seconds)
        )
        {
            throw NotNumberValidationException.Instance;
        }

        return TimeSpan.FromHours(hours)
            + TimeSpan.FromMinutes(minutes)
            + TimeSpan.FromSeconds(seconds);
    }
}
