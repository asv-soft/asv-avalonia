using System.Composition;

namespace Asv.Avalonia;

public sealed class SdmPercentUnitItem() : UnitItemBase(100)
{
    public const string Id = $"{SdmUnit.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Sdm_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
