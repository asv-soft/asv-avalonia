using System.Composition;

namespace Asv.Avalonia;

public sealed class ThrottlePartUnitItem() : IntensityUnitItemBase(-1, 1, 0.01)
{
    public const string Id = $"{ThrottleUnit.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_Throttle_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;
}
