using System.Composition;

namespace Asv.Avalonia;

public sealed class DdmLlzPercentUnitItem() : UnitItemBase(100)
{
    public const string Id = $"{DdmLlzUnit.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_DdmLlz_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => false;
}
