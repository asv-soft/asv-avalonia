namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when an attempt is made to restart the application.
/// This event allows the application to prevent restarting if needed.
/// If there are no restrictions, the application is allowed to restart.
/// </summary>
public class RestartApplicationAttemptEvent(IRoutable source)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    private readonly List<RestartApplicationRestriction> _restrictions = [];

    /// <summary>
    /// Gets the list of restrictions preventing the application from restarting.
    /// </summary>
    public IReadOnlyList<RestartApplicationRestriction> Restrictions => _restrictions;

    /// <summary>
    /// Adds a restriction preventing the application's restart.
    /// </summary>
    /// <param name="restriction">The restriction that prevents the application from restarting.</param>
    public void AddRestriction(RestartApplicationRestriction restriction)
    {
        _restrictions.Add(restriction);
    }
}

/// <summary>
/// Represents a reason why the application cannot be restarted.
/// This can be used to provide detailed feedback on why the restarting attempt failed.
/// </summary>
public class RestartApplicationRestriction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestartApplicationRestriction"/> class.
    /// </summary>
    /// <param name="source">The source of the restriction, typically a UI component or module.</param>
    /// <param name="message">The message explaining why the application cannot be restarted.</param>
    public RestartApplicationRestriction(IRoutable source, string message)
    {
        Source = source;
        Message = message;
    }

    /// <summary>
    /// Gets or sets the source that imposed the restriction.
    /// </summary>
    public IRoutable Source { get; set; }

    /// <summary>
    /// Gets or sets the message explaining why the application cannot be restarted.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// Requests approval from the application's components before restarting the application.
/// If any component prevents the restart, the method returns the list of restrictions.
/// </summary>
public static class RestartApplicationAttemptEventMixin
{
    public static async ValueTask<
        IReadOnlyList<RestartApplicationRestriction>
    > RequestRestartApplicationApproval(this IRoutable src)
    {
        var eve = new RestartApplicationAttemptEvent(src);
        await src.Rise(eve);
        return eve.Restrictions;
    }
}
