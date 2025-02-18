namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when an attempt is made to close a page.
/// This event allows page children to prevent closing by adding restrictions.
/// If there are no restrictions, the page is allowed to close.
/// </summary>
public class PageCloseAttemptEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Tunnel)
{
    private readonly List<PageCloseRestriction> _restrictions = new();

    /// <summary>
    /// Gets the list of restrictions preventing the page from closing.
    /// </summary>
    public IReadOnlyList<PageCloseRestriction> Restrictions => _restrictions;

    /// <summary>
    /// Adds a restriction preventing the page from closing.
    /// </summary>
    /// <param name="restriction">The restriction that prevents the page from closing.</param>
    public void AddRestriction(PageCloseRestriction restriction)
    {
        _restrictions.Add(restriction);
    }
}

/// <summary>
/// Represents a reason why a page cannot be closed.
/// This can be used to provide detailed feedback on why the closing attempt failed.
/// </summary>
public class PageCloseRestriction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageCloseRestriction"/> class.
    /// </summary>
    /// <param name="source">The source of the restriction, typically a UI component or module.</param>
    /// <param name="message">The message explaining why the page cannot be closed.</param>
    public PageCloseRestriction(IRoutable source, string message)
    {
        Source = source;
        Message = message;
    }

    /// <summary>
    /// Gets or sets the source that imposed the restriction.
    /// </summary>
    public IRoutable Source { get; set; }

    /// <summary>
    /// Gets or sets the message explaining why the page cannot be closed.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// Requests approval from child components before closing the page.
/// If any child prevents the closure, the method returns false and provides the list of restrictions
/// </summary>
public static class PageCloseAttemptEventMixin
{
    public static async ValueTask<IReadOnlyList<PageCloseRestriction>> RequestChildCloseApproval(
        this IPage src
    )
    {
        var eve = new PageCloseAttemptEvent(src);
        await src.Rise(eve);
        return eve.Restrictions;
    }
}
