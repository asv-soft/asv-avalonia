using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class HistoricalUnitProperty : BindableUnitProperty, IHistoricalProperty<double>
{
    public HistoricalUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILoggerFactory loggerFactory,
        string? format = null
    )
        : base(id, modelValue, unit, loggerFactory, format) { }

    protected override async ValueTask ApplyValueToModel(double value, CancellationToken cancel)
    {
        var newValue = new DoubleArg(value);
        await this.ExecuteCommand(ChangeDoublePropertyCommand.Id, newValue, cancel);
    }
}
