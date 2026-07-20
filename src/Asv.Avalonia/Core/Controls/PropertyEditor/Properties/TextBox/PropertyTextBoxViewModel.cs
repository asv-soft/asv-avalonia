using System.ComponentModel;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyTextBoxViewModel : PropertyViewModel, ISupportCancel
{
    private CancellationTokenSource? _applyCancel;
    private bool _changesFromUser;
    private string? _lastTextValue;
    private readonly IUndoChangeSink<ValueUndoChange<string?>>? _undoValueSink;

    protected PropertyTextBoxViewModel(string typeId, bool enableUndo = true)
        : base(typeId)
    {
        Text = new BindableReactiveProperty<string?>().AddTo(ref DisposableBag);
        ApplyFromUserCommand = new ReactiveCommand(
            (_, cancel) => ApplyFromUser(cancel),
            AwaitOperation.Drop
        ).AddTo(ref DisposableBag);
        CancelCommand = new ReactiveCommand(_ => Cancel()).AddTo(ref DisposableBag);

        if (enableUndo)
        {
            _undoValueSink = Undo.Register<ValueUndoChange<string?>>(
                    "Value",
                    OnUndoValue,
                    OnRedoValue
                )
                .AddTo(ref DisposableBag);
        }

        Text.Skip(1)
            .Where(_ => _changesFromUser)
            .Subscribe(_ => IsSync = false)
            .AddTo(ref DisposableBag);

        Observable
            .FromEventHandler<DataErrorsChangedEventArgs>(
                h => Text.ErrorsChanged += h,
                h => Text.ErrorsChanged -= h
            )
            .Subscribe(_ => HasValidationErrors = Text.HasErrors)
            .AddTo(ref DisposableBag);
    }

    private ValueTask OnRedoValue(ValueUndoChange<string?> change, CancellationToken cancel)
    {
        Text.Value = change.NewValue;
        return ApplyFromUser(cancel);
    }

    private ValueTask OnUndoValue(ValueUndoChange<string?> change, CancellationToken cancel)
    {
        Text.Value = change.OldValue;
        return ApplyFromUser(cancel);
    }

    protected void ApplyValueFromModel(string? newValue)
    {
        ClearModelErrors();
        if (IsInEditMode)
        {
            return;
        }
        ApplyTextFromModel(newValue, newValue, true, true);
    }

    protected void ApplyTextFromModel(
        string? newValue,
        string? lastTextValue,
        bool isSync,
        bool updateFlag
    )
    {
        ClearModelErrors();
        _changesFromUser = false;
        Text.Value = newValue;
        _lastTextValue = lastTextValue;
        _changesFromUser = true;
        IsSync = isSync;
        if (updateFlag)
        {
            MarkUpdated();
        }
    }

    public bool HasValidationErrors
    {
        get;
        private set => SetField(ref field, value);
    }

    public BindableReactiveProperty<string?> Text { get; }

    public ReactiveCommand ApplyFromUserCommand { get; }

    public ReactiveCommand CancelCommand { get; }

    public bool IsInEditMode
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSync
    {
        get;
        protected set => SetField(ref field, value);
    } = true;

    public string? Units
    {
        get;
        set => SetField(ref field, value);
    }

    public async ValueTask ApplyFromUser(CancellationToken cancel = default)
    {
        if (IsBusy)
        {
            return;
        }

        if (IsSync)
        {
            IsInEditMode = false;
            return;
        }

        if (HasValidationErrors)
        {
            return;
        }

        ClearModelErrors();
        var oldValue = _lastTextValue;
        var newValue = Text.Value;
        var applyCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
        _applyCancel = applyCancel;
        IsBusy = true;
        try
        {
            await ApplyFromUserCore(applyCancel.Token);
            if (!EqualityComparer<string?>.Default.Equals(oldValue, newValue))
            {
                _undoValueSink?.PublishUpdate(oldValue, newValue);
            }
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            // do nothing
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsInEditMode = false;
            if (ReferenceEquals(_applyCancel, applyCancel))
            {
                _applyCancel = null;
            }
            applyCancel.Dispose();
            IsBusy = false;
            IsSync = true;
        }
    }

    protected abstract ValueTask ApplyFromUserCore(CancellationToken cancel);

    public void Cancel()
    {
        if (IsInEditMode == false)
        {
            return;
        }

        if (IsBusy)
        {
            return;
        }
        IsInEditMode = false;
        ApplyTextFromModel(_lastTextValue, _lastTextValue, true, false);
    }
}
