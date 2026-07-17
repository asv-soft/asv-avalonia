# Make Module truly modular

The key feature of modules is the ability to be disabled entirely or partially.
Currently, the module cannot do this, so we need to extend its capabilities.

## Adding AppHost extension

Create an AppHost folder in the root directory of Asv.Avalonia.Samples.CreateModule.
Inside this folder, add three files: ModuleModuleMixin, ModuleModuleOptionsBuilder, and ModuleModuleOptions.

### ModuleModuleOptions

In this file, we define the configuration options for the module.
These options will later be used by the builder.

```c#
public class ModuleModuleOptions
{
    public const string Section = "Module"; // Configuration section for the module
    public required bool IsEnabled { get; set; }
}
```

### ModuleModuleOptionsBuilder

Add one base method for the builder.
We will extend this class later, but for now this method is enough.

```c#
internal OptionsBuilder<ModuleModuleOptions> Build(OptionsBuilder<ModuleModuleOptions> options)
{
    return options.Configure(config =>
    {
        config.IsEnabled = true;
    });
}
```

### ModuleModuleMixin

This file extends the builder in Program.cs. We use this method to set up our module.

```c#
public static class ModuleModuleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleModule()
        {
            var options = builder
                .Services.AddOptions<ModuleModuleOptions>()
                .Bind(builder.Configuration.GetSection(ModuleModuleOptions.Section));

            var optionsBuilder = new ModuleModuleOptionsBuilder();
            optionsBuilder.Build(options);
            return builder;
        }
    }
}
```

## Updating the mixin to use options

Now we need to update the module's mixin to conditionally register components based on the options:

```c#
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

            return builder;
        }
    }
}
```

## Adding modularity to the app

Now our module is fully modular, and we need to enable it in the application.

Go to `Program.cs` and add the following line to the builder:

```c#
builder.UseModuleModule(opt => opt.WithCats());
```

Run the application. You should see the Cats page.

Now remove `UseModuleModule` from the builder.
Run the application again. The page should no longer be visible.

If you did everything right, now your module meets our standards for modules.
Next we will show you how to extend your module and add more settings to it so that you will be able to split your module into smaller parts.

## Extending the module

We have already implemented the basic module structure with some simple features.
In this section we are going to extend our module with new features. We will also add new settings to the builder.
This section is useful if you want to add to your module optional features.

### Adding a new page

We want to provide additional customization options for the user.
Let's add the ability to open a page with dogs.

Download a picture with dogs from the internet.

Add a new picture to the assets folder.

![new-pictures](how-to-create-module-dog-picture.png)

Create a new View and ViewModel for the dogs page.
We will skip the details of this step as it is similar to the Cats page implementation.

You can copy the following code to the files:

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
    public const MaterialIconKind PageIcon =  MaterialIconKind.Dog;
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

```C#
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

### Adding options to the builder

Add new options to the ModuleModuleOptions:

```c#
...
    public required bool IsDogsPageEnabled { get; set; }
    public required bool IsCatsPageEnabled { get; set; }
}
```

### Extending ModuleModuleOptionsBuilder

```c#
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

### Changing ModuleModuleMixin to use the new options

We need to change the builder extension to use our new options.
Here we will show you the advanced version of the builder extension that can take default options from the `appsettings.json`.
It also conditionally registers both cats and dogs pages based on the resolved options:

```c#
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
                builder.Services.AddSingleton<IAsyncCommand, OpenCatsPageCommand>();
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

Now you can run the application and see that all pages are disabled.
To enable them, modify the `UseModuleModule` call in `Program.cs` as follows:

```c#
builder.UseModuleModule(opt => opt.WithCats().WithDogs());
```

You should now see both pages.
Each page can be enabled or disabled individually.

![dogs-and-cats-pages](how-to-create-module-dogs-and-cats.png)

The next step is to make a NuGet package of your module.
