using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class PropertyButtonViewModel : PropertyViewModel
{
    private readonly Func<CancellationToken, ValueTask> _execute;
    private CancellationTokenSource? _executeCancel;

    public PropertyButtonViewModel()
        : this(NavId.GenerateRandomAsString(), _ => ValueTask.CompletedTask)
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = "Execute";
        ShortHeader = "Run";
        Description = "Button property with progress, update marker, and error state.";
        Icon = MaterialIconKind.PlayCircle;
        IconColor = AsvColorKind.Success;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                MarkUpdated();
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                IsBusy = !IsBusy;
            })
            .AddTo(ref DisposableBag);

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ErrorMessage = ErrorMessage == null ? "Command execution error" : null;
            })
            .AddTo(ref DisposableBag);
    }

    public PropertyButtonViewModel(
        string typeId,
        Func<CancellationToken, ValueTask> execute,
        Observable<bool>? canExecute = null
    )
        : base(typeId)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;

        if (canExecute is null)
        {
            ExecuteCommand = new ReactiveCommand(
                (_, cancel) => Execute(cancel),
                AwaitOperation.Drop
            ).AddTo(ref DisposableBag);
        }
        else
        {
            ExecuteCommand = canExecute
                .ToReactiveCommand<Unit>(
                    (_, cancel) => Execute(cancel),
                    awaitOperation: AwaitOperation.Drop
                )
                .AddTo(ref DisposableBag);
        }
    }

    public ReactiveCommand<Unit> ExecuteCommand { get; }

    public async ValueTask Execute(CancellationToken cancel = default)
    {
        if (IsBusy)
        {
            return;
        }

        ClearModelErrors();
        _executeCancel?.Cancel(false);
        _executeCancel?.Dispose();
        _executeCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        IsBusy = true;
        try
        {
            await _execute(_executeCancel.Token);
            MarkUpdated();
        }
        catch (OperationCanceledException) when (_executeCancel.IsCancellationRequested)
        {
            // Cancellation is an expected command lifecycle state.
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            _executeCancel?.Dispose();
            _executeCancel = null;
            IsBusy = false;
        }
    }
}
