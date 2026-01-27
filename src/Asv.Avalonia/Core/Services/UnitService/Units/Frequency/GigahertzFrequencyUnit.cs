using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(FrequencyBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class GigahertzFrequencyUnit() : UnitItemBase(0.000000001)
{
    public const string Id = $"{FrequencyBase.Id}.gigahertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Gigahertz_UnitItem_Name;
    public override string Description => RS.Gigahertz_Frequency_Description;
    public override string Symbol => RS.Gigahertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
