using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(ThrottleBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentThrottleUnit() : IntensityUnitItemBase(-100, 100, 100)
{
    public const string Id = $"{ThrottleBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Throttle_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
