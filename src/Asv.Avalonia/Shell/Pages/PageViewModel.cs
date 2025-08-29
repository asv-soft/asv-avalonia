using System.Windows.Input;
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
    protected readonly IStateSaver<TConfig> StateSaver;

    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
        IStateSaverFactory stateFactory,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        StateSaver = stateFactory.Create<TConfig>();
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        State = new BindableReactiveProperty<PageState>(StateSaver.Config.PageState);

        _sub1 = StateSaver.StartTracking(State, (v, c) => c.PageState = v);
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
    public BindableReactiveProperty<PageState> State { get; }
    public ICommand TryClose { get; }

    #region Dispose

    private readonly IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            State.Dispose();
            _sub1.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public abstract IExportInfo Source { get; }
}
