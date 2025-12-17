# How to create a new module

This tutorial will help you get more familiar with modules in Asv.Avalonia.
By the end of this guide, you will create a fully functional module that extends the functionality of the base framework.
We will do everything step by step, but you can check the full code for the Module [here](how-to-create-module-source-code.md).

## Requirements 

1. .NET 10 or higher
2. IDE ([Rider](https://www.jetbrains.com/rider/) is recommended)
3. Desktop environment (this module is fully functional only on desktop)

## Before we start

In this guide, we will skip the initial setup steps.
We recommend checking the [Get Started](project-setup.md) and [What is a Module](what-is-a-module.md) pages before proceeding.

This guide assumes that you have basic knowledge about the following topics:
- Basic concepts of C#
- Understanding Modules in Asv.Avalonia
- How to install the Asv.Avalonia framework 
- What NuGet is
- How to create a .NET project

> Note that in this guide, we use Rider as the IDE. 
> You may use any IDE you prefer, but some instructions might be unclear.
> { style="info" }

## Creating a project

Let's create a new C# project:

![create-project](how-to-create-module-create-project.png)

![project](how-to-create-module-project.png)

We will use a library template here to avoid unnecessary dependencies.

## Installing dependencies

Go to your NuGet manager and install the following dependencies:

![dependencies](how-to-create-module-dependencies.png)

You need Asv.Avalonia v1.0.3+. We use a private version of our framework here.

## Adding Demo project

> This demo project is only for testing the module.
> { style="info" }

We need to create a simple Avalonia MVVM application to debug our module.
We will skip the details of this project since they are not essential for module creation.
You can create this project yourself with the help of [Get Started](project-setup.md);

We will show only the Program.cs:

![demo-app-initial-structure](how-to-create-module-demo-app-initial-structure.png)

```C#
using Avalonia;
using System;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Module.Demo;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = AppHost.CreateBuilder(args);
        var dataFolder =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        builder
            .UseAvalonia(BuildAvaloniaApp)
            .UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))
            .UseJsonUserConfig(opt =>
                opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
            )
            .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
            .UseSoloRun(opt => opt.WithArgumentForwarding())
            .UseLogging(options =>
            {
                options.WithLogToFile(Path.Combine(dataFolder, "data", "logs"));
                options.WithLogToConsole();
                options.WithLogLevel(LogLevel.Trace);
            });

        using var host = builder.Build();
        host.ExitIfNotFirstInstance();
        host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseR3();
}
```
{collapsible="true" collapsed-title="Program.cs"}

Go to the `.csproj` file and add the following lines:

```xml
<ItemGroup>
    <ProjectReference Include="..\Asv.Avalonia.Module\Asv.Avalonia.Module.csproj" />
</ItemGroup>
```

This will enable design time for views in our module, and it will add the module to the application.

## Adding Features

We create modules to extend basic functions of the Asv.Avalonia framework.
That is why we need to consider what features the framework is missing.
The first idea that comes to mind is a video module for video transfer over the network.
The second idea is a cat module, which is arguably even more important.
We will create a page that shows us pictures of cats (or at least one picture with cats).
With this feature, users will be able to increase their mood just by looking at the picture.
With greater mood comes greater user experience!

### Planning things

We've already set the goal of our future module.
Now we need to define what exactly we will do to implement cats in our application:

Must have:
1. Special page for picture with cats;
2. Default picture.

Nice to have:
1. Page with dogs (some users may like dogs more than cats);

Now that we have planned everything, let's start building the MVP of our app.

### Adding a new page

Add a new file with the name CatsPageViewModel.cs and class CatsPageViewModel:

![cats-page](how-to-create-module-cats-page-initial.png)

> In this guide we will use the same structure that we use in the other projects in our company
> {style="info"}

Inherit this class from the page view model base and implement required components.

![required-components](how-to-create-module-required-components.png)

Download any picture from the internet and put it in the assets folder.
Create a view for the cats page.

```XML
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:cats="clr-namespace:Asv.Avalonia.Module.Pages.Cats"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Module.Pages.Cats.CatsPageView"
             x:DataType="cats:CatsPageViewModel">
    <Grid>
        <Image Source="avares://Asv.Avalonia.Module/Assets/cat.jpg"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
    </Grid>
</UserControl>
```

![base-view-for-cats](how-to-create-module-base-view-for-cats.png)

Now we take a picture with cats exactly from the Assets folder and put it in the axaml.
In our plan we described that it would be nice to be able to change the picture.
We are going to use ViewModel as a source for pictures.

Write the following code in the constructor:

```C#
var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Module/Assets/cat.jpg"))
            .DisposeItWith(Disposable);
var defaultPicture = new Bitmap(stream).DisposeItWith(Disposable);
        
SelectedImage = defaultPicture;
```

Add this property after the constructor:

```c#
public IImage? SelectedImage
{
    get;
    private init => SetField(ref field, value);
}
```

Now we can use this property in the axaml:

```XML
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:module="clr-namespace:Asv.Avalonia.Module"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Module.CatsPageView"
             x:DataType="module:CatsPageViewModel">
    <Design.DataContext>
        <module:CatsPageViewModel/>
    </Design.DataContext>
    <Grid>
        <Image Source="{CompiledBinding SelectedImage}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
    </Grid>
</UserControl>
```

### Preparing for the export

Now you can see the page in the design mode, but if you run the demo app you won't see the button to open the page.
To make the page a part of the application we need to set up the export.

#### Creating an open page command

Add the following two constants in the CatsPageViewModel right after the PageId constant.

```c#
public const MaterialIconKind PageIcon =  MaterialIconKind.Cat;
public const AsvColorKind PageIconColor = AsvColorKind.Info3;
```

Create Commands folder in the Asv.Avalonia.Module folder
In this folder create OpenCatsPageCommand.cs

Add the following code in the file:
```c#
using System.Composition;

namespace Asv.Avalonia.Module;

[ExportCommand]
[method: ImportingConstructor]
public class OpenCatsPageCommand(INavigationService nav)
    : OpenPageCommandBase(CatsPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    public const string Id = $"{BaseId}.open.cats";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open cats page",
        Description = "Command opens cats page",
        Icon = CatsPageViewModel.PageIcon,
        IconColor = CatsPageViewModel.PageIconColor,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
}
```

#### Adding button to the home page

In the Cats folder create a HomePageCatsPageExtension.cs file with the following content:
```c#
using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Module;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageCatsPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenCatsPageCommand
                .StaticInfo
                .CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
```

Now we are almost ready, and we need to make the most important step.

#### Adding export info

In the root folder of the Asv.Avalonia.Module create cs file with the name ModuleModule.
We usually call this file by adding Module word to the module's name. 
In our case we call it ModuleModule.

Implement IExportInfo interface

```c#
public class ModuleModule : IExportInfo
{
    public string ModuleName { get; }
}
```

Add name to the module and make this class simple singleton. 
We use ExportInfo as an export point for the dependencies. 
We also use such classes to mark components.

```c#
public class ModuleModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.Module";

    private ModuleModule() { }
    
    public static IExportInfo Instance { get; } = new ModuleModule();

    public string ModuleName => Name;
}
```

#### Marking components

Go to the command and page to mark them as part of the Module.

```c#
// Open page command
public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open cats page",
        Description = "Command opens cats page",
        Icon = CatsPageViewModel.PageIcon,
        IconColor = CatsPageViewModel.PageIconColor,
        DefaultHotKey = null,
        Source = ModuleModule.Instance,
    };
```

```c#
/// Page
...
    protected override void AfterLoadExtensions()
    {
        // ignore
    }

    public override IExportInfo Source => ModuleModule.Instance;
}
```

![mvp-module](how-to-create-module-mvp-module.png)

### Adding dependencies to the application

Head to the App.axaml.cs extend container configuration builder:

```c#
 containerCfg
            .WithDependenciesFromSystemModule()
            .WithDependenciesFromTheApp(this)
            .WithAssemblies([typeof(ModuleModule).Assembly]) // Add this line
            .WithDefaultConventions(conventions);
```

Now you can open the app and see our new Page in action:

![page-in-the-tools](how-to-create-module-page-in-the-tools.png)

![page-with-cats-in-demo](how-to-create-module-page-with-cats-simple.png)

The next step is to make the module more flexible.

