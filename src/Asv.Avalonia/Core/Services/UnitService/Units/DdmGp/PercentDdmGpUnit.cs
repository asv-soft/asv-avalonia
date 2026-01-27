using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmGpBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentDdmGpUnit() : UnitItemBase(100)
{
    public const string Id = $"{DdmGpBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_DdmGp_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => false;
}
