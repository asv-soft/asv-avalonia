using Asv.Common;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when an attempt is made to close a page.
/// This event allows page children to prevent closing by adding restrictions.
/// If there are no restrictions, the page is allowed to close.
/// </summary>
public class PageCloseAttemptEvent(IRoutable source)
    : AsyncRoutedEventWithRestrictionsBase(source, RoutingStrategy.Tunnel) { }

/// <summary>
/// Requests approval from child components before closing the page.
/// If any child prevents the closure, the method returns the list of restrictions.
/// </summary>
public static class PageCloseAttemptEventMixin
{
    public static async ValueTask<IReadOnlyList<Restriction>> RequestChildCloseApproval(
        this IPage src,
        CancellationToken cancel = default
    )
    {
        var eve = new PageCloseAttemptEvent(src);
        await src.Rise(eve, cancel);
        return eve.Restrictions;
    }
}
