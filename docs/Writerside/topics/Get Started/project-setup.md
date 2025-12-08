# Project setup

This guide explains how to run a minimal Asv.Avalonia application.
After setup, you can explore the [Test Drive section](pages.md) to learn how to customize application's pages.

This guide assumes that you are already familiar with the basics of Avalonia UI.

## Prerequisites

This project depends on Avalonia UI, so please review
the [Avalonia requirements](https://docs.avaloniaui.net/docs/overview/supported-platforms).

The current version of Asv.Avalonia targets .NET 9.0.
The framework is cross-platform, so it supports Windows, macOS, Linux, and mobile (Android/iOS).

> The only well-tested platform is Windows; mobile platforms have not been tested at all yet.
> {style="warning"}

> You can read more about supported platforms on the [Supported Platforms page](supported-platforms.md).
> {style="info"}

## Choosing an Editor

We recommend using JetBrains Rider with
the [AvaloniaRider plugin](https://plugins.jetbrains.com/plugin/14839-avaloniarider).
Read more in the [Avalonia Editor Setup guide](https://docs.avaloniaui.net/docs/get-started/set-up-an-editor).

## Creating a project

First, create a project for the application and add Avalonia to it. There are two ways to do this:

1. Use an Avalonia template (recommended and described below) by
   following [this guide](https://docs.avaloniaui.net/docs/get-started/test-drive/introduction) (steps 1 and 2).
2. Manual Setup: create an empty project and add Avalonia manually.

For this guide, we will use the "Avalonia .NET App" template. First, install the templates:

```
dotnet new install Avalonia.Templates
```

Then, create a new project:

```
dotnet new avalonia.app -o AsvAvaloniaTest
```

This creates an `AsvAvaloniaTest` project using the Desktop-only Avalonia template (`avalonia.app`).

## Adding Asv.Avalonia to the project

To install the [Asv.Avalonia](https://github.com/asv-soft/asv-avalonia) library, run the following command in your
project directory:

```
dotnet add package Asv.Avalonia
```

Now we must modify our template to run the Asv.Avalonia shell.

### 1. Clean up the Template

Since Asv.Avalonia handles the main window logic for us, we do not need the default window files.
So we can delete `MainWindow.axaml` and `MainWindow.axaml.cs`.

### 2. Configure Program.cs

We need to set up the application host. Change your `Main` method (from `Program.cs` file) from this:

```C#
public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);
```

To this:

```C#
public static void Main(string[] args) 
{
    var builder = AppHost.CreateBuilder(args);
    var dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    builder
        .UseAvalonia(BuildAvaloniaApp)
        
        // This setting defines where all app data (like a JSON user config) will be stored
        .UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))
        
        // Here you can define some JSON config settings. For example, we set autosave to 1 second
        .UseJsonUserConfig(opt => opt.WithAutoSave(TimeSpan.FromSeconds(1)))
        
        // This defines the source of app data (app name, version, etc.). We use the current assembly
        .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
        
        // Here we set up the logging system
        .UseLogging(options =>
        {
            options.WithLogToFile();
            options.WithLogToConsole();
            
            // Optional: here you can enable Log viewer page
            options.WithLogViewer();
        });

    using var host = builder.Build();
    host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
}
```

What is happening here? We are setting up a container with dependencies required by the application.
While most of them are required, you can configure options here, such as changing the settings folder path or logging
behavior.

### 3. Configure App.axaml.cs

Now, let's edit the `App.axaml.cs` code-behind file. Our App class should implement the following interfaces:

```
public class App : Application, IContainerHost, IShellHost
```

This implementation handles dependency injection and other tasks, such as running startup routines.

```C#
public class App : Application, IContainerHost, IShellHost
{
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        containerCfg
            .WithDependenciesFromSystemModule()
            .WithDependenciesFromTheApp(this)
            .WithDefaultConventions(conventions);

        _container = containerCfg.CreateContainer();

        DataTemplates.Add(new CompositionViewLocator(_container));

        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().AppCtor();
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

    public IExportInfo Source => SystemModule.Instance;

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

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().Initialize();
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
            if (desktop.MainWindow is TopLevel topLevel) TopLevel = topLevel;
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = _container.GetExport<IShell>(MobileShellViewModel.ShellId);
            if (singleViewPlatform.MainView is TopLevel topLevel) TopLevel = topLevel;
        }
        else
        {
            throw new Exception("Unknown platform");
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().OnFrameworkInitializationCompleted();
    }
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromTheApp(
        this ContainerConfiguration containerConfiguration,
        App app
    )
    {
        containerConfiguration.WithExport<IDataTemplateHost>(app).WithExport<IShellHost>(app);

        if (Design.IsDesignMode)
            containerConfiguration.WithExport(NullContainerHost.Instance);
        else
            containerConfiguration.WithExport<IContainerHost>(app);

        return containerConfiguration.WithAssemblies([app.GetType().Assembly]);
    }
}
```

The important part is:

```C#
containerCfg
    .WithDependenciesFromSystemModule()
    .WithDependenciesFromTheApp(this)
    .WithDefaultConventions(conventions);
```

Here we register dependencies from the
builder ([DI container](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-usage)) and modules
into [MEF](https://learn.microsoft.com/en-us/dotnet/framework/mef/), which are used in runtime.
If you want to add other modules to the app, you need to register them here as well.

### 4. Configure Styles

Finally, include the Asv.Avalonia styles in your `App.axaml` file:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="AsvAvaloniaTest.App"
             RequestedThemeVariant="Default">
   <Application.Styles>
      <StyleInclude Source="avares://Asv.Avalonia/Styling/Theme.axaml" />
   </Application.Styles>
</Application>
```

## Running the app

You can now try running the application.
You should see a shell with standard pages, such as Settings and the Log Viewer — these are located in the tools section
of the shell.
Below that is the items section, which currently contains the Device Browser.

![Shell page screenshot](shell-page.png)

If you want to customize the application (e.g., add new pages or commands), follow the [Test Drive guide](pages.md).
