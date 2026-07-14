using Xunit;

namespace Asv.Avalonia.Test;

public sealed class DataSizeUnitTest : IDisposable
{
    private readonly DataSizeUnit _unit = DataSizeUnitTestSetup.CreateUnit();

    [Fact]
    public void AvailableUnits_Get_ReturnsAllDataSizeItems()
    {
        // Arrange
        string[] expectedUnitItemIds =
        [
            DataSizeByteUnitItem.Id,
            DataSizeKilobyteUnitItem.Id,
            DataSizeMegabyteUnitItem.Id,
            DataSizeGigabyteUnitItem.Id,
            DataSizeTerabyteUnitItem.Id,
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
    public void InternationalSystemUnit_Get_ReturnsByteItem()
    {
        // Arrange
        const string expectedUnitItemId = DataSizeByteUnitItem.Id;

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
