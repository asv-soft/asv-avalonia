using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class CursorCoordinatesMode : IMapInteractionMode, ICursorMoveHandler
{
    private readonly ReactiveProperty<string?> _statusText = new("Move the cursor over the map");

    public string Title => "Coordinates";

    public MaterialIconKind Icon => MaterialIconKind.Crosshairs;

    public ReadOnlyReactiveProperty<string?> StatusText => _statusText;

    public AsvColorKind Accent => AsvColorKind.Info5;

    public void OnCursorMoved(GeoPoint cursor) =>
        _statusText.Value = $"{cursor.Latitude:F6}, {cursor.Longitude:F6}";

    public void Dispose() => _statusText.Dispose();
}
