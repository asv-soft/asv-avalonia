using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmGpBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class InPartsDdmGpUnit() : UnitItemBase(1)
{
    public const string Id = $"{DdmGpBase.Id}.inparts";

    public override string UnitItemId => Id;
    public override string Name => RS.InPartsDdmGp_Name;
    public override string Description => RS.InPartsDdmGp_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}
