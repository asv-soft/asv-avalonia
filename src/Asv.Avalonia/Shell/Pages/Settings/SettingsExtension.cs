using System.Composition;
using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsExtension() : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                SettingsAppearanceViewModel.PageId,
                RS.SettingsAppearanceViewModel_Name,
                MaterialIconKind.ThemeLightDark,
                SettingsAppearanceViewModel.PageId,
                NavigationId.Empty
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                SettingsUnitsViewModel.PageId,
                RS.SettingsUnitsViewModel_Name,
                MaterialIconKind.TemperatureCelsius,
                SettingsUnitsViewModel.PageId,
                NavigationId.Empty
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                SettingsCommandListViewModel.SubPageId,
                RS.SettingsCommandListViewModel_Name,
                MaterialIconKind.KeyboardSettings,
                SettingsCommandListViewModel.SubPageId,
                NavigationId.Empty
            ).DisposeItWith(contextDispose)
        );
    }
}
