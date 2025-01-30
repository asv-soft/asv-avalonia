using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Asv.Avalonia.Example.ViewModels;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public partial class App : Application, IContainerHost, IShellHost
{
    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();
        if (Design.IsDesignMode)
        {
            containerCfg
                .WithExport(NullAppHost.Instance.AppInfo)
                .WithExport(NullAppHost.Instance.AppPath)
                .WithExport(NullAppHost.Instance.Configuration)
                .WithExport(NullAppHost.Instance.Logs)
                .WithExport<ILoggerFactory>(NullAppHost.Instance.Logs)
                .WithExport(NullAppHost.Instance.Args)
                .WithExport(NullAppHost.Instance);
        }
        else
        {
            containerCfg
                .WithExport(AppHost.Instance.AppInfo)
                .WithExport(AppHost.Instance.AppPath)
                .WithExport(AppHost.Instance.Configuration)
                .WithExport(AppHost.Instance.Logs)
                .WithExport<ILoggerFactory>(AppHost.Instance.Logs)
                .WithExport(AppHost.Instance.Args)
                .WithExport(AppHost.Instance);
        }

        containerCfg
            .WithExport<IContainerHost>(this)
            .WithExport<IDataTemplateHost>(this)
            .WithExport<IShellHost>(this)
            .WithDefaultConventions(conventions);

        containerCfg = containerCfg.WithAssemblies(DefaultAssemblies.Distinct());

        // TODO: load plugin manager before creating container
        Host = containerCfg.CreateContainer();
        DataTemplates.Add(new CompositionViewLocator(Host));
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Shell = new DesktopShellViewModel(desktop, this);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = new MobileShellViewModel(singleViewPlatform, this);
        }

        base.OnFrameworkInitializationCompleted();
        if (Design.IsDesignMode == false)
        {
            Shell.OpenPage(SettingsPage.PageId);
        }
    }

    public CompositionHost Host { get; }
    public IShell Shell { get; private set; }
}
