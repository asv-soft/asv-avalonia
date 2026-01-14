# Customizing page

By the end of this guide, you’ll have a customized `HelloWorldPage`. We will start with a simple reactive example, and
then upgrade it to support Undo/Redo functionality.

You can also take a look at the [final source code](#source-code) if you’d like.

## Adding a Reactive Property

First, update the View Model `HelloWorldPageViewModel.cs`. We need to add a property to hold the text string.

```C#
[ImportingConstructor]
public HelloWorldPageViewModel(
    ICommandService cmd, 
    ILoggerFactory loggerFactory, 
    IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
{
    // Initialize it in the constructor
    Text = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
}

// Add a property
public BindableReactiveProperty<string?> Text { get; }
```

> `BindableReactiveProperty` implements `IDisposable`. It must be disposed when the View Model is destroyed to prevent
> memory leaks.
> We use `.DisposeItWith(Disposable)` to register it with the View Model's composite disposable container.
> This is fine for one-time subscriptions. For dynamic/recreated subscriptions, use SerialDisposable (from R3 package).
> {style="info"}

Next, update the template (`HelloWorldPage.axaml`) to bind to this property:

```xml
<StackPanel Orientation="Vertical" HorizontalAlignment="Center" Margin="10" Spacing="5">
    <TextBlock Text="{Binding Text.Value}"/>
    <TextBox Text="{Binding Text.Value}"/>
</StackPanel>
```

We have created a binding between the UI elements and our property.
If you run the app now and type in the `TextBox`, the `TextBlock` will update instantly to match.

## Adding a Command (Button)

Now let's create a button to reset the text. Add a command property to the View Model:

```C#
[ImportingConstructor]
public HelloWorldPageViewModel(
    ICommandService cmd, 
    ILoggerFactory loggerFactory, 
    IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
{
    // Initialize it in the constructor
    ResetTextCommand = new ReactiveCommand(c =>
    {
        Text.Value = string.Empty;
    }).DisposeItWith(Disposable);
    
    Text = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
}

// A new property
public ReactiveCommand ResetTextCommand { get; }

public BindableReactiveProperty<string?> Text { get; }
```

Finally, add the button to the XAML template:

```xml
<StackPanel Orientation="Vertical" HorizontalAlignment="Center" Margin="10" Spacing="5">
    <TextBlock Text="{Binding Text.Value}">Hello world</TextBlock>
    <StackPanel Orientation="Horizontal" Spacing="5">
        <TextBox Text="{Binding Text.Value}"/>
        <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
    </StackPanel>
</StackPanel>
```

## Running the app

You can now run the app and test the new functionality.

![Hello world page with some controls](hw-page-with-controls.png)

We now have a functional app with standard reactive properties from R3.

However, let's make the page more interesting by adding Undo/Redo support.

1. We will make the text input support "Undo/Redo" (historical property).
2. We will change the logic so the text block only updates when we hit "Save", and that "Save" action can also be
   undone/redone.

## Using Historical Properties

First, let's handle the text input. We want to be able to undo typing in the TextBox. To do this, we use a
`HistoricalStringProperty`.

Update your View Model properties:

```C#
// A new field
private readonly ReactiveProperty<string?> _inputText;

[ImportingConstructor]
public HelloWorldPageViewModel(
    ICommandService cmd, 
    ILoggerFactory loggerFactory, 
    IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
{
    // Initialize them in the constructor
    _inputText = new ReactiveProperty<string?>().DisposeItWith(Disposable);
    InputText = new HistoricalStringProperty(nameof(InputText), _inputText, loggerFactory)
        .SetRoutableParent(this)
        .DisposeItWith(Disposable);

    ResetTextCommand = new ReactiveCommand(c =>
    {
        Text.Value = string.Empty;
    }).DisposeItWith(Disposable);
    Text = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
}

// A new property
public HistoricalStringProperty InputText { get; }

public ReactiveCommand ResetTextCommand { get; }
public BindableReactiveProperty<string?> Text { get; }
```

> Note that `SetRoutableParent` is called on the `InputText` property. This is necessary to inform the component who its
> parent is, ensuring correct
> navigation and context handling.
> {style="info"}

We must also expose this property in `GetChildren` so the application knows this property exists for navigation purposes:

```C#
public HistoricalStringProperty InputText { get; }
public ReactiveCommand ResetTextCommand { get; }
public ReactiveCommand SaveTextCommand { get; }

public override IEnumerable<IRoutable> GetChildren()
{
    // Expose the property 
    yield return InputText;
}

protected override void AfterLoadExtensions()
{
}
```

Now, update the XAML. Note that for historical properties, we usually bind to `ViewValue.Value`:

```xml
<StackPanel Orientation="Vertical" HorizontalAlignment="Center" Margin="10" Spacing="5">
    <TextBlock Text="{Binding Text.Value}"/>
    <StackPanel Orientation="Horizontal" Spacing="5">
        <TextBox Text="{Binding InputText.ViewValue.Value}"/>
        <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
    </StackPanel>
</StackPanel>
```

If you run the app now, you can type in the text field and use Ctrl+Z to undo (or Ctrl-Y to redo) your typing changes.

## Making Commands Undoable

Now let's implement the "Save" button. When clicked, it updates the text block. We want to be able to Undo/Redo this "
Save" action.

First, rename our original `Text` property to `SavedText` in the View Model, and add a new command:

```C#
[ImportingConstructor]
public HelloWorldPageViewModel(
    ICommandService cmd, 
    ILoggerFactory loggerFactory, 
    IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
{
    // ...
}

// A renamed property
public BindableReactiveProperty<string?> SavedText { get; }

// A new property
public ReactiveCommand SaveTextCommand { get; }

public HistoricalStringProperty InputText { get; }
public ReactiveCommand ResetTextCommand { get; }
```

### Creating the Command Class

To support Undo/Redo, we need to create a specific command class. Create a new file `ChangeSavedTextPropertyCommand.cs`:

```C#
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace AsvAvaloniaTest;

[ExportCommand]
[Shared]
// This is a context command. It is generic: <ContextType, ArgumentType>
public class ChangeSavedTextPropertyCommand : ContextCommand<HelloWorldPageViewModel, StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.hello_world_page.change";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Change text",
        Description = "Changes text",
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    // This method runs when the command is executed
    public override ValueTask<StringArg?> InternalExecute(
        HelloWorldPageViewModel context,
        // This is the value passed when calling the command
        StringArg newValue,
        CancellationToken cancel
    )
    {
        // 1. Capture the current (old) value
        var oldValue = new StringArg(context.SavedText.Value ?? string.Empty);
        
        // 2. Apply the new value
        context.SavedText.Value = newValue.Value;
        
        // 3. Return the OLD value
        // This returned value is stored and used in undo/redo actions
        return ValueTask.FromResult<StringArg?>(oldValue);
    }
}
```

### Initializing the Commands

Back in `HelloWorldPageViewModel.cs`, initialize the `SavedText` and the commands in the constructor.

Instead of changing the property directly, we now use `this.ExecuteCommand(...)`.
This routes the action through the Undo/Redo system.

```C#
[ImportingConstructor]
public HelloWorldPageViewModel(
    ICommandService cmd, 
    ILoggerFactory loggerFactory, 
    IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
{
    // ...
    
    // Initializing a properties
    
    SavedText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    
    SaveTextCommand = new ReactiveCommand(async (_, cancel) =>
    {
        // Execute the undoable command using the ID we defined earlier
        // We pass the current value of InputText as the argument
        await this.ExecuteCommand(
            ChangeSavedTextPropertyCommand.Id, 
            CommandArg.CreateString(InputText.ModelValue.Value ?? string.Empty),
            cancel);
    }).DisposeItWith(Disposable);
    
    // Update Reset command to also use the undoable system
    ResetTextCommand = new ReactiveCommand(async (_, cancel) =>
    {
        await this.ExecuteCommand(
            ChangeSavedTextPropertyCommand.Id, 
            CommandArg.CreateString(string.Empty),
            cancel);
    }).DisposeItWith(Disposable);
}

public BindableReactiveProperty<string?> SavedText { get; }
public HistoricalStringProperty InputText { get; }

public ReactiveCommand ResetTextCommand { get; }
public ReactiveCommand SaveTextCommand { get; }
```

### Final XAML Update

Finally, update the template to bind to the new properties and add the Save button:

```xml
<StackPanel Orientation="Vertical" HorizontalAlignment="Center" Margin="10" Spacing="5">
    <TextBlock Text="{Binding SavedText.Value}"/>
    <StackPanel Orientation="Horizontal" Spacing="5">
        <TextBox Text="{Binding InputText.ViewValue.Value}"/>
        <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
        <Button Content="Save" Command="{Binding SaveTextCommand}"/>
    </StackPanel>
</StackPanel>
```

## Running the Updated App

You can run the app again.

1. Type something in the text field.
2. Click Save. The TextBlock updates.
3. Click Reset. The TextBlock clears.
4. Press Ctrl+Z (Undo). The TextBlock will revert to the saved text!
5. Press Ctrl+Z again. The TextBlock will revert to the state before saving.
6. Press Ctrl+Y (Redo) to re-apply your changes.

![Hello world page with some controls](hw-page-with-asv-controls.png)

## Whats next?

* Check out the [Asv.Avalonia.Example](https://github.com/asv-soft/asv-avalonia/tree/main/src/Asv.Avalonia.Example) for
  more complex example.
* Explore the rest of the documentation.

## Source code

```C#
using Avalonia;
using System;
using System.IO;
using System.Reflection;
using Asv.Avalonia;
using Avalonia.Controls;

namespace AsvAvaloniaTest;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
    {
        var builder = AppHost.CreateBuilder(args);
        var dataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        builder
            .UseAvalonia(BuildAvaloniaApp)
        
            // This setting defines where all app data (like a JSON user config) will be stored
            .UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))
        
            // Here you can define some JSON config settings. For example, we set autosave to 1 second
            .UseJsonUserConfig(opt => opt.WithAutoSave(TimeSpan.FromSeconds(1)))
        
            // This defines the source of app data (app name, version, etc.). We use the current assembly
            .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
        
            // Here we set up the logging system
            .UseLogging(options =>
            {
                options.WithLogToFile();
                options.WithLogToConsole();
            
                // Optional: here you can enable Log viewer page
                options.WithLogViewer();
            });

        using var host = builder.Build();
        host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseR3();
    }
}
```

{collapsible="true" collapsed-title="Program.cs"}

```C#
using System;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using R3;

namespace AsvAvaloniaTest;

public class App : Application, IContainerHost, IShellHost
{
    private readonly CompositionHost _container;
    private readonly Subject<IShell> _onShellLoaded = new();

    public App()
    {
        var conventions = new ConventionBuilder();
        var containerCfg = new ContainerConfiguration();

        containerCfg
            .WithDependenciesFromSystemModule()
            .WithDependenciesFromTheApp(this)
            .WithDefaultConventions(conventions);

        _container = containerCfg.CreateContainer();

        DataTemplates.Add(new CompositionViewLocator(_container));

        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().AppCtor();
    }

    public T GetExport<T>()
        where T : IExportable
    {
        return _container.GetExport<T>();
    }

    public T GetExport<T>(string contract)
        where T : IExportable
    {
        return _container.GetExport<T>(contract);
    }

    public bool TryGetExport<T>(string id, out T value)
        where T : IExportable
    {
        return _container.TryGetExport(id, out value);
    }

    public void SatisfyImports(object value)
    {
        _container.SatisfyImports(value);
    }

    public IExportInfo Source => SystemModule.Instance;

    public IShell Shell
    {
        get;
        private set
        {
            field = value;
            _onShellLoaded.OnNext(value);
        }
    }

    public Observable<IShell> OnShellLoaded => _onShellLoaded;
    public TopLevel TopLevel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            Shell = DesignTimeShellViewModel.Instance;
        }
        else if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Shell = _container.GetExport<IShell>(DesktopShellViewModel.ShellId);
            if (desktop.MainWindow is TopLevel topLevel) TopLevel = topLevel;
        }
        else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            Shell = _container.GetExport<IShell>(MobileShellViewModel.ShellId);
            if (singleViewPlatform.MainView is TopLevel topLevel) TopLevel = topLevel;
        }
        else
        {
            throw new Exception("Unknown platform");
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
        if (!Design.IsDesignMode) _container.GetExport<IAppStartupService>().OnFrameworkInitializationCompleted();
    }
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromTheApp(
        this ContainerConfiguration containerConfiguration,
        App app
    )
    {
        containerConfiguration.WithExport<IDataTemplateHost>(app).WithExport<IShellHost>(app);

        if (Design.IsDesignMode)
            containerConfiguration.WithExport(NullContainerHost.Instance);
        else
            containerConfiguration.WithExport<IContainerHost>(app);

        return containerConfiguration.WithAssemblies([app.GetType().Assembly]);
    }
}
```

{collapsible="true" collapsed-title="App.axaml.cs"}

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="AsvAvaloniaTest.App"
             RequestedThemeVariant="Default">
    <Application.Styles>
        <StyleInclude Source="avares://Asv.Avalonia/Styling/Theme.axaml" />
    </Application.Styles>
</Application>
```

{collapsible="true" collapsed-title="App.axaml"}

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:asvAvaloniaTest="clr-namespace:AsvAvaloniaTest"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="AsvAvaloniaTest.HelloWorldPage"
             x:DataType="asvAvaloniaTest:HelloWorldPageViewModel">
    <StackPanel Orientation="Vertical" HorizontalAlignment="Center" Margin="10" Spacing="5">
        <TextBlock Text="{Binding SavedText.Value}"/>
        <StackPanel Orientation="Horizontal" Spacing="5">
            <TextBox Text="{Binding InputText.ViewValue.Value}"/>
            <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
            <Button Content="Save" Command="{Binding SaveTextCommand}"/>
        </StackPanel>
    </StackPanel>
</UserControl>

```

{collapsible="true" collapsed-title="HelloWorldPage.axaml"}

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

{collapsible="true" collapsed-title="HelloWorldPage.axaml.cs"}

```C#
using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace AsvAvaloniaTest;

// Export the page so the container can find it
// The View Model must implement a basic page class (e.g., PageViewModel or TreePageViewModel)
[ExportPage(PageId)]
public class HelloWorldPageViewModel: PageViewModel<HelloWorldPageViewModel>
{
    // A unique ID for the page, used for routing
    public const string PageId = "hello_world_page";

    private readonly ReactiveProperty<string?> _inputText;
    
    // You can request dependencies from the MEF container via the constructor
    [ImportingConstructor]
    public HelloWorldPageViewModel(
        ICommandService cmd, 
        ILoggerFactory loggerFactory, 
        IDialogService dialogService) : base(PageId, cmd, loggerFactory, dialogService)
    {
        SavedText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        
        _inputText = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        InputText = new HistoricalStringProperty(nameof(InputText), _inputText, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        
        SaveTextCommand = new ReactiveCommand(async (_, cancel) =>
        {
            // Execute the undoable command using the ID we defined earlier
            // We pass the current value of InputText as the argument
            await this.ExecuteCommand(
                ChangeSavedTextPropertyCommand.Id, 
                CommandArg.CreateString(InputText.ModelValue.Value ?? string.Empty),
                cancel);
        }).DisposeItWith(Disposable);
        
        // Update Reset command to also use the undoable system
        ResetTextCommand = new ReactiveCommand(async (_, cancel) =>
        {
            await this.ExecuteCommand(
                ChangeSavedTextPropertyCommand.Id, 
                CommandArg.CreateString(string.Empty),
                cancel);
        }).DisposeItWith(Disposable);
    }
    
    public BindableReactiveProperty<string?> SavedText { get; }
    public HistoricalStringProperty InputText { get; }
    
    public ReactiveCommand ResetTextCommand { get; }
    public ReactiveCommand SaveTextCommand { get; }

    // -- Required Overrides --

    // If this page contains other routable controls (e.g., a list with custom VMs), return them here
    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return InputText;
    }

    protected override void AfterLoadExtensions()
    {
    }

    public override IExportInfo Source  => SystemModule.Instance;
}
```

{collapsible="true" collapsed-title="HelloWorldPageViewModel.cs"}

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

{collapsible="true" collapsed-title="HomePageHelloWorldPageExtension.cs"}

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

{collapsible="true" collapsed-title="OpenHelloWorldPageCommand.cs"}

```C#
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace AsvAvaloniaTest;

[ExportCommand]
[Shared]
// This is a context command. It is generic: <ContextType, ArgumentType>
public class ChangeSavedTextPropertyCommand : ContextCommand<HelloWorldPageViewModel, StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.hello_world_page.change";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Change text",
        Description = "Changes text",
        Icon = MaterialIconKind.PropertyTag,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    #endregion

    // This method runs when the command is executed
    public override ValueTask<StringArg?> InternalExecute(
        HelloWorldPageViewModel context,
        // This is the value passed when calling the command
        StringArg newValue,
        CancellationToken cancel
    )
    {
        // 1. Capture the current (old) value
        var oldValue = new StringArg(context.SavedText.Value ?? string.Empty);
        
        // 2. Apply the new value
        context.SavedText.Value = newValue.Value;
        
        // 3. Return the OLD value
        // This returned value is stored and used in undo/redo actions
        return ValueTask.FromResult<StringArg?>(oldValue);
    }
}
```

{collapsible="true" collapsed-title="ChangeSavedTextPropertyCommand.cs"}
