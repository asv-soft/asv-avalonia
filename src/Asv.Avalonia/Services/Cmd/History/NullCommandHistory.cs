using R3;

namespace Asv.Avalonia;

public class NullCommandHistory : ICommandHistory
{
    public static ICommandHistory Instance { get; } = new NullCommandHistory();
    public string Id => "design";

    public IRoutable HistoryOwner { get; } = new DesignTimeShellViewModel();
    public ReactiveCommand Undo { get; } = new();

    public ValueTask UndoAsync(CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public ReactiveCommand Redo { get; } = new();

    public ValueTask RedoAsync(CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable? param,
        CancellationToken cancel = default
    )
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        HistoryOwner.Dispose();
        Undo.Dispose();
        Redo.Dispose();
    }
}
