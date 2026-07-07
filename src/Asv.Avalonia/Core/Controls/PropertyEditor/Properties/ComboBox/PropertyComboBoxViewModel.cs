using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyComboBoxViewModel : PropertyViewModel
{
    private CancellationTokenSource? _selectCancel;
    private readonly IUndoChangeSink<ValueUndoChange<string>>? _undoValueSink;

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
            _undoValueSink = Undo.RegisterValue<string>("Value", SelectItemById, SelectItemById)
                .AddTo(ref DisposableBag);
        }
    }

    private ValueTask SelectItemById(string id, CancellationToken cancel)
    {
        var item = ItemsSource.FirstOrDefault(x => TryGetItemUndoId(x) == id);
        if (item is null)
        {
            throw new InvalidOperationException($"ComboBox item '{id}' not found.");
        }

        return SelectItem(item, cancel);
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
        var undoPreviousId =
            previousItem is not null && _undoValueSink is not null
                ? GetItemUndoId(previousItem)
                : null;
        var undoNewId =
            previousItem is not null && _undoValueSink is not null ? GetItemUndoId(item) : null;
        SelectedItem.Value = item;
        _selectCancel?.Cancel(false);
        _selectCancel?.Dispose();
        var selectCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        _selectCancel = selectCancel;
        IsBusy = true;

        try
        {
            await ApplyFromUser(item, selectCancel.Token);
            if (undoPreviousId is not null && undoNewId is not null)
            {
                _undoValueSink?.PublishUpdate(undoPreviousId, undoNewId);
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

    private static string GetItemUndoId(IHeadlinedViewModel item)
    {
        if (item is not IViewModel viewModel)
        {
            throw new InvalidOperationException(
                $"ComboBox item '{item.GetType().FullName}' must implement {nameof(IViewModel)} to support undo."
            );
        }

        return viewModel.Id.ToString();
    }

    private static string? TryGetItemUndoId(IHeadlinedViewModel item)
    {
        return item is IViewModel viewModel ? viewModel.Id.ToString() : null;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return ItemsView;
    }
}
