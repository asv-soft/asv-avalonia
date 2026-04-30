using System.Collections.Immutable;
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
    private const int MaxInfoBarMessages = 3;

    private readonly Subject<Unit> _onCloseEvent;
    private readonly ObservableList<IPage> _pages;
    private readonly ObservableList<ShellMessageViewModel> _infoMessagesSource;
    private readonly UnsavedChangesDialogPrefab _unsavedChangesDialogPrefab;

    private ShellViewModelConfig _config;
    private bool _isLoaded;
    private int _saveLayoutInProgress;
    private readonly ILogger<ShellViewModel> _logger;

    protected readonly IServiceProvider Container;
    protected readonly IDialogService DialogService;
    protected readonly IUndoStoreService UndoStoreService;

    protected ShellViewModel(
        string typeId,
        IServiceProvider ioc,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        IExtensionService ext
    )
        : base(typeId, default, ext)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(cfg);
        _logger = loggerFactory.CreateLogger<ShellViewModel>();


        #region Navigation

        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocusHandler, handledEventsToo: true)
            .AddTo(ref DisposableBag);
        Navigation = new NavigationController<IViewModel>(this, new NavigationStore("nav")).DisposeItWith(Disposable);

        Events.Catch<GoToEvent>(async (_, e, _) => await Navigation.GoTo(e.Path)).DisposeItWith(Disposable);
        
        #endregion
        
        Cfg = cfg;
        Container = ioc;
        LayoutService = ioc.GetRequiredService<ILayoutService>();
        DialogService = ioc.GetRequiredService<IDialogService>();
        
        
        UndoStoreService = ioc.GetRequiredService<IUndoStoreService>();

        _unsavedChangesDialogPrefab = DialogService.GetDialogPrefab<UnsavedChangesDialogPrefab>();

        WindowSateIconKind = new BindableReactiveProperty<MaterialIconKind>().DisposeItWith(
            Disposable
        );
        WindowStateHeader = new BindableReactiveProperty<string>().DisposeItWith(Disposable);

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
        SelectedPage
            .WhereNotNull()
            .SubscribeAwait(async (page, ct) => await page.RequestLoadLayout(LayoutService, ct))
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
                var err = last.Severity == ShellErrorState.Error;
                if (err)
                {
                    ErrorState = ShellErrorState.Error;
                    return;
                }
                var warn = last.Severity == ShellErrorState.Warning;
                if (warn)
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

        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
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
                // this need for ignore root shell when focus changed
                if (routable is IShell)
                {
                    return;
                }
                Navigation.ForceSelect(routable);
                break;
            }

            // Try to find IViewModel DataContext in logical parent
            control = control.GetLogicalParent() as Control;
        }
    }

    protected ILogger Logger => _logger;

    #region Tools

    public ObservableList<IMenuItem> LeftMenu { get; }
    public MenuTree LeftMenuView { get; }

    public ObservableList<IMenuItem> RightMenu { get; }
    public MenuTree RightMenuView { get; }

    #endregion

    #region Theme command

    public void ChangeTheme()
    {
        //this.ExecuteCommand(ChangeThemeFreeCommand.Id).SafeFireAndForget();
    }

    public void OpenSettings()
    {
        //this.ExecuteCommand(OpenSettingsCommand.Id).SafeFireAndForget();
    }

    #endregion

    #region MainMenu

    public ObservableList<IMenuItem> MainMenu { get; }
    public MenuTree MainMenuView { get; }

    #endregion

    #region Close
    public ReactiveCommand Close { get; }

    public Observable<Unit> OnClose => _onCloseEvent;

    protected virtual async ValueTask<bool> TryCloseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var page in _pages)
        {
            try
            {
                var reasons = await page.RequestChildCloseApproval(cancellationToken);

                if (reasons.Count != 0)
                {
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

    #endregion

    #region ChangeWindowState

    // TODO: Move to DesktopShellViewModel later
    public BindableReactiveProperty<MaterialIconKind> WindowSateIconKind { get; }

    // TODO: Move to DesktopShellViewModel later
    public BindableReactiveProperty<string> WindowStateHeader { get; }

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
    
    public override async ValueTask<IViewModel> Navigate(NavId id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page is null)
        {
            var pageContext = new PageContext(id.Args, UndoStoreService.CreateUndoHistoryStore(id));
            page = Container.CreatePage(id.TypeId, pageContext);
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

    private async ValueTask InternalCatchEvent(IViewModel src, AsyncRoutedEvent<IViewModel> e, CancellationToken cancel)
    {
        switch (e)
        {
            case GoToEvent goTo:
                await Navigation.GoTo(goTo.Path);
                break;
            case RestartApplicationEvent restart:
            {
                using var sub = _onCloseEvent.Take(1).Subscribe(_ => RestartApplicationCommon());

                await TryCloseAsync(restart.Cancel);

                break;
            }
            case PageCloseRequestedEvent close:
            {
                _logger.ZLogInformation($"Close page [{close.Page.Id}]");

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

                if (_pages.Count == 0)
                {
                    await Navigation.GoTo(new NavPath(new NavId(HomePageViewModel.PageId)));
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
                if (loadLayoutEvent.Sender is not IShell)
                {
                    return;
                }

                await InternalLoadLayoutEventHandler(loadLayoutEvent);
                break;
            case ShellMessageEvent showInfoMessageEvent:
                ShowMessage(showInfoMessageEvent.Message);
                break;
            default:
                await ValueTask.CompletedTask;
                break;
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

    protected virtual void RestartApplication(string[] args)
    {
        _logger.LogError(
            "Restart is not supported by shell type {ShellType}. Arguments: {Args}",
            GetType().Name,
            string.Join(" ", args)
        );
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
            _logger.ZLogInformation($"Try to load layout: {string.Join(",", _config.Pages)}");
            foreach (var page in _config.Pages)
            {
                await this.NavigateByPath(NavPath.Parse(page));
            }

            NavPath? NavPathToGo = null;
            if (_config.SelectedPageId is not null)
            {
                NavPathToGo = Pages
                    .FirstOrDefault(page => page.Id.ToString() == _config.SelectedPageId)
                    ?.GetPathFromRoot();
            }

            NavPathToGo ??= Pages.FirstOrDefault()?.GetPathFromRoot();

            if (NavPathToGo is not null)
            {
                await Navigation.GoTo(NavPathToGo.Value);
            }

            _isLoaded = true;
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error loading layout: {e.Message}");
        }
        finally
        {
            _sub1.Disposable = Pages
                .ObserveAdd(cancellationToken)
                .Subscribe(_ => SaveLayoutToFile(loadLayoutEvent.LayoutService));
            _sub2.Disposable = Pages
                .ObserveRemove(cancellationToken)
                .Subscribe(_ => SaveLayoutToFile(loadLayoutEvent.LayoutService));
            _sub3.Disposable = SelectedPage.Subscribe(_ =>
                SaveLayoutToFile(loadLayoutEvent.LayoutService)
            );
            if (Pages.Count == 0)
            {
                await this.NavigateByPath(new NavPath(new NavId(HomePageViewModel.PageId)));
            }
        }
    }

    private void SaveLayoutToFile(ILayoutService layoutService)
    {
        if (Interlocked.CompareExchange(ref _saveLayoutInProgress, 1, 0) != 0)
        {
            _logger.LogWarning("Save layout is already in progress");
            return;
        }

        try
        {
            _config.Pages = Pages.Select(page => page.Id.ToString()).ToList();
            _config.SelectedPageId = SelectedPage.Value?.Id.ToString();

            _logger.ZLogTrace($"Save layout: {string.Join(",", _config.Pages)}");
            layoutService.SetInMemory(this, _config);
            layoutService.FlushFromMemory(this);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving layout: {EMessage}", e.Message);
        }
        finally
        {
            Interlocked.Exchange(ref _saveLayoutInProgress, 0);
        }
    }

    #endregion

    
    

    public IConfiguration Cfg { get; }
    public ILayoutService LayoutService { get; }

    public ShellErrorState ErrorState
    {
        get;
        set => SetField(ref field, value);
    }

    public string Header
    {
        get;
        set => SetField(ref field, value);
    }

    #region Status bar

    public ObservableList<IStatusItem> StatusItems { get; }
    public NotifyCollectionChangedSynchronizedViewList<IStatusItem> StatusItemsView { get; }

    #endregion

    #region Info bar

    public void ShowMessage(ShellMessage message)
    {
        if (_infoMessagesSource.Count >= MaxInfoBarMessages)
        {
            _infoMessagesSource.RemoveAt(0);
        }
        _infoMessagesSource.Add(
            new ShellMessageViewModel(
                Guid.NewGuid().ToString(),
                CloseInfoMessageCommand,
                message
            )
        );
    }

    public ReactiveCommand<ShellMessageViewModel> CloseInfoMessageCommand { get; }

    public NotifyCollectionChangedSynchronizedViewList<ShellMessageViewModel> InfoBarMessages { get; }

    #endregion

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    #region Dispose

    private readonly SerialDisposable _sub1 = new();
    private readonly SerialDisposable _sub2 = new();
    private readonly SerialDisposable _sub3 = new();
    

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();

            _pages.ClearWithItemsDispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public INavigationController<IViewModel> Navigation { get; }
}
