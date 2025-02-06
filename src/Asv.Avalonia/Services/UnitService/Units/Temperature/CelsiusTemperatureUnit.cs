using System.Composition;
using System.Globalization;

namespace Asv.Avalonia;

[ExportUnitItem(TemperatureBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class CelsiusTemperatureUnit() : UnitItemBase(1)
{
    private const double ZeroCelsiusInKelvin = 273.15;
    public const string Id = $"{SdmBase.Id}.celsius";

    public override string UnitItemId => Id;
    public override string Name => RS.Celsius_UnitItem_Name;
    public override string Description => RS.Celsius_Temperature_Description;
    public override string Symbol => "°C";
    public override bool IsInternationalSystemUnit => false;

    public override bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
    }

    public override ValidationResult ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new UnitItemValueIsNullOrEmptyError();
        }

        value = value.Replace(',', '.');
        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return new UnitItemValueIsNanError();
        }

        return ValidationResult.Success;
    }

    public override double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return double.NaN;
        }

        return double.TryParse(
            value.Replace(",", "."),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var v
        )
            ? v
            : double.NaN;
    }

    public override double FromSi(double siValue)
    {
        return siValue - ZeroCelsiusInKelvin;
    }

    public override double ToSi(double value)
    {
        return value + ZeroCelsiusInKelvin;
    }
}
