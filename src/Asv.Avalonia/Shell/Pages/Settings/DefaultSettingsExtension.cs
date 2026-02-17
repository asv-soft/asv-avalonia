using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// This helper extension adds all tree page items registered as keyed services of type <see cref="ITreePage"/>
/// with the key <see cref="SettingsPageViewModel.PageId"/> from the DI container to the settings page.
/// </summary>
public class DefaultSettingsExtension(
    [FromKeyedServices(SettingsPageViewModel.PageId)] IEnumerable<ITreePage> items
) : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        foreach (var treePage in items)
        {
            context.Nodes.Add(treePage);
            treePage.DisposeItWith(contextDispose);
        }
    }
}
