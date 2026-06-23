using Asv.Common;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

public static class MapInteractionMixin
{
    public static async Task<GeoPoint?> PickPointAsync(
        this IMap map,
        IMapAnchor? preview = null,
        string? status = null,
        CancellationToken ct = default
    )
    {
        if (!map.Interaction.TryActivate<PointInputMode>(out var mode, out var scope))
        {
            return null;
        }

        var tcs = new TaskCompletionSource<GeoPoint?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        if (preview is not null)
        {
            map.Anchors.Add(preview);
            mode.CursorMoved.Subscribe(cursor => preview.Location = cursor).AddTo(scope);
            Disposable.Create(() => map.Anchors.Remove(preview)).AddTo(scope);
        }

        mode.Clicked.Take(1)
            .Subscribe(point =>
            {
                tcs.TrySetResult(point);
                map.Interaction.Deactivate();
            })
            .AddTo(scope);

        Disposable.Create(() => tcs.TrySetResult(null)).AddTo(scope);

        map.Interaction.Status.Value = status;

        await using var reg = ct.Register(() =>
            Dispatcher.UIThread.Post(map.Interaction.Deactivate)
        );
        return await tcs.Task;
    }
}
