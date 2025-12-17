using System.Composition;
using Asv.Common;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<IShell>]
[method: ImportingConstructor]
public class DefaultStatusExtender(
    [ImportMany(ExportStatusItemAttribute.Contract)] IEnumerable<IStatusItem> items
) : IExtensionFor<IShell>
{
    public void Extend(IShell context, DisposableBag contextDispose)
    {
        context.StatusItems.AddRange(items.Select(x => x.DisposeItWith(contextDispose)));
    }
}
