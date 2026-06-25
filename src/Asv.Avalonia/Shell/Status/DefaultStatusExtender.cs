using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

public class DefaultStatusExtender(
    [FromKeyedServices(DefaultStatusExtender.Contract)] IEnumerable<IStatusItem> items
) : IExtensionFor<IShell>
{
    public const string StaticId = "ext.shell.status";

    public const string Contract = "shell.status";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.StatusItems.AddRange(items.Select(x => x.DisposeItWith(contextDispose)));
    }
}
