using System.Composition;

namespace Asv.Avalonia;

/// <summary>
/// Payload for ShowInputDialog prefab.
/// </summary>
public sealed class InputDialogPayload
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
/// Dialog for entering user's string.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class InputDialogPrefab(INavigationService nav)
    : IDialogPrefab<InputDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(InputDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextBoxViewModel { Message = dialogPayload.Message };
        var dialogContent = new ContentDialog(vm, nav)
        {
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

        if (vm.Parent is not null)
        {
            await vm.Navigate(vm.Parent.Id);
        }

        return null;
    }
}
