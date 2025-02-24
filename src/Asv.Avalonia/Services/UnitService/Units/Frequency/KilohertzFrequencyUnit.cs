using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(FrequencyBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class KilohertzFrequencyUnit() : UnitItemBase(1000)
{
    public const string Id = $"{FrequencyBase.Id}.kilohertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Kilohertz_UnitItem_Name;
    public override string Description => RS.Kilohertz_Frequency_Description;
    public override string Symbol => RS.Kilohertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
