using Asv.Common;
using Avalonia.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed record PointSelectionRequest
{
    public GeoPoint Initial { get; init; } = GeoPoint.Zero;

    public string? Prompt { get; init; }

    public MaterialIconKind Icon { get; init; } = MaterialIconKind.MapMarker;

    public AsvColorKind IconColor { get; init; } = AsvColorKind.Info5;
}

public sealed class PointSelectionMode(PointSelectionRequest request)
    : IMapInteractionMode,
        IMapClickHandler,
        ICursorMoveHandler
{
    private const string ModeId = "map-interaction.point-selection";

    private readonly TaskCompletionSource<GeoPoint?> _completion = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );
    private readonly ReactiveProperty<string?> _statusText = new(request.Prompt);

    private IMapInteractionContext? _context;
    private IMapAnchor? _preview;

    public string Title => request.Prompt ?? "Select point";

    public MaterialIconKind Icon => request.Icon;

    public ReadOnlyReactiveProperty<string?> StatusText => _statusText;

    public AsvColorKind Accent => request.IconColor;

    public Task<GeoPoint?> Completion => _completion.Task;

    public void OnActivated(IMapInteractionContext context, CompositeDisposable until)
    {
        _context = context;
        var preview = new MapAnchor(ModeId + ".preview")
        {
            Header = request.Prompt ?? string.Empty,
            Location = request.Initial,
            Icon = request.Icon,
            IconColor = request.IconColor,
            IconSize = 32,
            IsReadOnly = true,
            IsAnnotationVisible = !string.IsNullOrEmpty(request.Prompt),
        };
        _preview = preview;
        context.Anchors.Add(preview);

        Disposable
            .Create(() =>
            {
                context.Anchors.Remove(preview);
                _preview = null;
                _context = null;
                _completion.TrySetResult(null);
            })
            .AddTo(until);
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

    public void Dispose() => _statusText.Dispose();
}
