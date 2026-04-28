using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example;

public sealed class MapControlsPageViewConfig
{
    public GeoPoint? RulerStartPoint { get; set; }
    public GeoPoint? RulerStopPoint { get; set; }
}

public partial class MapControlsPageView : UserControl
{
    private readonly ILayoutService _layoutService;
    private MapControlsPageViewConfig? _config;

    public MapControlsPageView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapControlsPageView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (Design.IsDesignMode)
        {
            return;
        }

        LoadLayout();
    }

    private void OnRulerChanged(object? sender, RulerChangedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        SaveLayout(e);
    }

    private void SaveLayout(RulerChangedEventArgs e)
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

        _layoutService.SetInMemory(this, _config);
    }

    private void LoadLayout()
    {
        _config = _layoutService.Get<MapControlsPageViewConfig>(this);

        if (_config is not { RulerStartPoint: { } start, RulerStopPoint: { } stop })
        {
            return;
        }

        PART_Ruler.StartPoint = start;
        PART_Ruler.StopPoint = stop;
    }
}
