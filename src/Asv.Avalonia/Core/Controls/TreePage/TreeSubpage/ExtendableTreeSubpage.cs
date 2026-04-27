using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class ExtendableTreeSubpage<TSubContext>
    : ViewModel<TSubContext>,
        ITreeSubpage
    where TSubContext : class, ITreeSubpage
{
    protected ExtendableTreeSubpage(
        string id,
        IExtensionService ext
    )
        : base(id, default, ext)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];

    public override IEnumerable<IViewModel> GetChildren() => Menu;
}

public abstract class ExtendableTreeSubpage<TContext, TSubContext>
    : ViewModel<TSubContext>, ITreeSubpage
    where TSubContext : class, ITreeSubpage
    where TContext : class, IPage
{
    protected ExtendableTreeSubpage(
        string id,
        IExtensionService ext
    )
        : base(id, default, ext)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public abstract ValueTask Init(TContext context);

    public override IEnumerable<IViewModel> GetChildren() => Menu;
}
