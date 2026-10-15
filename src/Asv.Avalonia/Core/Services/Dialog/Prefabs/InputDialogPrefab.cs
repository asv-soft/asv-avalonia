using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for ShowInputDialog prefab.
/// </summary>
public sealed class InputDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.FormTextbox;

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
    public ShellErrorState Status { get; init; } = ShellErrorState.Normal;

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
/// Dialog for entering user's string.
/// </summary>
public sealed class InputDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<InputDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(InputDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextBoxViewModel(loggerFactory)
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
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return vm.Input.CurrentValue;
        }

        return null;
    }
}
