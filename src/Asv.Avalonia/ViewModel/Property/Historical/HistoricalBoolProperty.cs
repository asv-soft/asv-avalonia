using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalBoolProperty : ValidationBoolProperty, IHistoricalProperty<bool>
{
    public HistoricalBoolProperty(
        NavigationId id,
        ReactiveProperty<bool> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, modelValue, loggerFactory, parent) { }

    protected override async ValueTask ChangeModelValue(bool value, CancellationToken cancel)
    {
        var newValue = new BoolArg(value);
        await this.ExecuteCommand(ChangeBoolPropertyCommand.Id, newValue, cancel);
    }
}
