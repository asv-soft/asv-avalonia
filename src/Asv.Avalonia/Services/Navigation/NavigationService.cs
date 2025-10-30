using System.Composition;
using System.Diagnostics;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class NavigationServiceConfig
{
    public IList<string> Pages { get; } = [];
}

[Export(typeof(INavigationService))]
[Shared]
public class NavigationService : AsyncDisposableOnce, INavigationService
{
    private readonly IContainerHost _ioc;
    private readonly IShellHost _host;
    private readonly IConfiguration _cfgSvc;
    private readonly IDisposable _disposeIt;
    private readonly ReactiveProperty<IRoutable?> _selectedControl;
    private readonly ReactiveProperty<NavigationPath> _selectedControlPath;
    private readonly ObservableStack<NavigationPath> _backwardStack = new();
    private readonly ObservableStack<NavigationPath> _forwardStack = new();
    private readonly ILogger<NavigationService> _logger;
    private IDisposable? _sub2;
    private IDisposable? _sub1;
    private int _saveLayoutInProgress;

    [ImportingConstructor]
    public NavigationService(
        IContainerHost ioc,
        IShellHost host,
        IConfiguration cfgSvc,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(ioc);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(cfgSvc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<NavigationService>();
        _ioc = ioc;
        _host = host;
        _cfgSvc = cfgSvc;
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
        _host.OnShellLoaded.SubscribeAwait(LoadLayout).AddTo(ref dispose);
        _disposeIt = dispose.Build();
    }

    private void PushNavigation(NavigationPath navigationPath)
    {
        _logger.ZLogTrace($"Push navigation history: {navigationPath}");
        _backwardStack.Push(navigationPath);
        _forwardStack.Clear();
    }

    private async ValueTask LoadLayout(IShell shell, CancellationToken cancellationToken)
    {
        try
        {
            var cfg = _cfgSvc.Get<NavigationServiceConfig>();
            _logger.ZLogInformation($"Try to load layout: {string.Join(",", cfg.Pages)}");
            foreach (var page in cfg.Pages)
            {
                await GoTo(new NavigationPath(page));
            }
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error loading layout: {e.Message}");
        }
        finally
        {
            _sub1 = shell.Pages.ObserveAdd().Subscribe(_ => SaveLayout());
            _sub2 = shell.Pages.ObserveRemove().Subscribe(_ => SaveLayout());
            if (_host.Shell.Pages.Count == 0)
            {
                await GoHomeAsync();
            }
        }
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

    private void SaveLayout()
    {
        if (Interlocked.CompareExchange(ref _saveLayoutInProgress, 1, 0) != 0)
        {
            _logger.LogWarning("Save layout is already in progress");
            return;
        }

        try
        {
            var cfg = new NavigationServiceConfig();
            foreach (var page in _host.Shell.Pages)
            {
                cfg.Pages.Add(page.Id.ToString());
            }
            _logger.ZLogTrace($"Save layout: {string.Join(",", cfg.Pages)}");
            _cfgSvc.Set(cfg);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving layout: {EMessage}", e.Message);
            Debug.Assert(false, $"Error saving layout: {e.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _saveLayoutInProgress, 0);
        }
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
            if (result is ISupportFocus focus)
            {
                focus.Focus();
            }
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
            _sub1?.Dispose();
            _sub2?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _sub1?.Dispose();
        _sub2?.Dispose();
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
