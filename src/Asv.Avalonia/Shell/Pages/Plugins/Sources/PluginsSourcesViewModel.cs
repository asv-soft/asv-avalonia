using System.Composition;
using FluentAvalonia.UI.Controls;
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
    }

    [ImportingConstructor]
    public PluginsSourcesViewModel(ICommandService cmd, IPluginManager mng, ILogService log)
        : base(PageId, cmd)
    {
        _mng = mng;
        _log = log;

        Update = new ReactiveCommand(_ =>
        {
            _items.Clear();
            _items.AddRange(mng.Servers);
        });
        Update.IgnoreOnErrorResume(ex =>
            log.Error(Title, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToUpdate, ex)
        );

        Remove = new ReactiveCommand<PluginSourceViewModel>(x =>
        {
            mng.RemoveServer(x.Model);
            Update.Execute(Unit.Default);
        });
        Update.IgnoreOnErrorResume(ex =>
            log.Error(Title, RS.PluginsSourcesViewModel_PluginsSourcesViewModel_ErrorToRemove, ex)
        );

        /*Actions = new ReadOnlyObservableCollection<IMenuItem>([
            new MenuItem($"{Id}.action.add")
            {
                Header = RS.PluginsSourcesViewModel_AddAction_Label,
                Icon = MaterialIconKind.WebPlus,
                Command = ReactiveCommand.CreateFromTask(AddImpl).DisposeItWith(Disposable)
            }
        ]);*/

        Edit = new ReactiveCommand<PluginSourceViewModel>(EditImpl);

        Items = _items.ToNotifyCollectionChanged(x => new PluginSourceViewModel(
            $"Source[{x.SourceUri}]",
            x,
            this
        ));
    }

    public string Title => "Servers";
    public NotifyCollectionChangedSynchronizedViewList<PluginSourceViewModel> Items { get; set; }
    public BindableReactiveProperty<PluginSourceViewModel> SelectedItem { get; set; }
    public ReactiveCommand Add { get; }
    public ReactiveCommand Update { get; }
    public ReactiveCommand<PluginSourceViewModel> Remove { get; }
    public ReactiveCommand<PluginSourceViewModel> Edit { get; }

    private async Task AddImpl()
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

    private async void EditImpl(PluginSourceViewModel arg)
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
