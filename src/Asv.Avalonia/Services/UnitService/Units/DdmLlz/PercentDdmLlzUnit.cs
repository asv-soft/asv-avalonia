using System.Composition;

namespace Asv.Avalonia.DdmLlz;

[ExportUnitItem(DdmLlzBase.Id)]
[Shared]
[method: ImportingConstructor]
public class PercentDdmLlzUnit() : UnitItemBase(0.01)
{
    public const string Id = $"{DdmLlzBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_DdmLlz_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;
}
