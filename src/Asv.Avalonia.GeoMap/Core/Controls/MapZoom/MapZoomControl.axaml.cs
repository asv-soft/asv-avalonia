using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Optional map overlay for changing and displaying the current zoom level.
/// </summary>
public sealed partial class MapZoomControl : UserControl
{
    private const string ZoomInResourceKey = "MapZoomControl_ZoomIn";
    private const string ZoomOutResourceKey = "MapZoomControl_ZoomOut";
    private const string ZoomInControlName = "PART_ZoomInButton";
    private const string ZoomOutControlName = "PART_ZoomOutButton";
    private const string ZoomTextControlName = "PART_ZoomText";
    private bool _isUpdatingZoom;
    private Button? _zoomInButton;
    private Button? _zoomOutButton;
    private TextBlock? _zoomText;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapZoomControl"/> control.
    /// </summary>
    public MapZoomControl()
    {
        InitializeComponent();
        _zoomInButton = this.GetControl<Button>(ZoomInControlName);
        _zoomOutButton = this.GetControl<Button>(ZoomOutControlName);
        _zoomText = this.GetControl<TextBlock>(ZoomTextControlName);
        UpdateZoomState();
    }

    /// <summary>
    /// Gets the localized label for the zoom-in button.
    /// </summary>
    public string ZoomInText =>
        RS.ResourceManager.GetString(ZoomInResourceKey, RS.Culture) ?? "Zoom in";

    /// <summary>
    /// Gets the localized label for the zoom-out button.
    /// </summary>
    public string ZoomOutText =>
        RS.ResourceManager.GetString(ZoomOutResourceKey, RS.Culture) ?? "Zoom out";

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == ZoomProperty
            || change.Property == MinZoomProperty
            || change.Property == MaxZoomProperty
        )
        {
            UpdateZoomState();
        }
    }

    private void OnZoomInClick(object? sender, RoutedEventArgs e)
    {
        ChangeZoom(1);
        e.Handled = true;
    }

    private void OnZoomOutClick(object? sender, RoutedEventArgs e)
    {
        ChangeZoom(-1);
        e.Handled = true;
    }

    private void ChangeZoom(int step)
    {
        var (minimum, maximum) = GetOrderedBounds();
        SetCurrentValue(ZoomProperty, Math.Clamp(Zoom + step, minimum, maximum));
    }

    private void UpdateZoomState()
    {
        if (_isUpdatingZoom)
        {
            return;
        }

        var (minimum, maximum) = GetOrderedBounds();
        var zoom = Math.Clamp(Zoom, minimum, maximum);

        _isUpdatingZoom = true;
        try
        {
            if (zoom != Zoom)
            {
                SetCurrentValue(ZoomProperty, zoom);
            }
        }
        finally
        {
            _isUpdatingZoom = false;
        }

        CanZoomIn = zoom < maximum;
        CanZoomOut = zoom > minimum;

        if (_zoomInButton is not null)
        {
            _zoomInButton.IsEnabled = CanZoomIn;
        }

        if (_zoomOutButton is not null)
        {
            _zoomOutButton.IsEnabled = CanZoomOut;
        }

        if (_zoomText is not null)
        {
            _zoomText.Text = zoom.ToString(CultureInfo.CurrentCulture);
        }
    }

    private (int Minimum, int Maximum) GetOrderedBounds() =>
        (Math.Min(MinZoom, MaxZoom), Math.Max(MinZoom, MaxZoom));
}
