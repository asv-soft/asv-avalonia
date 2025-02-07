using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[Export(typeof(ICommandFactory))]
[Shared]
public class ChangeCurrentUnitItemCommandFactory : ICommandFactory
{
    private readonly IUnit _unit;

    [ImportingConstructor]
    public ChangeCurrentUnitItemCommandFactory(IUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _unit = unit;
    }

    public ICommandInfo Info => ChangeCurrentUnitItemCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new ChangeCurrentUnitItemCommand(_unit);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class ChangeCurrentUnitItemCommand(IUnit unit) : IUndoRedoCommand
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

    private PersistableChange<string>? _state;

    public ICommandInfo Info => StaticInfo;

    public IPersistable Save()
    {
        return _state ?? throw new InvalidOperationException();
    }

    public void Restore(IPersistable state)
    {
        if (state is PersistableChange<string> memento)
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
        if (parameter is Persistable<string> memento)
        {
            // execute with parameter
            var oldValue = unit.Current.Value.UnitItemId;
            unit.AvailableUnits.TryGetValue(memento.Value, out var unitItem);
            if (unitItem is not null)
            {
                unit.Current.Value = unitItem;
            }

            _state = new PersistableChange<string>(oldValue, memento.Value);
        }
        else
        {
            // execute without parameter
            var oldValue = unit.Current.Value.UnitItemId;
            var temp = unit.AvailableUnits.Values.ToList();
            var index = temp.IndexOf(unit.Current.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            var newValue = temp[index].UnitItemId;
            unit.Current.Value = temp[index];
            _state = new PersistableChange<string>(oldValue, newValue);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask Undo(IRoutable? context, CancellationToken cancel = default)
    {
        if (_state is null)
        {
            return ValueTask.CompletedTask;
        }

        unit.AvailableUnits.TryGetValue(_state.OldValue, out var unitItem);
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

        unit.AvailableUnits.TryGetValue(_state.NewValue, out var unitItem);
        if (unitItem is not null)
        {
            unit.Current.Value = unitItem;
        }

        return ValueTask.CompletedTask;
    }
}
