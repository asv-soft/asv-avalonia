using System.Composition;

namespace Asv.Avalonia;

public sealed class ThrottlePercentUnit() : IntensityUnitItemBase(-100, 100, 1)
{
    public const string Id = $"{ThrottleUnit.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Throttle_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
