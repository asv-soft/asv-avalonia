using Xunit;

namespace Asv.Avalonia.Test;

public sealed class ByteRateFormatterTest()
    : ScaledUnitFormatterTestBase(
        DataRateUnitTestSetup.CreateUnit(),
        DataRateUnit.ScaleFactor,
        [
            DataRateBytePerSecondUnitItem.Id,
            DataRateKilobytePerSecondUnitItem.Id,
            DataRateMegabytePerSecondUnitItem.Id,
            DataRateGigabytePerSecondUnitItem.Id,
            DataRateTerabytePerSecondUnitItem.Id,
        ]
    )
{
    protected override IDataFormatter CreateFormatter(IUnitService unitService)
    {
        return new ByteRateFormatter(unitService);
    }

    [Fact]
    public void Print_1024BytesPerSecond_UsesDecimalKilobytes()
    {
        // Arrange
        using var unit = DataRateUnitTestSetup.CreateUnit();
        var formatter = new ByteRateFormatter(new UnitService([unit]));
        var kilobyte = unit.AvailableUnits[DataRateKilobytePerSecondUnitItem.Id];
        var expected = $"1.02 {kilobyte.Symbol}";

        // Act
        var actual = formatter.Print(1024, "0.##");

        // Assert
        Assert.Equal(expected, actual);
    }
}
