using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public abstract class PageViewModel<TContext> : ExtendableViewModel<TContext>, IPage
    where TContext : class, IPage
{
    private readonly UnsavedChangesDialogPrefab _unsavedChangesDialogPrefab;

    protected PageViewModel(
        string typeId,
        NavArgs args,
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(typeId, args, loggerFactory, ext)
    {
        History = cmd.CreateHistory(this);
        
        UndoHistory = new UndoHistory<IViewModel>(this, new JsonUndoHistoryStore())
            .AddTo(ref DisposableBag);
        Icon = MaterialIconKind.Window;
        Title = typeId;
        HasChanges = new BindableReactiveProperty<bool>(false);
        TryClose = new BindableAsyncCommand(ClosePageCommand.Id, this);
        _unsavedChangesDialogPrefab = dialogService.GetDialogPrefab<UnsavedChangesDialogPrefab>();
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
                    var result = await _unsavedChangesDialogPrefab.ShowDialogAsync(
                        new UnsavedChangesDialogPayload
                        {
                            Restrictions = reasons,
                            Title = RS.PageViewModel_CloseConfirmDialog_Title,
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
    public IUndoHistory<IViewModel> UndoHistory { get; }
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
}
