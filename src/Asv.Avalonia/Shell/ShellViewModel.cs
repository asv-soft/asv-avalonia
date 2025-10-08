using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class ShellViewModel : ExtendableViewModel<IShell>, IShell
{
    private readonly ObservableList<IPage> _pages;
    private readonly IContainerHost _container;
    private readonly ICommandService _cmd;

    protected ShellViewModel(
        IContainerHost ioc,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        string id
    )
        : base(id, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _container = ioc;
        _cmd = ioc.GetExport<ICommandService>();
        Navigation = ioc.GetExport<INavigationService>();
        _pages = new ObservableList<IPage>();
        PagesView = _pages.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Close = new ReactiveCommand((_, c) => CloseAsync(c));
        ChangeWindowState = new ReactiveCommand((_, c) => ChangeWindowModeAsync(c));
        Collapse = new ReactiveCommand((_, c) => CollapseAsync(c));
        SelectedPage = new BindableReactiveProperty<IPage?>();
        MainMenu = new ObservableList<IMenuItem>();
        MainMenuView = new MenuTree(MainMenu).DisposeItWith(Disposable);
        MainMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        MainMenu.DisposeRemovedItems().DisposeItWith(Disposable);
        SelectedPage
            .Subscribe(page =>
            {
                Logger.LogInformation($"Navigated to {page?.Id}");
                Navigation.ForceFocus(page);
            })
            .DisposeItWith(Disposable);

        StatusItems = [];
        StatusItemsView = StatusItems.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        StatusItems.Sort(StatusItemComparer.Instance);
        StatusItems
            .ObserveAdd()
            .Subscribe(_ => StatusItems.Sort(StatusItemComparer.Instance))
            .DisposeItWith(Disposable);
        StatusItems.SetRoutableParent(this).DisposeItWith(Disposable);
        StatusItems.DisposeRemovedItems().DisposeItWith(Disposable);

        LeftMenu = new ObservableList<IMenuItem>();
        LeftMenuView = new MenuTree(LeftMenu);
        LeftMenu.Sort(HeadlinedComparer.Instance);
        LeftMenu
            .ObserveAdd()
            .Subscribe(_ => LeftMenu.Sort(HeadlinedComparer.Instance))
            .DisposeItWith(Disposable);
        LeftMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        LeftMenu.DisposeRemovedItems().DisposeItWith(Disposable);

        RightMenu = new ObservableList<IMenuItem>();
        RightMenuView = new MenuTree(RightMenu);
        RightMenu.Sort(HeadlinedComparer.Instance);
        RightMenu
            .ObserveAdd()
            .Subscribe(_ => RightMenu.Sort(HeadlinedComparer.Instance))
            .DisposeItWith(Disposable);
        RightMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        RightMenu.DisposeRemovedItems().DisposeItWith(Disposable);
    }

    #region  Tools

    public ObservableList<IMenuItem> LeftMenu { get; }
    public MenuTree LeftMenuView { get; }

    public ObservableList<IMenuItem> RightMenu { get; }
    public MenuTree RightMenuView { get; }

    #endregion

    #region Theme command

    public void ChangeTheme()
    {
        this.ExecuteCommand(ChangeThemeFreeCommand.Id).SafeFireAndForget();
    }

    public void OpenSettings()
    {
        this.ExecuteCommand(OpenSettingsCommand.Id).SafeFireAndForget();
    }

    #endregion

    #region MainMenu

    public ObservableList<IMenuItem> MainMenu { get; }
    public MenuTree MainMenuView { get; }

    #endregion

    #region Close
    public ReactiveCommand Close { get; }

    protected virtual ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region ChangeWindowState

    // TODO: Move to DesktopShellViewModel later
    public BindableReactiveProperty<MaterialIconKind> WindowSateIconKind { get; } = new();

    // TODO: Move to DesktopShellViewModel later
    public BindableReactiveProperty<string> WindowStateHeader { get; } = new();

    public ReactiveCommand ChangeWindowState { get; }

    protected virtual ValueTask ChangeWindowModeAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Collapse
    public ReactiveCommand Collapse { get; }

    protected virtual ValueTask CollapseAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Pages

    protected ObservableList<IPage> InternalPages => _pages;
    public IReadOnlyObservableList<IPage> Pages => _pages;
    public BindableReactiveProperty<IPage?> SelectedPage { get; }
    public NotifyCollectionChangedSynchronizedViewList<IPage> PagesView { get; }

    #endregion

    #region Routable
    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page == null)
        {
            if (_container.TryGetExport<IPage>(id.Id, out page))
            {
                page.Parent = this;
                page.InitArgs(id.Args);
                _pages.Add(page);

                SelectedPage.Value = page;
            }

            return ValueTask.FromResult<IRoutable>(page);
        }

        if (page.Id == SelectedPage.Value?.Id)
        {
            return ValueTask.FromResult<IRoutable>(page);
        }

        SelectedPage.Value = page;

        return base.Navigate(id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => _pages;

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case ExecuteCommandEvent cmd:
                await _cmd.Execute(cmd.CommandId, cmd.Source, cmd.CommandArg, cmd.Cancel);
                break;
            case RestartApplicationEvent:
                Environment.Exit(0);
                break;
            case PageCloseRequestedEvent close:
            {
                Logger.ZLogInformation($"Close page [{close.Page.Id}]");

                // TODO: save page layout
                if (_pages is [HomePageViewModel])
                {
                    return;
                }

                var current = SelectedPage.Value; // TODO: fix page selection
                var removedIndex = _pages.IndexOf(close.Page);
                if (removedIndex < 0)
                {
                    break;
                }

                _pages.Remove(close.Page);
                close.Page.Parent = null;
                close.Page.Dispose();

                if (_pages.Count == 0)
                {
                    await Navigation.GoHomeAsync();
                    break;
                }

                if (current?.Id == close.Page.Id)
                {
                    SelectedPage.Value = null;

                    var newIndex = removedIndex < _pages.Count ? removedIndex : 0;
                    SelectedPage.Value = _pages[newIndex];
                }
                else
                {
                    SelectedPage.Value = null;
                    SelectedPage.Value = current;
                }

                break;
            }

            default:
                await base.InternalCatchEvent(e);
                break;
        }
    }

    #endregion


    public virtual INavigationService Navigation { get; }

    public ShellErrorState ErrorState
    {
        get;
        set => SetField(ref field, value);
    }

    public string Title
    {
        get;
        set => SetField(ref field, value);
    }

    #region Status bar

    public ObservableList<IStatusItem> StatusItems { get; }
    public NotifyCollectionChangedSynchronizedViewList<IStatusItem> StatusItemsView { get; }

    #endregion

    #region Dispose

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close.Dispose();
            SelectedPage.Dispose();
            WindowSateIconKind.Dispose();
            WindowStateHeader.Dispose();
            PagesView.Dispose();
            _pages.ClearWithItemsDispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
