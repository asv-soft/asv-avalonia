namespace Asv.Avalonia;

public class SaveLayoutToFileEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel) { }

public static class SaveLayoutToFile
{
    public static ValueTask RequestSaveLayoutToFile(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutToFileEvent(src));
    }
}
