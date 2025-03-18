using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class CommandHistory : ICommandHistory
{
    private readonly ICommandService _cmd;
    private readonly ObservableStack<CommandSnapshot> _undoStack = new();
    private readonly ObservableStack<CommandSnapshot> _redoStack = new();
    private readonly ILogger<CommandHistory> _logger;

    public CommandHistory(
        IRoutable? historyOwner,
        ICommandService cmd,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(historyOwner);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<CommandHistory>();
        _cmd = cmd;
        var dispose = Disposable.CreateBuilder();
        cmd.OnCommand.Subscribe(TryAddToHistory).AddTo(ref dispose);
        HistoryOwner = historyOwner;
        Undo = new ReactiveCommand((_, token) => UndoAsync(token)).AddTo(ref dispose);
        Redo = new ReactiveCommand((_, token) => RedoAsync(token)).AddTo(ref dispose);
        CheckUndoRedoCanExecute();
    }

    private void TryAddToHistory(CommandEventArgs cmd)
    {
        if (
            cmd.Snapshot.OldValue != null
            && cmd.Context.GetAncestorsToRoot().Contains(HistoryOwner)
        )
        {
            _undoStack.Push(cmd.Snapshot);
            _redoStack.Clear();
            CheckUndoRedoCanExecute();
        }
    }

    public IRoutable HistoryOwner { get; }
    public ReactiveCommand Undo { get; }

    public IObservableCollection<CommandSnapshot> UndoStack => _undoStack;

    public async ValueTask UndoAsync(CancellationToken cancel = default)
    {
        if (_undoStack.TryPop(out var command))
        {
            await _cmd.Undo(command, cancel);
            _redoStack.Push(command);
            CheckUndoRedoCanExecute();
        }
    }

    private void CheckUndoRedoCanExecute()
    {
        Undo.ChangeCanExecute(_undoStack.Count != 0);
        Redo.ChangeCanExecute(_redoStack.Count != 0);
    }

    public ReactiveCommand Redo { get; }

    public async ValueTask RedoAsync(CancellationToken cancel = default)
    {
        if (_redoStack.TryPop(out var command))
        {
            await _cmd.Redo(command, cancel);
            _undoStack.Push(command);
            CheckUndoRedoCanExecute();
        }
    }

    public IObservableCollection<CommandSnapshot> RedoStack => _redoStack;

    public void Dispose()
    {
        HistoryOwner.Dispose();
        Undo.Dispose();
        Redo.Dispose();
    }
}
