namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Describes how a map interaction session should present itself and when it may be interrupted.
/// </summary>
public sealed record MapInteractionRequest
{
    /// <summary>
    /// Gets or sets the status text shown while the interaction is active.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets or sets the accent color shown while the interaction is active.
    /// </summary>
    public AsvColorKind? Accent { get; init; }

    /// <summary>
    /// Gets or sets the lifecycle policy for the interaction session.
    /// </summary>
    public MapInteractionLifecycle Lifecycle { get; init; } = MapInteractionLifecycle.LockFree;
}

/// <summary>
/// Defines whether an active interaction blocks new sessions or allows them to replace the current one.
/// </summary>
public enum MapInteractionLifecycle
{
    /// <summary>
    /// Prevents a new interaction from starting while one is already active.
    /// </summary>
    Lock,

    /// <summary>
    /// Allows a new interaction to replace the current one.
    /// </summary>
    LockFree,
}
