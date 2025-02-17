using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(LongitudeBase.Id)]
[Shared]
public sealed class DegLongitudeUnit : LongitudeUnitItemBase
{
    public const string Id = $"{LongitudeBase.Id}.deg";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Longitude_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
