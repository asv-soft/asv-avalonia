using Asv.Common;

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
        CancellationToken cancel = default,
        RoutingStrategy routingStrategy = RoutingStrategy.Tunnel
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        return src.Rise(new LoadLayoutEvent(src, layoutService, routingStrategy), cancel);
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

        return src.Rise(new LoadLayoutEvent(src, layoutService, RoutingStrategy.Direct), cancel);
    }

    public static async ValueTask<TConfig> HandleLoadLayoutAsync<TConfig>(
        this IRoutable target,
        LoadLayoutEvent e,
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
        this IRoutable target,
        LoadLayoutEvent e,
        Action<TConfig> loadCallback
    )
        where TConfig : class, new()
    {
        var config = e.LayoutService.Get<TConfig>(target);
        loadCallback(config);
        return config;
    }
}
