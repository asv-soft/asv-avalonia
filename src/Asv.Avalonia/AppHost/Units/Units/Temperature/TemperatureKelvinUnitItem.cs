namespace Asv.Avalonia;

public sealed class TemperatureKelvinUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{TemperatureUnit.Id}.kelvin";

    public override string UnitItemId => Id;
    public override string Name => RS.Kelvin_UnitItem_Name;
    public override string Description => RS.Kelvin_Temperature_Description;
    public override string Symbol => "K";
    public override bool IsInternationalSystemUnit => true;
}
