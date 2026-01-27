using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VoltageBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class VoltVoltageUnit() : UnitItemBase(1)
{
    public const string Id = $"{VoltageBase.Id}.volts";
    public override string UnitItemId => Id;
    public override string Name => RS.Volt_UnitItem_Name;
    public override string Description => RS.Volt_Voltage_Description;
    public override string Symbol => RS.Volt_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
