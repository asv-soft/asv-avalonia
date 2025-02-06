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

    public string? GetValidationErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return RS.UnitItemBase_Validation_ValueIsEmptyError;
        }

        value = value.Trim().Replace(',', '.');

        if (Units.All.Any(x => value.EndsWith(x)))
        {
            value = value[..^1];
        }

        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            return RS.UnitItemBase_Validation_ValueIsNaNError;
        }

        return null;
    }

    public double Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return double.NaN;
        }

        input = input.Trim().Replace(',', '.');

        double decMul = 1;
        if (Units.Kilo.Any(x => input.EndsWith(x)))
        {
            decMul = 1_000;
            input = input[..^1];
        }
        else if (Units.Mega.Any(x => input.EndsWith(x)))
        {
            decMul = 1_000_000;
            input = input[..^1];
        }
        else if (Units.Giga.Any(x => input.EndsWith(x)))
        {
            decMul = 1_000_000_000;
            input = input[..^1];
        }

        if (
            !double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
        )
        {
            return double.NaN;
        }

        return result * decMul;
    }

    public string Print(double value, string? format = null)
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    public string PrintWithUnits(double value, string? format = null)
    {
        return $"{value.ToString(format, CultureInfo.InvariantCulture)} {Symbol}";
    }

    public double FromSi(double siValue)
    {
        return siValue * multiplier;
    }

    public double ToSi(double value)
    {
        return value / multiplier;
    }
}
