using Asv.Avalonia.GeoMap;
using Asv.Common;
using Xunit;

namespace Asv.Avalonia.Test;

public class MapPreloadTilePlannerTest
{
    [Fact]
    public void Circle_ExcludesBoundingBoxCornersAndIncludesEdgeIntersections()
    {
        var request = new MapPreloadRequest
        {
            Center = new GeoPoint(0, 0, 0),
            RadiusKm = 800,
            MinZoom = 6,
            MaxZoom = 6,
            Providers = [new PreloadTestProvider("circle")],
        };
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(request, TestContext.Current.CancellationToken)
            .ToArray();

        Assert.Equal(12, keys.Length);
        Assert.DoesNotContain(keys, key => key.X == 30 && key.Y == 30);
        Assert.Contains(keys, key => key.X == 30 && key.Y == 31);
        Assert.Contains(keys, key => key.X == 33 && key.Y == 32);
        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void Antimeridian_WrapsXWithoutDuplicateTiles()
    {
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = new GeoPoint(0, 179.99, 0),
                    RadiusKm = 5,
                    MinZoom = 10,
                    MaxZoom = 10,
                    Providers = [new PreloadTestProvider("date")],
                },
                TestContext.Current.CancellationToken
            )
            .ToArray();

        Assert.Equal(4, keys.Length);
        Assert.Equal(new[] { 0, 1023 }, keys.Select(key => key.X).Distinct().Order().ToArray());
        Assert.Equal(new[] { 511, 512 }, keys.Select(key => key.Y).Distinct().Order().ToArray());
        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void InclusiveZoomRange_ClampsEachProviderAndDeduplicatesProviderIds()
    {
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = new GeoPoint(10, 10, 0),
                    RadiusKm = 0.001,
                    MinZoom = 1,
                    MaxZoom = 3,
                    Providers =
                    [
                        new PreloadTestProvider("first", 2, 4),
                        new PreloadTestProvider("second", 1, 2),
                        new PreloadTestProvider("FIRST", 2, 4),
                    ],
                },
                TestContext.Current.CancellationToken
            )
            .ToArray();

        Assert.Equal(4, keys.Length);
        Assert.Equal(
            new ushort[] { 2, 3 },
            keys.Where(key => key.Provider.Info.Id == "first").Select(key => key.Zoom).ToArray()
        );
        Assert.Equal(
            new ushort[] { 1, 2 },
            keys.Where(key => key.Provider.Info.Id == "second").Select(key => key.Zoom).ToArray()
        );
    }

    [Fact]
    public void PolarRegion_ClipsYWithoutWrappingToOppositeHemisphere()
    {
        var provider = new PreloadTestProvider("polar");
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = new GeoPoint(89, 45, 0),
                    RadiusKm = 600,
                    MinZoom = 3,
                    MaxZoom = 3,
                    Providers = [provider],
                },
                TestContext.Current.CancellationToken
            )
            .ToArray();

        Assert.NotEmpty(keys);
        Assert.All(keys, key => Assert.Equal(0, key.Y));
        Assert.Equal(keys.Length, keys.Distinct().Count());
        Assert.Empty(
            MapPreloadTilePlanner.EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = new GeoPoint(90, 0, 0),
                    RadiusKm = 1,
                    MinZoom = 3,
                    MaxZoom = 3,
                    Providers = [provider],
                },
                TestContext.Current.CancellationToken
            )
        );
    }

    [Theory]
    [InlineData(23, 4427320, 3960095)]
    [InlineData(24, 8854641, 7920190)]
    [InlineData(30, 566697073, 506892160)]
    public void HighZoomProvider_UsesProjectionWithoutIntegerWorldSizeOverflow(
        int zoom,
        int x,
        int y
    )
    {
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = new GeoPoint(10, 10, 0),
                    RadiusKm = 0.000001,
                    MinZoom = zoom,
                    MaxZoom = zoom,
                    Providers = [new PreloadTestProvider("high-zoom", 0, 30)],
                },
                TestContext.Current.CancellationToken
            )
            .ToArray();

        var key = Assert.Single(keys);
        Assert.Equal(x, key.X);
        Assert.Equal(y, key.Y);
    }

    [Theory]
    [InlineData(0, 0, 19, 0.0449660181862269)]
    [InlineData(0, 179.99, 18, 0.0449660181862269)]
    [InlineData(75, 40, 17, 75.0449660181862)]
    public void BoundaryVertices_AreClosedAndContainedInPlannedTiles(
        double latitude,
        double longitude,
        int zoom,
        double northLatitude
    )
    {
        var provider = new PreloadTestProvider("boundary");
        var center = new GeoPoint(latitude, longitude, 0);
        var boundary = MapPreloadTilePlanner.CreateBoundary(center, 5);
        var keys = MapPreloadTilePlanner
            .EnumerateTiles(
                new MapPreloadRequest
                {
                    Center = center,
                    RadiusKm = 5,
                    MinZoom = zoom,
                    MaxZoom = zoom,
                    Providers = [provider],
                },
                TestContext.Current.CancellationToken
            )
            .ToHashSet();

        Assert.Equal(73, boundary.Count);
        Assert.Equal(northLatitude, boundary[0].Latitude, 10);
        Assert.Equal(72, boundary.Take(72).Distinct().Count());
        Assert.Equal(boundary[0], boundary[^1]);
        Assert.All(
            boundary,
            point =>
            {
                Assert.InRange(point.Latitude, -90, 90);
                Assert.InRange(point.Longitude, -180, 180);
                var pixel = provider.Projection.Wgs84ToPixels(point, zoom, provider.TileSize);
                Assert.Contains(
                    new TileKey(
                        (int)Math.Floor(pixel.X / provider.TileSize),
                        (int)Math.Floor(pixel.Y / provider.TileSize),
                        (ushort)zoom,
                        provider
                    ),
                    keys
                );
            }
        );
    }

    [Fact]
    public void Enumeration_ObservesCancellationBeforeExpandingLargeRegion()
    {
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        var request = new MapPreloadRequest
        {
            Center = GeoPoint.Zero,
            RadiusKm = 20000,
            MinZoom = 19,
            MaxZoom = 19,
            Providers = [new PreloadTestProvider("large")],
        };
        using var tiles = MapPreloadTilePlanner
            .EnumerateTiles(request, cancel.Token)
            .GetEnumerator();
        Assert.True(tiles.MoveNext());
        cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => tiles.MoveNext());
    }

    [Theory]
    [InlineData(-1, 1, 2)]
    [InlineData(double.NaN, 1, 2)]
    [InlineData(1, 4, 2)]
    [InlineData(1, -1, 2)]
    [InlineData(1, 1, 31)]
    public void InvalidRequest_IsRejected(double radius, int minZoom, int maxZoom)
    {
        var request = new MapPreloadRequest
        {
            RadiusKm = radius,
            MinZoom = minZoom,
            MaxZoom = maxZoom,
            Providers = [new PreloadTestProvider("invalid")],
        };
        Assert.ThrowsAny<ArgumentException>(() =>
            MapPreloadTilePlanner
                .EnumerateTiles(request, TestContext.Current.CancellationToken)
                .ToArray()
        );
    }
}
