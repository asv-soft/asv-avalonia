using Asv.Common;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

public static class MapInteractionMixin
{
    public static Task<GeoPoint?> PickPointAsync(
        this IMap map,
        MapInteractionRequest request,
        IMapAnchor? preview = null,
        string? status = null,
        CancellationToken ct = default
    )
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
            PickPointOnUiThreadAsync(map, request, preview, status, ct)
        );
    }

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
