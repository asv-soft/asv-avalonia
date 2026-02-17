namespace Asv.Avalonia;

/// <summary>
/// Defines an asynchronous command that can be executed within a routable context.
/// </summary>
public interface IAsyncCommand
{
    /// <summary>
    /// Gets metadata information about the command.
    /// </summary>
    ICommandInfo Info { get; }

    /// <summary>
    /// Determines whether the command can be executed in the given context with the provided parameter.
    /// </summary>
    /// <param name="context">The routable context in which the command is executed.</param>
    /// <param name="parameter">The input parameter that may affect execution.</param>
    /// <param name="targetContext">If the command can be executed, this parameter should be set to the target context where the command will be executed.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    bool CanExecute(IRoutable context, CommandArg parameter, out IRoutable targetContext);

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="targetContext">The routable context where the command is executed.</param>
    /// <param name="newValue">The new value to be applied during execution.</param>
    /// <param name="cancel">A cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.
    /// If the command supports undo functionality, it should return a non-null <see cref="CommandArg"/> value,
    /// which represents the previous state before execution. This value will be used to revert the operation
    /// when an undo action is triggered.
    /// </returns>
    ValueTask<CommandArg?> Execute(
        IRoutable targetContext,
        CommandArg newValue,
        CancellationToken cancel = default
    );
}

public interface IAsyncCommand<TArg> : IAsyncCommand
    where TArg : CommandArg
{
    ValueTask<TArg?> Execute(
        IRoutable targetContext,
        TArg newValue,
        CancellationToken cancel = default
    );
}
