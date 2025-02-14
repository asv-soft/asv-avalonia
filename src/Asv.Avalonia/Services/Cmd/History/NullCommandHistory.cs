using ObservableCollections;
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

    public IObservableCollection<CommandSnapshot> UndoStack { get; } =
        new ObservableList<CommandSnapshot>();

    public ReactiveCommand Redo { get; } = new();

    public ValueTask RedoAsync(CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }

    public IObservableCollection<CommandSnapshot> RedoStack { get; } =
        new ObservableList<CommandSnapshot>();

    public void Dispose()
    {
        HistoryOwner.Dispose();
        Undo.Dispose();
        Redo.Dispose();
    }
}
