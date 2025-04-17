using System.Composition;

namespace Asv.Avalonia;

/// <summary>
/// Payload for YesOrNoDialog prefab.
/// </summary>
public sealed class YesOrNoDialogPayload
{
    /// <summary>
    /// Gets the title of the dialog.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the message displayed in the dialog.
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Dialog that shows yes or no options.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class YesOrNoDialogPrefab(INavigationService nav)
    : IDialogPrefab<YesOrNoDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(YesOrNoDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextViewModel { Message = dialogPayload.Message };

        var dialogContent = new ContentDialog(vm, nav)
        {
            Title = dialogPayload.Title,
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result == ContentDialogResult.Primary;
    }
}
