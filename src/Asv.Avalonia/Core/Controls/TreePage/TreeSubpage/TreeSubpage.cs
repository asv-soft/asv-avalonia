using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class TreeSubpage : RoutableViewModel, ITreeSubpage
{
    protected TreeSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];

    public override IEnumerable<IRoutable> GetChildren() => Menu;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Menu.RemoveAll();
        }

        base.Dispose(disposing);
    }
}

public abstract class TreeSubpage<TContext>(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage(id, loggerFactory),
        ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}
