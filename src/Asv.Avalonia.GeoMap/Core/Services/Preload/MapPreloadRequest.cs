using Asv.Common;

namespace Asv.Avalonia.GeoMap;

public sealed record MapPreloadRequest
{
    public GeoPoint Center { get; init; }
    public double RadiusKm { get; init; } = 5;
    public int MinZoom { get; init; } = 10;
    public int MaxZoom { get; init; } = 15;
    public IReadOnlyList<ITileProvider> Providers { get; init; } = [];
    public TimeSpan RequestDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public int Concurrency { get; init; } = 2;
}
