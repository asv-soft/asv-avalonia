using Xunit;

namespace Asv.Avalonia.Test;

public class DashboardViewModelTest
{
    [Fact]
    public void Tiles_AddTile_PutsTileIntoMatchingDensityList()
    {
        // Arrange
        using var dashboard = new DashboardViewModel("dashboard");
        var regular = new TestTileViewModel("regular") { Density = TileDensity.Regular };
        var compact = new TestTileViewModel("compact") { Density = TileDensity.Compact };
        var inline = new TestTileViewModel("inline") { Density = TileDensity.Inline };

        // Act
        dashboard.Tiles.Add(regular);
        dashboard.Tiles.Add(compact);
        dashboard.Tiles.Add(inline);

        // Assert
        Assert.Equal([regular], GetItems(dashboard.Regular));
        Assert.Equal([compact], GetItems(dashboard.Compact));
        Assert.Equal([inline], GetItems(dashboard.Inline));
    }

    [Fact]
    public void TileDensity_Change_MovesTileBetweenDensityLists()
    {
        // Arrange
        using var dashboard = new DashboardViewModel("dashboard");
        var tile = new TestTileViewModel("tile") { Density = TileDensity.Regular };
        dashboard.Tiles.Add(tile);

        // Act
        tile.Density = TileDensity.Inline;

        // Assert
        Assert.Empty(GetItems(dashboard.Regular));
        Assert.Empty(GetItems(dashboard.Compact));
        Assert.Equal([tile], GetItems(dashboard.Inline));
    }

    [Fact]
    public void MarkUpdated_StatusIconColorAlreadyHasFadeout_ReappliesFadeout()
    {
        // Arrange
        var tile = new TestTileViewModel("tile")
        {
            StatusIconColor = AsvColorKind.Success | AsvColorKind.Fadeout,
        };
        var changes = new List<AsvColorKind>();
        tile.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TileViewModel.StatusIconColor))
            {
                changes.Add(tile.StatusIconColor);
            }
        };

        // Act
        tile.MarkUpdated();

        // Assert
        Assert.Equal([AsvColorKind.Success, AsvColorKind.Success | AsvColorKind.Fadeout], changes);
    }

    private static List<ITileViewModel> GetItems(IEnumerable<ITileViewModel> source)
    {
        var result = new List<ITileViewModel>();
        foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }

    private sealed class TestTileViewModel(string id) : TileViewModel(id) { }
}
