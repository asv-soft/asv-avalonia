using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Provides shortcuts for opening registered dialogs without constructing payloads.
/// </summary>
public static class GeoMapDialogServiceExtensions
{
    /// <summary>
    /// Opens the EditApiKey dialog with the supplied values and appearance.
    /// </summary>
    public static Task<string?> ShowEditApiKeyDialogAsync(
        this IDialogService service,
        string? currentApiKey,
        MaterialIconKind? icon = MaterialIconKind.Key,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<EditApiKeyDialogPrefab>().ShowDialogAsync(
            new EditApiKeyDialogPayload
            {
                CurrentApiKey = currentApiKey,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }

    /// <summary>
    /// Opens the GeoPoint dialog with the supplied values and appearance.
    /// </summary>
    public static Task<GeoPoint?> ShowGeoPointDialogAsync(
        this IDialogService service,
        GeoPoint initialLocation,
        MaterialIconKind? icon = MaterialIconKind.MapMarker,
        ShellErrorState status = ShellErrorState.Normal,
        AsvColorKind iconColor = AsvColorKind.None,
        DialogSizeMode sizeMode = DialogSizeMode.Compact
    )
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetDialogPrefab<GeoPointDialogPrefab>().ShowDialogAsync(
            new GeoPointDialogPayload
            {
                InitialLocation = initialLocation,
                SizeMode = sizeMode,
                Icon = icon,
                IconColor = iconColor,
                Status = status,
            }
        );
    }
}
