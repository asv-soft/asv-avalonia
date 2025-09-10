namespace Asv.Avalonia;

public sealed class LoadStateEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble) { }

public static class LoadStateMixin
{
    public static ValueTask RequestLoadState(this IRoutable src, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadStateEvent(src));
    }
}
