namespace Asv.Avalonia;

public sealed class LoadLayoutEvent(
    IRoutable source,
    ILayoutService layoutService,
    RoutingStrategy routingStrategy = RoutingStrategy.Tunnel
) : LayoutEventBase(source, layoutService, routingStrategy) { }

public static class LoadLayoutMixin
{
    public static ValueTask RequestLoadLayout(
        this IRoutable src,
        ILayoutService layoutService,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadLayoutEvent(src, layoutService));
    }

    public static ValueTask RequestLoadLayoutForSelfOnly(
        this IRoutable src,
        ILayoutService layoutService,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadLayoutEvent(src, layoutService, RoutingStrategy.Direct));
    }

    public static async ValueTask<TConfig> HandleLoadLayoutAsync<TConfig>(
        this LoadLayoutEvent e,
        IRoutable target,
        Func<TConfig, CancellationToken, ValueTask> loadCallback,
        CancellationToken cancel = default
    )
        where TConfig : class, new()
    {
        var config = e.LayoutService.Get<TConfig>(target);
        await loadCallback(config, cancel);
        return config;
    }

    public static TConfig HandleLoadLayout<TConfig>(
        this LoadLayoutEvent e,
        IRoutable target,
        Action<TConfig> loadCallback
    )
        where TConfig : class, new()
    {
        var config = e.LayoutService.Get<TConfig>(target);
        loadCallback(config);
        return config;
    }
}
