using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Represents an active map interaction session.
/// </summary>
public interface IMapInteractionSession : IDisposable
{
    /// <summary>
    /// Gets the lifecycle policy that controls how the session can be interrupted.
    /// </summary>
    MapInteractionLifecycle Lifecycle { get; }

    /// <summary>
    /// Gets the disposable container used to track subscriptions and cleanup actions for the session.
    /// </summary>
    CompositeDisposable Disposable { get; }
}
