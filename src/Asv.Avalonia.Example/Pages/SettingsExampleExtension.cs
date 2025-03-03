using System.Composition;
using Material.Icons;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExampleExtension() : IExtensionFor<ISettingsPage>
{
    private TreePage? _node4;

    public void Extend(ISettingsPage context)
    {
        _node4 = new TreePage(
            SettingsConnectionViewModel.SubPageId,
            "Connection",
            MaterialIconKind.Connection,
            SettingsConnectionViewModel.SubPageId
        );
        context.Nodes.Add(_node4);
    }

    public void Dispose()
    {
        _node4?.Dispose();
    }
}
