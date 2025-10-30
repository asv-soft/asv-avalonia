using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(LatitudeBase.Id)]
[Shared]
public sealed class DegreeLatitudeUnit : LatitudeUnitItemBase
{
    public const string Id = $"{LatitudeBase.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Latitude_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;

    
}
