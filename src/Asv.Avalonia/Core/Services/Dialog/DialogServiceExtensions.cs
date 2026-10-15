using Avalonia.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Provides shortcuts for opening registered dialogs without constructing payloads.
/// </summary>
public static class DialogServiceExtensions
{
    /// <summary>
    /// Opens the YesOrNo dialog with the supplied values and appearance.
    /// Returns true for Yes, false for No, and null when dismissed.
    /// </summary>
    public static Task<bool?> ShowYesOrNoDialogAsync(
        this IDialogService service,
        string title,
        string message,
        MaterialIconKind? icon = MaterialIconKind.HelpCircle,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<YesOrNoDialogPrefab>().ShowDialogResultAsync(
            new YesOrNoDialogPayload
            {
                Title = title,
                Message = message,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the SaveCancel dialog with the supplied values and appearance.
    /// </summary>
    public static Task<bool> ShowSaveCancelDialogAsync(
        this IDialogService service,
        string title,
        string message,
        MaterialIconKind? icon = MaterialIconKind.ContentSave,
        ShellErrorState status = ShellErrorState.Warning,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<SaveCancelDialogPrefab>().ShowDialogAsync(
            new SaveCancelDialogPayload
            {
                Title = title,
                Message = message,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the Input dialog with the supplied values and appearance.
    /// </summary>
    public static Task<string?> ShowInputDialogAsync(
        this IDialogService service,
        string title,
        string message,
        MaterialIconKind? icon = MaterialIconKind.FormTextbox,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<InputDialogPrefab>().ShowDialogAsync(
            new InputDialogPayload
            {
                Title = title,
                Message = message,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the HotKeyCapture dialog with the supplied values and appearance.
    /// </summary>
    public static Task<KeyGesture?> ShowHotKeyCaptureDialogAsync(
        this IDialogService service,
        string title,
        string message,
        KeyGesture? currentHotKey = null,
        MaterialIconKind? icon = MaterialIconKind.Keyboard,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<HotKeyCaptureDialogPrefab>().ShowDialogAsync(
            new HotKeyCaptureDialogPayload
            {
                Title = title,
                Message = message,
                CurrentHotKey = currentHotKey,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the MarkdownDetails dialog with the supplied values and appearance.
    /// </summary>
    public static Task<bool> ShowMarkdownDetailsDialogAsync(
        this IDialogService service,
        string title,
        string markdownText,
        string originalText,
        MaterialIconKind? icon = MaterialIconKind.FileDocumentOutline,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<MarkdownDetailsDialogPrefab>().ShowDialogAsync(
            new MarkdownDetailsDialogPayload
            {
                Title = title,
                MarkdownText = markdownText,
                OriginalText = originalText,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the UnsavedChanges dialog with the supplied values and appearance.
    /// </summary>
    public static Task<bool> ShowUnsavedChangesDialogAsync(
        this IDialogService service,
        string title,
        IEnumerable<Restriction> restrictions,
        MaterialIconKind? icon = MaterialIconKind.ContentSaveAlert,
        ShellErrorState status = ShellErrorState.Warning,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<UnsavedChangesDialogPrefab>().ShowDialogAsync(
            new UnsavedChangesDialogPayload
            {
                Title = title,
                Restrictions = restrictions,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the OpenFile dialog with the supplied values.
    /// </summary>
    public static Task<string?> ShowOpenFileDialogAsync(
        this IDialogService service,
        string title,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<OpenFileDialogDesktopPrefab>().ShowDialogAsync(
            new OpenFileDialogPayload
            {
                Title = title,
                TypeFilter = typeFilter,
                InitialDirectory = initialDirectory,
            }
        );
    }

    /// <summary>
    /// Opens the SaveFile dialog with the supplied values.
    /// </summary>
    public static Task<string?> ShowSaveFileDialogAsync(
        this IDialogService service,
        string title,
        string? defaultExt = null,
        string? suggestedFileName = null,
        string? typeFilter = null,
        string? initialDirectory = null
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<SaveFileDialogDesktopPrefab>().ShowDialogAsync(
            new SaveFileDialogPayload
            {
                Title = title,
                DefaultExt = defaultExt,
                SuggestedFileName = suggestedFileName,
                TypeFilter = typeFilter,
                InitialDirectory = initialDirectory,
            }
        );
    }

    /// <summary>
    /// Opens the SelectFolder dialog with the supplied values.
    /// </summary>
    public static Task<string?> ShowSelectFolderDialogAsync(
        this IDialogService service,
        string title,
        string? oldPath = null
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<SelectFolderDialogDesktopPrefab>().ShowDialogAsync(
            new SelectFolderDialogPayload
            {
                Title = title,
                OldPath = oldPath,
            }
        );
    }

    /// <summary>
    /// Opens the ObserveFolder dialog with the supplied values.
    /// </summary>
    public static Task<Unit> ShowObserveFolderDialogAsync(
        this IDialogService service,
        string title,
        string? defaultPath
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<ObserveFolderDialogPrefab>().ShowDialogAsync(
            new ObserveFolderDialogPayload
            {
                Title = title,
                DefaultPath = defaultPath,
            }
        );
    }
}
