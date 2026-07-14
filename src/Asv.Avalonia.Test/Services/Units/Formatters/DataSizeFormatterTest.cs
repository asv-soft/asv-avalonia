namespace Asv.Avalonia.Test;

public sealed class DataSizeFormatterTest()
    : ScaledUnitFormatterTestBase(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeUnit.ScaleFactor,
        [
            DataSizeByteUnitItem.Id,
            DataSizeKilobyteUnitItem.Id,
            DataSizeMegabyteUnitItem.Id,
            DataSizeGigabyteUnitItem.Id,
            DataSizeTerabyteUnitItem.Id,
        ]
    )
{
    protected override IDataFormatter CreateFormatter(IUnitService unitService)
    {
        return new DataSizeFormatter(unitService);
    }
}
