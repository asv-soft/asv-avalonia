namespace Asv.Avalonia;

public sealed class VoltageVoltUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{VoltageUnit.Id}.volts";
    public override string UnitItemId => Id;
    public override string Name => RS.Volt_UnitItem_Name;
    public override string Description => RS.Volt_Voltage_Description;
    public override string Symbol => RS.Volt_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
