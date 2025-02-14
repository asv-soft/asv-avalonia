using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmGpBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MicroAmpRuDdmGpUnit() : UnitItemBase(0.175 / 250)
{
    public const string Id = $"{DdmGpBase.Id}.micro.amp.ru";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroAmpRuDdmGp_Name;
    public override string Description => RS.MicroAmpRuDdmGp_Description;
    public override string Symbol => RS.Ddm_µA_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
