using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyUnitViewModel : PropertyTextBoxViewModel
{
    private readonly string? _format;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoUnitSink;
    private double _lastValue;
    private readonly IUndoChangeSink<ValueUndoChange<double>> _undoValueSink;
    private bool _changeUnitFromViewModel;

    protected PropertyUnitViewModel(string typeId, IUnit unit, string? format = null)
        : base(typeId)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _format = format;
        Unit = unit;
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(ChangeUnit).AddTo(ref DisposableBag);
        _undoUnitSink = Undo.Register<ValueUndoChange<string>>(nameof(Unit), UndoUnit, RedoUnit)
            .AddTo(ref DisposableBag);
        Text.EnableValidation(ValidateText).AddTo(ref DisposableBag);
        Text.ForceValidate();

        _undoValueSink = Undo.Register<ValueUndoChange<double>>("Value", OnUndoValue, OnRedoValue)
            .AddTo(ref DisposableBag);

        Unit.CurrentUnitItem.Skip(1)
            .Subscribe(_ => ApplyTextFromUnit(_lastValue, true))
            .AddTo(ref DisposableBag);
    }

    private ValueTask OnRedoValue(ValueUndoChange<double> change, CancellationToken cancel)
    {
        Text.Value = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(change.NewValue, _format);
        return ApplyFromUser();
    }

    private ValueTask OnUndoValue(ValueUndoChange<double> change, CancellationToken cancel)
    {
        Text.Value = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(change.OldValue, _format);
        return ApplyFromUser();
    }

    private Exception? ValidateText(string? userValue)
    {
        var result = Unit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);

        if (result.IsSuccess)
        {
            var resultSi = ValidateSiValue(Unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue));
            return resultSi;
        }
        return result.ValidationException?.GetExceptionWithLocalizationOrSelf();
    }

    protected virtual Exception? ValidateSiValue(double siValue)
    {
        return null;
    }

    private ValueTask RedoUnit(ValueUndoChange<string> change, CancellationToken cancel)
    {
        ChangeUnit(change.NewValue);
        return ValueTask.CompletedTask;
    }

    private ValueTask UndoUnit(ValueUndoChange<string> change, CancellationToken cancel)
    {
        ChangeUnit(change.OldValue);
        return ValueTask.CompletedTask;
    }

    protected void ApplyValueFromModel(double siValue)
    {
        _lastValue = siValue;
        ApplyValueFromModel(Unit.CurrentUnitItem.CurrentValue.PrintFromSi(siValue, _format));
    }

    public ReactiveCommand<IUnitItem> ChangeUnitCommand { get; }

    private void ChangeUnit(IUnitItem obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var oldUnitItemId = Unit.CurrentUnitItem.Value.UnitItemId;
        var newUnitItemId = obj.UnitItemId;
        if (oldUnitItemId == newUnitItemId)
        {
            return;
        }

        var currentValue = ReadCurrentValueBeforeUnitChange();
        var wasSync = IsSync;
        ChangeUnit(newUnitItemId, currentValue, wasSync);
        _undoUnitSink.Publish(oldUnitItemId, newUnitItemId);
    }

    private void ChangeUnit(string unitItemId)
    {
        ChangeUnit(unitItemId, _lastValue, true);
    }

    private void ChangeUnit(string unitItemId, double currentValue, bool isSync)
    {
        _changeUnitFromViewModel = true;
        try
        {
            Unit.CurrentUnitItem.Value = Unit[unitItemId];
        }
        finally
        {
            _changeUnitFromViewModel = false;
        }

        ApplyTextFromUnit(currentValue, isSync);
    }

    private double ReadCurrentValueBeforeUnitChange()
    {
        if (IsSync || Text.HasErrors)
        {
            return _lastValue;
        }

        try
        {
            return Unit.CurrentUnitItem.CurrentValue.ParseToSi(Text.Value);
        }
        catch
        {
            return _lastValue;
        }
    }

    private void ApplyTextFromUnit(double siValue, bool isSync)
    {
        if (_changeUnitFromViewModel)
        {
            return;
        }

        var textValue = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(siValue, _format);
        var lastTextValue = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(_lastValue, _format);
        ApplyTextFromModel(textValue, lastTextValue, isSync, false);
    }

    protected override ValueTask ApplyFromUser(CancellationToken cancel)
    {
        var value = Unit.CurrentUnitItem.CurrentValue.ParseToSi(Text.Value);
        _undoValueSink.Publish(_lastValue, value);
        return ApplyFromUser(value, cancel);
    }

    protected abstract ValueTask ApplyFromUser(double siValue, CancellationToken cancel);

    public IUnit Unit { get; }
}
