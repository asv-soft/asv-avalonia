using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Displays the ground distance represented by a horizontal span on the map.
/// </summary>
public sealed partial class MapScaleControl : UserControl
{
    private const double DefaultMaxBarWidth = 140.0;
    private static readonly IUnitItem DefaultDistanceUnit = new DistanceMeterUnitItem();
    private IDisposable? _unitSubscription;
    private bool _isAttached;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapScaleControl"/> control.
    /// </summary>
    public MapScaleControl()
    {
        InitializeComponent();
        UpdateScale();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UnitServiceProperty)
        {
            AttachUnitSubscription();
        }

        if (
            change.Property == CenterMapProperty
            || change.Property == ZoomProperty
            || change.Property == ProviderProperty
            || change.Property == MaxBarWidthProperty
            || change.Property == UnitServiceProperty
        )
        {
            UpdateScale();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachUnitSubscription();
        UpdateScale();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        _unitSubscription?.Dispose();
        _unitSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void AttachUnitSubscription()
    {
        _unitSubscription?.Dispose();
        _unitSubscription = null;

        if (
            !_isAttached
            || UnitService is not { } unitService
            || !unitService.Units.TryGetValue(DistanceUnit.Id, out var distanceUnit)
            || distanceUnit is null
        )
        {
            return;
        }

        _unitSubscription = distanceUnit
            .CurrentUnitItem.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateScale());
    }

    private void UpdateScale()
    {
        var maxBarWidth = MaxBarWidth;
        var unit = GetDistanceUnit();

        if (!TryGetDistanceForWidth(maxBarWidth, out var maximumDistance))
        {
            InvalidateScale(maxBarWidth, unit);
            return;
        }

        var maximumValue = unit.FromSi(maximumDistance);
        if (!double.IsFinite(maximumValue) || maximumValue <= 0)
        {
            InvalidateScale(maxBarWidth, unit);
            return;
        }

        var niceValue = GetNiceDistance(maximumValue);
        var niceDistance = unit.ToSi(niceValue);
        if (!double.IsFinite(niceDistance) || niceDistance <= 0)
        {
            InvalidateScale(maxBarWidth, unit);
            return;
        }

        var numberFormat = GetNumberFormat(niceValue);
        var metersPerPixel = maximumDistance / maxBarWidth;
        IsScaleValid = true;
        ActualBarWidth = Math.Clamp(niceDistance / metersPerPixel, 0, maxBarWidth);
        ScaleDistanceMeters = niceDistance;
        MiddleText = unit.Print(niceValue * 0.5, numberFormat);
        MaximumText = unit.PrintWithUnits(niceValue, numberFormat);
    }

    private bool TryGetDistanceForWidth(double width, out double distance)
    {
        distance = 0;
        var provider = Provider ?? EmptyTileProvider.Instance;
        try
        {
            var tileSize = provider.TileSize;
            var providerInfo = provider.Info;
            if (
                tileSize <= 0
                || Zoom < providerInfo.MinZoom
                || Zoom > providerInfo.MaxZoom
            )
            {
                return false;
            }

            var centerPixel = provider.Projection.Wgs84ToPixels(
                CenterMap,
                Zoom,
                tileSize
            );
            if (!IsFinite(centerPixel))
            {
                return false;
            }

            var center = provider.Projection.PixelsToWgs84(
                centerPixel,
                Zoom,
                tileSize
            );
            var left = provider.Projection.PixelsToWgs84(
                new Point(centerPixel.X - 1, centerPixel.Y),
                Zoom,
                tileSize
            );
            var right = provider.Projection.PixelsToWgs84(
                new Point(centerPixel.X + 1, centerPixel.Y),
                Zoom,
                tileSize
            );

            var distanceSum = 0.0;
            var sampleCount = 0;
            AddDistanceSample(center, left, ref distanceSum, ref sampleCount);
            AddDistanceSample(center, right, ref distanceSum, ref sampleCount);
            if (sampleCount == 0)
            {
                return false;
            }

            distance = (distanceSum / sampleCount) * width;
            return double.IsFinite(distance) && distance > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private IUnitItem GetDistanceUnit()
    {
        if (
            UnitService is { } unitService
            && unitService.Units.TryGetValue(DistanceUnit.Id, out var distanceUnit)
            && distanceUnit is not null
        )
        {
            return distanceUnit.CurrentUnitItem.Value;
        }

        return DefaultDistanceUnit;
    }

    private void InvalidateScale(double maxBarWidth, IUnitItem unit)
    {
        const string ZeroFormat = "0";
        IsScaleValid = false;
        ActualBarWidth = maxBarWidth;
        ScaleDistanceMeters = 0;
        MiddleText = unit.Print(0, ZeroFormat);
        MaximumText = unit.PrintWithUnits(0, ZeroFormat);
    }

    private static void AddDistanceSample(
        GeoPoint center,
        GeoPoint sample,
        ref double distanceSum,
        ref int sampleCount
    )
    {
        if (
            !IsFinite(center)
            || !IsFinite(sample)
            || Math.Abs(sample.Longitude - center.Longitude) > 180.0
        )
        {
            return;
        }

        var sampleDistance = GeoMath.Distance(
            center.Latitude,
            center.Longitude,
            sample.Latitude,
            sample.Longitude
        );
        if (!double.IsFinite(sampleDistance) || sampleDistance <= 0)
        {
            return;
        }

        distanceSum += sampleDistance;
        sampleCount++;
    }

    private static double GetNiceDistance(double maximumValue)
    {
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(maximumValue)));
        var normalized = maximumValue / magnitude;
        var coefficient = normalized switch
        {
            >= 5 => 5,
            >= 3 => 3,
            >= 2 => 2,
            _ => 1,
        };

        return coefficient * magnitude;
    }

    private static string GetNumberFormat(double value)
    {
        var exponent = (int)Math.Floor(Math.Log10(value));
        var decimalPlaces = Math.Clamp(1 - exponent, 0, 12);
        return decimalPlaces == 0 ? "0" : $"0.{new string('#', decimalPlaces)}";
    }

    private static bool IsFinite(Point point) =>
        double.IsFinite(point.X) && double.IsFinite(point.Y);

    private static bool IsFinite(GeoPoint point) =>
        double.IsFinite(point.Latitude) && double.IsFinite(point.Longitude);
}
