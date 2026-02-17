namespace Asv.Avalonia;

public sealed class FrequencyGigahertzUnitItem() : UnitItemBase(0.000000001)
{
    public const string Id = $"{FrequencyUnit.Id}.gigahertz";

    public override string UnitItemId => Id;
    public override string Name => RS.Gigahertz_UnitItem_Name;
    public override string Description => RS.Gigahertz_Frequency_Description;
    public override string Symbol => RS.Gigahertz_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
