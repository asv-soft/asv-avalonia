using System.Composition;
using Material.Icons;
using InvalidOperationException = System.InvalidOperationException;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public sealed class ChangeCurrentUnitItemCommand : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.settings.current.unit.change";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeCurrentUnitItemCommand_CommandInfo_Name,
        Description = RS.ChangeCurrentUnitItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Settings,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion

    private readonly IUnitService _svc;

    [ImportingConstructor]
    public ChangeCurrentUnitItemCommand(IUnitService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not ActionCommandArg memento)
        {
            return ValueTask.FromException<ICommandArg?>(
                new CommandArgMismatchException(typeof(ActionCommandArg))
            );
        }

        if (memento.Id == null || memento.Value == null)
        {
            return ValueTask.FromException<ICommandArg?>(
                new InvalidOperationException("Unable to perform action. Pass a valid parameter.")
            );
        }

        _svc.Units.TryGetValue(memento.Id, out var unit);
        ArgumentNullException.ThrowIfNull(unit);
        var oldValue = unit;
        unit.AvailableUnits.TryGetValue(memento.Value, out var unitItem);
        if (unitItem is not null)
        {
            unit.CurrentUnitItem.Value = unitItem;
        }

        return ValueTask.FromResult<ICommandArg?>(
            new ActionCommandArg(
                oldValue.UnitId,
                oldValue.CurrentUnitItem.Value.UnitItemId,
                CommandParameterActionType.Change
            )
        );
    }

    public override ValueTask<ICommandArg?> Execute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel = default
    )
    {
        if (newValue is not ActionCommandArg memento)
        {
            return ValueTask.FromException<ICommandArg?>(
                new InvalidOperationException("Unable to perform action. Pass a valid parameter.")
            );
        }

        if (memento.Id == null || memento.Value == null)
        {
            return ValueTask.FromException<ICommandArg?>(
                new InvalidOperationException("Unable to perform action. Pass a valid parameter.")
            );
        }

        _svc.Units.TryGetValue(memento.Id, out var unit);
        ArgumentNullException.ThrowIfNull(unit);
        var oldValue = unit.CurrentUnitItem.Value.UnitItemId;
        unit.AvailableUnits.TryGetValue(memento.Value, out var unitItem);
        if (unitItem is not null)
        {
            unit.CurrentUnitItem.Value = unitItem;
        }

        return ValueTask.FromResult<ICommandArg?>(
            new ActionCommandArg(unit.UnitId, oldValue, CommandParameterActionType.Change)
        );
    }
}
