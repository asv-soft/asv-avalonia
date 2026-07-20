# How to create a new module

This tutorial shows how to create an Asv.Avalonia module and load it in a desktop demo application.
By the end of the guide, the module will add a page with a cat picture and an action on the home page that opens it.
You can also check the [complete source code](how-to-create-module-source-code.md).

## Requirements

1. .NET 10 or later
2. An IDE such as [Rider](https://www.jetbrains.com/rider/)
3. A desktop environment

## Before you start

This guide assumes that you already know how to create a .NET project and install a NuGet package.
See [Get Started](project-setup.md) and [What is a Module](what-is-a-module.md) for the initial application setup and module concepts.

## Create the module project

Create a .NET class library named `Asv.Avalonia.Module` that targets .NET 10.

![create-project](how-to-create-module-create-project.png)

Add the current Asv.Avalonia package and enable the image files as Avalonia resources:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Asv.Avalonia" Version="3.0.0-rc.1" />
    </ItemGroup>

    <ItemGroup>
        <AvaloniaResource Include="Assets\cat.jpg" />
    </ItemGroup>

    <ItemGroup>
        <AdditionalFiles Include="Shell\Pages\Cats\CatsPageView.axaml" />
    </ItemGroup>
</Project>
```

The sample project currently uses `Asv.Avalonia` 3.0.0-rc.1. If a newer compatible version is available, keep the module and demo application on the same version.

## Create the demo application

Create an Avalonia desktop application named `Asv.Avalonia.Module.Demo` and add a project reference to the module:

> The demo application is only a host for developing and testing the module. 
> The module remains a separate class library and is not tied to this demo. 
> You can reference it from other compatible Asv.Avalonia applications, either as a project reference or as a NuGet package, and enable it by calling `RegisterModuleModule()` during application setup.
> {style="note"}

```xml
<ItemGroup>
    <ProjectReference Include="..\Asv.Avalonia.Module\Asv.Avalonia.Module.csproj" />
</ItemGroup>
```

Use `AsvApplication` as the application base class:

```c#
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Module.Demo;

public class App : AsvApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
```
{collapsible="true" collapsed-title="App.axaml.cs"}

Configure the desktop shell in `Program.cs`. The module registration will be added after its registration hierarchy is implemented.

```c#
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Module.Demo;

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
            .With(new Win32PlatformOptions { OverlayPopups = true })
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false })
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true })
            .WithInterFont()
            .LogToTrace()
            .UseAsv(builder =>
            {
                builder
                    .RegisterDefault()
                    .RegisterDesktopShell();
            });
}
```
{collapsible="true" collapsed-title="Program.cs"}

## Add the Cats page

Create the following folders and files in the module project:

```text
Asv.Avalonia.Module/
├── Assets/
│   └── cat.jpg
└── Shell/
    └── Pages/
        └── Cats/
            ├── CatsPageView.axaml
            ├── CatsPageView.axaml.cs
            └── CatsPageViewModel.cs
```

Put a cat image at `Assets/cat.jpg`, then add the page view model:

```c#
using Asv.Common;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Module;

public class CatsPageViewModel : PageViewModel<CatsPageViewModel>
{
    public const string PageId = "cats";
    public const MaterialIconKind PageIcon = MaterialIconKind.Cat;
    public const AsvColorKind PageIconColor = AsvColorKind.Info3;

    public CatsPageViewModel()
        : this(
            DesignTime.PageContext,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public CatsPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext)
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Header = "Cats";
        Icon = PageIcon;
        IconColor = PageIconColor;

        var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Module/Assets/cat.jpg"))
            .DisposeItWith(Disposable);
        var defaultPicture = new Bitmap(stream).DisposeItWith(Disposable);

        SelectedImage = defaultPicture;
    }

    public IImage? SelectedImage
    {
        get;
        private init => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // No page extensions need post-load processing.
    }
}
```
{collapsible="true" collapsed-title="CatsPageViewModel.cs"}

The design-time constructor uses `DesignTime.PageContext` and `NullLoggerFactory`. The runtime constructor receives `IPageContext` from a dependency injection and passes it to `PageViewModel`.

Create the view:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Asv.Avalonia.Module"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Module.CatsPageView"
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
{collapsible="true" collapsed-title="CatsPageView.axaml"}

```c#
using Avalonia.Controls;

namespace Asv.Avalonia.Module;

public partial class CatsPageView : UserControl
{
    public CatsPageView()
    {
        InitializeComponent();
    }
}
```
{collapsible="true" collapsed-title="CatsPageView.axaml.cs"}

## Add an action to the home page

Create `HomePageCatsPageExtension.cs` in `Shell/Pages/Cats`. The action navigates directly to the page through `IHomePage`; a separate open-page command is not required.

```c#
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Module;

public class HomePageCatsPageExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.cats";

    public string Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-cats")
        {
            Header = "Open cats page",
            Description = "Opens the cats page",
            Icon = CatsPageViewModel.PageIcon,
            IconColor = CatsPageViewModel.PageIconColor,
            Command = new ReactiveCommand(async (_, _) =>
                await context.GoTo(new NavPath(new NavId(CatsPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
```
{collapsible="true" collapsed-title="HomePageCatsPageExtension.cs"}

## Build the registration hierarchy

The current module pattern uses small registration builders that mirror the feature folders. Each builder exposes `AppBuilder` through `IDependencyBuilder` and owns the default registrations for its scope.

First, register the Cats page and its home page extension in `Shell/Pages/Cats/CatsRegistrations.cs`:

```c#
namespace Asv.Avalonia.Module;

public static class CatsRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterCats()
        {
            builder.AppBuilder.Pages.Register<CatsPageViewModel, CatsPageView>(
                CatsPageViewModel.PageId
            );
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageCatsPageExtension>();
            return builder;
        }
    }
}
```

Create `Shell/Pages/PagesRegistrations.cs`:

```c#
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Module;

public static class PagesRegistrations
{
    extension(ShellRegistrations.Builder builder)
    {
        public Builder Pages => new(builder);

        public ShellRegistrations.Builder RegisterPages(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ShellRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterCats();
            return this;
        }
    }
}
```

Create `Shell/ShellRegistrations.cs`:

```c#
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Module;

public static class ShellRegistrations
{
    extension(ModuleModuleRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public ModuleModuleRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ModuleModuleRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterPages();
            return this;
        }
    }
}
```

Finally, create `ModuleModuleRegistrations.cs` at the project root:

```c#
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Module;

public static class ModuleModuleRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ModuleModule => new(builder);

        public IHostApplicationBuilder RegisterModuleModule(Action<Builder>? configure = null)
        {
            configure ??= module => module.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterShell();
            return this;
        }
    }
}
```

## Load the module

Add `RegisterModuleModule()` after the framework and desktop shell registrations in the demo application's `Program.cs`:

```c#
builder
    .RegisterDefault()
    .RegisterDesktopShell()
    .RegisterModuleModule();
```

Calling `RegisterModuleModule()` without a callback follows the default chain and registers the shell, pages, Cats page, and home page extension.

Run the demo application. The home page tools contain the action that opens the Cats page.

![page-in-the-tools](how-to-create-module-page-in-the-tools.png)

![page-with-cats-in-demo](how-to-create-module-page-with-cats-simple.png)

Next, learn how to [add optional features through the registration hierarchy](how-to-create-module-make-module-really-modular.md).
