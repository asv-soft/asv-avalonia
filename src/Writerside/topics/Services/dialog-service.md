# Dialog Service

<img src="guide-dialog-expanded.png" dark-src="guide-dialog-expanded.png" alt="Expanded Markdown details dialog in the running example application" thumbnail="true"/>

Open Home → Controls gallery → Dialogs, select Expanded, then open MarkdownDetailsDialog. The title and action buttons remain outside the scrolling body.

`IDialogService` provides access to registered dialog prefabs. Use the extension methods in
`Asv.Avalonia` to open standard dialogs with ordinary arguments instead of constructing a payload.
Call them from the UI thread, for example from a view model command, and await the result.

## Confirmation

```C#
using Asv.Avalonia;
using Material.Icons;

bool? confirmed = await dialogService.ShowYesOrNoDialogAsync(
    title: "Delete item",
    message: "Delete the selected item?",
    icon: MaterialIconKind.Delete,
    status: ShellErrorState.Warning,
    iconColor: AsvColorKind.Warning
);

if (confirmed == true)
{
    // Apply the confirmed action.
}
```

The result is `true` for Yes, `false` for No, and `null` when dismissed without a choice.
Check `== true` before applying an action that requires confirmation.

## Standard dialogs

All methods below are extensions of `IDialogService`. Return types include the asynchronous task.

| Method | Required arguments | Result |
|---|---|---|
| `ShowYesOrNoDialogAsync` | `title`, `message` | `Task<bool?>`: Yes, No, or dismissal |
| `ShowSaveCancelDialogAsync` | `title`, `message` | `Task<bool>`: `true` only for Save |
| `ShowInputDialogAsync` | `title`, `message` | `Task<string?>`: entered text, or `null` |
| `ShowHotKeyCaptureDialogAsync` | `title`, `message` | `Task<KeyGesture?>`: captured shortcut, or `null` |
| `ShowMarkdownDetailsDialogAsync` | `title`, `markdownText`, `originalText` | `Task<bool>`: `true` when Copy details is chosen |
| `ShowUnsavedChangesDialogAsync` | `title`, `restrictions` | `Task<bool>`: `true` for Yes |
| `ShowOpenFileDialogAsync` | `title` | `Task<string?>`: selected file path, or `null` |
| `ShowSaveFileDialogAsync` | `title` | `Task<string?>`: selected destination path, or `null` |
| `ShowSelectFolderDialogAsync` | `title` | `Task<string?>`: selected folder path, or `null` |
| `ShowObserveFolderDialogAsync` | `title`, `defaultPath` | `Task<Unit>`: launches the system file manager |

`ShowSaveCancelDialogAsync` currently combines Don't Save and dismissal into `false`.
`ShowUnsavedChangesDialogAsync` also returns `false` for both No and dismissal.
The file picker methods choose paths; they do not read or write the selected files.

Optional arguments specific to each dialog:

- HotKeyCapture: `currentHotKey`.
- OpenFile: `typeFilter`, `initialDirectory`.
- SaveFile: `defaultExt`, `suggestedFileName`, `typeFilter`, `initialDirectory`.
- SelectFolder: `oldPath`.

```C#
var text = await dialogService.ShowInputDialogAsync("Name", "Enter a name:");
var shortcut = await dialogService.ShowHotKeyCaptureDialogAsync("Shortcut", "Press a key combination");
var file = await dialogService.ShowOpenFileDialogAsync("Open file", typeFilter: "txt,md");
var destination = await dialogService.ShowSaveFileDialogAsync(
    "Save file", defaultExt: "txt", suggestedFileName: "notes.txt"
);
var folder = await dialogService.ShowSelectFolderDialogAsync("Select folder");
await dialogService.ShowObserveFolderDialogAsync("Open folder", defaultPath: folder);
```

## Icon, frame color, and size

The six standard prefabs that create `ContentDialog`, and both GeoMap prefabs, accept the following
optional arguments. Their payloads expose the matching properties.

| Argument / property | Type | Behavior |
|---|---|---|
| `icon` / `Icon` | `MaterialIconKind?` | Icon to the left of the title; `null` removes the icon and its spacing |
| `iconColor` / `IconColor` | `AsvColorKind` | Asv palette color; defaults to `None`, using the standard theme color |
| `status` / `Status` | `ShellErrorState` | Frame color, independent of the icon color |
| `sizeMode` / `SizeMode` | `DialogSizeMode` | Dialog sizing; defaults to `Compact` |

The frame uses `SystemAccentColorDark3` in the normal state, `AsvForegroundWarningBrush` for
`Warning`, and `AsvForegroundErrorBrush` for `Error`. `Success` currently uses the normal frame color.
The title and button panel remain visible while the content scrolls. The close button at the top
right dismisses the dialog without a choice, like Escape; for YesOrNo this returns `null`.

| Prefab | Default icon | Default status |
|---|---|---|
| YesOrNo | `HelpCircle` | `Normal` |
| SaveCancel | `ContentSave` | `Warning` |
| Input | `FormTextbox` | `Normal` |
| HotKeyCapture | `Keyboard` | `Normal` |
| MarkdownDetails | `FileDocumentOutline` | `Normal` |
| UnsavedChanges | `ContentSaveAlert` | `Warning` |
| EditApiKey (GeoMap) | `Key` | `Normal` |
| GeoPoint (GeoMap) | `MapMarker` | `Normal` |

A directly constructed `ContentDialog` defaults to `Icon = null`, `IconColor = None`,
`Status = Normal`, and `SizeMode = Compact`. Prefabs supply the defaults listed above.
System file and folder pickers use operating-system styling and do not accept these appearance arguments.

### Size modes

| Mode | Width | Height |
|---|---|---|
| `Compact` | Content-sized, limited to 320–548 | Content-sized, limited to 184–756 |
| `Wide` | Fills the available width | Content-sized, limited to 756 |
| `Expanded` | Fills the available width | Fills the available height |

Dimensions are Avalonia logical units. `Compact` preserves the existing outer margin of 100.
`Wide` and `Expanded` leave a margin of 60 on every side and follow window resizing.
In `Wide` and `Expanded`, the action buttons are grouped at the right edge and share the width required by the longest label.
`Compact` retains its existing stretched button layout.
The dimensions and margins are supplied by the dialog theme resources.
The older `FullSizeDesired` property remains available; it stretches vertically within the existing
limits. Prefer `SizeMode` for new callers.

## Markdown details

```C#
var markdown = """
    # Summary

    **Important:** review the changes before continuing.

    - [icon=CheckCircle;color=Success;] Validation completed.
    - [color=Warning;]A confirmation is required.[/color]

    ## Next steps

    1. Review the details.
    2. Copy the original text if needed.
    """;

await dialogService.ShowMarkdownDetailsDialogAsync(
    title: "Details",
    markdownText: markdown,
    originalText: markdown,
    iconColor: AsvColorKind.Info5,
    sizeMode: DialogSizeMode.Expanded
);
```

`markdownText` is rendered by the native `MarkdownViewer`; `originalText` is copied when the user
chooses Copy details. Supported formatting includes headings of levels 1–3, paragraphs, flat bullet
and numbered lists, bold text, Material icon tokens, Asv color spans, and escaped formatting markers.
This viewer does not support general HTML, tables, links, images, or fenced code blocks.

## GeoMap dialogs

Add `using Asv.Avalonia.GeoMap;` to access the GeoMap extensions. The corresponding module must
register its dialog prefabs.

```C#
var point = await dialogService.ShowGeoPointDialogAsync(initialLocation: currentPoint);
var apiKey = await dialogService.ShowEditApiKeyDialogAsync(currentApiKey: currentKey);
```

GeoPoint returns `GeoPoint?`, with `null` on cancellation. EditApiKey returns `string?`; cancelling
preserves the supplied key, while accepting an empty key returns `null`.

## Registration and custom prefabs

`RegisterDialogs()` registers the standard prefabs when no configuration callback is supplied.
A custom callback replaces that default registration, so call `RegisterDefault()` explicitly when
adding your own prefab alongside the standard ones:

```C#
builder.Core.Services.RegisterDialogs(dialogs => dialogs
    .RegisterDefault()
    .RegisterPrefab<MyDialogPrefab>());
```

Extensions obtain the registered prefab and construct its payload internally. `GetDialogPrefab<T>()`
and `TryGetDialogPrefab<T>()` remain available for custom prefabs and optional registrations.
The existing `YesOrNoDialogPrefab.ShowDialogAsync(payload)` returns `Task<bool>` for compatibility;
use `ShowDialogResultAsync(payload)` or the service extension to distinguish dismissal from No.

See [Dialogs and Notifications](recipe-book-app-dialogs-and-notifications.md) for a custom
`DialogViewModelBase` and prefab implementation. The example application's dialog gallery demonstrates
all ten standard dialogs and both GeoMap dialogs. Its SizeMode selector applies `Compact`, `Wide`, or
`Expanded` to the custom dialog and all ContentDialog prefab examples, including Markdown details.
The gallery initially selects `Compact`; system file and folder dialogs keep their native sizing.
