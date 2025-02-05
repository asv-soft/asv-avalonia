using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class HistoryItem
{
    public IUndoRedoCommand Command { get; }
    public string[] ContextPath { get; }

    public HistoryItem(IUndoRedoCommand command, string[] contextPath)
    {
        Command = command;
        ContextPath = contextPath;
    }

    public override string ToString()
    {
        return $"{Command.Info.Id}[{string.Join(">", ContextPath)}]";
    }
}

public class CommandHistory : ICommandHistory
{
    private readonly ICommandService _cmd;
    private readonly ObservableStack<HistoryItem> _undoStack = new();
    private readonly ObservableStack<HistoryItem> _redoStack = new();
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

    public IObservableCollection<HistoryItem> UndoStack => _undoStack;

    public async ValueTask UndoAsync(CancellationToken cancel = default)
    {
        if (_undoStack.TryPop(out var command))
        {
            var context = await GetContext(HistoryOwner, command.ContextPath);

            _logger.ZLogInformation(
                $"Undo command {command.Command.Info.Id} with {string.Join(">", command.ContextPath)} context"
            );
            await command.Command.Undo(context, cancel);
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
            var context = await GetContext(HistoryOwner, command.ContextPath);
            await command.Command.Redo(context, cancel);
            _undoStack.Push(command);
            CheckUndoRedoCanExecute();
        }
    }

    public IObservableCollection<HistoryItem> RedoStack => _redoStack;

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
            _undoStack.Push(new HistoryItem(undoable, contextPath));
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
