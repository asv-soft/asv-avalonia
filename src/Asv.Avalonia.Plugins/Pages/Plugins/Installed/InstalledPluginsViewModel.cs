using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public sealed class InstalledPluginsViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class InstalledPluginsViewModel
    : PageViewModel<InstalledPluginsViewModel, InstalledPluginsViewModelConfig>
{
    public const string PageId = "plugins.installed";
    public const MaterialIconKind PageIcon = MaterialIconKind.Plugin;

    private readonly IPluginManager _manager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly IConfiguration _cfg;
    private readonly INavigationService _navigation;
    private readonly ObservableList<ILocalPluginInfo> _plugins;
    private readonly ISynchronizedView<ILocalPluginInfo, InstalledPluginInfoViewModel> _view;

    public InstalledPluginsViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            DesignTime.Configuration,
            DesignTime.Navigation,
            NullDialogService.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public InstalledPluginsViewModel(
        ICommandService cmd,
        IPluginManager manager,
        IConfiguration cfg,
        INavigationService navigationService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, cfg, loggerFactory)
    {
        Title = RS.InstalledPluginsViewModel_Title;
        _manager = manager;
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _loggerFactory = loggerFactory;
        _navigation = navigationService;
        _cfg = cfg;
        _plugins = new ObservableList<ILocalPluginInfo>(_manager.Installed);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            SearchImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        SelectedPlugin =
            new BindableReactiveProperty<InstalledPluginInfoViewModel?>().DisposeItWith(Disposable);

        var isShowOnlyVerified = new ReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsShowOnlyVerified = new HistoricalBoolProperty(
            nameof(IsShowOnlyVerified),
            isShowOnlyVerified,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        InstallManually = new ReactiveCommand<IProgress<double>>(InstallManuallyImpl).DisposeItWith(
            Disposable
        );
        _view = _plugins
            .CreateView(info => new InstalledPluginInfoViewModel(
                new NavigationId(PageId, info.Id),
                manager,
                info,
                loggerFactory
            ))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        InstalledPluginsView = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        IsShowOnlyVerified.ViewValue.ObserveOnUIThreadDispatcher().Subscribe(_ => Search.Refresh());
        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public ReactiveCommand<IProgress<double>> InstallManually { get; }
    public NotifyCollectionChangedSynchronizedViewList<InstalledPluginInfoViewModel> InstalledPluginsView { get; }
    public BindableReactiveProperty<InstalledPluginInfoViewModel?> SelectedPlugin { get; }
    public HistoricalBoolProperty IsShowOnlyVerified { get; }

    private Task SearchImpl(string? text, IProgress<double> progress, CancellationToken cancel)
    {
        progress.Report(0);
        Dispatcher.UIThread.Invoke(() =>
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            _plugins.RemoveAll();
            if (string.IsNullOrWhiteSpace(text) && !IsShowOnlyVerified.ViewValue.Value)
            {
                _plugins.AddRange(_manager.Installed);
                return;
            }

            _plugins.AddRange(
                _manager.Installed.Where(model =>
                {
                    var isOk = true;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        isOk =
                            model.Title?.Contains(text, StringComparison.InvariantCultureIgnoreCase)
                            ?? false;
                    }

                    if (IsShowOnlyVerified.ViewValue.Value)
                    {
                        return isOk && model.IsVerified;
                    }

                    return isOk;
                })
            );
        });
        progress.Report(1);
        return Task.CompletedTask;
    }

    private async ValueTask InstallManuallyImpl(
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        var payload = new OpenFileDialogPayload
        {
            Title = "Select plugin to install",
            TypeFilter = "nupkg",
        };

        var pathToPlugin = await _openFileDialog.ShowDialogAsync(payload);

        if (string.IsNullOrWhiteSpace(pathToPlugin) || !NugetHelper.IsPathToNugetFile(pathToPlugin))
        {
            Logger.LogWarning("Invalid path to plugin");
            return;
        }

        try
        {
            await _manager.InstallManually(
                pathToPlugin,
                new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
                cancel
            );
            Logger.LogInformation("Plugin installed successfully");
            Search.Refresh();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error during manual plugin installation");
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var viewModel in _view)
        {
            yield return viewModel;
        }

        yield return Search;
        yield return IsShowOnlyVerified;
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => PluginManagerModule.Instance;
}
