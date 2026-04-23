using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

public class SaveLayoutToFileGlobalEvent(IViewModel source, ILayoutService layoutService)
    : LayoutEventBase(source, layoutService, RoutingStrategy.Bubble) { }

public static class SaveLayoutToFileGlobalMixin
{
    public static ValueTask RequestSaveAllLayoutToFile(
        this IViewModel src,
        ILayoutService layoutService,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutToFileGlobalEvent(src, layoutService), cancel);
    }
}
