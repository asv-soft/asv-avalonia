using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
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
public abstract class AppBase : Application, IContainerHost, IShellHost
{
    private readonly IAppStartupService? _appStartupService;
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    protected AppBase(ContainerConfiguration containerCfg)
    {
        containerCfg.WithDefaultConventions(new ConventionBuilder());
        containerCfg.WithExport<IDataTemplateHost>(this).WithExport<IShellHost>(this);

        if (Design.IsDesignMode)
        {
            containerCfg.WithExport(NullContainerHost.Instance);
        }
        else
        {
            containerCfg.WithExport<IContainerHost>(this);
        }

        // TODO (from Asv.Drones): load plugin manager before creating container
        _container = containerCfg.CreateContainer();

        DataTemplates.Add(new CompositionViewLocator(_container));

        if (_container.TryGetExport(out IAppStartupService startup))
        {
            _appStartupService = startup;
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

    public IExportInfo Source => SystemModule.Instance;

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _onShellLoaded.Dispose();
            Shell.Dispose();
            _container.Dispose();
        }
    }

    #endregion
}

public static class ContainerConfigurationExtensions
{
    public static ContainerConfiguration WithDependenciesFromTheAssembly(
        this ContainerConfiguration containerConfiguration,
        Assembly assembly
    )
    {
        return containerConfiguration.WithAssembly(assembly);
    }
}
