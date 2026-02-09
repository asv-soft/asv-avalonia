using System.Composition;
using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

public sealed class TemperatureCelsiusUnitItem() : UnitItemBase(1)
{
    private const double ZeroCelsiusInKelvin = 273.15;
    public const string Id = $"{SdmUnit.Id}.celsius";

    public override string UnitItemId => Id;
    public override string Name => RS.Celsius_UnitItem_Name;
    public override string Description => RS.Celsius_Temperature_Description;
    public override string Symbol => "°C";
    public override bool IsInternationalSystemUnit => false;

    public override double FromSi(double siValue)
    {
        return siValue - ZeroCelsiusInKelvin;
    }

    public override double ToSi(double value)
    {
        return value + ZeroCelsiusInKelvin;
    }
}
