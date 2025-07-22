using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

/// <summary>
/// Payload for PositionDialog prefab.
/// </summary>
public sealed class PositionDialogPayload
{
    public double? X { get; init; }
    public double? Y { get; init; }
    public double? Z { get; init; }
}

/// <summary>
/// Dialog for entering user's string.
/// </summary>
[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class PositionDialogPrefab(
    INavigationService nav,
    ILoggerFactory loggerFactory,
    IUnitService unitService
) : IDialogPrefab<PositionDialogPayload, string?>
{
    public async Task<string?> ShowDialogAsync(PositionDialogPayload dialogPayload)
    {
        using var vm = new PositionDialogViewModel(loggerFactory, unitService);

        vm.SetInitialCoordinates(dialogPayload.X, dialogPayload.Y, dialogPayload.Z);

        var dialogContent = new ContentDialog(vm, nav)
        {
            Height = 1200,
            Title = "Set Coordinates",
            PrimaryButtonText = "Apply",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogContent.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var coordinates = vm.GetResult();
            if (coordinates != null)
            {
                return $"{coordinates.X},{coordinates.Y},{coordinates.Z}";
            }
        }

        return null;
    }
}
