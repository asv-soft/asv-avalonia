using System.Composition;
using Asv.IO;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia.IO;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class ProtocolPortCommand(IDeviceManager manager) : NoContextCommand
{
    public const string Id = $"{BaseId}.port.change";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Add/remove/change port",
        Description = "Add/remove/change port",
        Icon = MaterialIconKind.SerialPort,
        DefaultHotKey = null,
        CustomHotKey = null,
        Source = IoModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandParameter?> InternalExecute(
        ICommandParameter newValue,
        CancellationToken cancel
    )
    {
        if (newValue is CommandParameterAction action)
        {
            return action.Action switch
            {
                CommandParameterActionType.Add => AddPort(action),
                CommandParameterActionType.Remove => Remove(action),
                CommandParameterActionType.Change => Change(action),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        return ValueTask.FromResult<ICommandParameter?>(null);
    }

    private ValueTask<ICommandParameter?> Change(CommandParameterAction action)
    {
        CommandParameterAction? rollback = null;
        var portToDelete = manager.Router.Ports.FirstOrDefault(x => x.Id == action.Id);
        if (portToDelete != null)
        {
            var oldConfig = portToDelete.Config.AsUri();
            manager.Router.RemovePort(portToDelete);
            rollback = new CommandParameterAction(
                action.Id,
                oldConfig.ToString(),
                CommandParameterActionType.Change
            );
        }
        if (string.IsNullOrWhiteSpace(action.Value))
        {
            throw new ArgumentException("Invalid port configuration");
        }
        var newConfig = new ProtocolPortConfig(new Uri(action.Value));
        manager.Router.AddPort(newConfig.AsUri());
        return ValueTask.FromResult<ICommandParameter?>(rollback);
    }

    private ValueTask<ICommandParameter?> AddPort(CommandParameterAction action)
    {
        if (string.IsNullOrWhiteSpace(action.Value))
        {
            throw new ArgumentException("Invalid port configuration");
        }
        var config = new ProtocolPortConfig(new Uri(action.Value));
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            config.Name = $"New {config.Scheme} port {manager.Router.Ports.Length + 1}";
        }
        var newPort = manager.Router.AddPort(config.AsUri());
        var rollback = new CommandParameterAction(
            newPort.Id,
            null,
            CommandParameterActionType.Remove
        );
        return new ValueTask<ICommandParameter?>(rollback);
    }

    private ValueTask<ICommandParameter?> Remove(CommandParameterAction action)
    {
        var portToDelete = manager.Router.Ports.FirstOrDefault(x => x.Id == action.Id);
        if (portToDelete != null)
        {
            var config = portToDelete.Config.AsUri();
            var rollback = new CommandParameterAction(
                null,
                config.ToString(),
                CommandParameterActionType.Add
            );
            manager.Router.RemovePort(portToDelete);
            return new ValueTask<ICommandParameter?>(rollback);
        }

        return ValueTask.FromResult<ICommandParameter?>(null);
    }
}
