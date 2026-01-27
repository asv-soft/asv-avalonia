using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmGpBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MicroAmpDdmGpUnit() : UnitItemBase(150 / 0.175)
{
    public const string Id = $"{DdmGpBase.Id}.micro.amp";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroAmp_UnitItem_Name;
    public override string Description => RS.MicroAmp_DdmGp_Description;
    public override string Symbol => RS.Ddm_µA_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
