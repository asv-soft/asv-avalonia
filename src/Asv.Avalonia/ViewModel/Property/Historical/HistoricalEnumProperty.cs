using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class HistoricalEnumProperty<TEnum>
    : ValidationEnumProperty<TEnum>,
        IHistoricalProperty<Enum>
    where TEnum : struct, Enum
{
    public HistoricalEnumProperty(
        NavigationId id,
        ReactiveProperty<Enum> modelValue,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, modelValue, loggerFactory, parent) { }

    protected override async ValueTask ChangeModelValue(Enum value, CancellationToken cancel)
    {
        var newValue = new StringArg(Enum.GetName(value.GetType(), value) ?? string.Empty);
        await this.ExecuteCommand(ChangeEnumPropertyCommand.Id, newValue, cancel);
    }
}
