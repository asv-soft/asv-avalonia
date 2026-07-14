namespace Asv.Avalonia;

public sealed class DataRateKilobytePerSecondUnitItem()
    : UnitItemBase(1.0 / DataRateUnit.ScaleFactor)
{
    public const string Id = $"{DataRateUnit.Id}.kilobyte_per_second";

    public override string UnitItemId => Id;
    public override string Name => RS.KilobytePerSecond_UnitItem_Name;
    public override string Description => RS.KilobytePerSecond_DataRate_Description;
    public override string Symbol => RS.KilobytePerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}
