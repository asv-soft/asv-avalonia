using System.Composition;

namespace Asv.Avalonia;

public sealed class VoltageMilliVoltUnitItem() : UnitItemBase(1000)
{
    public const string Id = $"{VoltageUnit.Id}.millivolts";
    public override string UnitItemId => Id;
    public override string Name => RS.MilliVolt_UnitItem_Name;
    public override string Description => RS.MilliVolt_Voltage_Description;
    public override string Symbol => RS.MilliVolt_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
