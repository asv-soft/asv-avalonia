using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

/// <summary>
///     Payload for PositionDialog prefab.
/// </summary>
public sealed class PositionDialogPayload
{
    public GeoPoint StartPosition { get; init; }
}

/// <summary>
///     Dialog for entering user's string.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class PositionDialogPrefab(
    INavigationService nav,
    ILoggerFactory loggerFactory,
    IUnitService unitService
) : IDialogPrefab<PositionDialogPayload, GeoPoint?>
{
    public async Task<GeoPoint?> ShowDialogAsync(PositionDialogPayload dialogPayload)
    {
        using var vm = new PositionDialogViewModel(loggerFactory, unitService);

        vm.SetInitialCoordinates(dialogPayload.StartPosition);

        var dialogContent = new ContentDialog(vm, nav)
        {
            Title = RS.PositionDialogPrefab_Content_Title,
            PrimaryButtonText = Avalonia.RS.DialogButton_Save,
            SecondaryButtonText = Avalonia.RS.DialogButton_Cancel,
            DefaultButton = ContentDialogButton.Primary,
        };

        vm.ApplyDialog(dialogContent);

        var result = await dialogContent.ShowAsync();

        return result is ContentDialogResult.Primary ? vm.GetResult() : null;
    }
}
