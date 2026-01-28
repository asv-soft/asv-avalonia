using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered by a page to notify its parent (IShell) that it requests closure.
/// The shell must handle this event and determine whether the page should be closed.
/// </summary>
public class PageCloseRequestedEvent(IPage source)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public IPage Page => source;
}

/// <summary>
/// Provides an extension method for requesting a page closure by raising a <see cref="PageCloseRequestedEvent"/>.
/// This allows the shell or other components to react to the closure request.
/// </summary>
public static class PageCloseRequestMixin
{
    /// <summary>
    /// Raises a <see cref="PageCloseRequestedEvent"/> for the specified page.
    /// This event signals the parent shell that the page intends to close.
    /// The shell must handle this event and either allow or prevent the closure.
    /// </summary>
    /// <param name="src">The page that is requesting to close.</param>
    /// <param name="cancel">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing an asynchronous operation.</returns>
    public static ValueTask RequestClose(this IPage src, CancellationToken cancel = default)
    {
        return src.Rise(new PageCloseRequestedEvent(src), cancel);
    }
}
