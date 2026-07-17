# Module example source code

```
Asv.Avalonia.Samples.CreateModule
├── AppHost/
│   ├── ModuleModuleMixin.cs
│   ├── ModuleModuleOptions.cs
│   └── ModuleModuleOptionsBuilder.cs
├── Assets/
│   ├── cat.jpg
│   └── dog.jpg
├── Core/
│   └── Commands/
│       ├── Cats/
│       │   └── OpenCatsPageCommand.cs
│       └── Dogs/
│           └── OpenDogsPageCommand.cs
├── Shell/
│   └── Pages/
│       ├── Cats/
│       │   ├── CatsPageView.axaml
│       │   ├── CatsPageView.axaml.cs
│       │   ├── CatsPageViewModel.cs
│       │   └── HomePageCatsPageExtension.cs
│       └── Dogs/
│           ├── DogsPageView.axaml
│           ├── DogsPageView.axaml.cs
│           ├── DogsPageViewModel.cs
│           └── HomePageDogsPageExtension.cs
```

## AppHost {collapsible="true"}

```c#
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Samples.CreateModule;

public static class ModuleModuleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleModule(
            Action<ModuleModuleOptionsBuilder>? configure = null)
        {
            var options = builder
                .Services.AddOptions<ModuleModuleOptions>()
                .Bind(builder.Configuration.GetSection(ModuleModuleOptions.Section));

            var defaultOptions = builder
                .Configuration.GetSection(ModuleModuleOptions.Section)
                .Get<ModuleModuleOptions>();

            var optionsBuilder = defaultOptions is null
                ? new ModuleModuleOptionsBuilder()
                : new ModuleModuleOptionsBuilder(defaultOptions);

            if (configure is null)
            {
                return builder;
            }

            configure.Invoke(optionsBuilder);
            optionsBuilder.Build(options);

            var resolvedOptions = optionsBuilder.Resolve();

            if (!resolvedOptions.IsEnabled)
            {
                return builder;
            }

            if (resolvedOptions.IsCatsPageEnabled)
            {
                builder.Shell.Pages.Register<CatsPageViewModel, CatsPageView>(CatsPageViewModel.PageId);
                builder.Commands.Register<OpenCatsPageCommand>();
                builder.Extensions.Register<IHomePage, HomePageCatsPageExtension>();
            }

            if (resolvedOptions.IsDogsPageEnabled)
            {
                builder.Shell.Pages.Register<DogsPageViewModel, DogsPageView>(DogsPageViewModel.PageId);
                builder.Commands.Register<OpenDogsPageCommand>();
                builder.Extensions.Register<IHomePage, HomePageDogsPageExtension>();
            }

            return builder;
        }
    }
}
```
{collapsible="true" collapsed-title="ModuleModuleMixin.cs"}

```c#
namespace Asv.Avalonia.Samples.CreateModule;

public class ModuleModuleOptions
{
    public const string Section = "Module";
    public required bool IsEnabled { get; set; }
    public required bool IsDogsPageEnabled { get; set; }
    public required bool IsCatsPageEnabled { get; set; }
}
```
{collapsible="true" collapsed-title="ModuleModuleOptions.cs"}

```c#
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Samples.CreateModule;

public class ModuleModuleOptionsBuilder
{
    private bool _isCatsPageEnabled;
    private bool _isDogsPageEnabled;

    internal ModuleModuleOptionsBuilder() { }

    internal ModuleModuleOptionsBuilder(ModuleModuleOptions defaultOptions)
    {
        _isCatsPageEnabled = defaultOptions.IsCatsPageEnabled;
        _isDogsPageEnabled = defaultOptions.IsDogsPageEnabled;
    }

    public ModuleModuleOptionsBuilder WithCats()
    {
        _isCatsPageEnabled = true;
        return this;
    }

    public ModuleModuleOptionsBuilder WithDogs()
    {
        _isDogsPageEnabled = true;
        return this;
    }

    internal OptionsBuilder<ModuleModuleOptions> Build(OptionsBuilder<ModuleModuleOptions> options)
    {
        return options.Configure(config =>
        {
            config.IsDogsPageEnabled = _isDogsPageEnabled;
            config.IsCatsPageEnabled = _isCatsPageEnabled;
            config.IsEnabled = true;
        });
    }

    internal ModuleModuleOptions Resolve()
    {
        return new ModuleModuleOptions
        {
            IsEnabled = true,
            IsCatsPageEnabled = _isCatsPageEnabled,
            IsDogsPageEnabled = _isDogsPageEnabled,
        };
    }
}
```
{collapsible="true" collapsed-title="ModuleModuleOptionsBuilder.cs"}

## Commands {collapsible="true"}

```C#
namespace Asv.Avalonia.Samples.CreateModule;

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
{collapsible="true" collapsed-title="OpenCatsPageCommand.cs"}

```c#
namespace Asv.Avalonia.Samples.CreateModule;

public class OpenDogsPageCommand(INavigationService nav)
    : OpenPageCommandBase(DogsPageViewModel.PageId, nav)
{
    public override ICommandInfo Info => StaticInfo;

    #region Static

    public const string Id = $"{BaseId}.open.dogs";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open dogs page",
        Description = "Command opens dogs page",
        Icon = DogsPageViewModel.PageIcon,
        IconColor = DogsPageViewModel.PageIconColor,
        DefaultHotKey = null,
    };

    #endregion
}
```
{collapsible="true" collapsed-title="OpenDogsPageCommand.cs"}

## Cats page {collapsible="true"}

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
{collapsible="true" collapsed-title="CatsPageView.axaml"}

```C#
using Avalonia.Controls;

namespace Asv.Avalonia.Samples.CreateModule;

public partial class CatsPageView : UserControl
{
    public CatsPageView()
    {
        InitializeComponent();
    }
}
```
{collapsible="true" collapsed-title="CatsPageView.axaml.cs"}

```C#
using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Avalonia.Platform;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.Samples.CreateModule;

public class CatsPageViewModel : PageViewModel<CatsPageViewModel>
{
    public const string PageId = "cats";
    public const MaterialIconKind PageIcon = MaterialIconKind.Cat;
    public const AsvColorKind PageIconColor = AsvColorKind.Info3;

    public CatsPageViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, DesignTime.DialogService, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public CatsPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext)
        : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
        var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Samples.CreateModule/Assets/cat.jpg"))
            .DisposeItWith(Disposable);
        var defaultPicture = new Bitmap(stream).DisposeItWith(Disposable);

        SelectedImage = defaultPicture;
    }

    public IImage? SelectedImage
    {
        get;
        private init => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetChildren()
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
{collapsible="true" collapsed-title="HomePageCatsPageExtension.cs"}

## Dogs page {collapsible="true"}

```XML
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Asv.Avalonia.Samples.CreateModule"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Samples.CreateModule.DogsPageView"
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

```C#
using Avalonia.Controls;

namespace Asv.Avalonia.Samples.CreateModule;

public partial class DogsPageView : UserControl
{
    public DogsPageView()
    {
        InitializeComponent();
    }
}
```
{collapsible="true" collapsed-title="DogsPageView.axaml.cs"}

```C#
using System;
using System.Collections.Generic;
using Asv.Common;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Samples.CreateModule;

public class DogsPageViewModel : PageViewModel<DogsPageViewModel>
{
    public const string PageId = "dogs";
    public const MaterialIconKind PageIcon = MaterialIconKind.Dog;
    public const AsvColorKind PageIconColor = AsvColorKind.Info7;

    public DogsPageViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, DesignTime.DialogService, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DogsPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext)
        : base(PageId, cmd, loggerFactory, dialogService, ext)
    {
        var stream = AssetLoader
            .Open(new Uri("avares://Asv.Avalonia.Samples.CreateModule/Assets/dog.jpg"))
            .DisposeItWith(Disposable);
        var defaultPicture = new Bitmap(stream).DisposeItWith(Disposable);

        SelectedImage = defaultPicture;
    }

    public IImage? SelectedImage
    {
        get;
        private init => SetField(ref field, value);
    }

    public override IEnumerable<IRoutable> GetChildren()
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

```C#
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Samples.CreateModule;

public sealed class HomePageDogsPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenDogsPageCommand
                .StaticInfo
                .CreateAction(loggerFactory)
                .DisposeItWith(contextDispose)
        );
    }
}
```
{collapsible="true" collapsed-title="HomePageDogsPageExtension.cs"}
