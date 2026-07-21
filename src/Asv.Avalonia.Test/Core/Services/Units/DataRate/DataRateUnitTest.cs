using Xunit;

namespace Asv.Avalonia.Test;

public sealed class DataRateUnitTest : IDisposable
{
    private readonly DataRateUnit _unit = DataRateUnitTestSetup.CreateUnit();

    [Fact]
    public void AvailableUnits_Get_ReturnsAllDataRateItems()
    {
        // Arrange
        string[] expectedUnitItemIds =
        [
            DataRateBytePerSecondUnitItem.Id,
            DataRateKilobytePerSecondUnitItem.Id,
            DataRateMegabytePerSecondUnitItem.Id,
            DataRateGigabytePerSecondUnitItem.Id,
            DataRateTerabytePerSecondUnitItem.Id,
        ];

        // Act
        var actual = _unit.AvailableUnits;

        // Assert
        Assert.Equal(expectedUnitItemIds.Length, actual.Count);
        foreach (var unitItemId in expectedUnitItemIds)
        {
            Assert.Contains(unitItemId, actual);
        }
    }

    [Fact]
    public void InternationalSystemUnit_Get_ReturnsBytePerSecondItem()
    {
        // Arrange
        const string expectedUnitItemId = DataRateBytePerSecondUnitItem.Id;

        // Act
        var actual = _unit.InternationalSystemUnit;

        // Assert
        Assert.Equal(expectedUnitItemId, actual.UnitItemId);
    }

    public void Dispose()
    {
        _unit.Dispose();
    }
}
