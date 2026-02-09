using System.Composition;

namespace Asv.Avalonia;

public sealed class LongitudeDegreeUnitItem : LongitudeUnitItemBase
{
    public const string Id = $"{LongitudeUnit.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Longitude_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
