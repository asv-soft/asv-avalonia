# Adding pages

This guide demonstrates how to add a new page to the application.

## Creating a page

In Asv.Avalonia, pages are standard Avalonia UserControls paired with a specific View Model.

### 1. The View Model

Let's create the view model first. Create a file named `HelloWorldPageViewModel.cs`:

```C#
using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace AsvAvaloniaTest;

// Export the page so the container can find it
// The View Model must implement a basic page class (e.g., PageViewModel or TreePageViewModel)
[ExportPage(PageId)]
public class HelloWorldPageViewModel: PageViewModel<HelloWorldPageViewModel>
{
    // A unique ID for the page, used for routing
    public const string PageId = "hello_world_page";
    
    // You can request dependencies from the MEF container via the constructor
    [ImportingConstructor]
    public HelloWorldPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
    {
    }
    
    // -- Required Overrides --
    
    // If this page contains other routable controls (e.g., a list with custom VMs), return them here
    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
    }

    public override IExportInfo Source  => SystemModule.Instance;
}
```

### 2. The View (XAML)

Next, create the template file `HelloWorldPage.axaml`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:asvAvaloniaTest="clr-namespace:AsvAvaloniaTest"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AsvAvaloniaTest.HelloWorldPage"
             x:DataType="asvAvaloniaTest:HelloWorldPageViewModel">
    Hello world
</UserControl>
```

### 3. The Code-Behind

Create the code-behind file `HelloWorldPage.axaml.cs`.

```C#
using Asv.Avalonia;
using Avalonia.Controls;

namespace AsvAvaloniaTest;

// Link this View to our ViewModel
[ExportViewFor(typeof(HelloWorldPageViewModel))]
public partial class HelloWorldPage : UserControl
{
    public HelloWorldPage()
    {
        InitializeComponent();
    }
}
```

## Accessing the Page

We have created the page, but how do we navigate to it?
We need to create an Asv.Avalonia Command that opens the page, and then add that command to the tools list on the Home
Page.

### 1. Create the Command

Create a command class, for example, `OpenHelloWorldPageCommand.cs`:

```C#
using System.Composition;
using Asv.Avalonia;
using Material.Icons;

namespace AsvAvaloniaTest;

[ExportCommand]
[method: ImportingConstructor]
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
        Icon = MaterialIconKind.Abacus,
        IconColor = AsvColorKind.Info20,
        DefaultHotKey = null, // You can assign a hotkey to open this page from anywhere in the app
        Source = SystemModule.Instance,
    };

    #endregion
}
```

### 2. Extending the home page with a new tool

We use an "Extension" to inject our command into the Home Page's tool list. Create `HomePageHelloWorldPageExtension.cs`:

```C#
using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace AsvAvaloniaTest;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
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

## Run the App

You can now run the application. On the Home Page, you will find a new tool button that opens your "Hello World" page.

![Shell page with a new tool screenshot](shell-with-hw-tool.png)

![Hello world page](hw-page.png)

In the [next chapter](customizing-page.md), we will customize this page by adding reactive properties and buttons.