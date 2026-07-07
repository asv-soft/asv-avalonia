using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// This helper extension adds all tree page items registered as keyed services of type <see cref="ITreePageMenuItem"/>
/// with the key <see cref="SettingsPageViewModel.PageId"/> from the DI container to the settings page.
/// </summary>
public class DefaultSettingsExtension(
    [FromKeyedServices(SettingsPageViewModel.PageId)] IEnumerable<ITreePageMenuItem> items
) : IExtensionFor<ISettingsPage>
{
    public const string StaticId = "ext.settings.tree";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        foreach (var treePageMenuItem in items)
        {
            context.Nodes.Add(treePageMenuItem);
            treePageMenuItem.DisposeItWith(contextDispose);
        }
    }
}
