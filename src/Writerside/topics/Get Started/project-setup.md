# Project setup

This guide explains how to run a minimal Asv.Avalonia application.
After setup, you can explore the [Test Drive section](pages.md) to learn how to customize application's pages.

This guide assumes that you are already familiar with the basics of Avalonia UI.

## Prerequisites

This project depends on Avalonia UI, so please review
the [Avalonia requirements](https://docs.avaloniaui.net/docs/overview/supported-platforms).

The current version of Asv.Avalonia targets .NET 10.0.
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

> This guide requires **Asv.Avalonia 2.0.0 or later**. Version 2.0.0 introduced the new application host and DI container that all examples below depend on. 
> Earlier versions have a different API and the steps will not work with them.
> {style="warning"}

Now we must modify our template to run the Asv.Avalonia shell.

### 1. Clean up the Template

Since Asv.Avalonia handles the main window logic for us, we do not need the default window files.
So we can delete `MainWindow.axaml` and `MainWindow.axaml.cs`.

### 2. Configure Program.cs

We need to set up the application host. Change your `Program` class to this:

```C#
class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
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

The `Main` method builds and starts the Avalonia application. After the window closes,
it gracefully stops and disposes the `AppHost`. If anything goes wrong, `HandleApplicationCrash` logs the error.

In `BuildAvaloniaApp`, we configure Avalonia and initialize the framework via `.UseAsv(...)`.
This call sets up the application host and internally integrates the R3 reactive framework.
Inside the builder callback you can customize the application: enable or disable features
like the Log Viewer or single-instance mode, and set up the desktop shell.

### 3. Configure App.axaml.cs

Now, let's edit the `App.axaml.cs` code-behind file. Our App class should inherit from `AsvApplication`:

```C#
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Samples.GetStarted;

public class App : AsvApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
```

`AsvApplication` handles dependency injection setup, view locator registration, shell initialization,
and platform detection — so your `App` class only needs to load the XAML.

### 4. Configure Styles

Finally, include the Asv.Avalonia styles in your `App.axaml` file:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Asv.Avalonia.Samples.GetStarted.App"
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
