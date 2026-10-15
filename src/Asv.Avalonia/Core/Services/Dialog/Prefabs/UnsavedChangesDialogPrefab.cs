using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for UnsavedChangesDialog prefab.
/// </summary>
public sealed class UnsavedChangesDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.ContentSaveAlert;

    /// <summary>
    /// Gets or inits the palette color of the dialog icon.
    /// </summary>
    public AsvColorKind IconColor { get; init; } = AsvColorKind.None;

    /// <summary>
    /// Gets or inits the dialog sizing mode.
    /// </summary>
    public DialogSizeMode SizeMode { get; init; } = DialogSizeMode.Compact;

    /// <summary>
    /// Gets or inits the dialog status that determines the frame color.
    /// </summary>
    public ShellErrorState Status { get; init; } = ShellErrorState.Warning;

    /// <summary>
    /// Gets or inits the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or inits the unsaved changes of the app.
    /// </summary>
    public required IEnumerable<Restriction> Restrictions { get; init; }
}

/// <summary>
/// Dialog that shows unsaved changes of the app.
/// </summary>
public sealed class UnsavedChangesDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<UnsavedChangesDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(UnsavedChangesDialogPayload dialogPayload)
    {
        using var vm = new DialogItemUnsavedChangesViewModel(loggerFactory)
        {
            Restrictions = dialogPayload.Restrictions,
        };

        var dialogContent = new ContentDialog(vm)
        {
            SizeMode = dialogPayload.SizeMode,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            Status = dialogPayload.Status,
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result == ContentDialogResult.Primary;
    }
}
