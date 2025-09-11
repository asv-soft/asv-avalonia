using Asv.Common;
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

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case SaveStateEvent saveState:
            {
                if (saveState.Source is not ITreeSubpage page)
                {
                    break;
                }

                if (page.Id != Id)
                {
                    break;
                }

                await HandleSubpageSave();
                break;
            }

            case LoadStateEvent loadState:
            {
                if (loadState.Source is not ITreeSubpage page)
                {
                    break;
                }

                if (page.Id != Id)
                {
                    break;
                }

                await HandleSubpageLoad();
                break;
            }

            default:
                break;
        }

        await base.InternalCatchEvent(e);
    }

    protected virtual ValueTask HandleSubpageSave()
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask HandleSubpageLoad()
    {
        return ValueTask.CompletedTask;
    }
}

public abstract class TreeSubpage<TContext>(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage(id, loggerFactory),
        ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}
