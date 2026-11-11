using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyUnitViewModel : PropertyTextBoxViewModel
{
    private const string EditFormat = "R";
    private readonly string? _format;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoUnitSink;
    private readonly IUndoChangeSink<ValueUndoChange<double>>? _undoValueSink;
    private double _lastValue;
    private bool _changeUnitFromViewModel;

    protected PropertyUnitViewModel(
        string typeId,
        IUnit unit,
        string? format = null,
        bool enableValueUndo = true
    )
        : base(typeId, enableUndo: false)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _format = format;
        Unit = unit;
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(ChangeUnit).AddTo(ref DisposableBag);
        Text.EnableValidation(ValidateText).AddTo(ref DisposableBag);
        Text.ForceValidate();
        Text.Subscribe(UpdateDisplayText).AddTo(ref DisposableBag);

        _undoUnitSink = Undo.RegisterValue<string>("Unit", ChangeUnit, ChangeUnit)
            .AddTo(ref DisposableBag);

        if (enableValueUndo)
        {
            _undoValueSink = Undo.RegisterValue<double>(
                    "Value",
                    ApplyHistoryValue,
                    ApplyHistoryValue
                )
                .AddTo(ref DisposableBag);
        }

        Unit.CurrentUnitItem.Skip(1)
            .Subscribe(_ => ApplyTextFromUnit(_lastValue, true))
            .AddTo(ref DisposableBag);
    }

    public IUnit Unit { get; }

    public string? DisplayText
    {
        get;
        private set => SetField(ref field, value);
    }

    public ReactiveCommand<IUnitItem> ChangeUnitCommand { get; }

    protected void ApplyValueFromModel(double siValue)
    {
        _lastValue = siValue;
        ApplyValueFromModel(Unit.CurrentUnitItem.CurrentValue.PrintFromSi(siValue, EditFormat));
    }

    protected override async ValueTask ApplyFromUserCore(CancellationToken cancel)
    {
        var oldValue = _lastValue;
        var newValue = Unit.CurrentUnitItem.CurrentValue.ParseToSi(Text.Value);

        await ApplyFromUser(newValue, cancel);

        _undoValueSink?.PublishUpdate(oldValue, newValue);
    }

    protected abstract ValueTask ApplyFromUser(double siValue, CancellationToken cancel);

    protected virtual Exception? ValidateSiValue(double siValue)
    {
        return null;
    }

    private Exception? ValidateText(string? userValue)
    {
        var result = Unit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);

        if (result.IsSuccess)
        {
            return ValidateSiValue(Unit.CurrentUnitItem.CurrentValue.ParseToSi(userValue));
            ;
        }

        return result.ValidationException?.GetExceptionWithLocalizationOrSelf();
    }

    private void UpdateDisplayText(string? text)
    {
        var unit = Unit.CurrentUnitItem.CurrentValue;
        DisplayText = Text.HasErrors ? text : unit.Print(unit.Parse(text), _format);
    }

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
        _undoUnitSink.PublishUpdate(oldUnitItemId, newUnitItemId);
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

        var textValue = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(siValue, EditFormat);
        var lastTextValue = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(_lastValue, EditFormat);
        ApplyTextFromModel(textValue, lastTextValue, isSync, false);
        UpdateDisplayText(Text.Value);
    }

    private ValueTask ApplyHistoryValue(double siValue, CancellationToken cancel)
    {
        Text.Value = Unit.CurrentUnitItem.CurrentValue.PrintFromSi(siValue, EditFormat);
        return ApplyFromUser(cancel);
    }
}
