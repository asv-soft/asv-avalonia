using Asv.Common;
using Avalonia;

namespace Asv.Avalonia.GeoMap;

public static class MapPreloadTilePlanner
{
    private const double EarthRadiusKm = 6371.0088;
    private const double DegreesToRadians = Math.PI / 180;

    public static IEnumerable<TileKey> EnumerateTiles(
        MapPreloadRequest request,
        CancellationToken cancellationToken = default
    )
    {
        Validate(request);
        var providers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var provider in request.Providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!providers.Add(provider.Info.Id))
            {
                continue;
            }

            var minZoom = Math.Max(request.MinZoom, provider.Info.MinZoom);
            var maxZoom = Math.Min(request.MaxZoom, provider.Info.MaxZoom);
            for (var zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                foreach (var key in EnumerateLevel(request, provider, zoom, cancellationToken))
                {
                    yield return key;
                }
            }
        }
    }

    public static IReadOnlyList<GeoPoint> CreateBoundary(
        GeoPoint center,
        double radiusKm,
        int segments = 72
    )
    {
        ValidateRegion(center, radiusKm);
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 3);
        var points = new GeoPoint[checked(segments + 1)];
        var latitude = center.Latitude * DegreesToRadians;
        var longitude = center.Longitude * DegreesToRadians;
        var radius = Math.Min(radiusKm / EarthRadiusKm, Math.PI);
        var sinRadius = Math.Sin(radius);
        var cosRadius = Math.Cos(radius);
        var sinLatitude = Math.Sin(latitude);
        var cosLatitude = Math.Cos(latitude);
        var sinLongitude = Math.Sin(longitude);
        var cosLongitude = Math.Cos(longitude);
        for (var i = 0; i < segments; i++)
        {
            var bearing = 2 * Math.PI * i / segments;
            var north = Math.Cos(bearing) * sinRadius;
            var east = Math.Sin(bearing) * sinRadius;
            var horizontal = (cosLatitude * cosRadius) - (sinLatitude * north);
            var x = (horizontal * cosLongitude) - (east * sinLongitude);
            var y = (horizontal * sinLongitude) + (east * cosLongitude);
            var z = (sinLatitude * cosRadius) + (cosLatitude * north);
            var pointLatitude = Math.Atan2(z, Math.Sqrt((x * x) + (y * y))) / DegreesToRadians;
            var pointLongitude = Math.Atan2(y, x) / DegreesToRadians;
            points[i] = new GeoPoint(pointLatitude, pointLongitude, center.Altitude);
        }

        points[^1] = points[0];
        return points;
    }

    internal static void Validate(MapPreloadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Providers);
        ValidateRegion(request.Center, request.RadiusKm);

        if (request.MinZoom < 0 || request.MaxZoom > 30 || request.MinZoom > request.MaxZoom)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "The zoom range must be ordered and within 0 through 30."
            );
        }

        if (request.Concurrency is < 1 or > 64 || request.RequestDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Concurrency must be within 1 through 64 and delay must be non-negative."
            );
        }

        foreach (var provider in request.Providers)
        {
            ArgumentNullException.ThrowIfNull(provider);
            if (provider.TileSize <= 0 || string.IsNullOrWhiteSpace(provider.Info.Id))
            {
                throw new ArgumentException(
                    "Each provider must have a tile size and identifier.",
                    nameof(request)
                );
            }
        }
    }

    private static void ValidateRegion(GeoPoint center, double radiusKm)
    {
        if (
            !double.IsFinite(center.Latitude)
            || center.Latitude is < -90 or > 90
            || !double.IsFinite(center.Longitude)
            || center.Longitude is < -180 or > 180
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(center),
                "The center must be a valid geographic coordinate."
            );
        }

        if (!double.IsFinite(radiusKm) || radiusKm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radiusKm),
                "The radius must be finite and non-negative."
            );
        }
    }

    private static IEnumerable<TileKey> EnumerateLevel(
        MapPreloadRequest request,
        ITileProvider provider,
        int zoom,
        CancellationToken cancellationToken
    )
    {
        var count = 1 << zoom;
        var worldSize = (double)count * provider.TileSize;
        var projection = provider.Projection;
        var northLimit = projection
            .PixelsToWgs84(new Point(0, 0), zoom, provider.TileSize)
            .Latitude;

        // Exact bottom/right world boundaries wrap in the existing projection.
        var southLimit = projection
            .PixelsToWgs84(new Point(0, Math.BitDecrement(worldSize)), zoom, provider.TileSize)
            .Latitude;
        var angularRadius = Math.Min(request.RadiusKm / EarthRadiusKm, Math.PI);
        var latitudeRadius = angularRadius / DegreesToRadians;
        var north = Math.Min(request.Center.Latitude + latitudeRadius, northLimit);
        var south = Math.Max(request.Center.Latitude - latitudeRadius, southLimit);
        if (north < south)
        {
            yield break;
        }

        var top =
            projection.Wgs84ToPixels(new GeoPoint(north, 0, 0), zoom, provider.TileSize).Y
            / provider.TileSize;
        var bottom =
            projection.Wgs84ToPixels(new GeoPoint(south, 0, 0), zoom, provider.TileSize).Y
            / provider.TileSize;
        var minY = Math.Clamp((int)Math.Floor(Math.BitDecrement(top)), 0, count - 1);
        var maxY = Math.Clamp((int)Math.Floor(Math.BitIncrement(bottom)), 0, count - 1);
        var reachesPole = Math.Abs(request.Center.Latitude) + latitudeRadius >= 90;
        var longitudeRadius = reachesPole
            ? 180
            : Math.Asin(
                Math.Clamp(
                    Math.Sin(angularRadius) / Math.Cos(request.Center.Latitude * DegreesToRadians),
                    -1,
                    1
                )
            ) / DegreesToRadians;
        var left = (request.Center.Longitude - longitudeRadius + 180) / 360 * count;
        var right = (request.Center.Longitude + longitudeRadius + 180) / 360 * count;
        var minX = (long)Math.Floor(Math.BitDecrement(left));
        var maxX = (long)Math.Floor(Math.BitIncrement(right));
        if (reachesPole || maxX - minX + 1 >= count)
        {
            minX = 0;
            maxX = count - 1;
        }

        for (var y = minY; y <= maxY; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tileNorth = projection
                .PixelsToWgs84(new Point(0, (double)y * provider.TileSize), zoom, provider.TileSize)
                .Latitude;
            var tileSouth = projection
                .PixelsToWgs84(
                    new Point(
                        0,
                        Math.Min((double)(y + 1) * provider.TileSize, Math.BitDecrement(worldSize))
                    ),
                    zoom,
                    provider.TileSize
                )
                .Latitude;
            for (var unwrappedX = minX; unwrappedX <= maxX; unwrappedX++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var x = (int)(((unwrappedX % count) + count) % count);
                var west = ((double)x / count * 360) - 180;
                var east = ((double)(x + 1) / count * 360) - 180;
                if (Intersects(request.Center, angularRadius, tileSouth, tileNorth, west, east))
                {
                    yield return new TileKey(x, y, (ushort)zoom, provider);
                }
            }
        }
    }

    private static bool Intersects(
        GeoPoint center,
        double radius,
        double south,
        double north,
        double west,
        double east
    )
    {
        var longitudeDistance =
            center.Longitude >= west && center.Longitude <= east
                ? 0
                : Math.Min(
                    LongitudeDistance(center.Longitude, west),
                    LongitudeDistance(center.Longitude, east)
                );
        var centerLatitude = center.Latitude * DegreesToRadians;
        var southLatitude = south * DegreesToRadians;
        var northLatitude = north * DegreesToRadians;
        var a = Math.Sin(centerLatitude);
        var b = Math.Cos(centerLatitude) * Math.Cos(longitudeDistance * DegreesToRadians);
        var closestLatitude = Math.Clamp(Math.Atan2(a, b), southLatitude, northLatitude);

        // Maximizing the spherical dot product gives the shortest distance to the tile,
        // including intersections along an edge that contains no corner in the circle.
        var maximumDot = Math.Max(
            (a * Math.Sin(closestLatitude)) + (b * Math.Cos(closestLatitude)),
            Math.Max(
                (a * Math.Sin(southLatitude)) + (b * Math.Cos(southLatitude)),
                (a * Math.Sin(northLatitude)) + (b * Math.Cos(northLatitude))
            )
        );
        return maximumDot >= Math.Cos(radius) - 1E-14;
    }

    private static double LongitudeDistance(double first, double second)
    {
        var difference = Math.Abs(first - second);
        return Math.Min(difference, 360 - difference);
    }
}
