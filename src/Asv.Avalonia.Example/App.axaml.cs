using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using Asv.Avalonia.Example.Api;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using R3;

namespace Asv.Avalonia.Example;

public class App : Application, IContainerHost, IShellHost
{
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        if (Design.IsDesignMode)
        {
            containerCfg
                .WithExport<IDataTemplateHost>(this)
                .WithExport<IShellHost>(this)
                .WithExport(NullContainerHost.Instance)
                .WithExport<IConfiguration>(new InMemoryConfiguration())
                .WithExport(NullLoggerFactory.Instance)
                .WithExport(NullAppPath.Instance)
                .WithExport(NullAppInfo.Instance)
                .WithExport<IMeterFactory>(new DefaultMeterFactory())
                .WithExport(NullPluginManager.Instance)
                .WithExport(TimeProvider.System)
                .WithExport(NullLogReaderService.Instance)
                .WithExportFromModule(
                    IoModule.Instance,
                    Options.Create(new IoModuleOptions { EnableDevices = true })
                )
                .WithDefaultConventions(conventions);
        }
        else
        {
            // TODO: use it when plugin manager implementation will be finished
            // var pluginManager = AppHost.Instance.GetService<IPluginManager>();
            containerCfg
                .WithExport<IDataTemplateHost>(this)
                .WithExport<IShellHost>(this)
                .WithExport<IContainerHost>(this)
                .WithExport(AppHost.Instance.GetService<IConfiguration>())
                .WithExport(AppHost.Instance.GetService<ILoggerFactory>())
                .WithExport(AppHost.Instance.GetService<IAppPath>())
                .WithExport(AppHost.Instance.GetService<IAppInfo>())
                .WithExport(AppHost.Instance.GetService<IMeterFactory>())
                .WithExport(AppHost.Instance.GetService<ISoloRunFeature>())
                .WithExport(AppHost.Instance.GetService<TimeProvider>())
                .WithExport(AppHost.Instance.GetService<ILogReaderService>())
                .WithExportFromModule(
                    IoModule.Instance,
                    AppHost.Instance.GetService<IOptions<IoModuleOptions>>()
                )
                .WithDefaultConventions(conventions);
        }

        containerCfg = containerCfg.WithAssemblies(DefaultAssemblies.Distinct());

        // TODO: load plugin manager before creating container
        _container = containerCfg.CreateContainer();

        DataTemplates.Add(new CompositionViewLocator(_container));

        if (!Design.IsDesignMode)
        {
            _container.GetExport<IAppStartupService>().AppCtor();
        }
    }

    private IEnumerable<Assembly> DefaultAssemblies
    {
        get
        {
            yield return GetType().Assembly; // Asv.Avalonia.Example
            yield return typeof(AppHost).Assembly; // Asv.Avalonia
            yield return typeof(GeoMapModule).Assembly; // Asv.Avalonia.GeoMap
            yield return typeof(ApiModule).Assembly; // Asv.Avalonia.Example.Api

            // TODO: use it when plugin manager implementation will be finished
            // yield return typeof(PluginManagerModule).Assembly; // Asv.Avalonia.Plugins
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (!Design.IsDesignMode)
        {
            _container.GetExport<IAppStartupService>().Initialize();
        }
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
        if (!Design.IsDesignMode)
        {
            _container.GetExport<IAppStartupService>().OnFrameworkInitializationCompleted();
        }
    }

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
    public IExportInfo Source => SystemModule.Instance;
    public TopLevel TopLevel { get; private set; }
}
