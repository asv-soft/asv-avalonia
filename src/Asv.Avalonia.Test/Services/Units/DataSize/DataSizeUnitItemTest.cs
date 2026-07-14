using Asv.Cfg;

namespace Asv.Avalonia.Test;

public sealed class DataSizeByteUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeByteUnitItem.Id,
        1.0
    ) { }

public sealed class DataSizeKilobyteUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeKilobyteUnitItem.Id,
        DataSizeUnit.ScaleFactor
    ) { }

public sealed class DataSizeMegabyteUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeMegabyteUnitItem.Id,
        DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor
    ) { }

public sealed class DataSizeGigabyteUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeGigabyteUnitItem.Id,
        DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor * DataSizeUnit.ScaleFactor
    ) { }

public sealed class DataSizeTerabyteUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataSizeUnitTestSetup.CreateUnit(),
        DataSizeTerabyteUnitItem.Id,
        DataSizeUnit.ScaleFactor
            * DataSizeUnit.ScaleFactor
            * DataSizeUnit.ScaleFactor
            * DataSizeUnit.ScaleFactor
    ) { }

internal static class DataSizeUnitTestSetup
{
    public static DataSizeUnit CreateUnit()
    {
        IUnitItem[] items =
        [
            new DataSizeByteUnitItem(),
            new DataSizeKilobyteUnitItem(),
            new DataSizeMegabyteUnitItem(),
            new DataSizeGigabyteUnitItem(),
            new DataSizeTerabyteUnitItem(),
        ];

        return new DataSizeUnit(new InMemoryConfiguration(), items);
    }
}
