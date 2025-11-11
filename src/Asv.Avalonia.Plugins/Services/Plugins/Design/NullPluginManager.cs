using System.Reflection;
using Asv.Common;

namespace Asv.Avalonia.Plugins;

public class NullPluginManager : IPluginManager
{
    public static IPluginManager Instance { get; } = new NullPluginManager();

    public IReadOnlyList<IPluginServerInfo> Servers => [NullPluginServerInfo.Instance];

    public IReadOnlyList<Assembly> PluginsAssemblies { get; } = new List<Assembly>();
    public SemVersion ApiVersion => new(new Version());

    public void AddServer(PluginServer server) { }

    public void RemoveServer(IPluginServerInfo server) { }

    public Task<IReadOnlyList<IPluginSearchInfo>> Search(
        SearchQuery query,
        CancellationToken cancel
    )
    {
        return Task.FromResult<IReadOnlyList<IPluginSearchInfo>>([NullPluginSearchInfo.Instance]);
    }

    public Task<IReadOnlyList<string>> ListPluginVersions(
        SearchQuery query,
        string pluginId,
        CancellationToken cancel
    )
    {
        return Task.FromResult<IReadOnlyList<string>>([NullPluginSearchInfo.Instance.LastVersion]);
    }

    public Task Install(
        IPluginServerInfo source,
        string packageId,
        string version,
        IProgress<ProgressMessage>? progress,
        CancellationToken cancel
    )
    {
        return Task.CompletedTask;
    }

    public Task InstallManually(
        string from,
        IProgress<ProgressMessage>? progress,
        CancellationToken cancel
    )
    {
        return Task.CompletedTask;
    }

    public void Uninstall(ILocalPluginInfo plugin) { }

    public void CancelUninstall(ILocalPluginInfo pluginInfo) { }

    public IEnumerable<ILocalPluginInfo> Installed { get; } = [NullLocalPluginInfo.Instance];

    public bool IsInstalled(string packageId, out ILocalPluginInfo? info)
    {
        info = null;
        return false;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
