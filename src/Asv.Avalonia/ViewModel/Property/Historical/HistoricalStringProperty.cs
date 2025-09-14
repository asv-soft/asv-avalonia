using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalStringProperty
    : ValidationStringProperty,
        IHistoricalProperty<string?>
{
    public HistoricalStringProperty(
        string id,
        ReactiveProperty<string?> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent,
        IList<Func<string?, ValidationResult>>? validationRules = null
    )
        : base(id, modelValue, loggerFactory, parent, validationRules) { }

    protected override async ValueTask ChangeModelValue(string? value, CancellationToken cancel)
    {
        var newValue = new StringArg(value ?? string.Empty);
        await this.ExecuteCommand(ChangeStringPropertyCommand.Id, newValue, cancel);
    }
}
