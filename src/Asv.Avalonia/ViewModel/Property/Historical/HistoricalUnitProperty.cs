using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalUnitProperty : BindableUnitProperty, IHistoricalProperty<double>
{
    public HistoricalUnitProperty(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IRoutable parent,
        string? format = null
    )
        : base(id, modelValue, unit, layoutService, loggerFactory, parent, format) { }

    protected override async ValueTask ChangeModelValue(double value, CancellationToken cancel)
    {
        var newValue = new DoubleArg(value);
        await this.ExecuteCommand(ChangeDoublePropertyCommand.Id, newValue, cancel);
    }
}
