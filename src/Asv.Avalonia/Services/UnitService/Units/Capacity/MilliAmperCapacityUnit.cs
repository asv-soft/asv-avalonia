using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(CapacityBase.Id)]
[Shared]
[method: ImportingConstructor]
public class MilliAmperCapacityUnit() : UnitItemBase(0.001)
{
    public const string Id = $"{CapacityBase.Id}.mah";
    public override string UnitItemId => Id;
    public override string Name => RS.Capacity_UnitItem_Name;
    public override string Description => RS.Capacity_UnitItem_Description;
    public override string Symbol => RS.Capacity_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
