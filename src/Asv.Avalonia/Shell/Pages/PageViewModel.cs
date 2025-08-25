using System.Windows.Input;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageConfig()
{
    public PageState PageState { get; set; } = PageState.Tab;
}

public abstract class PageViewModel<TContext, TConfig>
    : ExtendableViewModel<TContext>,
        IPage,
        IConfigurable<TConfig>
    where TContext : class, IPage
    where TConfig : PageConfig, new()
{
    public IConfiguration CfgService { get; init; }
    public TConfig Config { get; init; }

    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
        IConfiguration cfgService,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        CfgService = cfgService;
        Config = cfgService.Get<TConfig>();
        State = new BindableReactiveProperty<PageState>(Config.PageState);

        _sub1 = State.Skip(1).Subscribe(_ => HasChanges.Value = true);
        _sub2 = HasChanges
            .Skip(1)
            .Where(hasChanges => hasChanges)
            .SubscribeAwait(
                async (_, ct) =>
                {
                    await SaveChanges(ct);
                    HasChanges.Value = false;
                }
            );
    }

    public async ValueTask TryCloseAsync(bool isForce)
    {
        Logger.ZLogTrace($"Try close page {Title}[{Id}]");
        try
        {
            if (!isForce)
            {
                var reasons = await this.RequestChildCloseApproval();
                if (reasons.Count != 0)
                {
                    return;
                }
            }

            await this.RequestClose();
        }
        catch (Exception e)
        {
            Logger.ZLogError(e, $"Error on close page {Title}[{Id}]: {e.Message}");
        }
    }

    public MaterialIconKind Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public string Title
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommandHistory History { get; }
    public BindableReactiveProperty<bool> HasChanges { get; }
    public BindableReactiveProperty<PageState> State { get; }
    public ICommand TryClose { get; }

    public virtual ValueTask SaveChanges(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Config.PageState = State.Value;
        CfgService.Set(Config);

        return ValueTask.CompletedTask;
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            HasChanges.Dispose();
            State.Dispose();
            _sub1.Dispose();
            _sub2.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public abstract IExportInfo Source { get; }
}
