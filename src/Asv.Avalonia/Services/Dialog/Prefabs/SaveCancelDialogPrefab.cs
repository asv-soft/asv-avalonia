using System.Composition;

namespace Asv.Avalonia;

/// <summary>
/// Payload for ShowSaveCancelDialog prefab.
/// </summary>
public sealed class SaveCancelDialogPayload
{
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
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class SaveCancelDialogPrefab(INavigationService nav)
    : IDialogPrefab<SaveCancelDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(SaveCancelDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextViewModel { Message = dialogPayload.Message };
        var dialogContent = new ContentDialog(vm, nav)
        {
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Save,
            SecondaryButtonText = RS.DialogButton_DontSave,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result == ContentDialogResult.Primary;
    }
}
