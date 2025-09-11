using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ObservableCollections;
using R3;
using IConfiguration = Asv.Cfg.IConfiguration;

namespace Asv.Avalonia.Plugins;

[ExportSettings(PageId)]
public class PluginsSourcesViewModel : SettingsSubPage
{
    public const string PageId = "plugins.sources";

    private readonly IPluginManager _mng;
    private readonly INavigationService _navigation;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ReactiveCommand _update;
    private readonly ISynchronizedView<IPluginServerInfo, PluginSourceViewModel> _view;

    public PluginsSourcesViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            DesignTime.Configuration,
            DesignTime.LoggerFactory,
            DesignTime.Navigation
        )
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
        Items = items.ToNotifyCollectionChanged(x => new PluginSourceViewModel(
            x,
            NullLoggerFactory.Instance,
            this
        ));
    }

    [ImportingConstructor]
    public PluginsSourcesViewModel(
        ICommandService cmd,
        IPluginManager mng,
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        INavigationService navigationService
    )
        : base(PageId, loggerFactory)
    {
        _mng = mng;
        _navigation = navigationService;
        _loggerFactory = loggerFactory;
        var items = new ObservableList<IPluginServerInfo>();
        _view = items.CreateView(info => new PluginSourceViewModel(info, loggerFactory, this));
        SelectedItem = new BindableReactiveProperty<PluginSourceViewModel?>();
        Items = _view.ToNotifyCollectionChanged();

        _update = new ReactiveCommand(_ =>
        {
            items.Clear();
            items.AddRange(mng.Servers);
        });
        _update.IgnoreOnErrorResume(ex =>
            Logger.LogError(
                $"{RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate}\nWith error:{ex.Message}"
            )
        );
        _update.Execute(Unit.Default);

        Add = new ReactiveCommand(AddImpl);
        Add.IgnoreOnErrorResume(ex =>
            Logger.LogError(
                $"{RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate}\nWith error:{ex.Message}"
            )
        );
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginSourceViewModel> Items { get; set; }
    public BindableReactiveProperty<PluginSourceViewModel?> SelectedItem { get; set; }
    public ReactiveCommand Add { get; }

    private async ValueTask AddImpl(Unit unit, CancellationToken token)
    {
        using var viewModel = new SourceViewModel(_mng, _loggerFactory, null);
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
            await viewModel.Update();
            _update.Execute(Unit.Default);
        }
    }

    public async ValueTask EditImpl(PluginSourceViewModel arg, CancellationToken token)
    {
        using var viewModel = new SourceViewModel(_mng, _loggerFactory, arg);
        var dialog = new ContentDialog(viewModel, _navigation)
        {
            Title = RS.PluginsSourcesViewModel_EditImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_EditImpl_Save,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        viewModel.ApplyDialog(dialog);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await viewModel.Update();
            _update.Execute(Unit.Default);
        }
    }

    public void RemoveImpl(PluginSourceViewModel arg)
    {
        _mng.RemoveServer(arg.Model);
        _update.Execute(Unit.Default);
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var item = _view.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem.Value = item;
            return ValueTask.FromResult<IRoutable>(item);
        }

        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _update.Dispose();
            _view.Dispose();
            Items.Dispose();
            SelectedItem.Dispose();
            Add.Dispose();
        }

        base.Dispose(disposing);
    }
}
