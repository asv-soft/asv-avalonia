using Avalonia.Input;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ICommandService
{
    IEnumerable<ICommandInfo> Commands { get; }
    IAsyncCommand? CreateCommand(string commandId);
    ICommandHistory CreateHistory(IRoutable owner);
    bool CanExecuteCommand(string commandId, IRoutable context, out IRoutable? target);
    bool ChangeHotKey(string commandId, KeyGesture? hotKey);
    bool CanExecuteCommand(
        KeyGesture hotKey,
        IRoutable context,
        out IAsyncCommand? command,
        out IRoutable? target
    );
}
