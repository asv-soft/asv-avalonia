using System.Composition;
using Avalonia.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsHotKeysListViewModel : SettingsSubPage
{
    public const string PageId = "settings.hot_keys";

    private readonly ReactiveProperty<string?> _searchText;

    #region Design

    public SettingsHotKeysListViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        var observableList = new ObservableList<ICommandInfo>(
            [
                new CommandInfo
                {
                    Id = "command1",
                    Name = "Command1",
                    Description = "Description for Command1",
                    Icon = MaterialIconKind.Abacus,
                    DefaultHotKey = new HotKeyInfo(KeyGesture.Parse("Ctrl+Shift+A")),
                    Source = SystemModule.Instance,
                },
                new CommandInfo
                {
                    Id = "command2",
                    Name = "Command2",
                    Description = "Description for Command2",
                    Icon = MaterialIconKind.ABCOff,
                    DefaultHotKey = new HotKeyInfo(KeyGesture.Parse("Ctrl+F")),
                    Source = SystemModule.Instance,
                },
                new CommandInfo
                {
                    Id = "command3",
                    Name = "Command3",
                    Description = "Description for Command3",
                    Icon = MaterialIconKind.AbTesting,
                    DefaultHotKey = null,
                    Source = SystemModule.Instance,
                },
            ]
        );
        HotKeysView = observableList.CreateView(x => new HotKeyViewModel(
            this,
            x,
            DesignTime.CommandService,
            NullDialogService.Instance,
            DesignTime.LoggerFactory
        ));
    }

    #endregion

    [ImportingConstructor]
    public SettingsHotKeysListViewModel(
        ICommandService svc,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, loggerFactory)
    {
        _searchText = new ReactiveProperty<string?>();
        SearchText = new HistoricalStringProperty("search_text", _searchText, loggerFactory)
        {
            Parent = this,
        };

        SelectedItem = new BindableReactiveProperty<HotKeyViewModel?>();

        SelectedItem
            .Where(it => it != null)
            .SubscribeAwait(
                (vm, _) =>
                {
                    if (vm!.EditCommand.CanExecute(null))
                    {
                        vm.EditCommand.Execute(null);
                    }

                    return ValueTask.CompletedTask;
                }
            );

        var observableList = new ObservableList<ICommandInfo>(svc.Commands);
        observableList.Sort(CommandInfoComparer.Instance);
        HotKeysView = observableList.CreateView(x => new HotKeyViewModel(
            this,
            x,
            svc,
            dialogService,
            loggerFactory
        ));
    }

    public HistoricalStringProperty SearchText { get; set; }
    public BindableReactiveProperty<HotKeyViewModel?> SelectedItem { get; set; }
    public ISynchronizedView<ICommandInfo, HotKeyViewModel> HotKeysView { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return SearchText;
    }

    public override IExportInfo Source => SystemModule.Instance;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _searchText.Dispose();
            SearchText.Dispose();
            SelectedItem.Dispose();
            HotKeysView.Dispose();
        }

        base.Dispose(disposing);
    }
}
