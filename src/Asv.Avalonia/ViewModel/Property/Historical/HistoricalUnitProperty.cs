using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class HistoricalUnitProperty : BindableUnitProperty, IHistoricalProperty<double>
{
    private readonly IUndoChangeSink<ValueUndoChange<double>> _undoSink;

    public HistoricalUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILoggerFactory loggerFactory,
        string? format = null
    )
        : base(id, modelValue, unit, loggerFactory, format)
    {
        _undoSink = Undo.CreateValueChange<double>("default", ApplyUnitValue, ApplyUnitValue)
            .DisposeItWith(Disposable);
    }

    private void ApplyUnitValue(double value)
    {
        ModelValue.Value = value;
    }

    protected override ValueTask ApplyValueToModel(double value, CancellationToken cancel)
    {
        var oldValue = ModelValue.Value;
        if (oldValue.Equals(value))
        {
            return ValueTask.CompletedTask;
        }

        ApplyUnitValue(value);
        _undoSink.Publish(oldValue, value);
        return ValueTask.CompletedTask;
    }
}
