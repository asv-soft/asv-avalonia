using System.Windows.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    private readonly IDialogService _dialogService;

    protected PageViewModel(
        NavigationId id,
        ICommandService cmd,
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
        _dialogService = dialogService;
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

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind? Status
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind StatusColor
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

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
            HasChanges.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public abstract IExportInfo Source { get; }
}
