using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

public abstract class UnitItemBase(double multiplier) : IUnitItem
{
    public abstract string UnitItemId { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Symbol { get; }
    public abstract bool IsInternationalSystemUnit { get; }

    public virtual bool IsValid(string? value)
    {
        return InvariantNumberParser.TryParse(value, out double _).IsSuccess;
    }

    public virtual ValidationResult ValidateValue(string? value)
    {
        var result = InvariantNumberParser.TryParse(value, out double _);
        if (result.IsSuccess)
        {
            return result;
        }

        return new UnitException(
            result.ValidationException?.Message,
            result.ValidationException?.LocalizedMessage
        );
    }

    public virtual double Parse(string? value)
    {
        InvariantNumberParser.TryParse(value, out double result);
        return result;
    }

    public virtual string Print(double value, string? format = null)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    public virtual string PrintWithUnits(double value, string? format = null)
    {
        return $"{value.ToString(format, CultureInfo.InvariantCulture)} {Symbol}";
    }

    public virtual double FromSi(double siValue)
    {
        return siValue * multiplier;
    }

    public virtual double ToSi(double value)
    {
        return value / multiplier;
    }
}
