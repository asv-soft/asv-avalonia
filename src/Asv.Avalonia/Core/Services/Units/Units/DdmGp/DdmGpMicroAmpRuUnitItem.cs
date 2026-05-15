namespace Asv.Avalonia;

public sealed class DdmGpMicroAmpRuUnitItem() : UnitItemBase(250 / 0.175)
{
    public const string Id = $"{DdmGpUnit.Id}.micro.amp.ru";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroAmpRu_UnitItem_Name;
    public override string Description => RS.MicroAmpRu_DdmGp_Description;
    public override string Symbol => RS.Ddm_µA_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
