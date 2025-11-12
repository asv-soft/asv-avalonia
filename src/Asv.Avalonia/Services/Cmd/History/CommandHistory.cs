using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class CommandHistory : ICommandHistory
{
    private const string DefaultHistoryFolderName = "command-history";
    private const string UndoPostfix = ".undo.json";
    private const string RedoPostfix = ".redo.json";

    private readonly ICommandService _cmd;
    private readonly string _historyFolder;
    private readonly ObservableStack<CommandSnapshot> _undoStack = new();
    private readonly ObservableStack<CommandSnapshot> _redoStack = new();
    private readonly ILogger<CommandHistory> _logger;

    public CommandHistory(
        IRoutable? historyOwner,
        ICommandService cmd,
        IAppPath path,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(historyOwner);
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<CommandHistory>();
        _cmd = cmd;
        _historyFolder = Path.Combine(path.UserDataFolder, DefaultHistoryFolderName);
        if (!Directory.Exists(_historyFolder))
        {
            Directory.CreateDirectory(_historyFolder);
        }

        var dispose = Disposable.CreateBuilder();
        cmd.OnCommand.Subscribe(TryAddToHistory).AddTo(ref dispose);
        HistoryOwner = historyOwner;

        Undo = new ReactiveCommand((_, token) => UndoAsync(token)).AddTo(ref dispose);
        Redo = new ReactiveCommand((_, token) => RedoAsync(token)).AddTo(ref dispose);

        _undoStack
            .ObserveCountChanged(true)
            .Subscribe(c => Undo.ChangeCanExecute(c != 0))
            .AddTo(ref dispose);
        _redoStack
            .ObserveCountChanged(true)
            .Subscribe(c => Redo.ChangeCanExecute(c != 0))
            .AddTo(ref dispose);

        historyOwner
            .ObservePropertyChanged(x => x.Id)
            .Subscribe(TryLoadHistoryFromFile)
            .AddTo(ref dispose);
    }

    private void GetFilePath(NavigationId navigationId, out string undoPath, out string redoPath)
    {
        var baseName = NavigationId.NormalizeTypeId(navigationId.ToString().ToLower());
        undoPath = Path.Combine(_historyFolder, $"{baseName}{UndoPostfix}");
        redoPath = Path.Combine(_historyFolder, $"{baseName}{RedoPostfix}");
    }

    private void SaveHistoryToFile()
    {
        GetFilePath(HistoryOwner.Id, out var undoPath, out var redoPath);

        try
        {
            CommandHistoryFile.Save(undoPath, _undoStack.Reverse(), HistoryOwner.Id.ToString());
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Failed to save undo history to file {undoPath}");
        }

        try
        {
            CommandHistoryFile.Save(redoPath, _redoStack.Reverse(), HistoryOwner.Id.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to save redo history to file {redoPath}");
        }
    }

    private void TryLoadHistoryFromFile(NavigationId navigationId)
    {
        // history id can be changed after full load of the history owner
        GetFilePath(navigationId, out var undoPath, out var redoPath);
        try
        {
            if (File.Exists(undoPath))
            {
                _undoStack.Clear();
                foreach (var snapshot in CommandHistoryFile.Load(undoPath, null))
                {
                    _undoStack.Push(snapshot);
                }

                _logger.ZLogDebug(
                    $"Loaded undo history from file {undoPath} {_undoStack.Count} items"
                );
            }
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Failed to load undo history from file {undoPath}");
        }

        try
        {
            if (File.Exists(redoPath))
            {
                _redoStack.Clear();
                foreach (var snapshot in CommandHistoryFile.Load(redoPath, null))
                {
                    _redoStack.Push(snapshot);
                }

                _logger.ZLogDebug(
                    $"Loaded redo history from file {redoPath} {_redoStack.Count} items"
                );
            }
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Failed to load redo history from file {redoPath}");
        }
    }

    private void TryAddToHistory(CommandSnapshot snapshot)
    {
        if (
            snapshot.OldValue != null
            && snapshot.ContextPath.StartWith(HistoryOwner.GetPathToRoot())
        )
        {
            _undoStack.Push(snapshot);
            _redoStack.Clear();
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
        }
    }

    public ReactiveCommand Redo { get; }

    public async ValueTask RedoAsync(CancellationToken cancel = default)
    {
        if (_redoStack.TryPop(out var command))
        {
            await _cmd.Redo(command, cancel);
            _undoStack.Push(command);
        }
    }

    public IObservableCollection<CommandSnapshot> RedoStack => _redoStack;

    public void Dispose()
    {
        SaveHistoryToFile();
        Undo.Dispose();
        Redo.Dispose();
    }
}
