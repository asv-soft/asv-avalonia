# Module example source code

The completed example contains a module library and a desktop demo application.

```text
Asv.Avalonia.Module/
├── Assets/
│   ├── cat.jpg
│   └── dog.jpg
├── Shell/
│   ├── Pages/
│   │   ├── Cats/
│   │   │   ├── CatsPageView.axaml
│   │   │   ├── CatsPageView.axaml.cs
│   │   │   ├── CatsPageViewModel.cs
│   │   │   ├── CatsRegistrations.cs
│   │   │   └── HomePageCatsPageExtension.cs
│   │   ├── Dogs/
│   │   │   ├── DogsPageView.axaml
│   │   │   ├── DogsPageView.axaml.cs
│   │   │   ├── DogsPageViewModel.cs
│   │   │   ├── DogsRegistrations.cs
│   │   │   └── HomePageDogsPageExtension.cs
│   │   └── PagesRegistrations.cs
│   └── ShellRegistrations.cs
├── Asv.Avalonia.Module.csproj
└── ModuleModuleRegistrations.cs

Asv.Avalonia.Module.Demo/
├── App.axaml
├── App.axaml.cs
├── app.manifest
├── Asv.Avalonia.Module.Demo.csproj
└── Program.cs
```

## Module project

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
        <AvaloniaResource Include="Assets\dog.jpg" />
    </ItemGroup>

    <ItemGroup>
        <AdditionalFiles Include="Shell\Pages\Cats\CatsPageView.axaml" />
        <AdditionalFiles Include="Shell\Pages\Dogs\DogsPageView.axaml" />
    </ItemGroup>
</Project>
```
{collapsible="true" collapsed-title="Asv.Avalonia.Module.csproj"}

## Registration hierarchy

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
{collapsible="true" collapsed-title="ModuleModuleRegistrations.cs"}

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
{collapsible="true" collapsed-title="ShellRegistrations.cs"}

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
            this.RegisterDogs();
            this.RegisterCats();
            return this;
        }
    }
}
```
{collapsible="true" collapsed-title="PagesRegistrations.cs"}

## Cats page

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
{collapsible="true" collapsed-title="CatsRegistrations.cs"}

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
        // ignore
    }
}
```
{collapsible="true" collapsed-title="CatsPageViewModel.cs"}

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

## Dogs page

```c#
namespace Asv.Avalonia.Module;

public static class DogsRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterDogs()
        {
            builder.AppBuilder.Pages.Register<DogsPageViewModel, DogsPageView>(
                DogsPageViewModel.PageId
            );
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageDogsPageExtension>();
            return builder;
        }
    }
}
```
{collapsible="true" collapsed-title="DogsRegistrations.cs"}

```c#
using Asv.Common;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Module;

public class DogsPageViewModel : PageViewModel<DogsPageViewModel>
{
    public const string PageId = "dogs";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dog;
    public const AsvColorKind PageIconColor = AsvColorKind.Info7;

    public DogsPageViewModel()
        : this(
            DesignTime.PageContext,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DogsPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext)
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        Header = "Dogs";
        Icon = PageIcon;
        IconColor = PageIconColor;

        var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Module/Assets/dog.jpg"))
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
        // ignore
    }
}
```
{collapsible="true" collapsed-title="DogsPageViewModel.cs"}

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Asv.Avalonia.Module"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Module.DogsPageView"
             x:DataType="local:DogsPageViewModel">
    <Design.DataContext>
        <local:DogsPageViewModel/>
    </Design.DataContext>
    <Grid>
        <Image Source="{CompiledBinding SelectedImage}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
    </Grid>
</UserControl>
```
{collapsible="true" collapsed-title="DogsPageView.axaml"}

```c#
using Avalonia.Controls;

namespace Asv.Avalonia.Module;

public partial class DogsPageView : UserControl
{
    public DogsPageView()
    {
        InitializeComponent();
    }
}
```
{collapsible="true" collapsed-title="DogsPageView.axaml.cs"}

```c#
using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Module;

public sealed class HomePageDogsPageExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.dogs";

    public string Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-dogs")
        {
            Header = "Open dogs page",
            Description = "Opens the dogs page",
            Icon = DogsPageViewModel.PageIcon,
            IconColor = DogsPageViewModel.PageIconColor,
            Command = new ReactiveCommand(async (_, _) =>
                await context.GoTo(new NavPath(new NavId(DogsPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}
```
{collapsible="true" collapsed-title="HomePageDogsPageExtension.cs"}

## Demo application

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ApplicationManifest>app.manifest</ApplicationManifest>
        <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
        <LangVersion>default</LangVersion>
    </PropertyGroup>

    <ItemGroup>
        <AvaloniaResource Include="Assets\**"/>
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Asv.Avalonia" Version="3.0.0-rc.1" />
        <PackageReference Include="Avalonia.Desktop" Version="12.0.4" />
        <PackageReference Include="Avalonia.Themes.Fluent" Version="12.0.4" />
        <PackageReference Include="Avalonia.Fonts.Inter" Version="12.0.4" />
        <PackageReference Condition="'$(Configuration)' == 'Debug'" Include="AvaloniaUI.DiagnosticsSupport" Version="2.2.1" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\Asv.Avalonia.Module\Asv.Avalonia.Module.csproj" />
    </ItemGroup>
</Project>
```
{collapsible="true" collapsed-title="Asv.Avalonia.Module.Demo.csproj"}

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Asv.Avalonia.Module.Demo.App"
             RequestedThemeVariant="Default">
    <Application.Styles>
        <StyleInclude Source="avares://Asv.Avalonia/Theme.axaml" />
    </Application.Styles>
</Application>
```
{collapsible="true" collapsed-title="App.axaml"}

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
                    .RegisterDesktopShell()
                    .RegisterModuleModule();
            });
}
```
{collapsible="true" collapsed-title="Program.cs"}

The demo project references the module project and uses the same `Asv.Avalonia` version as the module.
