using System.Composition;

namespace Asv.Avalonia;

public sealed class DistanceNauticalMileUnitItem() : UnitItemBase(0.00053995680345572)
{
    public const string Id = $"{DistanceUnit.Id}.nautical.mile";

    public override string UnitItemId => Id;
    public override string Name => RS.NauticalMile_UnitItem_Name;
    public override string Description => RS.NauticalMile_Distance_Description;
    public override string Symbol => "NM";
    public override bool IsInternationalSystemUnit => false;
}
