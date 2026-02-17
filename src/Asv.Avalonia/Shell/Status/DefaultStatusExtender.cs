using Asv.Common;

namespace Asv.Avalonia;

public class DefaultStatusExtender(IEnumerable<IStatusItem> items) : IExtensionFor<IShell>
{
    public const string Contract = "shell.status";

    public void Extend(IShell context, R3.CompositeDisposable contextDispose)
    {
        context.StatusItems.AddRange(items.Select(x => x.DisposeItWith(contextDispose)));
    }
}
