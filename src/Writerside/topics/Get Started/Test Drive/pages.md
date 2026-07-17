# Adding pages

This guide demonstrates how to add a new page to the application.

## Creating a page

In Asv.Avalonia, pages are standard Avalonia UserControls paired with a specific View Model.

### 1. ViewModel for the page

Let's create the view model first. Create a file named `HelloWorldPageViewModel.cs`:

```C#
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Samples.GetStarted;

// The View Model must implement a basic page class (e.g., PageViewModel or TreePageViewModel)
public class HelloWorldPageViewModel: PageViewModel<HelloWorldPageViewModel>
{
    // A unique ID for the page, used for routing
    public const string PageId = "hello_world_page";

    // Dependencies are injected via the constructor from the IServiceCollection container
    public HelloWorldPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext) : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
    }

    // -- Required Overrides --

    // If this page contains other routable controls (e.g., a list with custom VMs), return them here
    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
    }
}
```

### 2. View for the page

Next, create the new UserControl `HelloWorldPage.axaml`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Asv.Avalonia.Samples.GetStarted"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Samples.GetStarted.HelloWorldPage"
             x:DataType="local:HelloWorldPageViewModel">
    Hello world
</UserControl>
```

## Accessing the Page

We have created the page, but how do we navigate to it?
We need to create an Asv.Avalonia Command that opens the page, and then add that command to the tools list on the Home
Page.

### 1. Create the Command

Create a command class, for example, `OpenHelloWorldPageCommand.cs`:

```C#
using Material.Icons;

namespace Asv.Avalonia.Samples.GetStarted;

public class OpenHelloWorldPageCommand(INavigationService nav)
    : OpenPageCommandBase(HelloWorldPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    // An unique id for the command
    public const string Id = $"{BaseId}.open.hello_world_page";

    // You can customize command metadata however you like
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open HelloWorldPage",
        Description = "Opens HelloWorldPage",
        Icon = MaterialIconKind.Abacus, // The icon will be used in the tools list
        IconColor = AsvColorKind.Info20,
        DefaultHotKey = null, // You can assign a hotkey to open this page from anywhere in the app
    };

    #endregion
}
```

### 2. Extending the home page with a new tool

We use an "Extension" to inject our command into the Home Page's tool list. Create `HomePageHelloWorldPageExtension.cs`:

```C#
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.GetStarted;

public class HomePageHelloWorldPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenHelloWorldPageCommand
                .StaticInfo.CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
```

### 3. Register everything in Program.cs

All components (pages, views, commands, extensions) must be registered in the builder chain in `Program.cs`:

```C#
class Program 
{
    // ...
    
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseAsv(builder =>
            {
                builder
                    .UseDefault()
                    .UseOptionalLogViewer()
                    .UseOptionalSoloRun(opt => opt.WithArgumentForwarding())
                    .UseDesktopShell();

                // Register the View and ViewModel
                builder.Shell.Pages.Register<HelloWorldPageViewModel, HelloWorldPage>(HelloWorldPageViewModel.PageId);

                // Register the command
                builder.Commands.Register<OpenHelloWorldPageCommand>();

                // Register the extension that adds the tool to the home page
                builder.Extensions.Register<IHomePage, HomePageHelloWorldPageExtension>();
            });
}
```

## Run the App

You can now run the application. On the Home Page, you will find a new tool button that opens your "Hello World" page.

![Shell page with a new tool screenshot](shell-with-hw-tool.png)

![Hello world page](hw-page.png)

In the [next chapter](customizing-page.md), we will customize this page by adding reactive properties and buttons.