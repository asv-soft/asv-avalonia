using System.Composition;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[Export(typeof(INavigationService))]
[Shared]
public class NavigationService : AsyncDisposableOnce, INavigationService
{
    private readonly IContainerHost _ioc;
    private readonly IShellHost _host;
    private readonly IDisposable _disposeIt;
    private readonly ReactiveProperty<IRoutable?> _selectedControl;
    private readonly ReactiveProperty<string[]> _selectedControlPath;
    private readonly ObservableStack<string[]> _backwardStack = new();
    private readonly ObservableStack<string[]> _forwardStack = new();

    [ImportingConstructor]
    public NavigationService(IContainerHost ioc, IShellHost host)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(ioc);
        _ioc = ioc;
        _host = host;
        var dispose = Disposable.CreateBuilder();
        _selectedControl = new ReactiveProperty<IRoutable?>().AddTo(ref dispose);
        _selectedControlPath = new ReactiveProperty<string[]>([]).AddTo(ref dispose);

        // global event handlers for focus IRoutable controls
        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocusHandler, handledEventsToo: true)
            .AddTo(ref dispose);

        Backward = new ReactiveCommand((_, _) => BackwardAsync()).AddTo(ref dispose);
        Forward = new ReactiveCommand((_, _) => ForwardAsync()).AddTo(ref dispose);

        _disposeIt = dispose.Build();
    }

    public ValueTask<IRoutable> GoTo(string[] path)
    {
        if (path.Length == 0)
        {
            return NavException.AsyncEmptyPathException();
        }

        return _host.Shell.NavigateByPath(path[0] == _host.Shell.Id ? path[1..] : path);
    }

    #region Focus

    private void GotFocusHandler(TopLevel top, GotFocusEventArgs args)
    {
        if (args.Source is not Control source)
        {
            return;
        }

        var control = source;
        while (control != null)
        {
            if (control.DataContext is IRoutable routable)
            {
                SelectedControlChanged(routable);
                break;
            }

            // Try to find IRoutable DataContext in logical parent
            control = control.GetLogicalParent() as Control;
        }
    }

    private void SelectedControlChanged(IRoutable? routable)
    {
        _selectedControl.Value = routable;
        if (routable == null)
        {
            return;
        }

        _selectedControlPath.Value = routable.GetPathToRoot();
    }

    public ReadOnlyReactiveProperty<IRoutable?> SelectedControl => _selectedControl;
    public ReadOnlyReactiveProperty<string[]> SelectedControlPath => _selectedControlPath;

    #endregion

    #region Forward / Backward

    public ReactiveCommand Forward { get; }

    public IObservableCollection<string[]> ForwardStack => _forwardStack;

    public async ValueTask ForwardAsync()
    {
        if (_forwardStack.TryPop(out var path))
        {
            _backwardStack.Push(path);
            await GoTo(path);
            CheckBackwardForwardCanExecute();
        }
    }

    public IObservableCollection<string[]> BackwardStack => _backwardStack;

    public async ValueTask BackwardAsync()
    {
        if (_backwardStack.TryPop(out var path))
        {
            _forwardStack.Push(path);
            await GoTo(path);
            CheckBackwardForwardCanExecute();
        }
    }

    public ReactiveCommand Backward { get; }

    private void CheckBackwardForwardCanExecute()
    {
        Backward.ChangeCanExecute(_backwardStack.Count != 0);
        Forward.ChangeCanExecute(_forwardStack.Count != 0);
    }

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposeIt.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_disposeIt is IAsyncDisposable disposeItAsyncDisposable)
        {
            await disposeItAsyncDisposable.DisposeAsync();
        }
        else
        {
            _disposeIt.Dispose();
        }

        await base.DisposeAsyncCore();
    }

    #endregion

    public IExportInfo Source => SystemModule.Instance;
}
