using System.Composition;

namespace Asv.Avalonia;

public sealed class DdmLlzMicroAmpRuUnitItem() : UnitItemBase(250 / 0.155)
{
    public const string Id = $"{DdmLlzUnit.Id}.micro.amp.ru";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroAmpRu_UnitItem_Name;
    public override string Description => RS.MicroAmpRu_DdmLlz_Description;
    public override string Symbol => RS.Ddm_µA_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
