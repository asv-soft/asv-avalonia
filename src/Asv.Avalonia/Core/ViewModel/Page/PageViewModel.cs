using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ViewModel<TContext>, IPage
    where TContext : class, IPage
{
    private readonly UnsavedChangesDialogPrefab _unsavedChangesDialogPrefab;
    private readonly ILogger<PageViewModel<TContext>> _logger;

    protected PageViewModel(
        string typeId,
        IPageContext context,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(typeId, context.NavArgs, ext)
    {
        History = cmd.CreateHistory(this);
        _logger = loggerFactory.CreateLogger<PageViewModel<TContext>>();
        
        UndoHistory = new UndoHistory<IViewModel>(this, context.UndoStore)
            .AddTo(ref DisposableBag);
        Icon = MaterialIconKind.Window;
        Header = typeId;
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        _unsavedChangesDialogPrefab = dialogService.GetDialogPrefab<UnsavedChangesDialogPrefab>();
    }

    public async ValueTask TryCloseAsync(bool isForce)
    {
        _logger.ZLogTrace($"Try close page {Header}[{Id}]");
        try
        {
            if (!isForce)
            {
                var reasons = await this.RequestChildCloseApproval();
                if (reasons.Count != 0)
                {
                    var result = await _unsavedChangesDialogPrefab.ShowDialogAsync(
                        new UnsavedChangesDialogPayload
                        {
                            Restrictions = reasons,
                            Title = RS.PageViewModel_CloseConfirmDialog_Title,
                        }
                    );
                    _logger.ZLogTrace($"Try close page {Header}[{Id}] result: {result}");
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
            _logger.ZLogError(e, $"Error on close page {Header}[{Id}]: {e.Message}");
        }
    }

    public MaterialIconKind? Icon
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

    public string Header
    {
        get;
        set => SetField(ref field, value);
    }

    public ICommandHistory History { get; }
    public IUndoHistory<IViewModel> UndoHistory { get; }
    public ICommand TryClose { get; }

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            History.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
