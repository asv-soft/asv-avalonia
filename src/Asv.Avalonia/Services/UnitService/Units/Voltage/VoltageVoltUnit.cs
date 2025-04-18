using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VoltageItemBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class VoltageVoltUnit() : UnitItemBase(1)
{
    public const string Id = $"{VoltageItemBase.Id}.volts";
    public override string UnitItemId => Id;
    public override string Name => RS.VoltageVoltUnit_Name;
    public override string Description => RS.VoltageVoltUnit_Description;
    public override string Symbol => RS.VoltageVoltUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}