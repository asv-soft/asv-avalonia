using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Asv.Avalonia.GeoMap;

/// <summary>A round map toolbar button that toggles the polygon altitude layer.</summary>
public sealed partial class MapAltitudeToggle : UserControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty = AvaloniaProperty.Register<
        MapAltitudeToggle,
        bool
    >(nameof(IsChecked), defaultBindingMode: BindingMode.TwoWay);

    public MapAltitudeToggle()
    {
        InitializeComponent();
    }

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string ToolTipText => RS.ResourceManager.GetString("MapView_AltitudeLayer", RS.Culture)!;
}
