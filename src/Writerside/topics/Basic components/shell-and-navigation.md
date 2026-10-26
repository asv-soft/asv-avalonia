# Shell and navigation

<img src="guide-shell-home.png" dark-src="guide-shell-home.png" alt="Application shell and Home page with example devices" thumbnail="true"/>

The screenshot shows the running example application on macOS with its demo-device browser. Open pages are hosted in tabs; Home entries navigate to registered pages.

The shell hosts pages, navigation, menus, status items, and layout persistence. Use `IShell` for extensions; the default desktop implementation is `DesktopShellViewModel`.

## Register a page

A page is an `IPage` view model paired with an Avalonia control. Derive ordinary pages from `PageViewModel<TContext>` and forward the page ID, `IPageContext`, logger factory, dialog service, and extension service to its constructor. Use `PageViewModel<MyPageViewModel>` when the concrete view model is also its extension context.

Register the pair during host setup, after registering the shell:

```c#
builder.RegisterDefault().RegisterDesktopShell();
builder.Shell.Pages.Register<MyPageViewModel, MyPageView>(MyPageViewModel.PageId);
```

`Register` adds the keyed page factory and the view-locator mapping. The factory supplies `IPageContext`; register other constructor dependencies with the host. A page ID must match the ID used in navigation. See [How to create a new module](how-to-create-module.md) for a complete page, view, and registration hierarchy.

## Navigate with arguments

Navigation uses types from `Asv.Modeling`. From an attached view model or shell, navigate to a page with:

```c#
await context.GoTo(new NavPath(new NavId(MyPageViewModel.PageId)), cancel);
```

Use `NavArgs` for values that identify the page instance:

```c#
var args = new NavArgs(new KeyValuePair<string, string?>("file", filePath));
await context.GoTo(new NavPath(new NavId(MyPageViewModel.PageId, args)), cancel);
```

Read arguments from the constructor's `context.NavArgs`, for example with `FirstOrDefault(x => x.Key == "file").Value`. Validate missing or invalid values in the page. The repository's `TextFilePageViewModel.CreateOpenArgs` illustrates this pattern.

The shell reuses an open page whose complete `NavId` matches the requested ID. Different arguments can therefore create different page instances. Navigation selects the existing instance instead of recreating it. Avoid putting secrets in navigation arguments: IDs are used in navigation and persisted page layout.

## Home page and entry points

`builder.Shell.Pages.SetHomePage(pageId)` selects the fallback page. Register that page as well. After restoring layout, the shell opens the configured home page if no pages remain; it also opens it when the final page closes.

To add a home-page action, implement `IExtensionFor<IHomePage>`, create an `ActionViewModel`, and add it to `context.Tools`. Assign a command that calls `context.GoTo(...)`. Register the extension with:

```c#
builder.Extensions.Register<IHomePage, MyHomeExtension>();
```

To add a shell menu, implement `IExtensionFor<IShell>` and add a `MenuItem` to `LeftMenu`, `RightMenu`, or `MainMenu`. Register it through `builder.Extensions.Register<IShell, MyShellExtension>()`. See `HomePageLogViewerExtension` and `ShellLeftMenuExtenderExample` in the repository for working examples, including command disposal through the extension's `CompositeDisposable`.

`IShellHost.ExecuteNowOrWhenShellLoaded` is available to services that need the shell after initialization. Dispose its returned subscription when the service stops.

## Ownership and persistence

The shell assigns the parent relationship for pages and disposes removed pages. Register page subscriptions and commands in the page's disposal collection so they end with that instance. Do not reuse one page view model across independent visual trees.

The default shell persists opened page IDs and the selected page in its layout store. It creates page-specific undo and layout stores through `IAppPath`. This restores registered pages and saved layout; it does not serialize arbitrary view-model fields. Implement the page's own loading and persistence where required.

## Ask before closing

Call `await page.TryCloseAsync(false, cancel)` for a normal close. It tunnels a `PageCloseAttemptEvent` through the page's children. A handler can add a `Restriction` when work is unsaved. The base page shows the unsaved-changes dialog when restrictions exist and returns without closing when the user declines.

The accepted close raises `PageCloseRequestedEvent`, which bubbles to the shell. The shell removes the page and updates selection. `TryCloseAsync(true, cancel)` skips the approval step; reserve it for a flow that has already made that decision. Calling `RequestClose` directly also bypasses the base page's approval dialog.

For an implementation, inspect `TextFilePageViewModel.OnPageCloseAttempt`: it adds a restriction only when `IsModified` is true. Shell shutdown separately requests approval from its pages before the desktop lifetime shuts down. Adding a restriction requests user confirmation; it does not save the document automatically.
