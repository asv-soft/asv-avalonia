using R3;

namespace Asv.Avalonia;

public class SaveStateEvent(IRoutable source) : AsyncRoutedEvent(source, RoutingStrategy.Bubble) { }

public static class SaveStateMixin
{
    public static ValueTask RequestSaveState(this IRoutable src, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveStateEvent(src));
    }

    public static IDisposable SubscribeSaveState<T>(
        this Observable<T> source,
        IRoutable routable,
        int skip = 1
    )
    {
        return source
            .Skip(skip)
            .SubscribeAwait(async (_, ct) => await routable.RequestSaveState(ct));
    }
}
