using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExtension() : IExtensionFor<ISettingsPage>
{
    private TreePage? _node1;
    private TreePage? _node2;
    private TreePage? _node3;

    public void Extend(ISettingsPage context)
    {
        _node1 = new TreePage(
            SettingsAppearanceViewModel.PageId,
            RS.SettingsAppearanceViewModel_Name,
            MaterialIconKind.ThemeLightDark,
            SettingsAppearanceViewModel.PageId
        );
        _node2 = new TreePage(
            SettingsUnitsViewModel.PageId,
            RS.SettingsUnitsViewModel_Name,
            MaterialIconKind.TemperatureCelsius,
            SettingsUnitsViewModel.PageId
        );
        _node3 = new TreePage(
            SettingsKeymapViewModel.SubPageId,
            RS.SettingsKeymapViewModel_Name,
            MaterialIconKind.KeyboardSettings,
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
