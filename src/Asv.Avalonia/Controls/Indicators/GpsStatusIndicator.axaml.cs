using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Material.Icons;

namespace Asv.Avalonia;

public enum GpsFixType
{
    /// <summary>
    /// No GPS connected
    /// GPS_FIX_TYPE_NO_GPS
    /// </summary>
    GpsFixTypeNoGps = 0,

    /// <summary>
    /// No position information, GPS is connected
    /// GPS_FIX_TYPE_NO_FIX
    /// </summary>
    GpsFixTypeNoFix = 1,

    /// <summary>
    /// 2D position
    /// GPS_FIX_TYPE_2D_FIX
    /// </summary>
    GpsFixType2dFix = 2,

    /// <summary>
    /// 3D position
    /// GPS_FIX_TYPE_3D_FIX
    /// </summary>
    GpsFixType3dFix = 3,

    /// <summary>
    /// DGPS/SBAS aided 3D position
    /// GPS_FIX_TYPE_DGPS
    /// </summary>
    GpsFixTypeDgps = 4,

    /// <summary>
    /// RTK float, 3D position
    /// GPS_FIX_TYPE_RTK_FLOAT
    /// </summary>
    GpsFixTypeRtkFloat = 5,

    /// <summary>
    /// RTK Fixed, 3D position
    /// GPS_FIX_TYPE_RTK_FIXED
    /// </summary>
    GpsFixTypeRtkFixed = 6,

    /// <summary>
    /// Static fixed, typically used for base stations
    /// GPS_FIX_TYPE_STATIC
    /// </summary>
    GpsFixTypeStatic = 7,

    /// <summary>
    /// PPP, 3D position.
    /// GPS_FIX_TYPE_PPP
    /// </summary>
    GpsFixTypePpp = 8,
}

/// <summary>
/// https://en.wikipedia.org/wiki/Dilution_of_precision_(navigation)
/// </summary>
public enum DopStatusEnum
{
    Unknown,
    Ideal,
    Excellent,
    Good,
    Moderate,
    Fair,
    Poor,
}

[PseudoClasses(
    PseudoClassesHelper.Critical,
    PseudoClassesHelper.Warning,
    PseudoClassesHelper.Normal,
    PseudoClassesHelper.Unknown
)]
public partial class GpsStatusIndicator : IndicatorBase
{
    public static void SetPseudoClass(GpsStatusIndicator indicator)
    {
        var dopStatus = indicator.DopStatus;

        indicator.PseudoClasses.Set(
            PseudoClassesHelper.Unknown,
            dopStatus == DopStatusEnum.Unknown
        );
        indicator.PseudoClasses.Set(
            PseudoClassesHelper.Critical,
            dopStatus is DopStatusEnum.Fair or DopStatusEnum.Poor
        );
        indicator.PseudoClasses.Set(
            PseudoClassesHelper.Warning,
            dopStatus == DopStatusEnum.Moderate
        );
        indicator.PseudoClasses.Set(
            PseudoClassesHelper.Normal,
            dopStatus is DopStatusEnum.Ideal or DopStatusEnum.Excellent or DopStatusEnum.Good
        );

        indicator.IconKind = GetIcon(indicator.FixType);
    }

    private static GpsFixType UpdateValue(AvaloniaObject avaloniaObject, GpsFixType gpsFixType)
    {
        // TODO: AVALONIA11 => implement this
        if (avaloniaObject is not GpsStatusIndicator indicator)
        {
            return GpsFixType.GpsFixType2dFix;
        }

        SetPseudoClass(indicator);
        return GpsFixType.GpsFixType2dFix;
    }

    private static MaterialIconKind GetIcon(GpsFixType fixType)
    {
        return fixType switch
        {
            GpsFixType.GpsFixTypeNoGps => MaterialIconKind.CrosshairsQuestion,
            GpsFixType.GpsFixTypeNoFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixType2dFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixType3dFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixTypeDgps => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixTypeRtkFloat => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypeRtkFixed => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypeStatic => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypePpp => MaterialIconKind.CrosshairsGps,
            _ => throw new ArgumentOutOfRangeException(nameof(fixType), fixType, null),
        };
    }
}
