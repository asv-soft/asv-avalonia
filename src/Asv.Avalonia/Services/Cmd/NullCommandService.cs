using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class NullCommandService : ICommandService
{
    public NullCommandService()
    {
        Commands = new[] { ChangeThemeCommand.StaticInfo, UndoCommand.StaticInfo };
    }

    public static ICommandService Instance { get; } = new NullCommandService();
    public IEnumerable<ICommandInfo> Commands { get; }

    public IAsyncCommand? CreateCommand(string commandId)
    {
        return null;
    }

    public ICommandHistory CreateHistory(IRoutable owner)
    {
        return NullCommandHistory.Instance;
    }

    public bool CanExecuteCommand(string commandId, IRoutable context, out IRoutable? target)
    {
        target = null;
        return false;
    }

    public void ChangeHotKey(string commandId, KeyGesture? hotKey)
    {
        // do nothing
    }

    public bool CanExecuteCommand(
        KeyGesture hotKey,
        IRoutable context,
        out IAsyncCommand? command,
        out IRoutable? target
    )
    {
        command = null;
        target = null;
        return false;
    }

    public bool TryGetCommand(
        KeyGesture gesture,
        IRoutable context,
        out IAsyncCommand? command,
        out IRoutable? target
    )
    {
        command = null;
        target = null;
        return false;
    }
}
