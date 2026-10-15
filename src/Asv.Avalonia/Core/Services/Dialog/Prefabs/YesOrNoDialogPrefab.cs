using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for YesOrNoDialog prefab.
/// </summary>
public sealed class YesOrNoDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.HelpCircle;

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
/// Dialog that shows yes or no options.
/// </summary>
public sealed class YesOrNoDialogPrefab(ILoggerFactory loggerFactory)
    : IDialogPrefab<YesOrNoDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(YesOrNoDialogPayload dialogPayload)
    {
        return await ShowDialogResultAsync(dialogPayload) == true;
    }

    /// <summary>
    /// Shows the dialog and returns null when it is dismissed without a choice.
    /// </summary>
    public async Task<bool?> ShowDialogResultAsync(YesOrNoDialogPayload dialogPayload)
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
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result switch
        {
            ContentDialogResult.Primary => true,
            ContentDialogResult.Secondary => false,
            _ => null,
        };
    }
}
