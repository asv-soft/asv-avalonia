using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(CapacityBase.Id)]
[Shared]
[method: ImportingConstructor]
public class MilliAmperCapacityUnit() : UnitItemBase(0.001)
{
    // TODO:Localize
    public const string Id = $"{CapacityBase.Id}.mah";
    public override string UnitItemId => Id;
    public override string Name => "Capacity";
    public override string Description => "Capacity_Description";
    public override string Symbol => "mAh";
    public override bool IsInternationalSystemUnit => true;
}
