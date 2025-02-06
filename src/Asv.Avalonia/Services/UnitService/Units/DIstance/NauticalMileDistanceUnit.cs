using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DistanceBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class NauticalMileDistanceUnit() : UnitItemBase(1852.0)
{
    public const string Id = $"{DistanceBase.Id}.nautical.mile";

    public override string UnitItemId => Id;
    public override string Name => RS.NauticalMile_UnitItem_Name;
    public override string Description => RS.NauticalMile_Distance_Description;
    public override string Symbol => "NM";
    public override bool IsInternationalSystemUnit => false;
}
