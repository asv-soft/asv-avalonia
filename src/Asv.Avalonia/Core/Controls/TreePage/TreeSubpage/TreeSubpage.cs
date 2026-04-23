using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class TreeSubpage : ViewModelBase, ITreeSubpage
{
    protected TreeSubpage(string typeId, ILoggerFactory loggerFactory)
        : base(typeId, loggerFactory)
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

public abstract class TreeSubpage<TContext>(string typeId, ILoggerFactory loggerFactory)
    : TreeSubpage(typeId, loggerFactory),
        ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}
