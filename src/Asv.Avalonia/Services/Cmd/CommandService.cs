using System.Collections.Immutable;
using System.Composition;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[Export(typeof(ICommandService))]
[Shared]
public class CommandService : ICommandService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImmutableDictionary<string, ICommandFactory> _commands;

    [ImportingConstructor]
    public CommandService(
        [ImportMany] IEnumerable<ICommandFactory> factories,
        ILoggerFactory loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _commands = factories.ToImmutableDictionary(x => x.Info.CommandId);
    }

    public IEnumerable<ICommandInfo> Commands => _commands.Values.Select(x => x.Info);

    public IAsyncCommand? CreateCommand(string commandId)
    {
        return _commands.TryGetValue(commandId, out var command) ? command.Create() : null;
    }

    public ICommandHistory CreateHistory(IRoutable owner)
    {
        var history = new CommandHistory(owner, this, _loggerFactory);
        return history;
    }

    public bool CanExecuteCommand(string commandId, IRoutable context, out IRoutable? target)
    {
        if (_commands.TryGetValue(commandId, out var command))
        {
            return command.CanExecute(context, out target);
        }
        
        target = null;
        return false;
    }
}


