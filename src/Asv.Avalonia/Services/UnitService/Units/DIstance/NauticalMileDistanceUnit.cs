using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DistanceBase.Id)]
[Shared]
[method: ImportingConstructor]
public class NauticalMileDistanceUnit() : UnitItemBase(1852.0)
{
    public const string Id = $"{DistanceBase.Id}.nautical.mile";

    public override string UnitItemId => Id;
    public override string Name => RS.NauticalMileDistanceUnit_Name;
    public override string Description => RS.NauticalMileDistanceUnit_Description;
    public override string Symbol => RS.NauticalMileDistanceUnit_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
