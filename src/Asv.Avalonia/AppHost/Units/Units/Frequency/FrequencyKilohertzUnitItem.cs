using System.Composition;

namespace Asv.Avalonia;

public sealed class FrequencyKilohertzUnitItem() : UnitItemBase(0.001)
{
    public const string Id = $"{FrequencyUnit.Id}.kilohertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Kilohertz_UnitItem_Name;
    public override string Description => RS.Kilohertz_Frequency_Description;
    public override string Symbol => RS.Kilohertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
