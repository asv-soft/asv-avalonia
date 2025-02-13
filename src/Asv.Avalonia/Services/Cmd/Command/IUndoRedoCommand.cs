namespace Asv.Avalonia;

/// <summary>
/// Defines an asynchronous undo-redo command interface that extends the basic
/// asynchronous command functionality with redo and undo operations.
/// </summary>
/// <remarks>
/// Implement this interface to enable asynchronous command execution
/// with the ability to persist state, and support undo and redo operations.
/// </remarks>
public interface IUndoRedoCommand : IAsyncCommand
{
    ValueTask Undo(IRoutable context, CancellationToken cancel = default);
    ValueTask Redo(IRoutable context, CancellationToken cancel = default);
}
