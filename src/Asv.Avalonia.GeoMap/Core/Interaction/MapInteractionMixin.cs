using Asv.Common;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Convenience extensions for starting map interaction sessions from an <see cref="IMap"/>.
/// </summary>
public static class MapInteractionMixin
{
    /// <summary>
    /// Picks a point using a custom interaction request.
    /// </summary>
    /// <param name="map">The map to use for picking.</param>
    /// <param name="request">The interaction request that controls the session lifecycle and UI state.</param>
    /// <param name="preview">Optional preview anchor that follows the cursor until a point is selected.</param>
    /// <param name="ct">A cancellation token that ends the interaction when signaled.</param>
    /// <returns>The selected point, or <c>null</c> if the interaction could not be started or was canceled.</returns>
    public static Task<GeoPoint?> PickPointAsync(
        this IMap map,
        MapInteractionRequest request,
        IMapAnchor? preview = null,
        CancellationToken ct = default
    )
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
            PickPointOnUiThreadAsync(map, request, preview, null, ct)
        );
    }

    /// <summary>
    /// Picks a point using the default interaction request shape.
    /// </summary>
    /// <param name="map">The map to use for picking.</param>
    /// <param name="preview">Optional preview anchor that follows the cursor until a point is selected.</param>
    /// <param name="status">Optional status text shown while the point is being picked.</param>
    /// <param name="ct">A cancellation token that ends the interaction when signaled.</param>
    /// <returns>The selected point, or <c>null</c> if the interaction could not be started or was canceled.</returns>
    public static Task<GeoPoint?> PickPointAsync(
        this IMap map,
        IMapAnchor? preview = null,
        string? status = null,
        CancellationToken ct = default
    )
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
            PickPointOnUiThreadAsync(map, null, preview, status, ct)
        );
    }

    /// <summary>
    /// Runs point picking on the UI thread and waits for either a click or cancellation.
    /// </summary>
    /// <param name="map">The map to use for picking.</param>
    /// <param name="request">The optional interaction request.</param>
    /// <param name="preview">Optional preview anchor that follows the cursor until a point is selected.</param>
    /// <param name="status">Optional status text used when no explicit request is provided.</param>
    /// <param name="ct">A cancellation token that ends the interaction when signaled.</param>
    /// <returns>The selected point, or <c>null</c> if the interaction could not be started or was canceled.</returns>
    private static async Task<GeoPoint?> PickPointOnUiThreadAsync(
        IMap map,
        MapInteractionRequest? request,
        IMapAnchor? preview,
        string? status,
        CancellationToken ct
    )
    {
        var internalRequest =
            request ?? new MapInteractionRequest { Status = status, Accent = AsvColorKind.Warning };

        if (!map.Interaction.TryBegin(internalRequest, out var session))
        {
            return null;
        }

        using (session)
        {
            var tcs = new TaskCompletionSource<GeoPoint?>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            if (preview is not null)
            {
                map.Anchors.Add(preview);
                map.Interaction.CursorMoved.Subscribe(cursor => preview.Location = cursor)
                    .AddTo(session.Disposable);
                Disposable.Create(() => map.Anchors.Remove(preview)).AddTo(session.Disposable);
            }

            map.Interaction.Clicked.Take(1)
                .Subscribe(point => tcs.TrySetResult(point))
                .AddTo(session.Disposable);
            session.Disposable.AddAction(() => tcs.TrySetResult(null));

            await using var reg = ct.Register(() => map.Interaction.End(session));
            return await tcs.Task;
        }
    }
}
