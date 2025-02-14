using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmGpBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentDdmGpUnit() : UnitItemBase(0.01)
{
    public const string Id = $"{DdmGpBase.Id}.percents";

    public override string UnitItemId => Id;
    public override string Name => RS.PercentDdmGp_Name;
    public override string Description => RS.PercentDdmGp_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => false;
}
