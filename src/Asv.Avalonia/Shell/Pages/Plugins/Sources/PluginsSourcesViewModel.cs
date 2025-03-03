using System.Composition;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class PluginsSourcesViewModel : PageViewModel<PluginsSourcesViewModel>
{
    public const string PageId = "plugins.sources";

    private readonly IPluginManager _mng;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly ReactiveCommand _update;
    private readonly ObservableList<IPluginServerInfo> _items;
    private readonly ISynchronizedView<IPluginServerInfo, PluginSourceViewModel> _view;

    public PluginsSourcesViewModel()
        : this(DesignTime.CommandService, DesignTime.PluginManager, NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        _items = new ObservableList<IPluginServerInfo>(
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
        Items = _items.ToNotifyCollectionChanged(x => new PluginSourceViewModel(
            x,
            NullLoggerFactory.Instance,
            this
        ));
    }

    [ImportingConstructor]
    public PluginsSourcesViewModel(
        ICommandService cmd,
        IPluginManager mng,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd)
    {
        _mng = mng;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PluginsSourcesViewModel>();
        _items = new ObservableList<IPluginServerInfo>();
        _view = _items.CreateView(info => new PluginSourceViewModel(info, loggerFactory, this));
        SelectedItem = new BindableReactiveProperty<PluginSourceViewModel?>();
        Items = _view.ToNotifyCollectionChanged();

        _update = new ReactiveCommand(_ =>
        {
            _items.Clear();
            _items.AddRange(mng.Servers);
        });
        _update.IgnoreOnErrorResume(ex =>
            _logger.LogError(
                $"RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate\nWith error:{ex.Message}"
            )
        );
        _update.Execute(Unit.Default);

        Add = new ReactiveCommand(AddImpl);
        Add.IgnoreOnErrorResume(ex =>
            _logger.LogError(
                $"{RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate}\nWith error:{ex.Message}"
            )
        );
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginSourceViewModel> Items { get; set; }
    public BindableReactiveProperty<PluginSourceViewModel?> SelectedItem { get; set; }
    public ReactiveCommand Add { get; }

    private async ValueTask AddImpl(Unit unit, CancellationToken token)
    {
        var dialog = new ContentDialog
        {
            Title = RS.PluginsSourcesViewModel_AddImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_AddImpl_Add,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        using var viewModel = new SourceViewModel(_mng, _loggerFactory, null);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _update.Execute(Unit.Default);
        }
    }

    public async ValueTask EditImpl(PluginSourceViewModel arg, CancellationToken token)
    {
        if (SelectedItem.CurrentValue?.Id != arg.Id)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = RS.PluginsSourcesViewModel_EditImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_EditImpl_Save,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        using var viewModel = new SourceViewModel(_mng, _loggerFactory, arg);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _update.Execute(Unit.Default);
        }
    }

    public void RemoveImpl(PluginSourceViewModel arg)
    {
        if (SelectedItem.CurrentValue?.Id != arg.Id)
        {
            return;
        }

        _mng.RemoveServer(arg.Model);
        _update.Execute(Unit.Default);
    }

    public override ValueTask<IRoutable> Navigate(string id)
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

    protected override void AfterLoadExtensions() { }

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
