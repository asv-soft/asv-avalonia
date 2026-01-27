using System.Composition;
using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(TemperatureBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class FahrenheitTemperatureUnit() : UnitItemBase(1)
{
    private const double ZeroCelsiusInKelvin = 273.15;
    private const double ZeroCelsiusInFahrenheit = 32;

    public const string Id = $"{SdmBase.Id}.fahrenheit";

    public override string UnitItemId => Id;
    public override string Name => RS.Fahrenheit_UnitItem_Name;
    public override string Description => RS.Fahrenheit_Temperature_Description;
    public override string Symbol => "°F";
    public override bool IsInternationalSystemUnit => false;

    public override double FromSi(double siValue)
    {
        return ((siValue - ZeroCelsiusInKelvin) * (9.0 / 5.0)) + ZeroCelsiusInFahrenheit;
    }

    public override double ToSi(double value)
    {
        return ((value - ZeroCelsiusInFahrenheit) * (5.0 / 9.0)) + ZeroCelsiusInKelvin;
    }
}
