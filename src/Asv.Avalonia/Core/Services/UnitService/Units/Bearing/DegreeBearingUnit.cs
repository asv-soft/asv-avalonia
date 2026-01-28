using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(BearingBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class DegreeBearingUnit() : UnitItemBase(1)
{
    public const string Id = $"{BearingBase.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Bearing_Description;
    public override string Symbol => "\u00b0";
    public override bool IsInternationalSystemUnit => true;
}
