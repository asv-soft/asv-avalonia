using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmLlzBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentDdmLlzUnit() : UnitItemBase(100)
{
    public const string Id = $"{DdmLlzBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_DdmLlz_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => false;
}
