namespace Asv.Avalonia;

public sealed class FrequencyMegahertzUnitItem() : UnitItemBase(0.000001)
{
    public const string Id = $"{FrequencyUnit.Id}.megahertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Megahertz_UnitItem_Name;
    public override string Description => RS.Megahertz_Frequency_Description;
    public override string Symbol => RS.Megahertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
