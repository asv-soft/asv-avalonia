using R3;

namespace Asv.Avalonia;

public class NullCommandService : ICommandService
{
    private NullCommandService()
    {
        DesignTime.ThrowIfNotDesignMode();
        Commands =
        [
            ChangeThemeFreeCommand.StaticInfo,
            UndoCommand.StaticInfo,
            RedoCommand.StaticInfo,
            ChangeBoolPropertyCommand.StaticInfo,
            ChangeDoublePropertyCommand.StaticInfo,
        ];
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
        CommandArg param,
        CancellationToken cancel = default
    )
    {
        return ValueTask.CompletedTask;
    }

    public Observable<HotKeyInfo> OnHotKey { get; } = new Subject<HotKeyInfo>();
    public ReactiveProperty<bool> IsHotKeyRecognitionEnabled { get; } = new();

    public void ResetAllHotKeys()
    {
        // Do nothing
    }

    public void SetHotKey(string commandId, HotKeyInfo hotKey)
    {
        // Do nothing
    }

    public HotKeyInfo? GetHotKey(string commandId)
    {
        return HotKeyInfo.Parse("Ctrl + X ; K");
    }

    public ValueTask Undo(CommandSnapshot command, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Redo(CommandSnapshot command, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public Observable<CommandSnapshot> OnCommand { get; } = new Subject<CommandSnapshot>();
    public IExportInfo Source => SystemModule.Instance;
}
