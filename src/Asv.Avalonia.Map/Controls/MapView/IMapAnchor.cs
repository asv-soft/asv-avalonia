using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Map;

public interface IMapAnchor : IRoutable
{
    double Azimuth { get; }
    GeoPoint Location { get; }
    MaterialIconKind Icon { get; }
    IBrush Foreground { get; }
    HorizontalOffsetEnum CenterX { get; }
    VerticalOffsetEnum CenterY { get; }
    bool IsReadOnly { get; }
    bool IsSelected { get; }
    bool IsVisible { get; }
}

public class MapAnchor : RoutableViewModel, IMapAnchor
{
    private MaterialIconKind _icon;
    private GeoPoint _location;
    private double _azimuth;
    private IBrush _foreground = Brushes.NavajoWhite;
    private HorizontalOffsetEnum _centerX;
    private VerticalOffsetEnum _centerY;
    private bool _isReadOnly;
    private bool _isSelected;
    private bool _isVisible = true;

    public MapAnchor(string id)
        : base(id) { }

    public double Azimuth
    {
        get => _azimuth;
        set => SetField(ref _azimuth, value);
    }

    public GeoPoint Location
    {
        get => _location;
        set => SetField(ref _location, value);
    }

    public MaterialIconKind Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    public IBrush Foreground
    {
        get => _foreground;
        set => SetField(ref _foreground, value);
    }

    public HorizontalOffsetEnum CenterX
    {
        get => _centerX;
        set => SetField(ref _centerX, value);
    }

    public VerticalOffsetEnum CenterY
    {
        get => _centerY;
        set => SetField(ref _centerY, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetField(ref _isReadOnly, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
