using System.Composition;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Payload for YesOrNoDialog prefab.
/// </summary>
public sealed class YesOrNoDialogPayload
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
/// Dialog that shows yes or no options.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class YesOrNoDialogPrefab(INavigationService nav, ILoggerFactory loggerFactory)
    : IDialogPrefab<YesOrNoDialogPayload, bool>
{
    public async Task<bool> ShowDialogAsync(YesOrNoDialogPayload dialogPayload)
    {
        using var vm = new DialogItemTextViewModel(loggerFactory)
        {
            Message = dialogPayload.Message,
        };

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
