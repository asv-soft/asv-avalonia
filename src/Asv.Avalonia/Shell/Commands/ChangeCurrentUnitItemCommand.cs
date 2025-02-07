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
        var units = _svc.Units.Values.ToList();
        var cmds = units.ToDictionary(u => u.UnitId, u => new ChangeCurrentUnitItemCommand(u));
        return new CompositeChangeCurrentUnitItemCommand(cmds);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class CompositeChangeCurrentUnitItemCommand : IUndoRedoCommand
{
    private readonly IDictionary<string, ChangeCurrentUnitItemCommand> _commands;

    private string? _lastExecutedKey;

    public const string Id = "composite.current.unit.change";

    public CompositeChangeCurrentUnitItemCommand(
        IDictionary<string, ChangeCurrentUnitItemCommand> commands
    )
    {
        _commands = commands;
    }

    // TODO: Change info
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Composite",
        Description = "Composite cmd",
        Icon = MaterialIconKind.Star,
        DefaultHotKey = null,
        Order = 0,
    };

    public ICommandInfo Info => StaticInfo;

    public async ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    )
    {
        if (parameter is Persistable<UnitDelegate> persistable)
        {
            var key = persistable.Value.unitId;
            if (_commands.TryGetValue(key, out var command))
            {
                _lastExecutedKey = key;
                var param = new Persistable<string>(persistable.Value.unitItemId);
                await command.Execute(context, param, cancel);
            }
            else
            {
                throw new InvalidOperationException($"No command for unit with id '{key}'.");
            }
        }
        else
        {
            throw new InvalidOperationException("Unable to perform action. Pass valid parameter.");

            // var command = _commands.Values.FirstOrDefault();
            // if (command is null)
            // {
            //     throw new InvalidOperationException("No commands were found.");
            // }
            //
            // _lastExecutedKey = command.Info.Id;
            // await command.Execute(context, parameter, cancel);
        }
    }

    public IPersistable Save()
    {
        if (_lastExecutedKey is null)
        {
            throw new InvalidOperationException("No command to save state.");
        }

        var command = _commands[_lastExecutedKey];
        return command.Save();
    }

    public void Restore(IPersistable state)
    {
        foreach (var cmd in _commands.Values)
        {
            try
            {
                cmd.Restore(state);
                _lastExecutedKey = cmd.Info.Id;
                return;
            }
            catch
            {
                continue;
            }
        }

        throw new InvalidOperationException("Unable to restore state for any command.");
    }

    public async ValueTask Undo(IRoutable? context, CancellationToken cancel = default)
    {
        if (_lastExecutedKey is null)
        {
            return;
        }

        await _commands[_lastExecutedKey].Undo(context, cancel);
    }

    public async ValueTask Redo(IRoutable context, CancellationToken cancel = default)
    {
        if (_lastExecutedKey is null)
        {
            return;
        }

        await _commands[_lastExecutedKey].Redo(context, cancel);
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
