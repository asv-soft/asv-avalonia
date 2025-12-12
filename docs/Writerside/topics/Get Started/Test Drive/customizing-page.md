# Customizing page

By the end of this guide, you’ll have a customized `HelloWorldPage`. We will start with a simple reactive example, and
then upgrade it to support Undo/Redo functionality.

## Adding a Reactive Property

First, update the View Model (`HelloWorldPageViewModel.cs`). We need to add a property to hold the text string.

```C#
public BindableReactiveProperty<string?> Text { get; }
```

Initialize it in the constructor:

```C#
Text = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
```

> `BindableReactiveProperty` implements `IDisposable`. It must be disposed when the View Model is destroyed to prevent
> memory leaks.
> We use `.DisposeItWith(Disposable)` to register it with the View Model's composite disposable container.
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
public ReactiveCommand ResetTextCommand { get; }
```

Initialize it in the constructor:

```C#
ResetTextCommand = new ReactiveCommand(c =>
{
    Text.Value = string.Empty;
}).DisposeItWith(Disposable);
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
private readonly ReactiveProperty<string?> _inputText;
// ...
public HistoricalStringProperty InputText { get; }
```

Initialize them in the constructor:

```C#
_inputText = new ReactiveProperty<string?>().DisposeItWith(Disposable);
InputText = new HistoricalStringProperty(nameof(InputText), _inputText, loggerFactory)
    .SetRoutableParent(this)
    .DisposeItWith(Disposable);
```

> Note the call to `SetRoutableParent`. This is necessary to inform the component who its parent is, ensuring correct
> navigation and context handling.
> {style="info"}

We must also expose this property in `GetRoutableChildren` so the application knows this property exists for navigation
purposes:

```C#
public override IEnumerable<IRoutable> GetRoutableChildren()
{
    yield return InputText;
}
```

Now, update the XAML. Note that for historical properties, we usually bind to `ViewValue.Value`:

```C#
<TextBlock Text="{Binding Text.Value}"/>
<StackPanel Orientation="Horizontal" Spacing="5">
    <TextBox Text="{Binding InputText.ViewValue.Value}"/>
    <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
</StackPanel>
```

If you run the app now, you can type in the text field and use Ctrl+Z to undo (or Ctrl-Y to redo) your typing changes.

## Making Commands Undoable

Now let's implement the "Save" button. When clicked, it updates the text block. We want to be able to Undo/Redo this "
Save" action.

First, rename our original `Text` property to `SavedText` in the View Model, and add a new command:

```C#
public BindableReactiveProperty<string?> SavedText { get; }
// ... 
public ReactiveCommand SaveTextCommand { get; }
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
        
        // 3. Return the OLD value. 
        // This returned value is stored and used in undo/redo actions.
        return ValueTask.FromResult<StringArg?>(oldValue);
    }
}
```

### Initializing the Commands

Back in `HelloWorldPageViewModel.cs`, initialize the `SavedText` and the commands in the constructor.

Instead of changing the property directly, we now use `this.ExecuteCommand(...)`.
This routes the action through the Undo/Redo system.

```C#
SavedText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
// ...
SaveTextCommand = new ReactiveCommand(async (_, cancel) =>
{
    // Execute the undoable command using the ID we defined earlier.
    // We pass the current value of InputText as the argument.
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
```

### Final XAML Update

Finally, update the template to bind to the new properties and add the Save button:

```xml
<TextBlock Text="{Binding SavedText.Value}"/>
<StackPanel Orientation="Horizontal" Spacing="5">
    <TextBox Text="{Binding InputText.ViewValue.Value}"/>
    <Button Content="Reset" Command="{Binding ResetTextCommand}"/>
    <Button Content="Save" Command="{Binding SaveTextCommand}"/>
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
