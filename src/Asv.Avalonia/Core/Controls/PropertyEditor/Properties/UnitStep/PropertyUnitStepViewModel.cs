using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public class PropertyUnitStepViewModel : PropertyTextBoxViewModel
{
    private readonly IUnit _unit;
    private readonly Func<double, Exception?>? _validator;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoUnitSink;

    public PropertyUnitStepViewModel(
        string id,
        IUnit unit,
        Func<double, Exception?>? validator = null
    )
        : base(id, enableUndo: false)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _unit = unit;
        _validator = validator;

        UnitItem = new BindableReactiveProperty<IUnitItem>(
            unit.CurrentUnitItem.CurrentValue
        ).DisposeItWith(Disposable);
        Text.EnableValidation(text => Validate(text, UnitItem.Value));
        Text.Value = "1";
        _undoUnitSink = Undo.RegisterValue<string>("Unit", SetUnit, SetUnit)
            .AddTo(ref DisposableBag);
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(ChangeUnit).DisposeItWith(Disposable);
        UnitItem.Skip(1).Subscribe(_ => Text.ForceValidate()).DisposeItWith(Disposable);
    }

    public IEnumerable<IUnitItem> UnitItems => _unit.AvailableUnits.Values;
    public BindableReactiveProperty<IUnitItem> UnitItem { get; }
    public ReactiveCommand<IUnitItem> ChangeUnitCommand { get; }

    public bool TryGetValueInSi(out double value)
    {
        if (HasValidationErrors)
        {
            value = 0;
            return false;
        }

        value = UnitItem.Value.ParseToSi(Text.Value);
        return true;
    }

    public bool TrySetValue(string? text, string? unitItemId)
    {
        if (
            unitItemId is null
            || !_unit.AvailableUnits.TryGetValue(unitItemId, out var unitItem)
            || Validate(text, unitItem) is not null
        )
        {
            return false;
        }

        UnitItem.Value = unitItem;
        Text.Value = text;
        return true;
    }

    protected override ValueTask ApplyFromUserCore(CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }

    private void ChangeUnit(IUnitItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var previousUnitId = UnitItem.Value.UnitItemId;
        var newUnitId = item.UnitItemId;
        if (
            previousUnitId == newUnitId
            || !_unit.AvailableUnits.TryGetValue(newUnitId, out var unitItem)
        )
        {
            return;
        }

        UnitItem.Value = unitItem;
        _undoUnitSink.PublishUpdate(previousUnitId, newUnitId);
    }

    private void SetUnit(string unitItemId)
    {
        UnitItem.Value = _unit[unitItemId];
    }

    private Exception? Validate(string? text, IUnitItem unitItem)
    {
        var result = unitItem.ValidateValue(text);
        if (!result.IsSuccess)
        {
            return result.ValidationException?.GetExceptionWithLocalizationOrSelf()
                ?? new UnknownValidationException();
        }

        return _validator?.Invoke(unitItem.ParseToSi(text));
    }
}
