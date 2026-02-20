using Asv.Common;
using Asv.IO;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public class SettingsPluginsSourcesViewModel : SettingsSubPage
{
    public const string PageId = "plugins.sources";

    private readonly IPluginManager _pluginManager;
    private readonly ObservableList<IPluginServerInfo> _sources;
    private readonly ISynchronizedView<IPluginServerInfo, PluginsSourceViewModel> _view;
    private readonly INavigationService _navigationService;

    public SettingsPluginsSourcesViewModel()
        : this(NullPluginManager.Instance, DesignTime.Navigation, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        var items = new ObservableList<IPluginServerInfo>([
            new SourceInfo(
                new SourceRepository(
                    new PackageSource("https://api.nuget.org/v3/index.json", "test", true),
                    [new PluginResourceProvider()]
                )
            ),
            new SourceInfo(
                new SourceRepository(
                    new PackageSource("https://test.com", "test", true),
                    [new PluginResourceProvider()]
                )
            ),
        ]);
        Items = items
            .ToNotifyCollectionChanged(x => new PluginsSourceViewModel(
                x,
                NullNavigationService.Instance,
                NullLoggerFactory.Instance
            ))
            .DisposeItWith(Disposable);
    }

    public SettingsPluginsSourcesViewModel(
        IPluginManager pluginManager,
        INavigationService navigationService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        _pluginManager = pluginManager;
        _navigationService = navigationService;
        SelectedItem = new BindableReactiveProperty<PluginsSourceViewModel?>();

        _sources = new ObservableList<IPluginServerInfo>(_pluginManager.Servers);
        _view = _sources
            .CreateView(info => new PluginsSourceViewModel(info, navigationService, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        var add = new MenuItem(
            "add",
            RS.SettingsPluginsSourcesViewModel_MenuItem_Add_Title,
            loggerFactory
        )
        {
            Order = 0,
            Icon = MaterialIconKind.Add,
            Command = new BindableAsyncCommand(AddPluginsSourceCommand.Id, this),
        };

        var refresh = new MenuItem("refresh", string.Empty, loggerFactory)
        {
            Order = 1,
            Icon = MaterialIconKind.Refresh,
            Command = new ReactiveCommand(_ => Refresh()).DisposeItWith(Disposable),
        };
        Menu.Add(add);
        Menu.Add(refresh);

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginsSourceViewModel> Items { get; }
    public BindableReactiveProperty<PluginsSourceViewModel?> SelectedItem { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var child in base.GetChildren())
        {
            yield return child;
        }

        foreach (var view in _view)
        {
            yield return view;
        }
    }

    internal void Refresh()
    {
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                _sources.RemoveAll();
                _sources.AddRange(_pluginManager.Servers);
            });
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error to update info about plugin sources");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Value = null;
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        switch (e)
        {
            case RemovePluginsSourceEvent remove:
            {
                try
                {
                    _pluginManager.RemoveServer(remove.ServerInfo);
                    Refresh();
                    _navigationService.ForceFocus(this);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error to update plugin server list");
                }

                break;
            }

            case UpdatePluginsSourceEvent update:
            {
                try
                {
                    _pluginManager.RemoveServer(update.ServerInfo);
                    _pluginManager.AddServer(update.Server);
                    Refresh();
                    _navigationService.ForceFocus(this);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error to remove plugin server");
                }

                break;
            }
        }

        return ValueTask.CompletedTask;
    }
}
