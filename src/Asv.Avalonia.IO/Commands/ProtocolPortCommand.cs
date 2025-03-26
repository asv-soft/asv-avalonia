using System.Composition;
using Asv.IO;
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

    public static ICommandArg CreateAddArg(ProtocolPortConfig config)
    {
        return CommandArg.FromAddAction(config.AsUri().ToString());
    }

    public static ICommandArg CreateRemoveArg(IProtocolPort port)
    {
        return CommandArg.FromRemoveAction(port.Id);
    }

    public static ICommandArg CreateChangeArg(IProtocolPort port, ProtocolPortConfig newConfig)
    {
        return CommandArg.FromChangeAction(port.Id, newConfig.AsUri().ToString());
    }

    protected override ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is ActionCommandArg action)
        {
            return action.Action switch
            {
                CommandParameterActionType.Add => AddPort(action),
                CommandParameterActionType.Remove => Remove(action),
                CommandParameterActionType.Change => Change(action),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        return ValueTask.FromResult<ICommandArg?>(null);
    }

    private ValueTask<ICommandArg?> Change(ActionCommandArg action)
    {
        ICommandArg? rollback = null;
        var portToDelete = manager.Router.Ports.FirstOrDefault(x => x.Id == action.Id);
        if (portToDelete != null)
        {
            rollback = CreateChangeArg(portToDelete, portToDelete.Config);
            manager.Router.RemovePort(portToDelete);
        }
        if (string.IsNullOrWhiteSpace(action.Value))
        {
            throw new ArgumentException("Invalid port configuration");
        }
        var newConfig = new ProtocolPortConfig(new Uri(action.Value));
        manager.Router.AddPort(newConfig.AsUri());
        return ValueTask.FromResult(rollback);
    }

    private ValueTask<ICommandArg?> AddPort(ActionCommandArg action)
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
        return new ValueTask<ICommandArg?>(CreateRemoveArg(newPort));
    }

    private ValueTask<ICommandArg?> Remove(ActionCommandArg action)
    {
        var portToDelete = manager.Router.Ports.FirstOrDefault(x => x.Id == action.Id);
        if (portToDelete != null)
        {
            var rollback = CreateAddArg(portToDelete.Config);
            portToDelete.Disable();
            manager.Router.RemovePort(portToDelete);
            return new ValueTask<ICommandArg?>(rollback);
        }

        return ValueTask.FromResult<ICommandArg?>(null);
    }
}
