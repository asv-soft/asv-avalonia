using System.Composition;
using FluentAvalonia.UI.Controls;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class PluginsSourcesViewModel : PageViewModel<PluginsSourcesViewModel>
{
    public const string PageId = "plugins_sources";
    private readonly IPluginManager _mng;
    private readonly ILogService _log;
    private readonly ObservableList<IPluginServerInfo> _items = [];

    public PluginsSourcesViewModel()
        : this(DesignTime.CommandService, DesignTime.PluginManager, DesignTime.Log)
    {
        DesignTime.ThrowIfNotDesignMode();
        _items = new ObservableList<IPluginServerInfo>(
            new[]
            {
                new SourceInfo(
                    new SourceRepository(
                        new PackageSource("https://test.com", "test", true),
                        new[] { new PluginResourceProvider() }
                    )
                ),
            }
        );
        Items = _items.ToNotifyCollectionChanged(x => new PluginSourceViewModel(
            $"Source[{x.SourceUri}]",
            x,
            DesignTime.Log,
            this
        ));
    }

    [ImportingConstructor]
    public PluginsSourcesViewModel(ICommandService cmd, IPluginManager mng, ILogService log)
        : base(PageId, cmd)
    {
        _mng = mng;
        _log = log;

        Items = _items.ToNotifyCollectionChanged(x => new PluginSourceViewModel(
            $"Source[{x.SourceUri}]",
            x,
            log,
            this
        ));

        Update = new ReactiveCommand(_ =>
        {
            _items.Clear();
            _items.AddRange(mng.Servers);
        });
        Update.IgnoreOnErrorResume(ex =>
            log.Error(
                Title.Value,
                RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate,
                ex
            )
        );
        Update.Execute(Unit.Default);

        Add = new ReactiveCommand(AddImpl);
        Add.IgnoreOnErrorResume(ex =>
            log.Error(
                Title.Value,
                RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate,
                ex
            )
        );
    }

    public NotifyCollectionChangedSynchronizedViewList<PluginSourceViewModel> Items { get; set; }
    public BindableReactiveProperty<PluginSourceViewModel> SelectedItem { get; set; }
    public ReactiveCommand Add { get; }
    public ReactiveCommand Update { get; }
    public ReactiveCommand<PluginSourceViewModel> Remove { get; }
    public ReactiveCommand<PluginSourceViewModel> Edit { get; }

    private async void AddImpl(Unit unit)
    {
        var dialog = new ContentDialog
        {
            Title = RS.PluginsSourcesViewModel_AddImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_AddImpl_Add,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };

        using var viewModel = new SourceViewModel("NewSource", _mng, _log, null);
        viewModel.ApplyDialog(dialog);

        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            Update.Execute(Unit.Default);
        }
    }

    public async void EditImpl(PluginSourceViewModel arg)
    {
        var dialog = new ContentDialog
        {
            Title = RS.PluginsSourcesViewModel_EditImpl_Title,
            PrimaryButtonText = RS.PluginsSourcesViewModel_EditImpl_Save,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.PluginsSourcesViewModel_AddImpl_Cancel,
        };
        using var viewModel = new SourceViewModel($"Source[{arg.Id}]", _mng, _log, arg);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            Update.Execute(Unit.Default);
        }
    }

    public void RemoveImpl(PluginSourceViewModel arg)
    {
        _mng.RemoveServer(arg.Model);
        Update.Execute(Unit.Default);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }
}
