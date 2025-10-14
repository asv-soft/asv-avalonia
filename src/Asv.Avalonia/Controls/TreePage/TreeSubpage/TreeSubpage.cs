using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public abstract class TreeSubpage : RoutableViewModel, ITreeSubpage
{
    private readonly ILayoutService _layoutService;

    protected TreeSubpage(NavigationId id, ILayoutService layout, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        _layoutService = layout;
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
            Menu.RemoveAll();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case SaveLayoutEvent:
            {
                await HandleSaveLayout();
                break;
            }

            case LoadLayoutEvent:
            {
                await HandleLoadLayout();
                break;
            }

            case SaveLayoutToFileEvent:
                await HandleSaveLayout();
                _layoutService.FlushFromMemory(this);
                break;

            default:
                break;
        }

        await base.InternalCatchEvent(e);
    }

    protected virtual ValueTask HandleSaveLayout()
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask HandleLoadLayout()
    {
        return ValueTask.CompletedTask;
    }
}

public abstract class TreeSubpage<TContext>(
    NavigationId id,
    ILayoutService layout,
    ILoggerFactory loggerFactory
) : TreeSubpage(id, layout, loggerFactory), ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}
