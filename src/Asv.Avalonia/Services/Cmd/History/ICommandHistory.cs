using System.Windows.Input;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ICommandHistory : IDisposable
{
    IRoutable HistoryOwner { get; }
    ReactiveCommand Undo { get; }
    ValueTask UndoAsync(CancellationToken cancel = default);
    IObservableCollection<HistoryItem> UndoStack { get; }
    ReactiveCommand Redo { get; }
    ValueTask RedoAsync(CancellationToken cancel = default);
    IObservableCollection<HistoryItem> RedoStack { get; }
    ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable? param = null,
        CancellationToken cancel = default
    );
}
