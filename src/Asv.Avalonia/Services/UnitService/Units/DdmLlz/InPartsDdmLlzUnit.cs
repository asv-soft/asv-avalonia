using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DdmLlzBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class InPartsDdmLlzUnit() : UnitItemBase(1)
{
    public const string Id = $"{DdmLlzBase.Id}.in.parts";

    public override string UnitItemId => Id;
    public override string Name => RS.InParts_UnitItem_Name;
    public override string Description => RS.InParts_DdmLlz_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}
