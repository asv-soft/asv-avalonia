using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

public class SaveLayoutToFileGlobalAttemptEvent(IViewModel source)
    : AsyncRoutedEventWithRestrictionsBase(source, RoutingStrategy.Tunnel) { }

public static class SaveLayoutToFileGlobalAttemptMixin
{
    public static async ValueTask<
        IReadOnlyCollection<Restriction>
    > RequestChildApprovalToSaveLayoutToFile(this IViewModel src, CancellationToken cancel = default)
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
