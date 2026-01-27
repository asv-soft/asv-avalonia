using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmLlzBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MicroAmpDdmLlzUnit() : UnitItemBase(150 / 0.155)
{
    public const string Id = $"{DdmLlzBase.Id}.micro.amp";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroAmp_UnitItem_Name;
    public override string Description => RS.MicroAmp_DdmLlz_Description;
    public override string Symbol => RS.Ddm_µA_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
