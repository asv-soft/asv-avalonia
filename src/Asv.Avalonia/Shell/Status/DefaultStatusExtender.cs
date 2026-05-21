using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public class DefaultStatusExtender(
    [FromKeyedServices(DefaultStatusExtender.Contract)] IEnumerable<IStatusItem> items
) : IExtensionFor<IShell>
{
    public const string Contract = "shell.status";

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.StatusItems.AddRange(items.Select(x => x.DisposeItWith(contextDispose)));
    }
}
