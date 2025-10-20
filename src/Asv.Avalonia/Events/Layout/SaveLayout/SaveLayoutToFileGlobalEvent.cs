namespace Asv.Avalonia;

public class SaveLayoutToFileGlobalEvent(IRoutable source, ILayoutService layoutService)
    : LayoutEventBase(source, layoutService, RoutingStrategy.Bubble) { }

public static class SaveStateToFileGlobal
{
    public static ValueTask RequestSaveAllLayoutToFile(
        this IRoutable src,
        ILayoutService layoutService,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutToFileGlobalEvent(src, layoutService));
    }
}
