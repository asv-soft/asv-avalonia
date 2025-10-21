namespace Asv.Avalonia;

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
        CancellationToken cancel = default
    )
        where TConfig : class, new()
    {
        await saveCallback(cfgToSave, cancel);
        InternalHandleSaveLayout(e, target, cfgToSave);
    }

    public static void HandleSaveLayout<TConfig>(
        this SaveLayoutEvent e,
        IRoutable target,
        TConfig cfgToSave,
        Action<TConfig> saveCallback
    )
        where TConfig : class, new()
    {
        saveCallback(cfgToSave);
        InternalHandleSaveLayout(e, target, cfgToSave);
    }

    private static void InternalHandleSaveLayout<TConfig>(
        SaveLayoutEvent e,
        IRoutable target,
        TConfig cfg
    )
        where TConfig : class, new()
    {
        e.LayoutService.SetInMemory(target, cfg);

        if (e.IsFlushToFile)
        {
            e.LayoutService.FlushFromMemory(target);
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
