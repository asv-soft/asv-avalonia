# Plugin controls beside Settings

`IShell.HeaderItems` contains view models displayed horizontally beside Settings in insertion order. The shell resolves each model through the registered ViewLocator. This supports custom controls such as user avatars, account menus and connection indicators.

Register the view and shell extension in the plugin's normal registration method:

```csharp
builder.ViewLocator.RegisterViewFor<UserHeaderViewModel, UserHeaderView>();
builder.Extensions.Register<IShell, UserHeaderExtension>();
```

Add the model in `IExtensionFor<IShell>.Extend` and remove it when that extension unloads:

```csharp
public void Extend(IShell shell, CompositeDisposable lifetime)
{
    var item = new UserHeaderViewModel();
    shell.HeaderItems.Add(item);
    Disposable.Create(() => shell.HeaderItems.Remove(item)).DisposeItWith(lifetime);
}
```

The extension uses `Asv.Common` and `R3` for the disposal helpers. The shell owns inserted models: it assigns their parent, includes them in child traversal and disposes them on removal, replacement, clearing or shell shutdown. Mutate the collection on the UI thread and do not reuse a model in another shell. Header controls should stay compact; the tab area reserves their measured width.

All built-in shells support this through `ShellViewModel`. Existing custom implementations of `IShell` remain compatible; to support the new feature they must implement `HeaderItems` and render and own those models. The default interface implementation throws `NotSupportedException`.

See `ExampleUserHeaderExtension`, `ExampleUserHeaderViewModel` and `ExampleUserHeaderView` in the Example application's `Shell/Header` folder for a complete localized avatar and profile-card example. The card uses demo data and does not implement authentication.
