using System.Windows.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    private readonly ILayoutService _layoutService;

    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        _layoutService = layoutService;
        History = cmd.CreateHistory(this);
        Icon = MaterialIconKind.Window;
        Title = id.ToString();
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
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
    public ICommand TryClose { get; }

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

    #region Dispose

    private readonly IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            HasChanges.Dispose();
            _sub1.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public abstract IExportInfo Source { get; }
}
