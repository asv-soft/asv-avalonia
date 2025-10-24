namespace Asv.Avalonia;

public enum FlushingStrategy
{
    FlushExclusively,
    FlushBothViewModelAndView,
}

public class SaveLayoutEvent(
    IRoutable source,
    ILayoutService layoutService,
    bool isFlushToFile = false,
    RoutingStrategy routingStrategy = LayoutEventBase.DefaultRoutingStrategy
) : LayoutEventBase(source, layoutService, routingStrategy)
{
    public bool IsFlushToFile => isFlushToFile;
}

public static class SaveLayoutMixin
{
    public static ValueTask RequestSaveLayout(
        this IRoutable src,
        ILayoutService layoutService,
        CancellationToken cancel = default,
        RoutingStrategy routingStrategy = LayoutEventBase.DefaultRoutingStrategy
    )
    {
        return InternalSaveLayout(src, layoutService, false, routingStrategy, cancel);
    }

    public static ValueTask RequestSaveLayoutToFile(
        this IRoutable src,
        ILayoutService layoutService,
        CancellationToken cancel = default,
        RoutingStrategy routingStrategy = LayoutEventBase.DefaultRoutingStrategy
    )
    {
        return InternalSaveLayout(src, layoutService, true, routingStrategy, cancel);
    }

    public static async ValueTask HandleSaveLayoutAsync<TConfig>(
        this SaveLayoutEvent e,
        IRoutable target,
        TConfig cfgToSave,
        Func<TConfig, CancellationToken, ValueTask> saveCallback,
        CancellationToken cancel = default,
        FlushingStrategy flushingStrategy = FlushingStrategy.FlushExclusively
    )
        where TConfig : class, new()
    {
        await saveCallback(cfgToSave, cancel);
        InternalHandleSaveLayout(e, target, cfgToSave, flushingStrategy);
    }

    public static void HandleSaveLayout<TConfig>(
        this SaveLayoutEvent e,
        IRoutable target,
        TConfig cfgToSave,
        Action<TConfig> saveCallback,
        FlushingStrategy flushingStrategy = FlushingStrategy.FlushExclusively
    )
        where TConfig : class, new()
    {
        saveCallback(cfgToSave);
        InternalHandleSaveLayout(e, target, cfgToSave, flushingStrategy);
    }

    private static void InternalHandleSaveLayout<TConfig>(
        SaveLayoutEvent e,
        IRoutable target,
        TConfig cfg,
        FlushingStrategy flushingStrategy
    )
        where TConfig : class, new()
    {
        e.LayoutService.SetInMemory(target, cfg);

        if (!e.IsFlushToFile)
        {
            return;
        }

        switch (flushingStrategy)
        {
            case FlushingStrategy.FlushExclusively:
                e.LayoutService.FlushFromMemory(target);
                break;
            case FlushingStrategy.FlushBothViewModelAndView:
                e.LayoutService.FlushFromMemoryViewModelAndView(target);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(flushingStrategy),
                    flushingStrategy,
                    null
                );
        }
    }

    private static ValueTask InternalSaveLayout(
        this IRoutable src,
        ILayoutService layoutService,
        bool isFlushToFile,
        RoutingStrategy routingStrategy = LayoutEventBase.DefaultRoutingStrategy,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return default;
        }

        return src.Rise(new SaveLayoutEvent(src, layoutService, isFlushToFile, routingStrategy));
    }
}
