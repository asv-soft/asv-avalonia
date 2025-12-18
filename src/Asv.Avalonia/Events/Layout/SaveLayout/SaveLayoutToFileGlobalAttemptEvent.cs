using Asv.Common;

namespace Asv.Avalonia;

public class SaveLayoutToFileGlobalAttemptEvent(IRoutable source)
    : AsyncRoutedEventWithRestrictionsBase(source, RoutingStrategy.Tunnel) { }

public static class SaveLayoutToFileGlobalAttemptMixin
{
    public static async ValueTask<
        IReadOnlyCollection<Restriction>
    > RequestChildApprovalToSaveLayoutToFile(this IRoutable src, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return [];
        }

        var eve = new SaveLayoutToFileGlobalAttemptEvent(src);
        await src.Rise(eve, cancel);
        return eve.Restrictions;
    }
}
