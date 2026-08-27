using Asv.Common;
using Asv.Avalonia.GeoMap;
using Asv.Cfg;
using Avalonia;
using Xunit;

namespace Asv.Avalonia.Test;

public class MapScaleControlTest
{
    [Fact]
    public void Constructor_UsesValidMetricDefaults()
    {
        // Arrange & Act
        var control = new MapScaleControl();

        // Assert
        Assert.True(control.IsScaleValid);
        Assert.True(control.ScaleDistanceMeters > 0);
        Assert.InRange(control.ActualBarWidth, 1, control.MaxBarWidth);
        Assert.NotEmpty(control.MiddleText);
        Assert.EndsWith(new DistanceMeterUnitItem().Symbol, control.MaximumText);
        Assert.False(control.IsHitTestVisible);
    }

    [Fact]
    public void MapState_FromReferenceLayout_ProducesExpectedLabels()
    {
        // Arrange & Act
        var control = new MapScaleControl
        {
            CenterMap = new GeoPoint(53, 53, 0),
            Zoom = 13,
            MaxBarWidth = 140,
        };
        var unit = new DistanceMeterUnitItem();

        // Assert
        Assert.Equal(1000, control.ScaleDistanceMeters);
        Assert.Equal(unit.Print(500, "0.##"), control.MiddleText);
        Assert.Equal(unit.PrintWithUnits(1000, "0.##"), control.MaximumText);
    }

    [Fact]
    public void ZoomIncrease_HalvesSelectedScaleDistance()
    {
        // Arrange
        var control = new MapScaleControl
        {
            CenterMap = GeoPoint.Zero,
            Zoom = 10,
            MaxBarWidth = 140,
        };
        var initialDistance = control.ScaleDistanceMeters;

        // Act
        control.Zoom = 11;

        // Assert
        Assert.Equal(initialDistance * 0.5, control.ScaleDistanceMeters);
    }

    [Fact]
    public void HigherLatitude_ReducesSelectedScaleDistance()
    {
        // Arrange
        var control = new MapScaleControl
        {
            CenterMap = GeoPoint.Zero,
            Zoom = 10,
            MaxBarWidth = 140,
        };
        var equatorDistance = control.ScaleDistanceMeters;

        // Act
        control.CenterMap = new GeoPoint(60, 0, 0);

        // Assert
        Assert.True(control.ScaleDistanceMeters < equatorDistance);
    }

    [Fact]
    public void ProviderWithLargerTiles_ReducesSelectedScaleDistance()
    {
        // Arrange
        var control = new MapScaleControl
        {
            CenterMap = GeoPoint.Zero,
            Zoom = 10,
            MaxBarWidth = 140,
        };
        var initialDistance = control.ScaleDistanceMeters;

        // Act
        control.Provider = new TestTileProvider(512);

        // Assert
        Assert.Equal(initialDistance * 0.5, control.ScaleDistanceMeters);
    }

    [Fact]
    public void CenterNearAntimeridian_UsesLocalDistance()
    {
        // Arrange
        var control = new MapScaleControl
        {
            CenterMap = GeoPoint.Zero,
            Zoom = 10,
            MaxBarWidth = 140,
        };
        var referenceDistance = control.ScaleDistanceMeters;

        // Act
        control.CenterMap = new GeoPoint(0, 179.99, 0);

        // Assert
        Assert.True(control.IsScaleValid);
        Assert.Equal(referenceDistance, control.ScaleDistanceMeters);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(double.NaN)]
    public void InvalidMaxBarWidth_IsCoercedToDefault(double width)
    {
        // Arrange & Act
        var control = new MapScaleControl { MaxBarWidth = width };

        // Assert
        Assert.Equal(140, control.MaxBarWidth);
        Assert.True(control.IsScaleValid);
        Assert.InRange(control.ActualBarWidth, 0, control.MaxBarWidth);
    }

    [Fact]
    public void InvalidProvider_ClearsComputedState()
    {
        // Arrange
        var control = new MapScaleControl();
        Assert.True(control.ScaleDistanceMeters > 0);

        // Act
        control.Provider = new TestTileProvider(0);

        // Assert
        Assert.False(control.IsScaleValid);
        Assert.Equal(0, control.ScaleDistanceMeters);
        Assert.Equal("0", control.MiddleText);
    }

    [Fact]
    public void ThrowingProjection_ClearsComputedState()
    {
        // Arrange
        var control = new MapScaleControl();

        // Act
        control.Provider = new TestTileProvider(256, new ThrowingProjection());

        // Assert
        Assert.False(control.IsScaleValid);
        Assert.Equal(0, control.ScaleDistanceMeters);
    }

    [Fact]
    public void ZoomOutsideProviderRange_InvalidatesScale()
    {
        // Arrange
        var control = new MapScaleControl();

        // Act
        control.Zoom = EmptyTileProvider.StaticInfo.MaxZoom + 1;

        // Assert
        Assert.False(control.IsScaleValid);
        Assert.Equal(0, control.ScaleDistanceMeters);
    }

    [Fact]
    public void SmallNauticalMileScale_PreservesSignificantDigits()
    {
        // Arrange
        using var distanceUnit = new DistanceUnit(
            new InMemoryConfiguration(),
            [new DistanceMeterUnitItem(), new DistanceNauticalMileUnitItem()]
        );
        distanceUnit.CurrentUnitItem.Value = distanceUnit.AvailableUnits[
            DistanceNauticalMileUnitItem.Id
        ];

        // Act
        var control = new MapScaleControl
        {
            CenterMap = new GeoPoint(53, 53, 0),
            Zoom = 19,
            UnitService = new TestUnitService(distanceUnit),
        };

        // Assert
        Assert.True(control.IsScaleValid);
        Assert.NotEqual("0", control.MiddleText);
        Assert.StartsWith("0.0", control.MaximumText);
        Assert.EndsWith("NM", control.MaximumText);
    }

    private sealed class TestTileProvider(int tileSize, IMapProjection? projection = null)
        : ITileProvider
    {
        public TileProviderInfo Info => EmptyTileProvider.StaticInfo;
        public IMapProjection Projection { get; } = projection ?? WebMercatorProjection.Instance;
        public int TileSize { get; } = tileSize;

        public Task<Tile?> DownloadAsync(TileKey key, CancellationToken ct) =>
            Task.FromResult<Tile?>(null);
    }

    private sealed class ThrowingProjection : IMapProjection
    {
        public GeoPoint PixelsToWgs84(Point pixel, int zoom, int tileSize) =>
            throw new InvalidOperationException("Projection is unavailable.");

        public Point Wgs84ToPixels(GeoPoint wgs, int zoom, int tileSize) =>
            throw new InvalidOperationException("Projection is unavailable.");
    }

    private sealed class TestUnitService(IUnit distanceUnit) : IUnitService
    {
        public IReadOnlyDictionary<string, IUnit> Units { get; } =
            new Dictionary<string, IUnit> { [DistanceUnit.Id] = distanceUnit };
    }
}
