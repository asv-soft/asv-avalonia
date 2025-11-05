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

public abstract class PageViewModel<TContext, TConfig> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
    where TConfig : PageConfig, new()
{
    protected readonly IConfiguration CfgService;
    private readonly IDialogService _dialogService;
    protected readonly TConfig Config;

    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
        IConfiguration cfgService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(id, loggerFactory)
    {
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        CfgService = cfgService;
        _dialogService = dialogService;
        Config = cfgService.Get<TConfig>();
        State = new BindableReactiveProperty<PageState>(Config.PageState);

        _sub1 = State.Subscribe(_ => HasChanges.Value = true);
        _sub2 = HasChanges.SubscribeAwait(
            async (hasChanges, ct) =>
            {
                if (hasChanges)
                {
                    await SafeChanges(ct);
                    HasChanges.Value = false;
                }
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
                    var vm = _dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
                    var result = await vm.ShowDialogAsync(
                        new YesOrNoDialogPayload
                        {
                            Title = "Close page anyway?",
                            Message = string.Join('\n', reasons.Select(r => r.Message)),
                        }
                    );
                    Logger.ZLogTrace($"Try close page {Title}[{Id}] result: {result}");
                    if (!result)
                    {
                        return;
                    }
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

    protected virtual ValueTask SafeChanges(CancellationToken cancellationToken)
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
