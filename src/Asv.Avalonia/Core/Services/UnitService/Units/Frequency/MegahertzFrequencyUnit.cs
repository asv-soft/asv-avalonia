using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(FrequencyBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MegahertzFrequencyUnit() : UnitItemBase(0.000001)
{
    public const string Id = $"{FrequencyBase.Id}.megahertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Megahertz_UnitItem_Name;
    public override string Description => RS.Megahertz_Frequency_Description;
    public override string Symbol => RS.Megahertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
