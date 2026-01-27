using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(CapacityBase.Id)]
[Shared]
[method: ImportingConstructor]
public class MilliAmperePerHourCapacityUnit() : UnitItemBase(1000)
{
    public const string Id = $"{CapacityBase.Id}.mah";
    public override string UnitItemId => Id;
    public override string Name => RS.Mah_UnitItem_Name;
    public override string Description => RS.Mah_Capacity_Description;
    public override string Symbol => RS.Mah_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
