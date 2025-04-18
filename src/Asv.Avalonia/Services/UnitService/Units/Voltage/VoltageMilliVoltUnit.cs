using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VoltageItemBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class VoltageMilliVoltUnit() : UnitItemBase(0.001)
{
    public const string Id = $"{VoltageItemBase.Id}.milliVolts";
    public override string UnitItemId => Id;
    public override string Name => RS.VoltageMilliVoltUnit_Name;
    public override string Description => RS.VoltageMilliVoltUnit_Description;
    public override string Symbol => RS.VoltageMilliVoltUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}