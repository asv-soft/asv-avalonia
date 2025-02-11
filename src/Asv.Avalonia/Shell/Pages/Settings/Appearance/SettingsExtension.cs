using System.Composition;

namespace Asv.Avalonia;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExtension() : IExtensionFor<ISettingsPage>
{
    private TreePageNode? _node1;
    private TreePageNode? _node2;
    private TreePageNode? _node3;

    public void Extend(ISettingsPage context)
    {
        _node1 = new TreePageNode(
            SettingsAppearanceViewModel.PageId,
            RS.SettingsAppearanceViewModel_Name,
            SettingsAppearanceViewModel.PageId
        );
        _node2 = new TreePageNode(
            SettingsUnitsViewModel.PageId,
            RS.SettingsUnitsViewModel_Name,
            SettingsUnitsViewModel.PageId
        );
        _node3 = new TreePageNode(
            SettingsKeymapViewModel.SubPageId,
            RS.SettingsKeymapViewModel_Name,
            SettingsKeymapViewModel.SubPageId
        );
        context.Nodes.Add(_node1);
        context.Nodes.Add(_node2);
        context.Nodes.Add(_node3);
    }

    public void Dispose()
    {
        _node1?.Dispose();
        _node2?.Dispose();
        _node3?.Dispose();
    }
}
