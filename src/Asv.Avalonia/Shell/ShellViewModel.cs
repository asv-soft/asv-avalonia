using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ShellViewModel : ExtendableViewModel<IShell>, IShell
{
    private readonly ObservableList<IPage> _pages;
    private readonly IContainerHost _container;
    private readonly ICommandService _cmd;

    protected ShellViewModel(IContainerHost ioc, string id)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _container = ioc;
        _cmd = ioc.GetExport<ICommandService>();
        Navigation = ioc.GetExport<INavigationService>();
        _pages = new ObservableList<IPage>();
        PagesView = _pages.ToNotifyCollectionChangedSlim();
        ErrorState = new BindableReactiveProperty<ShellErrorState>(ShellErrorState.Normal);
        Close = new ReactiveCommand((_, c) => CloseAsync(c));
        Title = new BindableReactiveProperty<string>();

        SelectedPage = new BindableReactiveProperty<IPage>();
        MainMenu = new ObservableList<IMenuItem>();
        MainMenuView = new MenuTree(MainMenu).DisposeItWith(Disposable);
        MainMenu.ObserveAdd().Subscribe(x => x.Value.Parent = this).DisposeItWith(Disposable);
        MainMenu.ObserveRemove().Subscribe(x => x.Value.Parent = null).DisposeItWith(Disposable);
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
    public override ValueTask<IRoutable> Navigate(string id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page == null)
        {
            if (_container.TryGetExport<IPage>(id, out page))
            {
                _pages.Add(page);
                page.Parent = this;
                SelectedPage.Value = page;
            }

            return ValueTask.FromResult<IRoutable>(page);
        }

        SelectedPage.Value = page;
        return ValueTask.FromResult<IRoutable>(page);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return _pages;
    }

    protected override async ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ExecuteCommandEvent cmd)
        {
            await _cmd.Execute(cmd.CommandId, cmd.Source, cmd.CommandParameter);
        }

        if (e is RestartApplicationEvent)
        {
            Environment.Exit(0);
        }

        if (e is PageCloseRequestedEvent close)
        {
            // TODO: save page layout
            _pages.Remove(close.Page);
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
