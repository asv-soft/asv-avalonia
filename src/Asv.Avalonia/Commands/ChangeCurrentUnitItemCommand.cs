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
        DefaultHotKey = null,
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

        unit.AvailableUnits.TryGetValue(memento.Value, out var unitItem);
        if (unitItem is not null)
        {
            unit.Current.Value = unitItem;
        }

        return ValueTask.FromResult<ICommandArg?>(
            new ActionCommandArg(
                unit.UnitId,
                unit.Current.Value.UnitItemId,
                CommandParameterActionType.Change
            )
        );
    }
}
