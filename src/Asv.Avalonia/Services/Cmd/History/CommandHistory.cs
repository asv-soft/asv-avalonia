using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class CommandHistory : ICommandHistory
{
    private readonly ICommandService _cmd;
    private readonly Stack<(IUndoRedoCommand, string[])> _undoStack = new();
    private readonly Stack<(IUndoRedoCommand, string[])> _redoStack = new();
    private readonly ILogger<CommandHistory> _logger;

    public CommandHistory(IRoutable historyOwner, ICommandService cmd, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(historyOwner);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<CommandHistory>();
        _cmd = cmd;
        HistoryOwner = historyOwner;
        Undo = new ReactiveCommand((_, token) => UndoAsync(token));
        Redo = new ReactiveCommand((_, token) => RedoAsync(token));
    }

    public IRoutable HistoryOwner { get; }
    public ReactiveCommand Undo { get; }

    public async ValueTask UndoAsync(CancellationToken cancel = default)
    {
        if (_undoStack.TryPop(out var command))
        {
            _logger.ZLogInformation($"Undo command {command.Item1.Info}");
            var context = await GetContext(HistoryOwner, command.Item2);
            await command.Item1.Undo(context, cancel);
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
            var context = await GetContext(HistoryOwner, command.Item2);
            await command.Item1.Redo(context, cancel);
            _undoStack.Push(command);
            CheckUndoRedoCanExecute();
        }
    }

    private ValueTask<IRoutable> GetContext(IRoutable owner, string[] path)
    {
        if (path.Length == 0 || path[0] != HistoryOwner.Id)
        {
            // this command is not for us
            return ValueTask.FromResult(HistoryOwner);
        }

        return HistoryOwner.NavigateTo(path[1..]);
    }

    public ValueTask Execute(
        string commandId,
        IRoutable context,
        IPersistable? param,
        CancellationToken cancel = default
    )
    {
        var cmd = _cmd.CreateCommand(commandId);
        if (cmd == null)
        {
            return ValueTask.CompletedTask;
        }

        if (cmd is IUndoRedoCommand undoable)
        {
            var contextPath = context.GetAllFrom(HistoryOwner).Select(x => x.Id).ToArray();
            _undoStack.Push((undoable, contextPath));
            _redoStack.Clear();
            CheckUndoRedoCanExecute();
        }

        return cmd.Execute(context, param, cancel);
    }

    public void Dispose()
    {
        HistoryOwner.Dispose();
        Undo.Dispose();
        Redo.Dispose();
    }
}
