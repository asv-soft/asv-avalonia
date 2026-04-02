using Asv.Common;

namespace Asv.Avalonia;

public sealed class LatitudeDmsUnitItem : LatitudeUnitItemBase
{
    public const string Id = $"{LatitudeUnit.Id}.dms";

    public override string UnitItemId => Id;
    public override string Name => RS.Dms_UnitItem_Name;
    public override string Description => RS.Dms_Latitude_Description;
    public override string Symbol => RS.Dms_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override string Print(double value, string? format = null)
    {
        return GeoPointLatitude.PrintDms(value);
    }
}
