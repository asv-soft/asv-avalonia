using System;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia.Example;

public sealed class MapControlsPageViewConfig
{
    public GeoPoint? RulerStartPoint { get; set; }
    public GeoPoint? RulerStopPoint { get; set; }
}

public partial class MapControlsPageView : UserControl
{
    private readonly IDisposable? _layout;
    private readonly Subject<Unit>? _layoutChanged;
    private MapControlsPageViewConfig? _config = new();

    public MapControlsPageView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        _layoutChanged = new Subject<Unit>();
        _layout = this.RegisterLayout<MapControlsPageViewConfig, Unit>(
            nameof(MapControlsPageView),
            LoadLayout,
            SaveLayout,
            _layoutChanged
        );
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layout?.Dispose();
        _layoutChanged?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnRulerChanged(object? sender, RulerChangedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        UpdateLayout(e);
        _layoutChanged?.OnNext(Unit.Default);
    }

    private void UpdateLayout(RulerChangedEventArgs e)
    {
        if (_config is null || DataContext is null)
        {
            return;
        }

        if (e is { Start: { } start, Stop: { } stop })
        {
            _config.RulerStartPoint = start;
            _config.RulerStopPoint = stop;
        }
        else
        {
            _config.RulerStartPoint = null;
            _config.RulerStopPoint = null;
        }
    }

    private void LoadLayout(MapControlsPageViewConfig config)
    {
        _config = config;

        if (_config is not { RulerStartPoint: { } start, RulerStopPoint: { } stop })
        {
            return;
        }

        PART_Ruler.StartPoint = start;
        PART_Ruler.StopPoint = stop;
    }

    private MapControlsPageViewConfig? SaveLayout()
    {
        return _config;
    }
}
