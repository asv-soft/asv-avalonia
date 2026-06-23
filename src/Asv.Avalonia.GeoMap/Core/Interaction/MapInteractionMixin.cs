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
        if (!map.Interaction.TryBegin(out var session))
        {
            return null;
        }

        using (session)
        {
            session.Status = status;

            var tcs = new TaskCompletionSource<GeoPoint?>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            if (preview is not null)
            {
                map.Anchors.Add(preview);
                session
                    .CursorMoved.Subscribe(cursor => preview.Location = cursor)
                    .AddTo(session.Disposable);
                Disposable.Create(() => map.Anchors.Remove(preview)).AddTo(session.Disposable);
            }

            session
                .Clicked.Take(1)
                .Subscribe(point => tcs.TrySetResult(point))
                .AddTo(session.Disposable);
            Disposable.Create(() => tcs.TrySetResult(null)).AddTo(session.Disposable);

            await using var reg = ct.Register(() => Dispatcher.UIThread.Post(session.Dispose));
            return await tcs.Task;
        }
    }
}
