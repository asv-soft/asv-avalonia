namespace Asv.Avalonia;

public sealed class DataRateBytePerSecondUnitItem() : UnitItemBase(1.0)
{
    public const string Id = $"{DataRateUnit.Id}.byte_per_second";

    public override string UnitItemId => Id;
    public override string Name => RS.BytePerSecond_UnitItem_Name;
    public override string Description => RS.BytePerSecond_DataRate_Description;
    public override string Symbol => RS.BytePerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
