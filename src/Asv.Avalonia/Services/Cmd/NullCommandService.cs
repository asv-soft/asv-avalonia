using Avalonia.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class NullCommandService : ICommandService
{
    private NullCommandService()
    {
        DesignTime.ThrowIfNotDesignMode();
        Commands = [ChangeThemeCommand.StaticInfo, UndoCommand.StaticInfo, RedoCommand.StaticInfo];
    }

    public static ICommandService Instance { get; } = new NullCommandService();
    public IEnumerable<ICommandInfo> Commands { get; }

    public ICommandHistory CreateHistory(IRoutable? owner)
    {
        return NullCommandHistory.Instance;
    }

    public ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable param,
        CancellationToken cancel = default
    )
    {
        return ValueTask.CompletedTask;
    }

    public void SetHotKey(string commandId, KeyGesture hotKey)
    {
        // Do nothing
    }

    public KeyGesture? GetHostKey(string commandId)
    {
        return KeyGesture.Parse("Ctrl + X");
    }

    public ValueTask Undo(CommandSnapshot command, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Redo(CommandSnapshot command, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public Observable<CommandEventArgs> OnCommand { get; } = new Subject<CommandEventArgs>();
    public IExportInfo Source => SystemModule.Instance;
}
