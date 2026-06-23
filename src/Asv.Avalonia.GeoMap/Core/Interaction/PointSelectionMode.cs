using Asv.Common;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Parameters for a single point pick.
/// </summary>
public sealed record PointSelectionRequest
{
    public GeoPoint Initial { get; init; } = GeoPoint.Zero;

    public string? Prompt { get; init; }

    public MaterialIconKind Icon { get; init; } = MaterialIconKind.MapMarker;

    public AsvColorKind IconColor { get; init; } = AsvColorKind.Info5;
}

/// <summary>
/// Picks a single point on the map: a preview marker follows the cursor, left click commits, right
/// click / Esc cancels. The result is awaited via <see cref="Completion"/>.
/// </summary>
public sealed class PointSelectionMode(PointSelectionRequest request) : IMapInteractionMode
{
    public const string ModeId = "map-interaction.point-selection";

    private readonly TaskCompletionSource<GeoPoint?> _completion = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    private IMapInteractionContext? _context;
    private IMapAnchor? _preview;

    public string Id => ModeId;

    public string? StatusText => request.Prompt;

    public Task<GeoPoint?> Completion => _completion.Task;

    public void OnActivated(IMapInteractionContext context)
    {
        _context = context;
        _preview = new MapAnchor(ModeId + ".preview")
        {
            Header = request.Prompt ?? string.Empty,
            Location = request.Initial,
            Icon = request.Icon,
            IconColor = request.IconColor,
            IconSize = 32,
            IsReadOnly = true,
            IsAnnotationVisible = !string.IsNullOrEmpty(request.Prompt),
        };
        context.Anchors.Add(_preview);
    }

    public void OnCursorMoved(GeoPoint cursor)
    {
        _preview?.Location = cursor;
    }

    public void OnMapClick(GeoPoint point, MouseButton button, KeyModifiers modifiers)
    {
        if (button != MouseButton.Left)
        {
            return;
        }

        _completion.TrySetResult(point);
        _context?.RequestExit();
    }

    public void OnDeactivated()
    {
        if (_preview is not null)
        {
            _context?.Anchors.Remove(_preview);
            _preview = null;
        }

        _completion.TrySetResult(null);
    }
}
