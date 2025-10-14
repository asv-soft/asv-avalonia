using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginInfoViewModel : DisposableViewModel
{
    private readonly IPluginSearchInfo _pluginInfo;
    private readonly IPluginManager _manager;
    private readonly ObservableList<string> _pluginVersions;
    private ILocalPluginInfo? _localInfo;

    public PluginInfoViewModel()
        : base(DesignTime.Id, NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Install = CoreDesignTime.CancellableCommand<Unit>();
        Uninstall = CoreDesignTime.CancellableCommand<Unit>();
        CancelUninstall = CoreDesignTime.CancellableCommand<Unit>();
    }

    public PluginInfoViewModel(
        string id,
        IPluginSearchInfo pluginInfo,
        IPluginManager manager,
        ILayoutService layoutService,
        ILoggerFactory logFactory
    )
        : base(id, layoutService, logFactory)
    {
        _pluginInfo = pluginInfo;
        _manager = manager;
        Id = pluginInfo.Id;
        Install = new CancellableCommandWithProgress<Unit>(InstallImpl, "Install", logFactory);
        Uninstall = new CancellableCommandWithProgress<Unit>(
            UninstallImpl,
            "Uninstall",
            logFactory
        );
        CancelUninstall = new CancellableCommandWithProgress<Unit>(
            CancelUninstallImpl,
            "CancelUninstall",
            logFactory
        );
        IsInstalled = new BindableReactiveProperty<bool>(
            _manager.IsInstalled(pluginInfo.PackageId, out _localInfo)
        );
        IsUninstalled = new BindableReactiveProperty<bool>(true);
        if (_localInfo != null)
        {
            IsUninstalled.OnNext(_localInfo.IsUninstalled);
        }

        Name = pluginInfo.Title;
        Author = pluginInfo.Authors;
        Description = pluginInfo.Description;
        SourceName = pluginInfo.Source.Name;
        SourceUri = pluginInfo.Source.SourceUri;
        LastVersion = $"{pluginInfo.LastVersion} (API: {pluginInfo.ApiVersion})";
        IsApiCompatible = pluginInfo.ApiVersion == manager.ApiVersion;
        LocalVersion =
            _localInfo != null ? $"{_localInfo?.Version} (API: {_localInfo?.ApiVersion})" : null;
        DownloadCount = pluginInfo.DownloadCount.ToString();
        Tags = pluginInfo.Tags;
        Dependencies = new List<string>();
        IsVerified = new BindableReactiveProperty<bool>();
        SelectedVersion = new BindableReactiveProperty<string>();
        foreach (var dependency in pluginInfo.Dependencies)
        {
            if (dependency.VersionRange.MinVersion != null)
            {
                Dependencies.Add($"{dependency.Id} ( \u2265 {dependency.VersionRange.MinVersion})");
            }
        }

        if (Author != null)
        {
            IsVerified.OnNext(
                Author.Contains("https://github.com/asv-soft")
                    && SourceUri.Contains("https://nuget.pkg.github.com/asv-soft/index.json")
            );
        }

        Version = pluginInfo.LastVersion;

        _pluginVersions = new ObservableList<string>();
        PluginVersionsView = _pluginVersions.CreateView(x => x).ToNotifyCollectionChanged();

        Task.Factory.StartNew(GetPreviousVersions);
    }

    public bool IsApiCompatible { get; set; }
    public string Id { get; set; }
    public string? Author { get; set; }
    public string? SourceUri { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SourceName { get; set; }
    public string LastVersion { get; set; }
    public string Version { get; set; }
    public string? LocalVersion { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<string> PluginVersionsView { get; set; }
    public string? DownloadCount { get; set; }
    public string? Tags { get; set; }
    public List<string> Dependencies { get; set; }
    public BindableReactiveProperty<string> SelectedVersion { get; set; }
    public BindableReactiveProperty<bool> IsInstalled { get; set; }
    public BindableReactiveProperty<bool> IsUninstalled { get; set; }
    public BindableReactiveProperty<bool> IsVerified { get; set; }
    public CancellableCommandWithProgress<Unit> Uninstall { get; }
    public CancellableCommandWithProgress<Unit> CancelUninstall { get; }
    public CancellableCommandWithProgress<Unit> Install { get; }

    private Task UninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        progress.Report(0);
        cancel.ThrowIfCancellationRequested();

        _manager.Uninstall(_localInfo);
        IsUninstalled.OnNext(true);
        return Task.CompletedTask;
    }

    private Task CancelUninstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        if (_localInfo == null)
        {
            throw new Exception("Plugin not installed");
        }

        _manager.CancelUninstall(_localInfo);
        IsUninstalled.OnNext(false);
        return Task.CompletedTask;
    }

    private async Task InstallImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        await _manager.Install(
            _pluginInfo.Source,
            _pluginInfo.PackageId,
            SelectedVersion.Value,
            new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
            cancel
        );
        IsInstalled.OnNext(_manager.IsInstalled(_pluginInfo.PackageId, out _localInfo));
    }

    private async Task GetPreviousVersions()
    {
        var searchQuery = new SearchQuery() { Name = Name, IncludePrerelease = true };

        foreach (var server in _manager.Servers)
        {
            searchQuery.Sources.Add(server.SourceUri);
        }

        var previousVersions = await _manager.ListPluginVersions(
            searchQuery,
            _pluginInfo.PackageId,
            CancellationToken.None
        );
        _pluginVersions.Clear();
        _pluginVersions.AddRange(previousVersions);

        if (_pluginVersions.Count > 0)
        {
            SelectedVersion.OnNext(_pluginVersions[0]);
        }
    }
}
