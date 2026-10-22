using System.Globalization;
using Asv.Avalonia.Charts;
using Asv.Common;
using R3;

namespace Asv.Avalonia.IO;

public sealed record PacketChartRequest(PacketSnapshot packet, PacketFieldSnapshot field)
{
    public PacketSnapshot Packet { get; } = packet;
    public PacketFieldSnapshot Field { get; } = field;
}

public sealed class PacketFieldChart : PlotViewModel
{
    public const int MaximumPoints = 2000;
    private readonly int[] _indices;
    private readonly string[] _names;
    private readonly Queue<(double Time, double Value)> _points = new();
    private long _lastSequence = -1;
    private DateTimeOffset? _origin;

    public PacketFieldChart(
        PacketGroupKey key,
        IReadOnlyList<int> indices,
        IReadOnlyList<string> names,
        string units,
        IThemeService theme,
        Action<PacketFieldChart> remove)
        : base($"packet-field-chart-{Guid.NewGuid():N}", theme)
    {
        Key = key;
        _indices = indices.ToArray();
        _names = names.ToArray();
        Units = units;
        Header = string.Join(" / ", _names);
        Description = $"{key.DeviceId?.AsString() ?? RS.PacketInspector_Unidentified} · {key.Direction} · {key.PortId} / {key.EndpointId}";
        RemoveCommand = new ReactiveCommand(_ => remove(this)).DisposeItWith(Disposable);
    }

    public PacketGroupKey Key { get; }
    public string Header { get; }
    public string Description { get; }
    public string Units { get; }
    public ReactiveCommand RemoveCommand { get; }
    public int PointCount => _points.Count;
    public double? LatestValue => _points.Count > 0 ? _points.Last().Value : null;

    public bool Matches(PacketGroupKey key, IReadOnlyList<int> indices, IReadOnlyList<string> names) =>
        Key == key && _indices.SequenceEqual(indices) && _names.SequenceEqual(names);

    public void Append(IEnumerable<PacketSnapshot> packets)
    {
        var changed = false;
        foreach (var packet in packets)
        {
            if (packet.Sequence <= _lastSequence || packet.Key != Key)
            {
                continue;
            }
            _lastSequence = packet.Sequence;
            IReadOnlyList<PacketFieldSnapshot> fields = packet.Fields;
            PacketFieldSnapshot? field = null;
            for (var depth = 0; depth < _indices.Length; depth++)
            {
                var index = _indices[depth];
                if (index >= fields.Count || fields[index].Name != _names[depth])
                {
                    field = null;
                    break;
                }
                field = fields[index];
                fields = field.Children;
            }
            if (field == null || !TryGetNumber(field, out var value))
            {
                continue;
            }
            _origin ??= packet.Timestamp;
            _points.Enqueue(((packet.Timestamp - _origin.Value).TotalSeconds, value));
            while (_points.Count > MaximumPoints)
            {
                _points.Dequeue();
            }
            changed = true;
        }
        if (changed)
        {
            Refresh();
        }
    }

    public void Clear()
    {
        _points.Clear();
        _lastSequence = -1;
        _origin = null;
        Refresh();
    }

    public static bool TryGetNumber(PacketFieldSnapshot field, out double value)
    {
        value = default;
        var type = field.Type.TrimEnd('?').ToLowerInvariant();
        if (type is not ("int8" or "int16" or "int32" or "int64" or "uint8" or "uint16" or "uint32" or "uint64"
            or "half" or "float" or "double"))
        {
            return false;
        }
        if (field.NumericValue is { } number)
        {
            value = number;
            return double.IsFinite(value);
        }
        return double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && double.IsFinite(value);
    }

    protected override void RefreshStyles(ScottPlot.Plot plot)
    {
        base.RefreshStyles(plot);
        plot.FigureBackground.Color = ScottPlot.Color.FromARGB(0U);
        plot.DataBackground.Color = ScottPlot.Color.FromARGB(0U);
    }

    protected override void BeginDraw() { }

    protected override void Draw(AvaPlot panel)
    {
        panel.Plot.Clear();
        if (_points.Count == 0)
        {
            return;
        }
        var samples = _points.ToArray();
        var line = panel.Plot.Add.Scatter(samples.Select(x => x.Time).ToArray(), samples.Select(x => x.Value).ToArray());
        line.LineWidth = 2;
        line.MarkerSize = samples.Length == 1 ? 4 : 0;
        panel.Plot.YLabel(Units);
        panel.Plot.Axes.AutoScale();
    }

    protected override void EndDraw() { }
}
