# Property Editor

<img src="guide-property-editor.png" dark-src="guide-property-editor.png" alt="Compact and extended property editors in the controls gallery" thumbnail="true"/>

Open Home → Controls gallery → Property Editor. The gallery shows independent editor instances over shared demo-model values, including unit selectors, switches, sliders, and the extended layout.

`PropertyEditorViewModel` groups heterogeneous property view models into one form. The compact
`PropertyEditorView` and wide `ExtendedPropertyEditorView` render the same property types. Choose the
wide layout when descriptions and labels need a dedicated row; choose the compact layout for a
sidebar or a dense settings panel.

## Registration and presentation

The default `RegisterCore()` path registers the standard controls. When configuring controls
explicitly, include `RegisterPropertyEditor()` in the controls builder callback:

```c#
using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.RegisterCore(core =>
    core.RegisterControls(controls => controls.RegisterPropertyEditor()));
```

A custom callback replaces the corresponding default registration list. Register other control
families your application uses as well. The property editor registration maps both editor view models
and the built-in text, combo box, toggle group, unit, slider, switch, and button view models to views.
A custom property type needs its own view-locator mapping.

In a view with the framework view locator configured, present the editor as content:

```xml
<ContentControl Content="{Binding Editor}" />
```

## Connect properties to a model

This complete view-model example uses shared reactive model values. Its editor owns the property
view models; the containing view model owns the editor and model subscriptions.

```c#
using System;
using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using R3;

public sealed class ConnectionSettingsViewModel : ViewModel
{
    private readonly BindableReactiveProperty<string?> _name = new("Receiver");
    private readonly BindableReactiveProperty<bool> _enabled = new(true);

    public ConnectionSettingsViewModel() : base("connection-settings")
    {
        _name.DisposeItWith(Disposable);
        _enabled.DisposeItWith(Disposable);
        Editor = new PropertyEditorViewModel("properties");
        Editor.SetParent(this);
        Editor.DisposeItWith(Disposable);
        Editor.ItemsSource.Add(new PropertyTextBoxReactive(
            "name", _name,
            value => string.IsNullOrWhiteSpace(value)
                ? new ArgumentException("Enter a connection name.")
                : null)
        {
            Header = "Name",
            ShortHeader = "Name",
            Description = "The name shown in the connection list."
        });
        Editor.ItemsSource.Add(new PropertyToggleSwitchReactive("enabled", _enabled)
        {
            Header = "Enabled",
            Description = "Allow this connection to be used."
        });
    }

    public PropertyEditorViewModel Editor { get; }
}
```

Replace the constructor with `new ExtendedPropertyEditorViewModel("properties")` to use the wide
layout. Its outer row keeps the property icon while the inner control suppresses its duplicate
prefix through `PropertyEditorDisplayOptions.ShowPrefixIcon`.

## Choose a property adapter

| Type | Use |
|------|-----|
| `PropertyTextBoxReactive` | Edit a `ReactiveProperty<string?>`, with an optional synchronous validator. |
| `PropertyTextBoxCallback` | Read and write text through asynchronous callbacks and receive external updates through an `Observable<string?>`. |
| `PropertyToggleSwitchReactive` | Edit a reactive Boolean. |
| `PropertySliderReactive` | Edit a reactive number with explicit minimum and maximum. |
| `PropertyComboBoxReactive` | Select an `IHeadlinedViewModel` from `ItemsSource`. |
| `PropertyToggleButtonGroupReactive` | Present selection items as a button group. |
| `PropertyUnitReactive` | Edit a numeric model value through an `IUnit`. |
| `PropertyButtonViewModel` | Run an asynchronous action with optional reactive availability. |

`PropertyTextBoxViewModel.Text` is the editable buffer. `ApplyFromUserCommand` submits it;
`CancelCommand` restores the last model text. `IsInEditMode`, `IsSync`, `HasValidationErrors`, and
`ErrorMessage` distinguish editing, synchronization, validation, and operation failures. A callback
adapter's `Refresh(CancellationToken)` performs its read callback; construction does not perform
that read. Supply an update stream for later model changes and perform the initial refresh yourself.

`PropertyUnitReactive(string id, IUnit unit, ReactiveProperty<double> model, ...)` works with the
unit's numeric model representation and validates the SI value. Obtain units from `IUnitService`;
do not implement display conversion by changing the stored model value. See [Unit Service](unit-service.md)
for unit registration and conversion rules.

Text adapters enable text undo by default through `enableUndo`. Unit properties use separate value
and unit undo entries; avoid adding a second text history layer. Attach the editor to the application's
view-model tree so routed operations reach the appropriate controller. See [Undo and Redo](undo-redo.md).

## Filter advanced properties

Configure a property's scopes before adding it. An unscoped property is always included. A scoped
property is included when **any** of its scopes appears in the editor's `DisplayScopes` set.

```c#
var advanced = new PropertyToggleSwitchReactive("diagnostics", diagnosticsModel)
{
    Header = "Diagnostics"
};
advanced.DisplayScopes.Add("advanced");
editor.ItemsSource.Add(advanced);
editor.DisplayScopes.Add("advanced"); // Show advanced rows.
editor.DisplayScopes.Remove("advanced"); // Hide them again.
```

Here `diagnosticsModel` is an existing `ReactiveProperty<bool>` and `editor` is the owning editor.
Bind the view to `Items`, the filtered collection, rather than `ItemsSource`. Changing the editor's
scope set refreshes the filter. The current implementation does not subscribe to each property's
scope collection, so do not rely on changing property scopes after insertion to refresh a row.

## Ownership and practical limits

The editor assigns itself as parent of added properties and disposes removed properties. Disposing
the editor clears and disposes its remaining items. Removing a property is therefore a lifetime
operation, not a way to temporarily hide it; use display scopes for that.

Create separate property view-model instances for each editor, even when several editors share the
same reactive model. Do not attach one property instance to two visual trees. Dispose the shared
model at its actual owner, after its editors no longer use it.

`ShowHeader` controls the editor's label presentation. `Header`, `ShortHeader`, `Description`, `Icon`,
`IconColor`, and `Menu` belong to individual properties. Descriptions use the restricted native
[Markdown and Palette](markdown-and-palette.md) syntax. UI presentation state can be restored through
[Layout Persistence](layout-persistence.md); application settings still require an application store.

The running example's **Controls Gallery → Property Editor** demonstrates compact editors sharing
model values, the extended layout, scopes, units, validation, and property error menus. Consult
`PropertyEditorPageViewModel` for a larger composition and `PropertyEditorViewModelTest` for scope
and ownership examples.
