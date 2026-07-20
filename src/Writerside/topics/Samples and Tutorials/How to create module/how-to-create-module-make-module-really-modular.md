# Make the module modular

The registration hierarchy created in the previous step makes features composable without introducing a separate options model.
Every `Register...` method accepts an optional callback. 
With no callback, it registers the defaults for that scope; with a callback, the application selects the registrations explicitly.

This section adds a Dogs page and shows how an application can load both pages or only one of them.

## Add the Dogs page

Add `dog.jpg` to the module's `Assets` folder and register it in the project file:

```xml
<ItemGroup>
    <AvaloniaResource Include="Assets\cat.jpg" />
    <AvaloniaResource Include="Assets\dog.jpg" />
</ItemGroup>

<ItemGroup>
    <AdditionalFiles Include="Shell\Pages\Cats\CatsPageView.axaml" />
    <AdditionalFiles Include="Shell\Pages\Dogs\DogsPageView.axaml" />
</ItemGroup>
```

![new-pictures](how-to-create-module-dog-picture.png)

Create `Shell/Pages/Dogs` with the following files.

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
        // No page extensions need post-load processing.
    }
}
```
{collapsible="true" collapsed-title="DogsPageViewModel.cs"}

## Add the Dogs home page action

The Dogs action follows the same direct-navigation pattern as the Cats action:

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

## Register the Dogs feature

Create `DogsRegistrations.cs` in `Shell/Pages/Dogs`:

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

Update `PagesRegistrations.Builder.RegisterDefault()` so the default module contains both pages:

```c#
public Builder RegisterDefault()
{
    this.RegisterDogs();
    this.RegisterCats();
    return this;
}
```

No changes are required in the top-level or shell builders. Their default chain reaches `PagesRegistrations.Builder.RegisterDefault()` automatically.

## Select module features

The shortest application setup registers every default feature:

```c#
builder
    .RegisterDefault()
    .RegisterDesktopShell()
    .RegisterModuleModule();
```

To register only the Cats page, provide callbacks at each scope whose defaults you want to replace:

```c#
builder
    .RegisterDefault()
    .RegisterDesktopShell()
    .RegisterModuleModule(module =>
        module.RegisterShell(shell =>
            shell.RegisterPages(pages => pages.RegisterCats())
        )
    );
```

To register only the Dogs page, replace the final call with `pages.RegisterDogs()`.

Multiple page registrations remain chainable:

```c#
builder.RegisterModuleModule(module =>
    module.RegisterShell(shell =>
        shell.RegisterPages(pages =>
            pages
                .RegisterCats()
                .RegisterDogs()
        )
    )
);
```

If `RegisterModuleModule()` is omitted, none of the module's services, pages, or extensions are registered.

This design keeps registration close to each feature and lets new scopes or features participate in the same hierarchy without a central list of Boolean options.

![dogs-and-cats-pages](how-to-create-module-dogs-and-cats.png)

The next step is to [create a NuGet package](how-to-create-module-make-nuget.md).
