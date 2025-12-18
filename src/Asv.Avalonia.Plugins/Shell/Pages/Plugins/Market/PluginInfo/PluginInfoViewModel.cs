using System.Collections.Immutable;
using Asv.Common;
using Asv.IO;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public class PluginInfoViewModel : RoutableViewModel
{
    public const string ViewModelIdPart = "plugin";

    private readonly IPluginSearchInfo _pluginInfo;
    private readonly IPluginManager _manager;
    private readonly ObservableList<string> _pluginVersions;
    private ILocalPluginInfo? _localInfo;

    public PluginInfoViewModel()
        : this(NullPluginSearchInfo.Instance, NullPluginManager.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PluginInfoViewModel(
        IPluginSearchInfo pluginInfo,
        IPluginManager manager,
        ILoggerFactory logFactory
    )
        : base(new NavigationId(ViewModelIdPart, pluginInfo.Id), logFactory)
    {
        _pluginInfo = pluginInfo;
        _manager = manager;

        Install = new CancellableCommandWithProgress<Unit>(
            InstallImpl,
            RS.PluginInfoViewModel_InstallCommand_Title,
            logFactory
        ).DisposeItWith(Disposable);

        IsInstalled = new BindableReactiveProperty<bool>(
            _manager.IsInstalled(pluginInfo.PackageId, out _localInfo)
        );
        var isUninstalled = new ReactiveProperty<bool>(
            _localInfo?.IsUninstalled ?? false
        ).DisposeItWith(Disposable);
        IsUninstalled = new HistoricalBoolProperty(nameof(IsUninstalled), isUninstalled, logFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        SelectedVersion = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(
            Disposable
        );

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
        IsVerified = pluginInfo.IsVerified;
        Dependencies =
        [
            .. pluginInfo
                .Dependencies.Where(d => d.VersionRange.MinVersion is not null)
                .Select(d => $"{d.Id} ( \u2265 {d.VersionRange.MinVersion})"),
        ];

        IsUninstalled
            .ViewValue.Synchronize()
            .Skip(1)
            .Subscribe(uninstalled =>
            {
                if (_localInfo is null)
                {
                    throw new Exception("Plugin not installed");
                }

                if (uninstalled)
                {
                    manager.Uninstall(_localInfo);
                    return;
                }

                manager.CancelUninstall(_localInfo);
            })
            .DisposeItWith(Disposable);

        if (Author is not null)
        {
            IsVerified =
                Author.Contains("https://github.com/asv-soft")
                && SourceUri.Contains("https://nuget.pkg.github.com/asv-soft/index.json");
        }

        Version = pluginInfo.LastVersion;

        _pluginVersions = [];
        PluginVersionsView = _pluginVersions
            .CreateView(x => x)
            .ToNotifyCollectionChanged()
            .DisposeItWith(Disposable);

        GetPreviousVersions().SafeFireAndForget();
    }

    public bool IsApiCompatible { get; }
    public string? Author { get; }
    public string? SourceUri { get; }
    public string? Name { get; }
    public string? Description { get; }
    public string SourceName { get; }
    public string LastVersion { get; }
    public string Version { get; }
    public string? LocalVersion { get; }
    public string? DownloadCount { get; }
    public string? Tags { get; }
    public bool IsVerified
    {
        get;
        private init => SetField(ref field, value);
    }
    public ImmutableArray<string> Dependencies { get; }

    public BindableReactiveProperty<string> SelectedVersion { get; }
    public BindableReactiveProperty<bool> IsInstalled { get; }
    public HistoricalBoolProperty IsUninstalled { get; }
    public CancellableCommandWithProgress<Unit> Install { get; }
    public NotifyCollectionChangedSynchronizedViewList<string> PluginVersionsView { get; }

    public void Uninstall()
    {
        if (_localInfo is null)
        {
            throw new Exception("Plugin not installed");
        }

        IsUninstalled.ViewValue.OnNext(true);
    }

    public void CancelUninstall()
    {
        if (_localInfo is null)
        {
            throw new Exception("Plugin not installed");
        }

        IsUninstalled.ViewValue.OnNext(false);
    }

    private async Task InstallImpl(Unit unit, IProgress<double> progress, CancellationToken cancel)
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

    private async Task GetPreviousVersions(CancellationToken cancel = default)
    {
        var searchQuery = new SearchQuery
        {
            Name = Name,
            IncludePrerelease = true, // TODO: set it form the parent
        };

        foreach (var server in _manager.Servers)
        {
            searchQuery.Sources.Add(server.SourceUri);
        }

        var previousVersions = await _manager.ListPluginVersions(
            searchQuery,
            _pluginInfo.PackageId,
            cancel
        );

        Dispatcher.UIThread.Invoke(() =>
        {
            _pluginVersions.RemoveAll();
            _pluginVersions.AddRange(previousVersions);

            if (_pluginVersions.Count > 0)
            {
                SelectedVersion.OnNext(_pluginVersions[0]);
            }
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsUninstalled;
    }
}
