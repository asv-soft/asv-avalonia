using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(SdmBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentSdmUnit() : UnitItemBase(100)
{
    public const string Id = $"{SdmBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Sdm_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
