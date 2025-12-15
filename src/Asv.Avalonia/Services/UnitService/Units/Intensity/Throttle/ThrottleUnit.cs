using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(ThrottleBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class ThrottleUnit() : IntensityUnitItemBase(-1, 1, 0.01)
{
    public const string Id = $"{ThrottleBase.Id}.normalized";

    public override string UnitItemId => Id;
    public override string Name => RS.Normalized_UnitItem_Name;
    public override string Description => RS.Normalized_Throttle_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;
}
