using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportExtensionFor<ISettingsPage>]
[method: ImportingConstructor]
public class SettingsPluginsSourcesExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<ISettingsPage>
{
    public void Extend(ISettingsPage context, DisposableBag contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                SettingsPluginsSourcesViewModel.PageId,
                RS.SettingsPluginsSourcesViewModel_Name,
                MaterialIconKind.Cloud,
                SettingsPluginsSourcesViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
