# Module example source code

![image](how-to-create-module-final-structure.png)

## AppHost {collapsible="true"}

```c#
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Module;

public static class ModuleModuleMixin
{
    public static IHostApplicationBuilder UseModuleModule(
        this IHostApplicationBuilder builder,
        Action<ModuleModuleOptionsBuilder>? configure = null
    )
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
        return builder;
    }
}
```
{collapsible="true" collapsed-title="ModuleModuleMixin.cs"}

```c#
namespace Asv.Avalonia.Module;

public class ModuleModuleOptions
{
    public const string Section = "GeoMap";
    public required bool IsEnabled { get; set; }
    public required bool IsDogsPageEnabled { get; set; }
    public required bool IsCatsPageEnabled { get; set; }
}
```
{collapsible="true" collapsed-title="ModuleModuleOptions.cs"}

```c#
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Module;

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
}
```
{collapsible="true" collapsed-title="ModuleModuleOptionsBuilder.cs"}

## Commands {collapsible="true"}

```C#
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
        Source = ModuleModule.Instance,
    };

    #endregion
}
```
{collapsible="true" collapsed-title="OpenCatsPageCommand.cs"}

```c#
using System.Composition;

namespace Asv.Avalonia.Module;

[ExportCommand]
[method: ImportingConstructor]
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
        Source = ModuleModule.Instance,
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
{collapsible="true" collapsed-title="CatsPageView.axaml"}

```C#
using Avalonia.Controls;

namespace Asv.Avalonia.Module;

[ExportViewFor<CatsPageViewModel>]
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
using System.Composition;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Avalonia.Platform;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.Module;

[ExportPage(PageId)]
public class CatsPageViewModel : PageViewModel<CatsPageViewModel>
{
    public const string PageId = "cats";
    public const MaterialIconKind PageIcon =  MaterialIconKind.Cat;
    public const AsvColorKind PageIconColor = AsvColorKind.Info3;

    public CatsPageViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }
    
    [ImportingConstructor]
    public CatsPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) 
        : base(PageId, cmd, loggerFactory, dialogService)
    {
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // ignore
    }

    public override IExportInfo Source => ModuleModule.Instance;
}
```
{collapsible="true" collapsed-title="CatsPageViewModel.cs"}

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
{collapsible="true" collapsed-title="HomePageCatsPageExtension.cs"}

## Dogs page {collapsible="true"}

```XML
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:module="clr-namespace:Asv.Avalonia.Module"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Asv.Avalonia.Module.DogsPageView"
             x:DataType="module:DogsPageViewModel">
    <Design.DataContext>
        <module:DogsPageViewModel/>
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
[ExportViewFor<DogsPageViewModel>]
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
using System.Composition;
using Asv.Common;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Module;

[ExportPage(PageId)]
public class DogsPageViewModel : PageViewModel<DogsPageViewModel>
{
    public const string PageId = "dogs";
    public const MaterialIconKind PageIcon =  MaterialIconKind.Dog;
    public const AsvColorKind PageIconColor = AsvColorKind.Info7;

    public DogsPageViewModel()
        : this(DesignTime.CommandService, DesignTime.LoggerFactory, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }
    
    [ImportingConstructor]
    public DogsPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) 
        : base(PageId, cmd, loggerFactory, dialogService)
    {
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

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // ignore
    }

    public override IExportInfo Source => ModuleModule.Instance;
}
```
{collapsible="true" collapsed-title="DogsPageViewModel.cs"}

```C#
using System.Composition; 
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Module;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
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

## ModuleModule {collapsible="true"}

```c#
using System.Composition.Hosting;
using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Module;

public class ModuleModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.Module";

    private ModuleModule() { }
    
    public static IExportInfo Instance { get; } = new ModuleModule();

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromModuleModule(
        this ContainerConfiguration containerConfiguration
    )
    {
        if (Design.IsDesignMode)
        {
            return containerConfiguration.WithAssemblies([typeof(ModuleModule).Assembly]);
        }
        
        var exceptionTypes = new List<Type>();
        var options = AppHost.Instance.GetService<IOptions<ModuleModuleOptions>>().Value;

        if (!options.IsEnabled)
        {
            // if the module is disabled, we should remove all the dependencies from the export
            exceptionTypes.AddRange(typeof(ModuleModule).Assembly.GetTypes());
        }
        else if (!options.IsDogsPageEnabled)
        {
            // if we disable the dogs page, we should remove all dependencies for this page
            exceptionTypes.AddRange([
                typeof(DogsPageView),
                typeof(DogsPageViewModel),
                typeof(HomePageDogsPageExtension),
                typeof(OpenDogsPageCommand)
            ]);
        }
        else if (!options.IsCatsPageEnabled)
        {
            // if we disable the cats page, we should remove all dependencies for this page
            exceptionTypes.AddRange([
                typeof(CatsPageView),
                typeof(CatsPageViewModel),
                typeof(HomePageCatsPageExtension),
                typeof(OpenCatsPageCommand)
            ]);
        }

        var typesToExport = typeof(ModuleModule).Assembly
            .GetTypes()
            .Except(exceptionTypes);

        return containerConfiguration.WithParts(typesToExport);
    }
}
```
{collapsible="true" collapsed-title="ModuleModule.cs"}