using System.Composition;
using System.Diagnostics;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

[Export(typeof(INavigationService))]
[Shared]
public class NavigationService : AsyncDisposableOnce, INavigationService
{
    private readonly IShellHost _host;
    private readonly IDisposable _disposeIt;
    private readonly ReactiveProperty<IRoutable?> _selectedControl;
    private readonly ReactiveProperty<NavigationPath> _selectedControlPath;
    private readonly ObservableStack<NavigationPath> _backwardStack = new();
    private readonly ObservableStack<NavigationPath> _forwardStack = new();
    private readonly ILogger<NavigationService> _logger;

    [ImportingConstructor]
    public NavigationService(IShellHost host, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<NavigationService>();
        _host = host;
        var dispose = Disposable.CreateBuilder();

        _selectedControl = new ReactiveProperty<IRoutable?>().AddTo(ref dispose);
        _selectedControlPath = new ReactiveProperty<NavigationPath>().AddTo(ref dispose);
        _selectedControlPath.Subscribe(PushNavigation).AddTo(ref dispose);

        // global event handlers for focus IRoutable controls
        InputElement
            .GotFocusEvent.AddClassHandler<TopLevel>(GotFocusHandler, handledEventsToo: true)
            .AddTo(ref dispose);

        Backward = new ReactiveCommand((_, _) => BackwardAsync()).AddTo(ref dispose);
        Forward = new ReactiveCommand((_, _) => ForwardAsync()).AddTo(ref dispose);
        GoHome = new ReactiveCommand((_, _) => GoHomeAsync()).AddTo(ref dispose);
        SelectedControl = _selectedControl.ToReadOnlyReactiveProperty().AddTo(ref dispose);
        SelectedControlPath = _selectedControlPath.ToReadOnlyReactiveProperty().AddTo(ref dispose);

        _host
            .OnShellLoaded.SubscribeAwait(
                async (sh, ct) => await sh.RequestLoadLayoutForSelfOnly(ct)
            )
            .AddTo(ref dispose);
        _disposeIt = dispose.Build();
    }

    private void PushNavigation(NavigationPath navigationPath)
    {
        _logger.ZLogTrace($"Push navigation history: {navigationPath}");
        _backwardStack.Push(navigationPath);
        _forwardStack.Clear();
    }

    private void FocusControlChanged(IRoutable? routable)
    {
        if (routable == null)
        {
            _logger.ZLogWarning($"Selected control {routable} is null");
            return;
        }

        var path = routable.GetPathToRoot();

        if (_selectedControl.Value?.Id == routable.Id && _selectedControlPath.Value == path)
        {
            return;
        }

        if (path == default || path.Count == 0)
        {
            _logger.ZLogWarning($"Selected control {routable} has empty path");
            return;
        }

        if (path[0] != _host.Shell.Id)
        {
            Debug.Assert(
                false,
                "Selected control has no IShell parent!!! May be you forgot to set Parent for the control?"
            );
            _logger.ZLogWarning(
                $"Selected control {routable} has invalid path: {string.Join(",", path)}. It's must start with IShell Id: {_host.Shell.Id}"
            );
            return;
        }

        _selectedControl.Value = routable;
        _selectedControlPath.Value = path;
    }

    public async ValueTask<IRoutable> GoTo(NavigationPath path)
    {
        try
        {
            if (path.Count == 0)
            {
                _logger.LogError("Error navigating to empty path");
                await NavException.AsyncEmptyPathException();
            }

            _logger.ZLogInformation($"Navigate to '{string.Join(",", path)}'");
            var result = await _host.Shell.NavigateByPath(
                path[0] == _host.Shell.Id ? path[1..] : path
            );
            FocusControlChanged(result);
            return result;
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error on GoTo {path}: {e.Message}");
            throw;
        }
    }

    #region Focus

    public void ForceFocus(IRoutable? routable)
    {
        FocusControlChanged(routable);
    }

    private void GotFocusHandler(TopLevel top, GotFocusEventArgs args)
    {
        Debug.WriteLine($"GotFocusHandler: {args.Source}");
        if (args.Source is not Control source)
        {
            return;
        }

        var control = source;
        while (control != null)
        {
            if (control.DataContext is IRoutable routable)
            {
                Debug.WriteLine($"GotFocusHandler: {routable.GetPathToRoot()}");
                FocusControlChanged(routable);
                break;
            }

            // Try to find IRoutable DataContext in logical parent
            control = control.GetLogicalParent() as Control;
        }
    }

    public ReadOnlyReactiveProperty<IRoutable?> SelectedControl { get; }
    public ReadOnlyReactiveProperty<NavigationPath> SelectedControlPath { get; }

    #endregion

    #region Forward / Backward

    public ReactiveCommand Forward { get; }

    public IObservableCollection<NavigationPath> ForwardStack => _forwardStack;

    public async ValueTask ForwardAsync()
    {
        if (_forwardStack.TryPop(out var path))
        {
            _backwardStack.Push(path);
            await GoTo(path);
            CheckBackwardForwardCanExecute();
        }
    }

    public IObservableCollection<NavigationPath> BackwardStack => _backwardStack;

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

    #region Home page

    public async ValueTask GoHomeAsync()
    {
        var home = await GoTo(new NavigationPath(HomePageViewModel.PageId));

        FocusControlChanged(home);
    }

    public ReactiveCommand GoHome { get; }

    #endregion

    public IExportInfo Source => SystemModule.Instance;
}
