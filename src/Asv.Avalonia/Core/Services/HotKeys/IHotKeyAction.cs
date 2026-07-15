namespace Asv.Avalonia;

/// <summary>
/// Defines an executable hot key action.
/// </summary>
public interface IHotKeyAction : IHotKeyInfo
{
    /// <summary>
    /// Determines whether the action can execute for the given context.
    /// </summary>
    /// <param name="context">The view model context.</param>
    /// <returns><see langword="true"/> when the action can execute; otherwise, <see langword="false"/>.</returns>
    bool CanExecute(IViewModel context);

    /// <summary>
    /// Executes the action against the given context.
    /// </summary>
    /// <param name="context">The view model context.</param>
    /// <param name="cancel">A token that cancels the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Execute(IViewModel context, CancellationToken cancel = default);
}
