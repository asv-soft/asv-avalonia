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
    private readonly ILogger<ShellViewModel> _logger;

    protected ShellViewModel(IContainerHost ioc, ILoggerFactory loggerFactory, string id)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _container = ioc;
        _cmd = ioc.GetExport<ICommandService>();
        Navigation = ioc.GetExport<INavigationService>();
        _pages = new ObservableList<IPage>();
        _logger = loggerFactory.CreateLogger<ShellViewModel>();
        PagesView = _pages.ToNotifyCollectionChangedSlim();
        ErrorState = new BindableReactiveProperty<ShellErrorState>(ShellErrorState.Normal);
        Close = new ReactiveCommand((_, c) => CloseAsync(c));
        ChangeWindowState = new ReactiveCommand((_, c) => ChangeWindowModeAsync(c));
        Collapse = new ReactiveCommand((_, c) => CollapseAsync(c));
        Title = new BindableReactiveProperty<string>();

        SelectedPage = new BindableReactiveProperty<IPage>();
        MainMenu = new ObservableList<IMenuItem>();
        MainMenuView = new MenuTree(MainMenu).DisposeItWith(Disposable);
        MainMenu.SetRoutableParent(this, true).DisposeItWith(Disposable);
    }

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

    #region SaveLayout

    public virtual ValueTask SaveLayout()
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Pages

    protected ObservableList<IPage> InternalPages => _pages;
    public IReadOnlyObservableList<IPage> Pages => _pages;
    public BindableReactiveProperty<IPage> SelectedPage { get; }
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

        SelectedPage.Value = page;

        return base.Navigate(id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => _pages;

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case ExecuteCommandEvent cmd:
                await _cmd.Execute(cmd.CommandId, cmd.Source, cmd.CommandArg);
                break;
            case RestartApplicationEvent:
                Environment.Exit(0);
                break;
            case PageCloseRequestedEvent close:
            {
                _logger.ZLogInformation($"Close page [{close.Page.Id}]");

                // TODO: save page layout
                _pages.Remove(close.Page);
                close.Page.Parent = null;
                close.Page.Dispose();
                if (_pages.Count == 0)
                {
                    await Navigation.GoHomeAsync();
                }

                break;
            }
        }
    }

    #endregion

    public INavigationService Navigation { get; }

    public BindableReactiveProperty<ShellErrorState> ErrorState { get; }
    public BindableReactiveProperty<string> Title { get; }

    #region Dispose

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Title.Dispose();
            ErrorState.Dispose();
            Close.Dispose();
            SelectedPage.Dispose();
            WindowSateIconKind.Dispose();
            WindowStateHeader.Dispose();
            PagesView.Dispose();
            foreach (var page in _pages)
            {
                page.Dispose();
            }

            _pages.Clear();
        }

        base.Dispose(disposing);
    }

    #endregion
}
