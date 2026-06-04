using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyComboBoxViewModel : PropertyViewModel
{
    private CancellationTokenSource? _selectCancel;
    private readonly IUndoChangeSink<ValueUndoChange<IHeadlinedViewModel>>? _undoValueSink;

    protected PropertyComboBoxViewModel(string typeId, bool enableUndo = true)
        : base(typeId)
    {
        SelectedItem = new BindableReactiveProperty<IHeadlinedViewModel?>().AddTo(
            ref DisposableBag
        );
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().AddTo(ref DisposableBag);
        SelectItemCommand = new ReactiveCommand<IHeadlinedViewModel>(
            (item, cancel) => SelectItem(item, cancel),
            AwaitOperation.Drop
        ).AddTo(ref DisposableBag);

        if (enableUndo)
        {
            _undoValueSink = Undo.Register<ValueUndoChange<IHeadlinedViewModel>>(
                    "Value",
                    OnUndoValue,
                    OnRedoValue
                )
                .AddTo(ref DisposableBag);
        }
    }

    private ValueTask OnRedoValue(
        ValueUndoChange<IHeadlinedViewModel> change,
        CancellationToken cancel
    )
    {
        return SelectItem(change.NewValue, cancel);
    }

    private ValueTask OnUndoValue(
        ValueUndoChange<IHeadlinedViewModel> change,
        CancellationToken cancel
    )
    {
        return SelectItem(change.OldValue, cancel);
    }

    public NotifyCollectionChangedSynchronizedViewList<IHeadlinedViewModel> ItemsView { get; }

    public ObservableList<IHeadlinedViewModel> ItemsSource { get; } = [];

    public BindableReactiveProperty<IHeadlinedViewModel?> SelectedItem { get; }

    public ReactiveCommand<IHeadlinedViewModel> SelectItemCommand { get; }

    public async ValueTask SelectItem(IHeadlinedViewModel item, CancellationToken cancel = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsBusy)
        {
            return;
        }

        if (EqualityComparer<IHeadlinedViewModel?>.Default.Equals(SelectedItem.Value, item))
        {
            return;
        }

        ClearModelErrors();
        var previousItem = SelectedItem.Value;
        SelectedItem.Value = item;
        _selectCancel?.Cancel(false);
        _selectCancel?.Dispose();
        var selectCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        _selectCancel = selectCancel;
        IsBusy = true;

        try
        {
            await ApplyFromUser(item, selectCancel.Token);
            if (previousItem is not null)
            {
                _undoValueSink?.Publish(previousItem, item);
            }
            MarkUpdated();
        }
        catch (OperationCanceledException) when (selectCancel.IsCancellationRequested)
        {
            SelectedItem.Value = previousItem;
        }
        catch (Exception e)
        {
            SelectedItem.Value = previousItem;
            ApplyErrorFromModel(e);
        }
        finally
        {
            if (ReferenceEquals(_selectCancel, selectCancel))
            {
                _selectCancel = null;
            }

            selectCancel.Dispose();
            IsBusy = false;
        }
    }

    protected void ApplyValueFromModel(IHeadlinedViewModel? value)
    {
        ClearModelErrors();
        SelectedItem.Value = value;
        MarkUpdated();
    }

    protected abstract ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel);

    public override IEnumerable<IViewModel> GetChildren()
    {
        return ItemsView;
    }
}
