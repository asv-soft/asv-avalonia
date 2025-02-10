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
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

public partial class App : Application, IContainerHost, IShellHost
{
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
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow) ?? throw new Exception($"Can't find TopLevel");
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = new MobileShellViewModel(singleViewPlatform, this);
            TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView) ?? throw new Exception($"Can't find TopLevel");
        }

        base.OnFrameworkInitializationCompleted();
        if (Design.IsDesignMode == false)
        {
            Shell.OpenPage(SettingsPage.PageId);
        }
    }

    public CompositionHost Host { get; }
    public IShell Shell { get; private set; }
    public TopLevel TopLevel { get; set; }
}
