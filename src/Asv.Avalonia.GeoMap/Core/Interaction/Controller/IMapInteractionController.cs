using System.Diagnostics.CodeAnalysis;
using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Coordinates map interaction sessions and exposes interaction events.
/// </summary>
public interface IMapInteractionController
{
    /// <summary>
    /// Gets a value indicating whether a session is currently active.
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Gets the current interaction status text.
    /// </summary>
    IReadOnlyBindableReactiveProperty<string?> Status { get; }

    /// <summary>
    /// Gets the current interaction accent color.
    /// </summary>
    IReadOnlyBindableReactiveProperty<AsvColorKind?> Accent { get; }

    /// <summary>
    /// Starts a new interaction session if the controller is available.
    /// </summary>
    /// <param name="request">The interaction request to start.</param>
    /// <param name="session">When this method returns <c>true</c>, receives the active session.</param>
    /// <returns><c>true</c> if the session started successfully; otherwise, <c>false</c>.</returns>
    bool TryBegin(
        MapInteractionRequest request,
        [MaybeNullWhen(false)] out IMapInteractionSession session
    );

    /// <summary>
    /// Updates the status text for the specified session.
    /// </summary>
    /// <param name="session">The session to update.</param>
    /// <param name="value">The new status text.</param>
    void SetStatus(IMapInteractionSession session, string? value);

    /// <summary>
    /// Updates the accent color for the specified session.
    /// </summary>
    /// <param name="session">The session to update.</param>
    /// <param name="value">The new accent color.</param>
    void SetAccent(IMapInteractionSession session, AsvColorKind? value);

    /// <summary>
    /// Ends the specified session.
    /// </summary>
    /// <param name="session">The session to end.</param>
    void End(IMapInteractionSession session);

    /// <summary>
    /// Gets the stream of map click points received while a session is active.
    /// </summary>
    Observable<GeoPoint> Clicked { get; }

    /// <summary>
    /// Gets the stream of cursor positions received while a session is active.
    /// </summary>
    Observable<GeoPoint> CursorMoved { get; }

    /// <summary>
    /// Attaches the controller to a map view.
    /// </summary>
    /// <param name="map">The map control to attach.</param>
    void AttachMap(MapItemsControl map);

    /// <summary>
    /// Detaches the controller from the current map view.
    /// </summary>
    void DetachMap();
}
