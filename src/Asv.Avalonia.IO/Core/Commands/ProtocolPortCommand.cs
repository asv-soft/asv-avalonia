using System.Composition;
using Asv.IO;
using Material.Icons;

namespace Asv.Avalonia.IO;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class PortCrudCommand(IDeviceManager manager) : StatelessCrudCommand<StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.port.change";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.PortCrudCommand_CommandInfo_Name,
        Description = RS.PortCrudCommand_CommandInfo_Description,
        Icon = MaterialIconKind.SerialPort,
        DefaultHotKey = null,
        Source = IoModule.Instance,
    };

    public static ValueTask ExecuteRemove(IRoutable context, string portId)
    {
        return context.ExecuteCommand(Id, CommandArg.RemoveAction(portId));
    }

    public static ValueTask ExecuteChange(IRoutable context, string portId, ProtocolPortConfig cfg)
    {
        return context.ExecuteCommand(
            Id,
            CommandArg.ChangeAction(portId, new StringArg(cfg.AsUri().ToString()))
        );
    }

    public static ActionArg CreateAddArg(ProtocolPortConfig options)
    {
        return CommandArg.AddAction(new StringArg(options.AsUri().ToString()));
    }

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<string> Update(string subjectId, StringArg options)
    {
        // this is long-running operation, so we run it in a separate task
        return await Task.Factory.StartNew(
            () =>
            {
                var id = subjectId;
                var portToDelete = manager.Router.Ports.First(x => x.Id == id);

                if (string.IsNullOrWhiteSpace(options.Value))
                {
                    throw new ArgumentException("Invalid port configuration");
                }

                var newConfig = new ProtocolPortConfig(new Uri(options.Value));

                manager.Router.RemovePort(portToDelete);
                return manager.Router.AddPort(newConfig.AsUri()).Id;
            },
            TaskCreationOptions.LongRunning
        );
    }

    protected override async ValueTask Delete(string subjectId)
    {
        // this is long-running operation, so we run it in a separate task
        await Task.Factory.StartNew(
            () =>
            {
                var portToDelete = manager.Router.Ports.First(x => x.Id == subjectId);
                manager.Router.RemovePort(portToDelete);
            },
            TaskCreationOptions.LongRunning
        );
    }

    protected override async ValueTask<string> Create(StringArg options)
    {
        // this is a long-running operation, so we run it in a separate task
        return await Task.Factory.StartNew(
            () =>
            {
                var newConfig = new ProtocolPortConfig(new Uri(options.Value));
                return manager.Router.AddPort(newConfig.AsUri()).Id;
            },
            TaskCreationOptions.LongRunning
        );
    }

    protected override ValueTask<StringArg> Read(string subjectId)
    {
        var options = manager.Router.Ports.First(x => x.Id == subjectId).Config.AsUri().ToString();
        return ValueTask.FromResult(new StringArg(options));
    }
}
