using System.Composition;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Input;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ShellViewModel : ExtendableViewModel<IShell>, IShell
{
    private readonly ObservableStack<string[]> _backwardStack = new();
    private readonly ObservableStack<string[]> _forwardStack = new();
    private readonly ReactiveProperty<IRoutable> _selectedControl;
    private readonly ReactiveProperty<string[]> _selectedControlPath;

    private readonly ObservableList<IPage> _pages;
    private readonly IContainerHost _container;
    private readonly ICommandService _cmd;

    protected ShellViewModel(IContainerHost ioc, string id)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _container = ioc;
        _cmd = ioc.GetExport<ICommandService>();
        _pages = new ObservableList<IPage>();

        GoHome = new ReactiveCommand((_, c) => GoHomeAsync(c));
        PagesView = _pages.ToNotifyCollectionChangedSlim();
        ErrorState = new BindableReactiveProperty<ShellErrorState>(ShellErrorState.Normal);
        Close = new ReactiveCommand((_, c) => CloseAsync(c));

        Backward = new ReactiveCommand((_, c) => BackwardAsync(c));
        Forward = new ReactiveCommand((_, c) => ForwardAsync(c));
        Title = new BindableReactiveProperty<string>();

        SelectedPage = new BindableReactiveProperty<IPage?>();
        _selectedControl = new ReactiveProperty<IRoutable>(this);
        _selectedControlPath = new ReactiveProperty<string[]>(GetPath(this));
        _selectedControl
            .Subscribe(x => _selectedControlPath.Value = GetPath(x))
            .DisposeItWith(Disposable);
        _selectedControlPath
            .Subscribe(x =>
            {
                if (x is not { Length: > 0 })
                {
                    return;
                }

                _backwardStack.Push(x);
                _forwardStack.Clear();
                CheckBackwardForwardCanExecute();
            })
            .DisposeItWith(Disposable);
        ;

        // global event handlers for focus IRoutable controls
        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocus, handledEventsToo: true)
            .DisposeItWith(Disposable);
        ;

        // global event handlers for key events
        InputElement
            .KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDownCustom, handledEventsToo: true)
            .DisposeItWith(Disposable);
        ;

        MainMenu = new ObservableList<IMenuItem>();
        MainMenuView = new MenuTree(MainMenu).DisposeItWith(Disposable);
        MainMenu.ObserveAdd().Subscribe(x => x.Value.Parent = this).DisposeItWith(Disposable);
        MainMenu.ObserveRemove().Subscribe(x => x.Value.Parent = null).DisposeItWith(Disposable);
        ;
    }

    private void GotFocus(TopLevel control, GotFocusEventArgs args)
    {
        if (args.Source is Control { DataContext: IRoutable routable })
        {
            routable.RaiseFocusEvent();
        }
    }

    private void OnKeyDownCustom(TopLevel source, KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.KeyModifiers == KeyModifiers.None)
        {
            // we don't want to handle key events without modifiers
            return;
        }

        var gesture = new KeyGesture(keyEventArgs.Key, keyEventArgs.KeyModifiers);
        if (
            _cmd.CanExecuteCommand(
                gesture,
                SelectedControl.CurrentValue,
                out var command,
                out var target
            )
        )
        {
            if (target != null && command != null)
            {
                target.ExecuteCommand(command.Info.Id, null);
                keyEventArgs.Handled = true;
            }
        }
    }

    public ReadOnlyReactiveProperty<IRoutable> SelectedControl => _selectedControl;
    public ReadOnlyReactiveProperty<string[]> SelectedControlPath => _selectedControlPath;

    private string[] GetPath(IRoutable vm)
    {
        return vm.GetHierarchyFrom(this).Skip(1).Select(x => x.Id).ToArray();
    }

    #region MainMenu

    public ObservableList<IMenuItem> MainMenu { get; }
    public MenuTree MainMenuView { get; }

    #endregion

    #region Backward \ Forward \ Home

    public ReactiveCommand GoHome { get; }

    public async ValueTask GoHomeAsync(CancellationToken cancel = default) =>
        await Navigate(HomePageViewModel.PageId);

    public ReactiveCommand Backward { get; }

    public async ValueTask BackwardAsync(CancellationToken cancel = default)
    {
        if (_backwardStack.TryPop(out var path))
        {
            _forwardStack.Push(path);
            await this.NavigateByPath(path);
            CheckBackwardForwardCanExecute();
        }
    }

    private void CheckBackwardForwardCanExecute()
    {
        Backward.ChangeCanExecute(_backwardStack.Count != 0);
        Forward.ChangeCanExecute(_forwardStack.Count != 0);
    }

    public IObservableCollection<string[]> ForwardStack => _forwardStack;
    public ReactiveCommand Forward { get; }

    public async ValueTask ForwardAsync(CancellationToken cancel = default)
    {
        if (_forwardStack.TryPop(out var path))
        {
            _backwardStack.Push(path);
            await this.NavigateByPath(path);
            CheckBackwardForwardCanExecute();
        }
    }

    public IObservableCollection<string[]> BackwardStack => _backwardStack;

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
    public BindableReactiveProperty<IPage?> SelectedPage { get; }
    public NotifyCollectionChangedSynchronizedViewList<IPage> PagesView { get; }

    public ValueTask<IPage> OpenNewPage(string id)
    {
        if (_container.TryGetExport<IPage>(id, out var page))
        {
            _pages.Add(page);
            page.Parent = this;
            _selectedControl.Value = page;
        }

        return ValueTask.FromResult(page);
    }

    #endregion

    #region Routable
    public override async ValueTask<IRoutable> Navigate(string id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page == null)
        {
            page = await OpenNewPage(id);
        }

        SelectedPage.Value = page;
        return page;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return _pages;
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ExecuteCommandEvent cmd)
        {
            // write command to log
        }

        if (e is GotFocusEvent gotFocus)
        {
            _selectedControl.Value = gotFocus.Source;
        }

        if (e is PageCloseRequestedEvent close)
        {
            // TODO: save page layout
            _pages.Remove(close.Page);
        }

        return ValueTask.CompletedTask;
    }

    #endregion

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
            _selectedControl.Dispose();
            _selectedControlPath.Dispose();
            Title.Dispose();
            Backward.Dispose();
            Forward.Dispose();
            GoHome.Dispose();
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
