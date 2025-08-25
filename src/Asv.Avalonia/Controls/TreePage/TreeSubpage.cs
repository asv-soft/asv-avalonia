using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

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
}

public abstract class TreeSubpage<TContext>(NavigationId id, ILoggerFactory loggerFactory)
    : TreeSubpage(id, loggerFactory),
        ITreeSubpage<TContext>
    where TContext : class, IPage
{
    public abstract ValueTask Init(TContext context);
}

public abstract class TreeSubpage<TContext, TConfig> : TreeSubpage<TContext>, IConfigurable<TConfig>
    where TContext : class, IPage
    where TConfig : new()
{
    public IConfiguration CfgService { get; init; }
    public TConfig Config { get; init; }
    public BindableReactiveProperty<bool> HasChanges { get; }

    protected TreeSubpage(NavigationId id, IConfiguration cfg, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        CfgService = cfg;
        Config = CfgService.Get<TConfig>();
        HasChanges = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        HasChanges
            .Skip(1)
            .Where(hasChanges => hasChanges)
            .SubscribeAwait(
                async (_, ct) =>
                {
                    await SaveChanges(ct);
                    HasChanges.Value = false;
                }
            )
            .DisposeItWith(Disposable);
    }

    public virtual ValueTask SaveChanges(CancellationToken cancellationToken)
    {
        CfgService.Set(Config);
        return ValueTask.CompletedTask;
    }
}
