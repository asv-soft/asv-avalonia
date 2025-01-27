using System.Windows.Input;
using R3;

namespace Asv.Avalonia;

public interface ICommandHistory : IDisposable
{
    IRoutable HistoryOwner { get; }
    ReactiveCommand Undo { get; }
    ValueTask UndoAsync(CancellationToken cancel = default);
    ReactiveCommand Redo { get; }
    ValueTask RedoAsync(CancellationToken cancel = default);
    ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable? param = null,
        CancellationToken cancel = default
    );
}
