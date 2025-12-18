using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered by a page to notify its parent (IShell) that it requests restart.
/// The shell must handle this event and determine whether the application should be restarted.
/// </summary>
public sealed class RestartApplicationEvent(IRoutable source)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble) { }

/// <summary>
/// Provides an extension method for requesting an application's restart by raising a <see cref="RestartApplicationEvent"/>.
/// This allows the shell or other components to react to the restart request.
/// </summary>
public static class RestartApplicationMixin
{
    /// <summary>
    /// Raises a <see cref="RestartApplicationEvent"/> for the specified page.
    /// This event signals the parent shell that the application intends to restart.
    /// The shell must handle this event and either allow or prevent the restart.
    /// </summary>
    /// <param name="src">The routable that wants to restart the app.</param>
    /// <param name="cancel">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing an asynchronous operation.</returns>
    public static ValueTask RequestRestart(this IRoutable src, CancellationToken cancel = default)
    {
        return src.Rise(new RestartApplicationEvent(src), cancel);
    }
}
