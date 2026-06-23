namespace Asv.Avalonia.GeoMap;

public sealed record MapInteractionRequest
{
    public string? Status { get; init; }

    public AsvColorKind? Accent { get; init; }

    public MapInteractionStartMode StartMode { get; init; } = MapInteractionStartMode.FailIfBusy;
}

public enum MapInteractionStartMode
{
    FailIfBusy,
    CancelCurrent,
}
