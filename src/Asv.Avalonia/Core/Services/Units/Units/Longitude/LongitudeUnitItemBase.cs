using Asv.Common;

namespace Asv.Avalonia;

public abstract class LongitudeUnitItemBase() : UnitItemBase(1)
{
    public override bool IsValid(string? value)
    {
        return value != null && GeoPointLongitude.IsValid(value);
    }

    public override ValidationResult ValidateValue(string? value)
    {
        var result = GeoPointLongitude.ValidateValue(value);
        if (result.IsSuccess)
        {
            return result;
        }

        return new UnitException(
            result.ValidationException?.Message,
            result.ValidationException?.LocalizedMessage
        );
    }

    public override double Parse(string? value)
    {
        return value != null && GeoPointLongitude.TryParse(value, out var result)
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
