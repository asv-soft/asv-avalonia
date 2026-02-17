using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

/// <summary>
///     Payload for GeoPointDialog prefab.
/// </summary>
public sealed class GeoPointDialogPayload
{
    public GeoPoint InitialLocation { get; init; }
}

/// <summary>
///     Dialog for entering users's Geopoint.
/// </summary>
public sealed class GeoPointDialogPrefab(
    INavigationService nav,
    ILoggerFactory loggerFactory,
    IUnitService unitService
) : IDialogPrefab<GeoPointDialogPayload, GeoPoint?>
{
    public async Task<GeoPoint?> ShowDialogAsync(GeoPointDialogPayload dialogPayload)
    {
        using var vm = new GeoPointDialogViewModel(loggerFactory, unitService);

        vm.SetInitialLocation(dialogPayload.InitialLocation);

        var dialogContent = new ContentDialog(vm, nav)
        {
            Title = RS.GeoPointDialogPrefab_Content_Title,
            PrimaryButtonText = Avalonia.RS.DialogButton_Save,
            SecondaryButtonText = Avalonia.RS.DialogButton_Cancel,
            DefaultButton = ContentDialogButton.Primary,
        };

        vm.ApplyDialog(dialogContent);

        var result = await dialogContent.ShowAsync();

        return result is ContentDialogResult.Primary ? vm.GetResult() : null;
    }
}
