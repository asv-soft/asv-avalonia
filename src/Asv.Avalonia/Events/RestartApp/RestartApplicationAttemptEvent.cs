using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when an attempt is made to restart the application.
/// This event allows the application to prevent restarting if needed.
/// If there are no restrictions, the application is allowed to restart.
/// </summary>
public class RestartApplicationAttemptEvent(IRoutable source)
    : AsyncRoutedEventWithRestrictionsBase(source, RoutingStrategy.Bubble) { }

/// <summary>
/// Requests approval from the application's components before restarting the application.
/// If any component prevents the restart, the method returns the list of restrictions.
/// </summary>
public static class RestartApplicationAttemptEventMixin
{
    public static async ValueTask<IReadOnlyList<Restriction>> RequestRestartApplicationApproval(
        this IRoutable src,
        CancellationToken cancel = default
    )
    {
        var eve = new RestartApplicationAttemptEvent(src);
        await src.Rise(eve, cancel);
        return eve.Restrictions;
    }
}
