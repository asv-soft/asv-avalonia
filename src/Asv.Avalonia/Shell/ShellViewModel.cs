using System.Collections.Immutable;
using System.Diagnostics;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class ShellViewModelConfig
{
    public IList<string> Pages { get; set; } = [];
    public string? SelectedPageId { get; set; } = string.Empty;
}

public class ShellViewModel : ExtendableViewModel<IShell>, IShell
{
    private readonly ObservableList<IPage> _pages;
    private readonly IContainerHost _container;
    private readonly ICommandService _cmd;
    private ShellViewModelConfig _config;
    private bool _isLoaded;

    private int _saveLayoutInProgress;

    protected ShellViewModel(
        IContainerHost ioc,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        string id
    )
        : base(id, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(cfg);

        Cfg = cfg;
        _container = ioc;
        LayoutService = layoutService;
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
            .WhereNotNull()
            .SubscribeAwait(async (page, ct) => await page.RequestLoadLayout(layoutService, ct))
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
    public override async ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page is null)
        {
            if (_container.TryGetExport<IPage>(id.Id, out page))
            {
                page.Parent = this;
                page.InitArgs(id.Args);
                _pages.Add(page);

                SelectedPage.Value = page;
            }

            return page;
        }

        if (page.Id == SelectedPage.Value?.Id)
        {
            return page;
        }

        SelectedPage.Value = page;

        return await base.Navigate(id);
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

                if (_pages is [HomePageViewModel])
                {
                    return;
                }

                LayoutService.RemoveFromMemoryViewModelAndView(close.Page);
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
            case SaveLayoutToFileGlobalEvent saveLayoutToFileGlobalEvent:
                if (SelectedPage.Value is not null)
                {
                    await SelectedPage.Value.RequestSaveLayout(
                        saveLayoutToFileGlobalEvent.LayoutService
                    );
                }

                var restrictions = await this.RequestChildApprovalToSaveLayoutToFile();
                var ignore = restrictions.Select(r => r.Source).ToImmutableArray();

                saveLayoutToFileGlobalEvent.LayoutService.FlushFromMemory(ignore);
                break;
            case LoadLayoutEvent loadLayoutEvent:
                if (loadLayoutEvent.Source is not IShell)
                {
                    return;
                }

                await InternalLoadLayoutEventHandler(loadLayoutEvent);
                break;

            default:
                await base.InternalCatchEvent(e);
                break;
        }
    }

    private async ValueTask InternalLoadLayoutEventHandler(
        LoadLayoutEvent loadLayoutEvent,
        CancellationToken cancellationToken = default
    )
    {
        if (_isLoaded)
        {
            return;
        }

        try
        {
            _config = loadLayoutEvent.LayoutService.Get<ShellViewModelConfig>(this);
            Logger.ZLogInformation($"Try to load layout: {string.Join(",", _config.Pages)}");
            foreach (var page in _config.Pages)
            {
                await this.NavigateByPath(new NavigationPath(page));
            }

            NavigationPath? navigationPathToGo = null;
            if (_config.SelectedPageId is not null)
            {
                navigationPathToGo = Pages
                    .FirstOrDefault(page => page.Id == _config.SelectedPageId)
                    ?.GetPathToRoot();
            }

            navigationPathToGo ??= Pages.FirstOrDefault()?.GetPathToRoot();

            if (navigationPathToGo is not null)
            {
                await Navigation.GoTo(navigationPathToGo.Value);
            }

            _isLoaded = true;
        }
        catch (Exception e)
        {
            Logger.ZLogError(e, $"Error loading layout: {e.Message}");
        }
        finally
        {
            _sub1?.Dispose();
            _sub2?.Dispose();
            _sub3?.Dispose();

            _sub1 = Pages
                .ObserveAdd(CancellationToken.None)
                .Subscribe(_ => SaveLayoutToFile(loadLayoutEvent.LayoutService));
            _sub2 = Pages
                .ObserveRemove(CancellationToken.None)
                .Subscribe(_ => SaveLayoutToFile(loadLayoutEvent.LayoutService));
            _sub3 = SelectedPage.Subscribe(_ => SaveLayoutToFile(loadLayoutEvent.LayoutService));
            if (Pages.Count == 0)
            {
                await this.NavigateByPath(new NavigationPath(HomePageViewModel.PageId));
            }
        }
    }

    private void SaveLayoutToFile(ILayoutService layoutService)
    {
        if (Interlocked.CompareExchange(ref _saveLayoutInProgress, 1, 0) != 0)
        {
            Logger.LogWarning("Save layout is already in progress");
            return;
        }

        try
        {
            _config.Pages = Pages.Select(page => page.Id.ToString()).ToList();
            _config.SelectedPageId = SelectedPage.Value?.Id.ToString();

            Logger.ZLogTrace($"Save layout: {string.Join(",", _config.Pages)}");
            layoutService.SetInMemory(this, _config);
            layoutService.FlushFromMemory(this);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error saving layout: {EMessage}", e.Message);
            Debug.Assert(false, $"Error saving layout: {e.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _saveLayoutInProgress, 0);
        }
    }

    #endregion

    public virtual INavigationService Navigation { get; }

    public IConfiguration Cfg { get; }
    public ILayoutService LayoutService { get; }

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

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    #region Dispose

    private IDisposable? _sub1;
    private IDisposable? _sub2;
    private IDisposable? _sub3;

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
            _sub1?.Dispose();
            _sub2?.Dispose();
            _sub3?.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
