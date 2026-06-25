using Asv.Avalonia.InfoMessage;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class ShellViewModel : ViewModel<IShell>, IShell
{
    private readonly IServiceProvider _ioc;
    public const string TypeId = "shell";

    private readonly IAppPath _appPath;
    private readonly Subject<Unit> _onCloseEvent;
    private readonly ObservableList<IPage> _pages;
    private readonly ILogger<ShellViewModel> _logger;
    private readonly IAppRestartScheduler? _appRestartScheduler;

    private readonly IThemeService _themeService;
    private readonly IDialogService _dialogService;
    private bool _isOpeningHomePage;
    private bool _isLayoutLoaded;

    protected ShellViewModel(
        IServiceProvider ioc,
        ILoggerFactory loggerFactory,
        IAppPath appPath,
        IThemeService themeService,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(TypeId, default, ext)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(appPath);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(ext);

        _ioc = ioc;
        _appPath = appPath;
        _logger = loggerFactory.CreateLogger<ShellViewModel>();
        _themeService = themeService;
        _dialogService = dialogService;
        _appRestartScheduler = ioc.GetService<IAppRestartScheduler>();

        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocusHandler, handledEventsToo: true)
            .AddTo(ref DisposableBag);

        var navPath = _appPath.GetAppPathFolder("nav");
        var store = new NavigationStore(navPath);
        Navigation = new NavigationController<IViewModel>(this, store).DisposeItWith(Disposable);

        var layoutPath = _appPath.GetPageFolder(new NavId(TypeId), "layout");
        LayoutManager = new LayoutManager<IViewModel>(
            this,
            new JsonTokenLayoutStore(layoutPath, _logger)
        ).DisposeItWith(Disposable);

        WindowSateIconKind = new BindableReactiveProperty<MaterialIconKind>().DisposeItWith(
            Disposable
        );
        WindowStateHeader = new BindableReactiveProperty<string>().DisposeItWith(Disposable);

        _onCloseEvent = new Subject<Unit>().DisposeItWith(Disposable);

        #region Pages

        _pages = [];
        _pages.DisposeRemovedItems().DisposeItWith(Disposable);
        _pages.SetRoutableParent(this).DisposeItWith(Disposable);
        R3.Disposable.Create(() => _pages.ClearWithItemsDispose()).AddTo(ref DisposableBag);

        PagesView = _pages.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);

        #endregion

        Close = new ReactiveCommand(async (_, c) => await TryCloseAsync(c)).DisposeItWith(
            Disposable
        );

        SelectedPage = new BindableReactiveProperty<IPage?>().DisposeItWith(Disposable);
        _pages
            .ObserveChanged()
            .ObserveOnUIThreadDispatcher()
            .SubscribeAwait((_, cancel) => OpenHomePageIfNoPages(cancel), AwaitOperation.Drop)
            .DisposeItWith(Disposable);

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
        LeftMenu.Sort(ISupportOrder.Comparer.Instance);
        LeftMenu
            .ObserveAdd()
            .Subscribe(_ => LeftMenu.Sort(ISupportOrder.Comparer.Instance))
            .DisposeItWith(Disposable);
        LeftMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        LeftMenu.DisposeRemovedItems().DisposeItWith(Disposable);

        RightMenu = new ObservableList<IMenuItem>();
        RightMenuView = new MenuTree(RightMenu);
        RightMenu.Sort(ISupportOrder.Comparer.Instance);
        RightMenu
            .ObserveAdd()
            .Subscribe(_ => RightMenu.Sort(ISupportOrder.Comparer.Instance))
            .DisposeItWith(Disposable);
        RightMenu.SetRoutableParent(this).DisposeItWith(Disposable);
        RightMenu.DisposeRemovedItems().DisposeItWith(Disposable);

        Messages = new ShellMessageCollection().AddTo(ref DisposableBag);
        Messages.SetParent(this);
        CloseInfoMessageCommand = new ReactiveCommand<ShellMessageViewModel>(x =>
            Messages.ItemsSource.Remove(x)
        ).DisposeItWith(Disposable);

        Events.Catch<ShellMessageEvent>(x => ShowMessage(x.Message)).DisposeItWith(Disposable);
        Events
            .Catch<RestartApplicationEvent>(OnRestartApplicationRequested)
            .DisposeItWith(Disposable);
        Events.Catch<PageCloseRequestedEvent>((_, e, _) => ClosePage(e)).DisposeItWith(Disposable);
    }

    public ShellMessageCollection Messages { get; }

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
    public IReadOnlyObservableList<IPage> Pages => _pages;
    public BindableReactiveProperty<IPage?> SelectedPage { get; }
    public NotifyCollectionChangedSynchronizedViewList<IPage> PagesView { get; }
    public ObservableList<IStatusItem> StatusItems { get; }
    public NotifyCollectionChangedSynchronizedViewList<IStatusItem> StatusItemsView { get; }
    public ReactiveCommand<ShellMessageViewModel> CloseInfoMessageCommand { get; }

    protected ObservableList<IPage> InternalPages => _pages;

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
        Messages.ItemsSource.Add(
            new ShellMessageViewModel(Guid.NewGuid().ToString(), CloseInfoMessageCommand, message)
        );
    }

    protected override void AfterLoadExtensions()
    {
        // save opened pages
        var pagesLayout = Layout.Register<string[]>(
            nameof(Pages),
            async (pages, cancel) =>
            {
                foreach (var page in pages)
                {
                    cancel.ThrowIfCancellationRequested();
                    await Navigate(new NavId(page));
                }
            }
        );
        var pagesLayoutSave = Pages
            .ObserveChanged()
            .Skip(1)
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (_, cancel) =>
                    pagesLayout.SaveAsync(
                        Pages.Select(page => page.Id.ToString()).ToArray(),
                        cancel
                    ),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(pagesLayout, pagesLayoutSave).AddTo(ref DisposableBag);

        // Save last page layout
        var selectedPageLayout = Layout.Register<string>(
            nameof(SelectedPage),
            (page, _) =>
            {
                SelectExistingPage(page);
                return ValueTask.CompletedTask;
            }
        );
        var selectedPageLayoutSave = SelectedPage
            .Skip(1)
            .WhereNotNull()
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (page, cancel) => selectedPageLayout.SaveAsync(page.Id.ToString(), cancel),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(selectedPageLayout, selectedPageLayoutSave).AddTo(ref DisposableBag);

        RootTracking.ExecuteWhenRootAttached(LoadLayoutWhenRootAttached).AddTo(ref DisposableBag);
        return;

        async ValueTask LoadLayoutWhenRootAttached(IShell root, CancellationToken cancel)
        {
            _ = root;
            _isLayoutLoaded = false;
            try
            {
                await pagesLayout.LoadAsync(cancel);
                await selectedPageLayout.LoadAsync(cancel);
            }
            catch (OperationCanceledException) when (cancel.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.ZLogWarning(e, $"Error on loading shell layout: {e.Message}");
            }
            finally
            {
                if (!cancel.IsCancellationRequested && !IsDisposed)
                {
                    _isLayoutLoaded = true;
                    Dispatcher.UIThread.Post(
                        () =>
                            OpenHomePageIfNoPages(CancellationToken.None)
                                .AsTask()
                                .SafeFireAndForget(),
                        DispatcherPriority.Background
                    );
                }
            }
        }
    }

    private void SelectExistingPage(string? pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
        {
            return;
        }

        var selected = _pages.FirstOrDefault(page => page.Id == new NavId(pageId));
        if (selected is not null)
        {
            SelectedPage.Value = selected;
        }
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
                var prefab = _dialogService.GetDialogPrefab<UnsavedChangesDialogPrefab>();
                var result = await prefab.ShowDialogAsync(
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
        if (_appRestartScheduler is null)
        {
            _logger.LogWarning("Application restart is not supported.");
            return;
        }

        _appRestartScheduler.Schedule();
        var isClosing = await TryCloseAsync(restart.Cancel);

        if (!isClosing)
        {
            _appRestartScheduler.Cancel();
        }
    }

    private ValueTask ClosePage(PageCloseRequestedEvent close)
    {
        _logger.ZLogInformation($"Close page [{close.Page.Id}]");

        var current = SelectedPage.Value;
        var removedIndex = _pages.IndexOf(close.Page);
        if (removedIndex < 0)
        {
            return ValueTask.CompletedTask;
        }

        _pages.Remove(close.Page);

        if (_pages.Count == 0)
        {
            SelectedPage.Value = null;
            return ValueTask.CompletedTask;
        }

        if (current?.Id != close.Page.Id)
        {
            return ValueTask.CompletedTask;
        }

        SelectedPage.Value = null;
        var newIndex = removedIndex < _pages.Count ? removedIndex : _pages.Count - 1;
        SelectedPage.Value = _pages[newIndex];

        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenHomePageIfNoPages(CancellationToken cancel)
    {
        if (IsDisposed || _pages.Count != 0 || _isOpeningHomePage)
        {
            return;
        }

        _isOpeningHomePage = true;
        try
        {
            cancel.ThrowIfCancellationRequested();
            await this.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)));
        }
        finally
        {
            _isOpeningHomePage = false;
        }
    }

    public virtual void ChangeWindowMode()
    {
        // implemented in desktop shell
    }

    public virtual void CollapseWindow()
    {
        // implemented in desktop shell
    }
}
