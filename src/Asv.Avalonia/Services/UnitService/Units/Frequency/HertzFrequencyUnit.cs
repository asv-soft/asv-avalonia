using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(FrequencyBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class HertzFrequencyUnit() : UnitItemBase(1)
{
    public const string Id = $"{FrequencyBase.Id}.hertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Hertz_UnitItem_Name;
    public override string Description => RS.Hertz_Frequency_Description;
    public override string Symbol => RS.Hertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
