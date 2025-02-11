using System.Composition;
using Material.Icons;
using InvalidOperationException = System.InvalidOperationException;

namespace Asv.Avalonia;

[Export(typeof(ICommandFactory))]
[Shared]
public class ChangeCurrentUnitItemCommandFactory : ICommandFactory
{
    private readonly IUnitService _svc;

    [ImportingConstructor]
    public ChangeCurrentUnitItemCommandFactory(IUnitService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public ICommandInfo Info => ChangeCurrentUnitItemCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new ChangeCurrentUnitItemCommand(_svc);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class ChangeCurrentUnitItemCommand(IUnitService svc) : IUndoRedoCommand
{
    #region Static

    public const string Id = "current.unit.change";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeCurrentUnitItemCommand_CommandInfo_Name,
        Description = RS.ChangeCurrentUnitItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Settings,
        DefaultHotKey = null,
        Order = 0,
    };

    #endregion

    private PersistableChange<UnitDelegate>? _state;

    public ICommandInfo Info => StaticInfo;

    public IPersistable Save()
    {
        return _state ?? throw new InvalidOperationException();
    }

    public void Restore(IPersistable state)
    {
        if (state is PersistableChange<UnitDelegate> memento)
        {
            _state = memento;
        }
    }

    public ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    )
    {
        if (parameter is Persistable<UnitDelegate> memento)
        {
            // execute with parameter
            svc.Units.TryGetValue(memento.Value.unitId, out var unit);
            ArgumentNullException.ThrowIfNull(unit);

            var oldValue = new UnitDelegate(unit.UnitId, unit.Current.Value.UnitItemId);
            unit.AvailableUnits.TryGetValue(memento.Value.unitItemId, out var unitItem);
            if (unitItem is not null)
            {
                unit.Current.Value = unitItem;
            }

            _state = new PersistableChange<UnitDelegate>(oldValue, memento.Value);
        }
        else
        {
            // execute without parameter
            return ValueTask.FromException(
                new InvalidOperationException("Unable to perform action. Pass valid parameter.")
            );
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Undo(IRoutable? context, CancellationToken cancel = default)
    {
        if (_state is null)
        {
            return ValueTask.CompletedTask;
        }

        svc.Units.TryGetValue(_state.OldValue.unitId, out var unit);
        ArgumentNullException.ThrowIfNull(unit);

        unit.AvailableUnits.TryGetValue(_state.OldValue.unitItemId, out var unitItem);
        if (unitItem is not null)
        {
            unit.Current.Value = unitItem;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Redo(IRoutable context, CancellationToken cancel = default)
    {
        if (_state is null)
        {
            return ValueTask.CompletedTask;
        }

        svc.Units.TryGetValue(_state.OldValue.unitId, out var unit);
        ArgumentNullException.ThrowIfNull(unit);

        unit.AvailableUnits.TryGetValue(_state.NewValue.unitItemId, out var unitItem);
        if (unitItem is not null)
        {
            unit.Current.Value = unitItem;
        }

        return ValueTask.CompletedTask;
    }
}
