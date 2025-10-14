namespace Asv.Avalonia;

public sealed class LoadLayoutEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel) { }

public static class LoadLayoutMixin
{
    public static ValueTask RequestLoadLayout(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadLayoutEvent(src));
    }
}
