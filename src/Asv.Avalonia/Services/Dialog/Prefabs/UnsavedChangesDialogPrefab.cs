using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for UnsavedChangesDialog prefab.
/// </summary>
public sealed class UnsavedChangesDialogPayload
{
    /// <summary>
    /// Gets or inits the unsaved changes of the app.
    /// </summary>
    public required IEnumerable<Restriction> Restrictions { get; init; }
}

/// <summary>
/// Dialog that shows unsaved changes of the app.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class UnsavedChangesDialogPrefab(INavigationService nav, ILoggerFactory loggerFactory)
    : IDialogPrefab<UnsavedChangesDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(UnsavedChangesDialogPayload dialogPayload)
    {
        using var vm = new DialogItemUnsavedChangesViewModel(loggerFactory)
        {
            Restrictions = dialogPayload.Restrictions,
        };

        var dialogContent = new ContentDialog(vm, nav)
        {
            Title = RS.UnsavedChangesDialogPrefab_Dialog_Title,
            PrimaryButtonText = RS.DialogButton_Yes,
            SecondaryButtonText = RS.DialogButton_No,
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        return result == ContentDialogResult.Primary;
    }
}
