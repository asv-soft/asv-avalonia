using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

public abstract class TreeVisitorEvent(IViewModel source)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Tunnel)
{
    public abstract void Visit(IViewModel source);

    public static async ValueTask<IReadOnlyList<TViewModel>> VisitAll<TViewModel>(
        IViewModel context,
        CancellationToken cancel = default
    )
        where TViewModel : IViewModel
    {
        var root = context.GetRoot();
        var items = new List<TViewModel>();
        var ev = new TreeVisitorEvent<TViewModel>(root, items.Add);
        await root.Rise(ev, cancel);
        return items;
    }
}

public class TreeVisitorEvent<TViewModel>(IViewModel source, Action<TViewModel> visitor)
    : TreeVisitorEvent(source)
    where TViewModel : IViewModel
{
    public override void Visit(IViewModel source)
    {
        if (source is TViewModel target)
        {
            visitor(target);
        }
    }
}
