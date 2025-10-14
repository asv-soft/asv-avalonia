namespace Asv.Avalonia;

public class SaveLayoutToFileGlobalEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble) { }

public static class SaveStateToFileGlobal
{
    public static ValueTask RequestSaveAllLayoutToFile(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutToFileGlobalEvent(src));
    }
}
