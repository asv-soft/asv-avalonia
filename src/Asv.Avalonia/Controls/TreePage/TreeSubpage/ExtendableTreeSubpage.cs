using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class ExtendableTreeSubpage<TSubContext>
    : ExtendableViewModel<TSubContext>,
        ITreeSubpage
    where TSubContext : class, ITreeSubpage
{
    protected ExtendableTreeSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public abstract IExportInfo Source { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;
}

public abstract class ExtendableTreeSubpage<TContext, TSubContext>
    : ExtendableViewModel<TSubContext>,
        ITreeSubpage<TContext>
    where TSubContext : class, ITreeSubpage
    where TContext : class, IPage
{
    protected ExtendableTreeSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public abstract IExportInfo Source { get; }
    public abstract ValueTask Init(TContext context);

    public override IEnumerable<IRoutable> GetRoutableChildren() => Menu;
}
