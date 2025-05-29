using Asv.Common;
using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreeSubpage : IRoutable, IExportable
{
    MenuTree MenuView { get; }
    ObservableList<IMenuItem> Menu { get; }
}

public interface ITreeSubpage<in TContext> : ITreeSubpage
    where TContext : class, IPage
{
    ValueTask Init(TContext context);
}

public abstract class TreeSubpage : RoutableViewModel, ITreeSubpage
{
    protected TreeSubpage(NavigationId id)
        : base(id)
    {
        Menu.DisposeMany().DisposeItWith(Disposable);
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public abstract IExportInfo Source { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Menu.Clear();
        }

        base.Dispose(disposing);
    }
}

public abstract class TreeSubpage<TContext>(NavigationId id)
    : TreeSubpage(id),
        ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}
