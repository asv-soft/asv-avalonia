using System.Composition;
using Asv.Common;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportSettings(PageId)]
public class PluginsSourcesViewModel : SettingsSubPage
{
    public const string PageId = "plugins.sources";

    private readonly IPluginManager _pluginManager;
    private readonly INavigationService _navigation;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<IPluginServerInfo> _sources;
    private readonly ISynchronizedView<IPluginServerInfo, PluginSourceViewModel> _view;

    public PluginsSourcesViewModel()
        : this(NullPluginManager.Instance, DesignTime.Navigation, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        var items = new ObservableList<IPluginServerInfo>(
            [
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
            ]
        );
        Items = items
            .ToNotifyCollectionChanged(x => new PluginSourceViewModel(
                x,
                NullNavigationService.Instance,
                NullLoggerFactory.Instance
            ))
            .DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public PluginsSourcesViewModel(
        IPluginManager pluginManager,
        INavigationService navigationService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, loggerFactory)
    {
        _pluginManager = pluginManager;
        _navigation = navigationService;
        _loggerFactory = loggerFactory;

        Add = new ReactiveCommand(AddImpl).DisposeItWith(Disposable);
        Add.IgnoreOnErrorResume(ex =>
            Logger.LogError(ex, "Error to add info about plugin sources")
        );
        SelectedItem = new BindableReactiveProperty<PluginSourceViewModel?>().DisposeItWith(
            Disposable
        );

        _sources = new ObservableList<IPluginServerInfo>(_pluginManager.Servers);
        _view = _sources
            .CreateView(info => new PluginSourceViewModel(info, navigationService, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginSourceViewModel> Items { get; }
    public BindableReactiveProperty<PluginSourceViewModel?> SelectedItem { get; }
    public ReactiveCommand Add { get; }

    public void Refresh() => InternalUpdate();

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case RemovePluginSourceEvent remove:
            {
                _pluginManager.RemoveServer(remove.ServerInfo);
                InternalUpdate();
                break;
            }

            case UpdatePluginSourceEvent update:
            {
                _pluginManager.RemoveServer(update.ServerInfo);
                _pluginManager.AddServer(update.Server);
                InternalUpdate();
                break;
            }
        }

        return base.InternalCatchEvent(e);
    }

    private async ValueTask AddImpl(Unit unit, CancellationToken token)
    {
        using var viewModel = new SourceDialogViewModel(_loggerFactory);
        var dialog = new ContentDialog(viewModel, _navigation)
        {
            Title = RS.PluginsSourcesViewModel_AddImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_AddImpl_Add,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _pluginManager.AddServer(
                new PluginServer(
                    viewModel.Name.Value,
                    viewModel.SourceUri.Value,
                    viewModel.Username.Value,
                    viewModel.Password.Value
                )
            );
            InternalUpdate();
        }
    }

    private void InternalUpdate()
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }

        foreach (var view in _view)
        {
            yield return view;
        }
    }

    public override IExportInfo Source => PluginManagerModule.Instance;
}
