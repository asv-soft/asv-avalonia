using System.Reflection;
using Asv.Common;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public interface IPluginManager : IHostedService
{
    IReadOnlyList<IPluginServerInfo> Servers { get; }
    IEnumerable<ILocalPluginInfo> Installed { get; }
    IReadOnlyList<Assembly> PluginsAssemblies { get; }
    SemVersion ApiVersion { get; }
    void AddServer(PluginServer server);
    void RemoveServer(IPluginServerInfo server);
    Task<IReadOnlyList<IPluginSearchInfo>> Search(
        SearchQuery query,
        IProgress<ProgressMessage>? progress = null,
        CancellationToken cancel = default
    );
    Task<IReadOnlyList<string>> ListPluginVersions(
        SearchQuery query,
        string pluginId,
        CancellationToken cancel = default
    );
    Task Install(
        IPluginServerInfo source,
        string packageId,
        string version,
        IProgress<ProgressMessage>? progress = null,
        CancellationToken cancel = default
    );
    Task InstallManually(
        string from,
        IProgress<ProgressMessage>? progress = null,
        CancellationToken cancel = default
    );
    void Uninstall(ILocalPluginInfo plugin);
    void CancelUninstall(ILocalPluginInfo pluginInfo);
    bool IsInstalled(string packageId, out ILocalPluginInfo? info);
}
