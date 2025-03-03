using System.Windows.Input;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ICommandHistory : IDisposable
{
    IRoutable HistoryOwner { get; }
    ReactiveCommand Undo { get; }
    ValueTask UndoAsync(CancellationToken cancel = default);
    IObservableCollection<CommandSnapshot> UndoStack { get; }
    ReactiveCommand Redo { get; }
    ValueTask RedoAsync(CancellationToken cancel = default);
    IObservableCollection<CommandSnapshot> RedoStack { get; }
}
