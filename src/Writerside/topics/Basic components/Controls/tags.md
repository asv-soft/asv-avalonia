# Tags

## Overview

`Tag` is a compact templated control that displays a short piece of metadata: an optional icon, an optional bold key,
and an optional value. `TagViewModel` is a small view model for building dynamic tag collections that are rendered
with `Tag` controls through an items control.

The component provides:

- a key–value layout with an optional Material icon;
- status classes: `error`, `warning`, `success`, `unknown`;
- an info palette of 20 categorical colors: `info1`–`info20`;
- animation classes: `blink`, `blinkonce`, `fadein`, `fadeout`, `fadeinblink`;
- the `AsvPallete.Color` attached property that maps `AsvColorKind` flags to the classes above.

Each part of the tag is optional. The icon is hidden when `Icon` is `null`, the key and the `:` separator are hidden
when `Key` is `null`, and the value text is hidden when `Value` is `null`. The control height is derived from
`FontSize`, so tags scale together with their text.

## Usage

Add the control directly to XAML:

```xml
<UserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:asv="clr-namespace:Asv.Avalonia;assembly=Asv.Avalonia">

    <StackPanel Orientation="Horizontal" Spacing="4">
        <asv:Tag Key="ip" Value="127.0.0.1" />
        <asv:Tag Value="connected" Icon="Lan" Classes="success" />
        <asv:Tag Value="without icon" Icon="{x:Null}" />
    </StackPanel>
</UserControl>
```

For dynamic collections, fill an `ObservableList<TagViewModel>` and expose a synchronized view:

```C#
public class PortViewModel : ViewModel
{
    private readonly ObservableList<TagViewModel> _tagsSource = [];

    public PortViewModel()
        : base("port")
    {
        _tagsSource.Add(new TagViewModel("ip") { Key = "ip", Value = "127.0.0.1" });
        _tagsSource.Add(new TagViewModel("port") { Key = "port", Value = "7341" });
        _tagsSource.Add(
            new TagViewModel("rx") { Icon = MaterialIconKind.ArrowDownBold, Value = "12kb" }
        );
        _tagsSource.Add(
            new TagViewModel("status") { Value = "connected", Color = AsvColorKind.Success }
        );

        TagsView = _tagsSource.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<TagViewModel> TagsView { get; }
}
```

Render the collection with an items control and bind the view model color through `AsvPallete.Color`:

```xml
<ItemsControl ItemsSource="{Binding TagsView}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel Orientation="Horizontal" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <asv:Tag
                Margin="0,0,4,4"
                Key="{Binding Key}"
                Value="{Binding Value}"
                Icon="{Binding Icon}"
                asv:AsvPallete.Color="{Binding Color}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Styling

Without any classes the tag uses the `info3` colors of the theme palette. Every color class sets a matching
foreground and background brush pair from the theme, and the border always follows the foreground:

- `error`, `warning`, `success`, `unknown` mark a status;
- `info1`–`info20` pick one of 20 categorical colors for grouping and labeling.

Animation classes change how the tag appears:

| Class         | Behavior                                    |
|---------------|---------------------------------------------|
| `blink`       | Blinks indefinitely.                        |
| `blinkonce`   | Blinks three times quickly and stops.       |
| `fadein`      | Fades in from transparent to fully visible. |
| `fadeout`     | Fades out from visible to transparent.      |
| `fadeinblink` | Fades in, then blinks three times softly.   |

> The `Design.PreviewWith` section of
> [Tag.axaml](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/Tags/Tag.axaml)
> shows all tag variants: key and value combinations, every color class, and the animations. Open it in the IDE
> previewer to see the control live.
> {style="note"}

## API {collapsible="true" default-state="collapsed"}

### [Tag](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/Tags/Tag.properties.cs)

A templated control that displays an icon, a key, and a value. Standard `TemplatedControl` properties such as
`FontSize`, `Foreground`, `Background`, `BorderThickness`, and `CornerRadius` apply.

| Property | Type                | Description                                                                                     |
|----------|---------------------|-------------------------------------------------------------------------------------------------|
| `Icon`   | `MaterialIconKind?` | Gets or sets the icon. The default value is `MaterialIconKind.Tag`. `null` hides the icon.      |
| `Key`    | `string?`           | Gets or sets the key text shown in bold before the `:` separator. `null` hides the key part.    |
| `Value`  | `string?`           | Gets or sets the value text. `null` hides the value text.                                       |

### [TagViewModel](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Controls/Tags/TagViewModel.cs)

Provides the data for a single tag in a dynamic collection.

#### `TagViewModel` constructors

| Constructor               | Description                                             |
|---------------------------|---------------------------------------------------------|
| `TagViewModel(string id)` | Initializes a new instance of the `TagViewModel` class. |

| Property | Type                | Description                                                        |
|----------|---------------------|--------------------------------------------------------------------|
| `Color`  | `AsvColorKind`      | Gets or sets the color and animation flags applied to the tag.     |
| `Key`    | `string?`           | Gets or sets the key text.                                         |
| `Value`  | `string?`           | Gets or sets the value text.                                       |
| `Icon`   | `MaterialIconKind?` | Gets or sets the icon.                                             |

### [AsvColorKind](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Styles/Palette/AsvColorKind.cs)

A flags enumeration of visual kinds. Multiple members can be combined:

- **None**: No classes are applied.
- **Status** (`Unknown`, `Error`, `Warning`, `Success`): Status colors of the theme palette.
- **Info** (`Info1`–`Info20`): Categorical colors of the theme palette.
- **Animation** (`Blink`, `BlinkOnce`, `Fadein`, `FadeinBlink`, `Fadeout`): Appearance animations.
- **Size** (`Small`, `Medium`, `Large`): Size classes for controls that define them.

### [AsvPallete](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Styles/Palette/AsvColorKind.cs)

Synchronizes `AsvColorKind` flags with classes on any `StyledElement`.

| Property        | Type                             | Description                                          |
|-----------------|----------------------------------|------------------------------------------------------|
| `ColorProperty` | `AttachedProperty<AsvColorKind>` | Identifies the `AsvPallete.Color` attached property. |

| Method                                                | Return Type    | Description                                 |
|-------------------------------------------------------|----------------|---------------------------------------------|
| `SetColor(AvaloniaObject target, AsvColorKind value)` | `void`         | Sets the color flags on the target element. |
| `GetColor(AvaloniaObject target)`                     | `AsvColorKind` | Gets the color flags of the target element. |

#### `AsvPallete.SetColor`

| Parameter | Type             | Description                              |
|-----------|------------------|------------------------------------------|
| `target`  | `AvaloniaObject` | The element to apply the color flags to. |
| `value`   | `AsvColorKind`   | The color flags to apply.                |

#### `AsvPallete.GetColor`

| Parameter | Type             | Description                               |
|-----------|------------------|-------------------------------------------|
| `target`  | `AvaloniaObject` | The element to read the color flags from. |
