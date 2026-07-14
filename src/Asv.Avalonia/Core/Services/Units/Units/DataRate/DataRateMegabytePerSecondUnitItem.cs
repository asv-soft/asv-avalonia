namespace Asv.Avalonia;

public sealed class DataRateMegabytePerSecondUnitItem()
    : UnitItemBase(1.0 / (DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor))
{
    public const string Id = $"{DataRateUnit.Id}.megabyte_per_second";

    public override string UnitItemId => Id;
    public override string Name => RS.MegabytePerSecond_UnitItem_Name;
    public override string Description => RS.MegabytePerSecond_DataRate_Description;
    public override string Symbol => RS.MegabytePerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
