using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Provides a base class for Avalonia applications with built-in MEF dependency injection,
/// shell management, and application lifecycle orchestration.
/// </summary>
public abstract class ShellHost : Application, IContainerHost, IShellHost
{
    private readonly IAppStartupService? _appStartupService;
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    private IDisposable? _onCloseSubscription;
    private volatile int _isDisposed;

    protected ShellHost(
        Action<ContainerConfiguration> containerCfgOptions,
        Action<ConventionBuilder>? conventionBuilderOptions = null
    )
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        containerCfgOptions(containerCfg);
        conventionBuilderOptions?.Invoke(conventions);

        containerCfg.WithDefaultConventions(conventions);
        WithDependenciesFromTheApp(containerCfg, this);

        _container = containerCfg.CreateContainer();

        DataTemplates.Add(new CompositionViewLocator(_container));

        if (!Design.IsDesignMode)
        {
            _appStartupService = _container.GetExport<IAppStartupService>();
            _appStartupService.AppCtor();
        }
    }

    public override void Initialize()
    {
        LoadXaml();
        _appStartupService?.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            Shell = DesignTimeShellViewModel.Instance;
        }
        else if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Shell = _container.GetExport<IShell>(DesktopShellViewModel.ShellId);
            if (desktop.MainWindow is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = _container.GetExport<IShell>(MobileShellViewModel.ShellId);
            if (singleViewPlatform.MainView is TopLevel topLevel)
            {
                TopLevel = topLevel;
            }
        }
        else
        {
            throw new Exception("Unknown platform");
        }

        _onCloseSubscription = Shell.OnClose.Synchronize().Subscribe(_ => Dispose());

        base.OnFrameworkInitializationCompleted();

#if DEBUG
        this.AttachDevTools();
#endif

        _appStartupService?.OnFrameworkInitializationCompleted();
    }

    public IShell Shell
    {
        get;
        private set
        {
            field = value;
            _onShellLoaded.OnNext(value);
        }
    }

    public Observable<IShell> OnShellLoaded => _onShellLoaded;

    public TopLevel TopLevel { get; private set; }

    #region IContainerHost implementation

    public T GetExport<T>()
        where T : IExportable
    {
        return _container.GetExport<T>();
    }

    public T GetExport<T>(string contract)
        where T : IExportable
    {
        return _container.GetExport<T>(contract);
    }

    public bool TryGetExport<T>(string id, out T value)
        where T : IExportable
    {
        return _container.TryGetExport(id, out value);
    }

    public void SatisfyImports(object value)
    {
        _container.SatisfyImports(value);
    }

    #endregion

    /// <summary>
    /// Loads the XAML resources for the specific application instance.
    /// </summary>
    /// <remarks>
    /// This method must be implemented in the derived class by calling
    /// <c>AvaloniaXamlLoader.Load(this)</c>. This is required for the Avalonia
    /// compiler to correctly associate the XAML file with the class.
    /// </remarks>
    protected abstract void LoadXaml();

    private void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            return;
        }

        _onCloseSubscription?.Dispose();
        _onShellLoaded.Dispose();
        Shell.Dispose();
        _container.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void WithDependenciesFromTheApp(
        in ContainerConfiguration containerConfiguration,
        ShellHost app
    )
    {
        containerConfiguration.WithExport<IDataTemplateHost>(app).WithExport<IShellHost>(app);

        if (Design.IsDesignMode)
        {
            containerConfiguration.WithExport(NullContainerHost.Instance);
        }
        else
        {
            containerConfiguration.WithExport<IContainerHost>(app);
        }

        containerConfiguration.WithAssemblies([app.GetType().Assembly]);
    }

    public IExportInfo Source => SystemModule.Instance;
}
