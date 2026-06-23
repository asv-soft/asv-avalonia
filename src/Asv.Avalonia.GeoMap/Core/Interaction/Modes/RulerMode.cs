using Asv.Common;
using Avalonia.Input;
using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class RulerMode(IUnit distance)
    : IMapInteractionMode,
        IMapClickHandler,
        ICursorMoveHandler
{
    private const string RulerAnchorId = "map-interaction.ruler.line";

    private readonly List<GeoPoint> _points = [];
    private readonly ReactiveProperty<string?> _statusText = new(
        "Click points to measure · Esc to finish"
    );
    private IMapAnchor? _line;

    public string Title => "Ruler";

    public MaterialIconKind Icon => MaterialIconKind.Ruler;

    public ReadOnlyReactiveProperty<string?> StatusText => _statusText;

    public AsvColorKind Accent => AsvColorKind.Warning;

    public void OnActivated(IMapInteractionContext context, CompositeDisposable until)
    {
        _points.Clear();
        var line = new MapAnchor(RulerAnchorId)
        {
            Header = string.Empty,
            IsReadOnly = true,
            IsAnnotationVisible = false,
            IconSize = 0,
            IsPolygonClosed = false,
            PolygonPen = new Pen(Brushes.OrangeRed, 3),
        };
        _line = line;
        context.Anchors.Add(line);

        Disposable
            .Create(() =>
            {
                context.Anchors.Remove(line);
                _line = null;
                _points.Clear();
            })
            .AddTo(until);
    }

    public void OnMapClick(GeoPoint point, MouseButton button, KeyModifiers modifiers)
    {
        if (button != MouseButton.Left)
        {
            return;
        }

        if (_points.Count >= 2)
        {
            _points.Clear();
        }

        _points.Add(point);
        RebuildLine(null);
        UpdateStatus(null);
    }

    public void OnCursorMoved(GeoPoint cursor)
    {
        if (_points.Count != 1)
        {
            return;
        }

        RebuildLine(cursor);
        UpdateStatus(cursor);
    }

    private void RebuildLine(GeoPoint? head)
    {
        if (_line is null)
        {
            return;
        }

        _line.Polygon.Clear();
        _line.Polygon.AddRange(_points);

        if (head is { } h)
        {
            _line.Polygon.Add(h);
        }
    }

    private void UpdateStatus(GeoPoint? preview)
    {
        var total = 0.0;
        for (var i = 1; i < _points.Count; i++)
        {
            total += GeoMath.Distance(_points[i - 1], _points[i]);
        }

        if (preview is { } p && _points.Count > 0)
        {
            total += GeoMath.Distance(_points[^1], p);
        }

        _statusText.Value = distance.CurrentUnitItem.Value.PrintFromSiWithUnits(total, "F2");
    }

    public void Dispose() => _statusText.Dispose();
}
