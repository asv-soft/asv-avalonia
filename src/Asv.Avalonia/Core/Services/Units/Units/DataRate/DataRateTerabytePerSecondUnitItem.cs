namespace Asv.Avalonia;

public sealed class DataRateTerabytePerSecondUnitItem()
    : UnitItemBase(
        1.0
            / (
                DataRateUnit.ScaleFactor
                * DataRateUnit.ScaleFactor
                * DataRateUnit.ScaleFactor
                * DataRateUnit.ScaleFactor
            )
    )
{
    public const string Id = $"{DataRateUnit.Id}.terabyte_per_second";

    public override string UnitItemId => Id;
    public override string Name => RS.TerabytePerSecond_UnitItem_Name;
    public override string Description => RS.TerabytePerSecond_DataRate_Description;
    public override string Symbol => RS.TerabytePerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
