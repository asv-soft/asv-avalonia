using System.Composition;
using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia.IO;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsPageExtension() : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                SettingsConnectionViewModel.SubPageId,
                "Connection",
                MaterialIconKind.Connection,
                SettingsConnectionViewModel.SubPageId,
                NavigationId.Empty
            ).DisposeItWith(contextDispose)
        );
    }
}
