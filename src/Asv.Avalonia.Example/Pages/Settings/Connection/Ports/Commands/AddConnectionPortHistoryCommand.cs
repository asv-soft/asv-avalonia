using System.Buffers;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportCommand]
[Shared]
public class AddConnectionPortHistoryCommand : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.addPort";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeThemeCommand_CommandInfo_Name,
        Description = RS.ChangeThemeCommand_CommandInfo_Description,
        Icon = MaterialIconKind.SerialPort,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    private IMavlinkConnectionService _connectionService;

    [ImportingConstructor]
    public AddConnectionPortHistoryCommand(IMavlinkConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<IPersistable?> Execute(IRoutable context, IPersistable newValue,
        CancellationToken cancel = default)
    {
        if (newValue is Persistable<KeyValuePair<string, string>> keyPair)
        {
            _connectionService.AddConnection(keyPair.Value.Key, keyPair.Value.Value);
            return ValueTask.FromResult<IPersistable?>(new Persistable<string>(keyPair.Value.Value));
        }

        return default;
    }

    protected override ValueTask<IPersistable?> InternalExecute(IPersistable newValue, CancellationToken cancel)
    {
        if (newValue is Persistable<KeyValuePair<string, string>> keyPair)
        {
            _connectionService.AddConnection(keyPair.Value.Key, keyPair.Value.Value);
            return ValueTask.FromResult<IPersistable?>(new Persistable<string>(keyPair.Value.Value));
        }

        return default;
    }
}