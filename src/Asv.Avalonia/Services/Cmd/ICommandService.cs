using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public interface ICommandService : IExportable
{
    IEnumerable<ICommandInfo> Commands { get; }
    ICommandHistory CreateHistory(IRoutable? owner);
    ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable param,
        CancellationToken cancel = default
    );
    void SetHotKey(string commandId, KeyGesture hotKey);
    KeyGesture? GetHostKey(string commandId);
    Observable<CommandEventArgs> OnCommand { get; }
    ValueTask Undo(CommandSnapshot command, CancellationToken cancel = default);
    ValueTask Redo(CommandSnapshot command, CancellationToken cancel = default);
}

public sealed class CommandSnapshot(
    string commandId,
    string[] contextPath,
    IPersistable newValue,
    IPersistable? oldValue
)
{
    public string CommandId { get; set; } = commandId;
    public string[] ContextPath { get; set; } = contextPath;
    public IPersistable NewValue { get; set; } = newValue;
    public IPersistable? OldValue { get; set; } = oldValue;
}

public class CommandEventArgs(IRoutable context, IAsyncCommand command, CommandSnapshot snapshot)
{
    public IRoutable Context { get; } = context;
    public IAsyncCommand Command { get; } = command;
    public CommandSnapshot Snapshot { get; } = snapshot;
}
