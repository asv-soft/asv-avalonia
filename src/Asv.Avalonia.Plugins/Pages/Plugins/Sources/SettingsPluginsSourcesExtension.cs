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
    public void Extend(ISettingsPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                PluginsSourcesViewModel.PageId,
                RS.PluginsSourcesViewModel_Name,
                MaterialIconKind.Cloud,
                PluginsSourcesViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
