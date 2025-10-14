namespace Asv.Avalonia;

public class SaveLayoutEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel) { }

public static class SaveLayoutMixin
{
    public static ValueTask RequestSaveLayout(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutEvent(src));
    }
}
