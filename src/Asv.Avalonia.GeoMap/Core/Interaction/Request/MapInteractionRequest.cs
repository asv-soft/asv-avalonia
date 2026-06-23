namespace Asv.Avalonia.GeoMap;

public sealed record MapInteractionRequest
{
    public string? Status { get; init; }

    public AsvColorKind? Accent { get; init; }

    public MapInteractionLifecycle Lifecycle { get; init; } = MapInteractionLifecycle.LockFree;
}

public enum MapInteractionLifecycle
{
    Lock,
    LockFree,
}
