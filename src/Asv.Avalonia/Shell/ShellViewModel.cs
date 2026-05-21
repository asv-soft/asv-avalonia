using Asv.Avalonia.InfoMessage;
using Asv.Cfg;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
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

public class ShellViewModel : ViewModel<IShell>, IShell
{
    private readonly IServiceProvider _ioc;
    public const string TypeId = "shell";

    private const int MaxInfoBarMessages = 3;

    private readonly IAppPath _appPath;
    private readonly Subject<Unit> _layoutChanged;
    private readonly Subject<Unit> _onCloseEvent;
    private readonly ObservableList<ShellMessageViewModel> _infoMessagesSource;
    private readonly ObservableList<IPage> _pages;
    private readonly ILogger<ShellViewModel> _logger;
    private readonly UnsavedChangesDialogPrefab _unsavedChangesDialogPrefab;

    private ShellViewModelConfig _config;
    private bool _isLoaded;
    private bool _layoutTrackingStarted;
    private int _saveLayoutInProgress;
    private readonly IThemeService _themeService;

    protected ShellViewModel(
        IServiceProvider ioc,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        IExtensionService ext
    )
        : base(TypeId, default, ext)
    {
        _ioc = ioc;
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(cfg);

        _logger = loggerFactory.CreateLogger<ShellViewModel>();
        _config = new ShellViewModelConfig();
        _layoutChanged = new Subject<Unit>().DisposeItWith(Disposable);

        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocusHandler, handledEventsToo: true)
            .AddTo(ref DisposableBag);

        Navigation = new NavigationController<IViewModel>(
            this,
            new NavigationStore("nav")
        ).DisposeItWith(Disposable);
        var dialogService1 = ioc.GetRequiredService<IDialogService>();
        _appPath = ioc.GetRequiredService<IAppPath>();
        _themeService = ioc.GetRequiredService<IThemeService>();
        
        _unsavedChangesDialogPrefab = dialogService1.GetDialogPrefab<UnsavedChangesDialogPrefab>();
        var path = _appPath.GetPageFolder(new NavId(TypeId), "layout");
        LayoutManager = new LayoutManager<IViewModel>(this, new JsonTokenLayoutStore(path, _logger))
            .DisposeItWith(Disposable);

        WindowSateIconKind = new BindableReactiveProperty<MaterialIconKind>()
            .DisposeItWith(Disposable);
        WindowStateHeader = new BindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        _onCloseEvent = new Subject<Unit>().DisposeItWith(Disposable);

        _pages = new ObservableList<IPage>();
        _pages.DisposeRemovedItems().DisposeItWith(Disposable);
        _pages.SetRoutableParent(this).DisposeItWith(Disposable);
        PagesView = _pages.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        Close = new ReactiveCommand(async (_, c) => await TryCloseAsync(c)).DisposeItWith(
            Disposable
        );
        ChangeWindowState = new ReactiveCommand((_, c) => ChangeWindowModeAsync(c));
        Collapse = new ReactiveCommand((_, c) => CollapseAsync(c));
        SelectedPage = new BindableReactiveProperty<IPage?>().DisposeItWith(Disposable);

        MainMenu = new ObservableList<IMenuItem>();
        MainMenuView = new MenuTree(MainMenu).DisposeItWith(Disposable);
        MainMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        MainMenu.DisposeRemovedItems().DisposeItWith(Disposable);

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

        _infoMessagesSource = new ObservableList<ShellMessageViewModel>();
        InfoBarMessages = _infoMessagesSource
            .ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
        _infoMessagesSource.SetRoutableParent(this).DisposeItWith(Disposable);
        _infoMessagesSource.DisposeRemovedItems().DisposeItWith(Disposable);
        _infoMessagesSource
            .ObserveCountChanged()
            .Subscribe(_ =>
            {
                var last = _infoMessagesSource.LastOrDefault();
                if (last is null)
                {
                    ErrorState = ShellErrorState.Normal;
                    return;
                }
                if (last.Severity == ShellErrorState.Error)
                {
                    ErrorState = ShellErrorState.Error;
                    return;
                }
                if (last.Severity == ShellErrorState.Warning)
                {
                    ErrorState = ShellErrorState.Warning;
                    return;
                }
                ErrorState = ShellErrorState.Normal;
            })
            .DisposeItWith(Disposable);

        CloseInfoMessageCommand = new ReactiveCommand<ShellMessageViewModel>(x =>
            _infoMessagesSource.Remove(x)
        ).DisposeItWith(Disposable);

        Events.Catch<ShellMessageEvent>(x => ShowMessage(x.Message)).DisposeItWith(Disposable);
        Events.Catch<RestartApplicationEvent>(OnRestartApplicationRequested)
            .DisposeItWith(Disposable);
        Events.Catch<PageCloseRequestedEvent>(OnPageCloseRequested).DisposeItWith(Disposable);
    }

    protected ILogger Logger => _logger;

    public ILayoutManager<IViewModel> LayoutManager { get; }
    public INavigationController<IViewModel> Navigation { get; }

    public ObservableList<IMenuItem> LeftMenu { get; }
    public MenuTree LeftMenuView { get; }
    public ObservableList<IMenuItem> RightMenu { get; }
    public MenuTree RightMenuView { get; }
    public ObservableList<IMenuItem> MainMenu { get; }
    public MenuTree MainMenuView { get; }
    public ReactiveCommand Close { get; }
    public Observable<Unit> OnClose => _onCloseEvent;
    public BindableReactiveProperty<MaterialIconKind> WindowSateIconKind { get; }
    public BindableReactiveProperty<string> WindowStateHeader { get; }
    public ReactiveCommand ChangeWindowState { get; }
    public ReactiveCommand Collapse { get; }
    public IReadOnlyObservableList<IPage> Pages => _pages;
    public BindableReactiveProperty<IPage?> SelectedPage { get; }
    public NotifyCollectionChangedSynchronizedViewList<IPage> PagesView { get; }
    public ObservableList<IStatusItem> StatusItems { get; }
    public NotifyCollectionChangedSynchronizedViewList<IStatusItem> StatusItemsView { get; }
    public ReactiveCommand<ShellMessageViewModel> CloseInfoMessageCommand { get; }
    public NotifyCollectionChangedSynchronizedViewList<ShellMessageViewModel> InfoBarMessages { get; }

    protected ObservableList<IPage> InternalPages => _pages;

    public ShellErrorState ErrorState
    {
        get;
        set => SetField(ref field, value);
    }

    public string Header
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public void ChangeTheme()
    {
        var nextTheme =
            _themeService.CurrentTheme.Value.Id == ThemeService.DarkTheme
                ? ThemeService.LightTheme
                : ThemeService.DarkTheme;
        var theme = _themeService.Themes.FirstOrDefault(x => x.Id == nextTheme);
        if (theme is not null)
        {
            _themeService.CurrentTheme.Value = theme;
        }
    }

    public void OpenSettings()
    {
        this.GoTo(new NavPath(new NavId(SettingsPageViewModel.PageId))).SafeFireAndForget();
    }

    public override async ValueTask<IViewModel> Navigate(NavId id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page is null)
        {
            var undoFolder = _appPath.GetPageFolder(id, "undo");
            var layoutFolder = _appPath.GetPageFolder(id, "layout");
            var pageContext = new PageContext(
                id.Args,
                new JsonUndoHistoryStore(undoFolder, _logger),
                new JsonTokenLayoutStore(layoutFolder, _logger)
            );
            page = _ioc.CreatePage(id.TypeId, pageContext);
            _pages.Add(page);
            SelectedPage.Value = page;
            return page;
        }

        if (page.Id == SelectedPage.Value?.Id)
        {
            return page;
        }

        SelectedPage.Value = page;
        return await base.Navigate(id);
    }

    public override IEnumerable<IViewModel> GetChildren() => _pages;

    public void ShowMessage(ShellMessage message)
    {
        if (_infoMessagesSource.Count >= MaxInfoBarMessages)
        {
            _infoMessagesSource.RemoveAt(0);
        }

        _infoMessagesSource.Add(
            new ShellMessageViewModel(Guid.NewGuid().ToString(), CloseInfoMessageCommand, message)
        );
    }

    protected override void AfterLoadExtensions()
    {
        Layout
            .Register(nameof(ShellViewModel), LoadLayout, SaveLayout, _layoutChanged)
            .DisposeItWith(Disposable);
        Layout.LoadAll();
    }

    protected virtual async ValueTask<bool> TryCloseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var page in _pages)
        {
            try
            {
                var reasons = await page.RequestChildCloseApproval(cancellationToken);
                if (reasons.Count == 0)
                {
                    continue;
                }

                await Navigation.GoTo(page.GetPathFromRoot());
                var result = await _unsavedChangesDialogPrefab.ShowDialogAsync(
                    new UnsavedChangesDialogPayload
                    {
                        Restrictions = reasons,
                        Title = RS.DesktopShellViewModel_ExitConfirmDialog_Title,
                    }
                );
                if (!result)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.ZLogTrace(
                    e,
                    $"Error on requesting approval for the page {page.Header}[{page.Id}]: {e.Message}"
                );
            }
        }

        _onCloseEvent.OnNext(Unit.Default);
        return true;
    }

    protected virtual ValueTask ChangeWindowModeAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask CollapseAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual void RestartApplication(string[] args)
    {
        _logger.LogError(
            "Restart is not supported by shell type {ShellType}. Arguments: {Args}",
            GetType().Name,
            string.Join(" ", args)
        );
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pages.ClearWithItemsDispose();
        }

        base.Dispose(disposing);
    }

    private void GotFocusHandler(TopLevel top, RoutedEventArgs args)
    {
        if (args.Source is not Control source)
        {
            return;
        }

        var control = source;
        while (control != null)
        {
            if (control.DataContext is IViewModel routable)
            {
                if (routable is IShell)
                {
                    return;
                }
                Navigation.ForceSelect(routable);
                break;
            }

            control = control.GetLogicalParent() as Control;
        }
    }

    private async ValueTask OnRestartApplicationRequested(
        IViewModel src,
        RestartApplicationEvent restart,
        CancellationToken cancel
    )
    {
        using var sub = _onCloseEvent.Take(1).Subscribe(_ => RestartApplicationCommon());
        await TryCloseAsync(restart.Cancel);
    }

    private async ValueTask OnPageCloseRequested(
        IViewModel src,
        PageCloseRequestedEvent close,
        CancellationToken cancel
    )
    {
        await ClosePage(close);
    }

    private async ValueTask ClosePage(PageCloseRequestedEvent close)
    {
        _logger.ZLogInformation($"Close page [{close.Page.Id}]");

        if (_pages is [HomePageViewModel])
        {
            return;
        }

        var current = SelectedPage.Value;
        var removedIndex = _pages.IndexOf(close.Page);
        if (removedIndex < 0)
        {
            return;
        }

        _pages.Remove(close.Page);

        if (_pages.Count == 0)
        {
            await Navigation.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)));
            return;
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
    }

    private void RestartApplicationCommon()
    {
        try
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            RestartApplication(args);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to restart the application.");
        }
    }

    private void LoadLayout(ShellViewModelConfig config)
    {
        LoadLayoutAsync(config).SafeFireAndForget();
    }

    private async ValueTask LoadLayoutAsync(
        ShellViewModelConfig config,
        CancellationToken cancellationToken = default
    )
    {
        if (_isLoaded)
        {
            return;
        }

        try
        {
            _config = config;
            _logger.ZLogInformation($"Try to load layout: {string.Join(",", _config.Pages)}");
            foreach (var page in _config.Pages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.NavigateByPath(NavPath.Parse(page));
            }

            NavPath? navPathToGo = null;
            if (_config.SelectedPageId is not null)
            {
                navPathToGo = Pages
                    .FirstOrDefault(page => page.Id.ToString() == _config.SelectedPageId)
                    ?.GetPathFromRoot();
            }

            navPathToGo ??= Pages.FirstOrDefault()?.GetPathFromRoot();
            if (navPathToGo is not null)
            {
                await Navigation.GoTo(navPathToGo.Value);
            }

            if (Pages.Count == 0)
            {
                await this.NavigateByPath(new NavPath(new NavId(HomePageViewModel.PageId)));
            }

            _isLoaded = true;
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error loading layout: {e.Message}");
        }
        finally
        {
            StartLayoutTracking();
        }
    }

    private void StartLayoutTracking()
    {
        if (_layoutTrackingStarted)
        {
            return;
        }

        _layoutTrackingStarted = true;
        Pages.ObserveAdd().Subscribe(_ => NotifyLayoutChanged()).DisposeItWith(Disposable);
        Pages.ObserveRemove().Subscribe(_ => NotifyLayoutChanged()).DisposeItWith(Disposable);
        SelectedPage.Subscribe(_ => NotifyLayoutChanged()).DisposeItWith(Disposable);
    }

    private void NotifyLayoutChanged()
    {
        if (_isLoaded)
        {
            _layoutChanged.OnNext(Unit.Default);
        }
    }

    private ShellViewModelConfig? SaveLayout()
    {
        if (Interlocked.CompareExchange(ref _saveLayoutInProgress, 1, 0) != 0)
        {
            _logger.LogWarning("Save layout is already in progress");
            return null;
        }

        try
        {
            _config.Pages = Pages.Select(page => page.Id.ToString()).ToList();
            _config.SelectedPageId = SelectedPage.Value?.Id.ToString();

            _logger.ZLogTrace($"Save layout: {string.Join(",", _config.Pages)}");
            return _config;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving layout: {EMessage}", e.Message);
            return null;
        }
        finally
        {
            Interlocked.Exchange(ref _saveLayoutInProgress, 0);
        }
    }
}
