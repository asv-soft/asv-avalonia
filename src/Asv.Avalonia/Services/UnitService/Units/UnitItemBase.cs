using System.Globalization;

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
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim().Replace(',', '.');

        if (Units.All.Any(x => value.EndsWith(x)))
        {
            value = value[..^1];
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    public virtual ValidationResult ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new UnitItemValueIsNullOrEmptyError();
        }

        value = value.Trim().Replace(',', '.');

        if (Units.All.Any(x => value.EndsWith(x)))
        {
            value = value[..^1];
        }

        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            return new UnitItemValueIsNanError();
        }

        return ValidationResult.Success;
    }

    public virtual double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return double.NaN;
        }

        value = value.Trim().Replace(',', '.');

        double decMul = 1;
        if (Units.Kilo.Any(x => value.EndsWith(x)))
        {
            decMul = 1_000;
            value = value[..^1];
        }
        else if (Units.Mega.Any(x => value.EndsWith(x)))
        {
            decMul = 1_000_000;
            value = value[..^1];
        }
        else if (Units.Giga.Any(x => value.EndsWith(x)))
        {
            decMul = 1_000_000_000;
            value = value[..^1];
        }

        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return double.NaN;
        }

        return result * decMul;
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
