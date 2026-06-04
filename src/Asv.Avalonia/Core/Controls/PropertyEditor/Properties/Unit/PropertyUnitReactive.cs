using R3;

namespace Asv.Avalonia;

public class PropertyUnitReactive : PropertyUnitViewModel
{
    private readonly ReactiveProperty<double> _model;
    private readonly Func<double, Exception?>? _validator;

    public PropertyUnitReactive(
        string id,
        IUnit unit,
        ReactiveProperty<double> model,
        Func<double, Exception?>? validator = null,
        string? format = null,
        bool enableValueUndo = true
    )
        : base(id, unit, format, enableValueUndo)
    {
        _model = model;
        _model.Subscribe(
            ApplyValueFromModel,
            x =>
            {
                if (x.Exception == null)
                {
                    return;
                }
                ApplyErrorFromModel(x.Exception);
            }
        );
        _validator = validator;
    }

    protected override Exception? ValidateSiValue(double siValue)
    {
        if (_validator != null)
        {
            var result = _validator(siValue);
            if (result != null)
            {
                return result;
            }
        }
        return base.ValidateSiValue(siValue);
    }

    protected override ValueTask ApplyFromUser(double siValue, CancellationToken cancel)
    {
        _model.OnNext(siValue);
        return ValueTask.CompletedTask;
    }
}
