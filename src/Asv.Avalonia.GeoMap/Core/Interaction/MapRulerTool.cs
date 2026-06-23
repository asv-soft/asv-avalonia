using Asv.Common;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapRulerTool : IDisposable
{
    private const string LineAnchorId = "map-interaction.ruler.line";

    private readonly IMap _map;
    private readonly IUnit _distance;

    private bool _running;

    public MapRulerTool(IMap map, IUnitService unitService)
    {
        _map = map;
        _distance = unitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id);
        Command = new ReactiveCommand(_ => Toggle());
    }

    public ReactiveCommand Command { get; }

    private void Toggle()
    {
        if (_running)
        {
            _map.Interaction.Deactivate();
            return;
        }

        if (!_map.Interaction.TryActivate<PointInputMode>(out var mode, out var scope))
        {
            return;
        }

        _running = true;
        Disposable.Create(() => _running = false).AddTo(scope);

        var line = new MapAnchor(LineAnchorId)
        {
            IsReadOnly = true,
            IsAnnotationVisible = false,
            IconSize = 0,
            IsPolygonClosed = false,
            PolygonPen = new Pen(Brushes.OrangeRed, 3),
        };
        _map.Anchors.Add(line);

        Disposable.Create(() => _map.Anchors.Remove(line)).AddTo(scope);

        GeoPoint? start = null;
        GeoPoint? end = null;

        mode.Clicked.Subscribe(point =>
            {
                if (start is null || end is not null)
                {
                    start = point;
                    end = null;
                }
                else
                {
                    end = point;
                }

                Redraw(line, start, end);
                _map.Interaction.Status.Value = Format(start, end);
            })
            .AddTo(scope);

        mode.CursorMoved.Subscribe(cursor =>
            {
                if (start is null || end is not null)
                {
                    return;
                }

                Redraw(line, start, cursor);
                _map.Interaction.Status.Value = Format(start, cursor);
            })
            .AddTo(scope);

        _map.Interaction.Accent.Value = AsvColorKind.Warning;
        _map.Interaction.Status.Value = "Click points to measure · Esc to finish";
    }

    private static void Redraw(IMapAnchor line, GeoPoint? start, GeoPoint? end)
    {
        line.Polygon.Clear();

        if (start is not null)
        {
            line.Polygon.Add(start.Value);
        }
        if (end is not null)
        {
            line.Polygon.Add(end.Value);
        }
    }

    private string Format(GeoPoint? start, GeoPoint? end)
    {
        if (start is null || end is null)
        {
            return _distance.CurrentUnitItem.Value.PrintFromSiWithUnits(0, "F2");
        }

        return _distance.CurrentUnitItem.Value.PrintFromSiWithUnits(
            GeoMath.Distance(start, end),
            "F2"
        );
    }

    public void Dispose() => Command.Dispose();
}
