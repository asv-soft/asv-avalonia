using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

public abstract class TimeSpanCompositeUnitItemBase : UnitItemBase
{
    protected enum TimeSpanComponent
    {
        Hours,
        Minutes,
        Seconds,
    }

    private readonly TimeSpanComponent[] _components;
    private readonly IUnitItem[] _componentItems;

    protected TimeSpanCompositeUnitItemBase(params TimeSpanComponent[] components)
        : base(1)
    {
        if (components.Distinct().Count() != components.Length)
        {
            throw new ArgumentException("Duplicate components are not allowed", nameof(components));
        }

        _components = components;
        _componentItems = components
            .Select<TimeSpanComponent, IUnitItem>(component =>
            {
                return component switch
                {
                    TimeSpanComponent.Hours => new TimeSpanHourUnitItem(),
                    TimeSpanComponent.Minutes => new TimeSpanMinuteUnitItem(),
                    TimeSpanComponent.Seconds => new TimeSpanSecondUnitItem(),
                    _ => throw new ArgumentOutOfRangeException(nameof(component)),
                };
            })
            .ToArray();
    }

    public override bool IsValid(string? value) => ValidateAndParse(value, out _).IsSuccess;

    public override ValidationResult ValidateValue(string? value) => ValidateAndParse(value, out _);

    public override double Parse(string? value)
    {
        return ValidateAndParse(value, out var totalSeconds).IsSuccess ? totalSeconds : double.NaN;
    }

    public override string Print(double value, string? format = null)
    {
        var culture = CultureInfo.InvariantCulture;
        var isNegative = value < 0;
        var remaining = Math.Abs(value);
        var parts = new string[_components.Length];

        for (var i = 0; i < _components.Length; i++)
        {
            var perUnit = _components[i] switch
            {
                TimeSpanComponent.Hours => 3600,
                TimeSpanComponent.Minutes => 60,
                TimeSpanComponent.Seconds => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(TimeSpanComponent)),
            };
            var isLast = i == _components.Length - 1;

            var componentValue = isLast ? remaining / perUnit : Math.Floor(remaining / perUnit);
            if (!isLast)
            {
                remaining -= componentValue * perUnit;
            }

            var componentFormat = isLast
                ? format ?? GetDefaultLastComponentFormat(i)
                : GetFormat(i);
            parts[i] = componentValue.ToString(componentFormat, culture);
        }

        var joined = string.Join(culture.DateTimeFormat.TimeSeparator, parts);
        return isNegative && HasNonZeroDigit(parts) ? $"-{joined}" : joined;
    }

    private static string GetDefaultLastComponentFormat(int componentIndex)
    {
        return GetFormat(componentIndex) + ".######";
    }

    private static string GetFormat(int componentIndex)
    {
        return componentIndex == 0 ? "0" : "00";
    }

    private static bool HasNonZeroDigit(IEnumerable<string> parts)
    {
        return parts.Any(part => part.Any(static c => c is >= '1' and <= '9'));
    }

    private ValidationResult ValidateAndParse(string? value, out double totalSeconds)
    {
        totalSeconds = double.NaN;
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToUnitException(IsNullOrWhiteSpaceValidationException.Instance);
        }

        var parts = value.Split(CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator);
        if (parts.Length != _components.Length)
        {
            return InvalidFormat(value);
        }

        if (parts.Any(string.IsNullOrWhiteSpace))
        {
            return ToUnitException(
                new ValidationException(
                    "All time components must be specified",
                    RS.TimeSpanComposite_UnitItem_UnspecifiedTimeException
                )
            );
        }

        var isNegative = parts[0].TrimStart().StartsWith('-');
        var result = 0d;
        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0 && HasLeadingSign(parts[i]))
            {
                return InvalidFormat(value);
            }

            var validation = _componentItems[i].ValidateValue(parts[i]);
            if (validation.IsFailed)
            {
                return validation;
            }

            var componentSeconds = _componentItems[i].ParseToSi(parts[i]);
            result += isNegative ? Math.Abs(componentSeconds) : componentSeconds;
        }

        totalSeconds = isNegative ? -result : result;
        return ValidationResult.Success;
    }

    private static bool HasLeadingSign(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith('-') || trimmed.StartsWith('+');
    }

    private ValidationResult InvalidFormat(string value)
    {
        var expectedFormat = string.Join(
            ":",
            _components.Select(c =>
                c switch
                {
                    TimeSpanComponent.Hours => "hh",
                    TimeSpanComponent.Minutes => "mm",
                    TimeSpanComponent.Seconds => "ss",
                    _ => throw new ArgumentOutOfRangeException(nameof(c)),
                }
            )
        );
        return ToUnitException(
            new ValidationException(
                $"Invalid format. Expected '{expectedFormat}'. Got: '{value}'",
                string.Format(RS.TimeSpanComposite_UnitItem_InvalidFormatException, expectedFormat)
            )
        );
    }

    private static ValidationResult ToUnitException(ValidationException exception)
    {
        return new UnitException(exception.Message, exception, exception.LocalizedMessage);
    }
}
