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

    protected override ValueTask<IPersistable?> InternalExecute(
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not Persistable<UnitDelegate> memento)
        {
            return ValueTask.FromException<IPersistable?>(
                new InvalidOperationException("Unable to perform action. Pass a valid parameter.")
            );
        }

        _svc.Units.TryGetValue(memento.Value.unitId, out var unit);
        ArgumentNullException.ThrowIfNull(unit);

        var oldValue = new UnitDelegate(unit.UnitId, unit.Current.Value.UnitItemId);
        unit.AvailableUnits.TryGetValue(memento.Value.unitItemId, out var unitItem);
        if (unitItem is not null)
        {
            unit.Current.Value = unitItem;
        }

        return ValueTask.FromResult<IPersistable?>(new Persistable<UnitDelegate>(oldValue));
    }
}
