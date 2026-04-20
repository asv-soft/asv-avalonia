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

We will use a library template here to avoid unnecessary dependencies.

## Installing dependencies

Go to your NuGet manager and install the `Asv.Avalonia` **2.0.0 or later**. This guide targets the new application host and DI container introduced 
in 2.0.0 — earlier versions have a different API and the steps below will not work with them.

## Adding Demo project

> This demo project is only for testing the module.
> { style="info" }

We need to create a simple Avalonia MVVM application to debug our module.
We will skip the details of this project since they are not essential for module creation.
You can create this project yourself with the help of [Get Started](project-setup.md);

We will show only the Program.cs:

```C#
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Samples.CreateModule.Demo;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            AppHost.Instance.StopAsync().GetAwaiter().GetResult();
            Task.Factory.StartNew(AppHost.Instance.Dispose).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            AppHost.HandleApplicationCrash(e);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseAsv(builder =>
            {
                builder
                    .UseDefault()
                    .UseOptionalLogViewer()
                    .UseOptionalSoloRun(opt => opt.WithArgumentForwarding())
                    .UseDesktopShell();
            });
}
```
{collapsible="true" collapsed-title="Program.cs"}

Go to the `.csproj` file and add the following lines:

```xml
<ItemGroup>
    <ProjectReference Include="..\Asv.Avalonia.Samples.CreateModule\Asv.Avalonia.Samples.CreateModule.csproj" />
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

Add a new file with the name `CatsPageViewModel.cs` and class `CatsPageViewModel`:

```
Asv.Avalonia.Samples (solution)
└── Asv.Avalonia.Samples.CreateModule/
├── Assets/
├── Shell/
│   └── Pages/
│       └── Cats/
│           └── CatsPageViewModel.cs
└── Asv.Avalonia.Module.csproj.DotSettings
```

> In this guide we follow the same folder structure used in the main framework:
> - `Shell/Pages/<PageName>/` — ViewModel, View, and home page extension for each page
> - `Core/Commands/<PageName>/` — commands that open or operate on pages
> - `AppHost/` — mixin, options, and options builder for the module
> {style="info"}

Inherit this class from the page view model base and implement required components.

```c#
public class CatsPageViewModel : PageViewModel<CatsPageViewModel>
{
    public const string PageId = "cats";
    
    public CatsPageViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    ｝
        
    public CatsPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory,
        IDialogService dialogService)
        : base(PageId, cmd, loggerFactory, dialogService)
    {
    }
    
    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
    
    protected override void AfterLoadExtensions()
    {
        // ignore
    }
}
```

Download any picture from the internet and put it in the assets folder.
Create a view for the cats page.

```XML
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Asv.Avalonia.Samples.CreateModule"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Samples.CreateModule.CatsPageView"
             x:DataType="local:CatsPageViewModel">
    <Grid>
        <Image Source="avares://Asv.Avalonia.Samples.CreateModule/Assets/cat.jpg"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
    </Grid>
</UserControl>
```

Now we take a picture with cats exactly from the Assets folder and put it in the axaml.
In our plan we described that it would be nice to be able to change the picture.
We are going to use ViewModel as a source for pictures.

Write the following code in the constructor:

```C#
var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Samples.CreateModule/Assets/cat.jpg"))
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
             xmlns:local="clr-namespace:Asv.Avalonia.Samples.CreateModule"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Samples.CreateModule.CatsPageView"
             x:DataType="local:CatsPageViewModel">
    <Design.DataContext>
        <local:CatsPageViewModel/>
    </Design.DataContext>
    <Grid>
        <Image Source="{CompiledBinding SelectedImage}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
    </Grid>
</UserControl>
```

### Registering components

Now you can see the page in the design mode, but if you run the demo app you won't see the button to open the page.
To make the page a part of the application we need to register the components in the DI container.

#### Creating an open page command

Add the following two constants in the CatsPageViewModel right after the PageId constant.

```c#
public const MaterialIconKind PageIcon =  MaterialIconKind.Cat;
public const AsvColorKind PageIconColor = AsvColorKind.Info3;
```

Create `Core/Commands/Cats/` folder path in the `Asv.Avalonia.Samples.CreateModule` project.
In this folder create `OpenCatsPageCommand.cs`.

Add the following code in the file:
```c#
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
    };

    #endregion
}
```

#### Adding button to the home page

In `Shell/Pages/Cats/` create `HomePageCatsPageExtension.cs` with the following content:
```c#
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.CreateModule;

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

### Registering the module in the application

All module components must be explicitly registered in the builder chain.
Create an `AppHost/` folder in the module project and add `ModuleModuleMixin.cs` there:

```c#
public static class ModuleModuleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleModule()
        {
            // Register the page and its view
            builder.Shell.Pages.Register<CatsPageViewModel, CatsPageView>(CatsPageViewModel.PageId);

            // Register the command
            builder.Commands.Register<OpenCatsPageCommand>();

            // Register the home page extension
            builder.Extensions.Register<IHomePage, HomePageCatsPageExtension>();

            return builder;
        }
    }
}
```

Then call it in `Program.cs`:

```c#
builder.UseModuleModule();
```

Now you can open the app and see our new Page in action:

![page-in-the-tools](how-to-create-module-page-in-the-tools.png)

![page-with-cats-in-demo](how-to-create-module-page-with-cats-simple.png)

The next step is to make the module more flexible.
