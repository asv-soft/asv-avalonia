using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(LongitudeBase.Id)]
[Shared]
public sealed class DegreeLongitudeUnit : LongitudeUnitItemBase
{
    public const string Id = $"{LongitudeBase.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Longitude_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
