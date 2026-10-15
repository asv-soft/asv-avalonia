using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for ShowSaveCancelDialog prefab.
/// </summary>
public sealed class SaveCancelDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.ContentSave;

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
    /// Gets or inits the message displayed in the dialog.
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Dialog with options to Save, Don't Save, or Cancel.
/// </summary>
public sealed class SaveCancelDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<SaveCancelDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(SaveCancelDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextViewModel(loggerFactory)
        {
            Message = dialogPayload.Message,
        };
        var dialogContent = new ContentDialog(vm)
        {
            SizeMode = dialogPayload.SizeMode,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            Status = dialogPayload.Status,
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Save,
            SecondaryButtonText = RS.DialogButton_DontSave,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result == ContentDialogResult.Primary;
    }
}
