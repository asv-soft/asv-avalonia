# How to create new module

This tutorial will help you become more familiar with modules in Asv.Avalonia.
At the end of this page we will create a fully functional module that will extend functionality of the base framework.

## Requirements 

1. net 10+
2. IDE ([Rider](https://www.jetbrains.com/rider/) is recommended)
3. Desktop (this module is fully functional only on desktop)

## Before we start

In this guide we will skip parts with the initial setup. 
We recommend you to check **Get Started** and [What is a Module](what-is-a-module.md) pages before this guide.

This guide assumes that you have a basic knowledge about the following topics:
- Some basics about C#
- What is a Module 
- How to install the framework 
- What a nuget is
- How to create a dotnet project

> Note that in this guide we use Rider as an IDE. 
> You may use whatever suits you best insted, but some things may be unclear for you.
> {style="info"}

## Creating a project

Let's create new c# project:

![create-project](how-to-create-module-create-project.png)

![project](how-to-create-module-project.png)

We use a library template here to avoid unnecessary dependencies.

## Installing dependencies

Go to your nugget manager and install the following dependencies:

![dependencies](how-to-create-module-dependencies.png)

You need Asv.Avalonia v1.0.3+. We use a private version of our framework here.

## Adding a project for fast demonstration

We need to create a simple avalonia MVVM application to debug our module.
We will skip details of this project because it is not important for the module creation.
You can create this project by yourself with the help of **Get Started**;

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

Go to the `.csproj` file and add the following lines:

```xml
<ItemGroup>
    <ProjectReference Include="..\Asv.Avalonia.Module\Asv.Avalonia.Module.csproj" />
</ItemGroup>
```

This will enable design time for views in our module, and it will add the module to the application.

## Adding Features

We create modules to extend basic functions of the Asv.Avalonia framework.
That is why we need to think about what our framework is missing.
The first thing that comes to our minds is a video module that enables video transfer through the network.
And the second thing is cats. In my opinion, the second thing is way more important.
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
1. Dialog to add new pictures;
2. Page with dogs (some users may like dogs more than cats);
3. Ability to turn off pages with cats or dogs in the builder;

Now as we've planned everything, let's start building the mvp of our app

### Adding a new page

Add a new file with the name CatsPageViewModel.cs and class CatsPageViewModel:

![cats-page](how-to-create-module-cats-page-initial.png)

> In this guide we will use the same structure that we use in the other projects in our company
> {style="info"}

Inherit this class from the page view model base and implement required components.

![required-components](how-to-create-module-required-components.png)

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
