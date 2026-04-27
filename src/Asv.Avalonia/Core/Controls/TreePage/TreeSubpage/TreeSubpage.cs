using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class TreeSubpage : ViewModel, ITreeSubpage
{
    protected TreeSubpage(string typeId, NavArgs args)
        : base(typeId, args)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];

    public override IEnumerable<IViewModel> GetChildren() => Menu;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Menu.RemoveAll();
        }

        base.Dispose(disposing);
    }
}

public abstract class TreeSubpage<TContext>(string typeId, ITreeSubPageContext<TContext> context)
    : TreeSubpage(typeId, context.Args)
    where TContext : class, IPage
{
    
}
