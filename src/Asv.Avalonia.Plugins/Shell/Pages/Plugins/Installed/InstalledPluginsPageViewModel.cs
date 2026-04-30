using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public class InstalledPluginsPageViewModel : PageViewModel<InstalledPluginsPageViewModel>
{
    public const string PageId = "plugins.installed";
    public const MaterialIconKind PageIcon = MaterialIconKind.Plugin;

    private readonly IPluginManager _manager;
    private readonly IPluginBootloader _bootloader;
    private readonly OpenFileDialogDesktopPrefab _openFileDialog;
    private readonly ObservableList<ILocalPluginInfo> _plugins;
    private readonly ISynchronizedView<ILocalPluginInfo, InstalledPluginInfoViewModel> _view;
    private readonly ILogger<InstalledPluginsPageViewModel> _logger;

    public InstalledPluginsPageViewModel()
        : this(
            DesignTime.PageContext,
            DesignTime.CommandService,
            NullPluginManager.Instance,
            NullPluginBootloader.Instance,
            DesignTime.LoggerFactory,
            NullDialogService.Instance,
            DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
        IsShowOnlyVerified.ModelValue.Value = false;
    }

    public InstalledPluginsPageViewModel(
        IPageContext context,
        ICommandService cmd,
        IPluginManager manager,
        IPluginBootloader bootloader,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext)
        : base(PageId, context, cmd, loggerFactory, dialogService, ext)
    {
        Header = RS.InstalledPluginsPageViewModel_Title;
        Icon = PageIcon;
        _logger = loggerFactory.CreateLogger<InstalledPluginsPageViewModel>();
        _manager = manager;
        _bootloader = bootloader;
        _openFileDialog = dialogService.GetDialogPrefab<OpenFileDialogDesktopPrefab>();
        _plugins = new ObservableList<ILocalPluginInfo>(bootloader.Installed);

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
            isShowOnlyVerified
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

    public override IEnumerable<IViewModel> GetChildren()
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
                _plugins.AddRange(_bootloader.Installed);
                return;
            }

            _plugins.AddRange(
                _bootloader.Installed.Where(model =>
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
            _logger.LogWarning("Invalid path to plugin");
            return;
        }

        try
        {
            await _manager.InstallManually(
                pathToPlugin,
                new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
                cancel
            );
            _logger.LogInformation("Plugin installed successfully");
            Search.Refresh();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during manual plugin installation");
        }
    }
}
