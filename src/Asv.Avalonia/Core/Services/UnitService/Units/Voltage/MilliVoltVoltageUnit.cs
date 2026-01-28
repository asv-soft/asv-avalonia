using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VoltageBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MilliVoltVoltageUnit() : UnitItemBase(1000)
{
    public const string Id = $"{VoltageBase.Id}.millivolts";
    public override string UnitItemId => Id;
    public override string Name => RS.MilliVolt_UnitItem_Name;
    public override string Description => RS.MilliVolt_Voltage_Description;
    public override string Symbol => RS.MilliVolt_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
