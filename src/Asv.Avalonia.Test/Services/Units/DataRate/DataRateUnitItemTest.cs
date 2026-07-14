using Asv.Cfg;

namespace Asv.Avalonia.Test;

public sealed class DataRateBytePerSecondUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateBytePerSecondUnitItem.Id,
        1.0
    ) { }

public sealed class DataRateKilobytePerSecondUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateKilobytePerSecondUnitItem.Id,
        DataRateUnit.ScaleFactor
    ) { }

public sealed class DataRateMegabytePerSecondUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateMegabytePerSecondUnitItem.Id,
        DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor
    ) { }

public sealed class DataRateGigabytePerSecondUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateGigabytePerSecondUnitItem.Id,
        DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor * DataRateUnit.ScaleFactor
    ) { }

public sealed class DataRateTerabytePerSecondUnitItemTest()
    : UnitItemTestBase<UnitItemDefaultTestCases>(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateTerabytePerSecondUnitItem.Id,
        DataRateUnit.ScaleFactor
            * DataRateUnit.ScaleFactor
            * DataRateUnit.ScaleFactor
            * DataRateUnit.ScaleFactor
    ) { }

internal static class DataRateUnitTestSetup
{
    public static DataRateUnit CreateUnit()
    {
        IUnitItem[] items =
        [
            new DataRateBytePerSecondUnitItem(),
            new DataRateKilobytePerSecondUnitItem(),
            new DataRateMegabytePerSecondUnitItem(),
            new DataRateGigabytePerSecondUnitItem(),
            new DataRateTerabytePerSecondUnitItem(),
        ];

        return new DataRateUnit(new InMemoryConfiguration(), items);
    }
}
