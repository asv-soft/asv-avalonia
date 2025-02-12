using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

public partial class App : Application, IContainerHost, IShellHost
{
    private readonly CompositionHost _container;

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        AppHost.Instance.RegisterServices(containerCfg);

        containerCfg
            .WithExport<IContainerHost>(this)
            .WithExport<IDataTemplateHost>(this)
            .WithExport<IShellHost>(this)
            .WithDefaultConventions(conventions);

        containerCfg = containerCfg.WithAssemblies(DefaultAssemblies.Distinct());

        // TODO: load plugin manager before creating container
        _container = containerCfg.CreateContainer();
        DataTemplates.Add(new CompositionViewLocator(_container));
    }

    private IEnumerable<Assembly> DefaultAssemblies
    {
        get
        {
            yield return GetType().Assembly;
            yield return typeof(IAppHost).Assembly;
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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
        if (Design.IsDesignMode == false)
        {
            Shell.Navigate(SettingsPageViewModel.PageId);
            Shell.Navigate(HomePageViewModel.PageId);
            Shell.Navigate(DocumentPageViewModel.PageId);
            Shell.Navigate(MapExamplePageViewModel.PageId);
            Shell.Navigate(DialogBoardViewModel.PageId);
        }
#if DEBUG
        this.AttachDevTools();
#endif
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

    public IShell Shell { get; set; }
    public TopLevel TopLevel { get; private set; }
    public IExportInfo Source => SystemModule.Instance;
}
