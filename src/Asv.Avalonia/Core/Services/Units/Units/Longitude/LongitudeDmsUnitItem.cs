using Asv.Common;

namespace Asv.Avalonia;

public sealed class LongitudeDmsUnitItem : LongitudeUnitItemBase
{
    public const string Id = $"{LongitudeUnit.Id}.dms";

    public override string UnitItemId => Id;
    public override string Name => RS.Dms_UnitItem_Name;
    public override string Description => RS.Dms_Longitude_Description;
    public override string Symbol => RS.Dms_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override string Print(double value, string? format = null)
    {
        return GeoPointLongitude.PrintDms(value);
    }
}
