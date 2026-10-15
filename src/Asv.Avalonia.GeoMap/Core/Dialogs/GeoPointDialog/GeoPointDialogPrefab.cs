using Material.Icons;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

/// <summary>
///     Payload for GeoPointDialog prefab.
/// </summary>
public sealed class GeoPointDialogPayload
{
    /// <summary>
    /// Gets or inits the optional icon displayed before the dialog title.
    /// </summary>
    public MaterialIconKind? Icon { get; init; } = MaterialIconKind.MapMarker;

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

    public GeoPoint InitialLocation { get; init; }
}

/// <summary>
///     Dialog for entering users's Geopoint.
/// </summary>
public sealed class GeoPointDialogPrefab(
    ILoggerFactory loggerFactory,
    IUnitService unitService,
    IMapService mapService
) : IDialogPrefab<GeoPointDialogPayload, GeoPoint?>
{
    public async Task<GeoPoint?> ShowDialogAsync(GeoPointDialogPayload dialogPayload)
    {
        using var vm = new GeoPointDialogViewModel(loggerFactory, unitService, mapService);

        vm.ApplyValueToFields(dialogPayload.InitialLocation);

        var dialogContent = new ContentDialog(vm)
        {
            SizeMode = dialogPayload.SizeMode,
            Icon = dialogPayload.Icon,
            IconColor = dialogPayload.IconColor,
            Status = dialogPayload.Status,
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
