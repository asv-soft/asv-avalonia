using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportPage(PageId)]
public class InstalledPluginsPageViewModel : PageViewModel<InstalledPluginsPageViewModel>
{
    public const string PageId = "plugins.installed";
    public const MaterialIconKind PageIcon = MaterialIconKind.Plugin;

    private readonly IPluginManager _manager;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly ObservableList<ILocalPluginInfo> _plugins;
    private readonly ISynchronizedView<ILocalPluginInfo, InstalledPluginInfoViewModel> _view;

    public InstalledPluginsPageViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            DesignTime.LoggerFactory,
            NullDialogService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        IsShowOnlyVerified.ModelValue.Value = false;
    }

    [ImportingConstructor]
    public InstalledPluginsPageViewModel(
        ICommandService cmd,
        IPluginManager manager,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, cmd, loggerFactory, dialogService)
    {
        Title = RS.InstalledPluginsPageViewModel_Title;
        Icon = PageIcon;
        _manager = manager;
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
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

        var isShowOnlyVerified = new ReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsShowOnlyVerified = new HistoricalBoolProperty(
            nameof(IsShowOnlyVerified),
            isShowOnlyVerified,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        InstallManually = new ReactiveCommand<IProgress<double>>(InstallManuallyImpl).DisposeItWith(
            Disposable
        );
        _view = _plugins
            .CreateView(info => new InstalledPluginInfoViewModel(info, manager, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        InstalledPluginsView = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        IsShowOnlyVerified
            .ViewValue.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => Search.Refresh())
            .DisposeItWith(Disposable);
        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public ReactiveCommand<IProgress<double>> InstallManually { get; }
    public NotifyCollectionChangedSynchronizedViewList<InstalledPluginInfoViewModel> InstalledPluginsView { get; }
    public BindableReactiveProperty<InstalledPluginInfoViewModel?> SelectedPlugin { get; }
    public HistoricalBoolProperty IsShowOnlyVerified { get; }

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
            Title = RS.InstalledPluginsPageViewModel_IntallManuallyDialog_Title,
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

    public override IExportInfo Source => PluginManagerModule.Instance;
}
