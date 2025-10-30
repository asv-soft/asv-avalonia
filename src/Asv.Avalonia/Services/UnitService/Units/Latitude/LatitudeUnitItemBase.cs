using Asv.Common;

namespace Asv.Avalonia;

public abstract class LatitudeUnitItemBase() : UnitItemBase(1)
{
    public override bool IsValid(string? value)
    {
        return value != null && GeoPointLatitude.IsValid(value);
    }

    public override ValidationResult ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        var msg = GeoPointLatitude.GetErrorMessage(value);

        if (msg is not null)
        {
            return new ValidationResult
            {
                IsSuccess = false,
                ValidationException = new UnitException(msg),
            };
        }

        return ValidationResult.Success;
    }

    public override double Parse(string? value)
    {
        return value != null && GeoPointLatitude.TryParse(value, out var result)
            ? result
            : double.NaN;
    }

    public override string PrintWithUnits(double value, string? format = null)
    {
        return Print(value, format);
    }

    public override double FromSi(double siValue)
    {
        return siValue;
    }

    public override double ToSi(double value)
    {
        return value;
    }
}
