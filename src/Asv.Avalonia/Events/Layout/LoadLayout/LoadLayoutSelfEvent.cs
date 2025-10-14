namespace Asv.Avalonia;

public class LoadLayoutSelfEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Direct) { }

public static class LoadLayoutSelfMixin
{
    public static ValueTask RequestLoadLayoutForSelfOnly(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadLayoutSelfEvent(src));
    }
}
