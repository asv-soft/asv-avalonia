using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class ExtendableTreeSubpage<TSubContext>
    : ExtendableViewModel<TSubContext>,
        ITreeSubpage
    where TSubContext : class, ITreeSubpage
{
    protected ExtendableTreeSubpage(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(id, loggerFactory, ext)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];

    public override IEnumerable<IRoutable> GetChildren() => Menu;
}

public abstract class ExtendableTreeSubpage<TContext, TSubContext>
    : ExtendableViewModel<TSubContext>,
        ITreeSubpage<TContext>
    where TSubContext : class, ITreeSubpage
    where TContext : class, IPage
{
    protected ExtendableTreeSubpage(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(id, loggerFactory, ext)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public abstract ValueTask Init(TContext context);

    public override IEnumerable<IRoutable> GetChildren() => Menu;
}
