namespace Asv.Avalonia;

public sealed class DataRateGigabytePerSecondUnitItem()
    : UnitItemBase(
        1.0 / (DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor)
    )
{
    public const string Id = $"{DataRateUnit.Id}.gigabyte_per_second";

    public override string UnitItemId => Id;
    public override string Name => RS.GigabytePerSecond_UnitItem_Name;
    public override string Description => RS.GigabytePerSecond_DataRate_Description;
    public override string Symbol => RS.GigabytePerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
