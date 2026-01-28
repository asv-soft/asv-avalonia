using Avalonia;

namespace Asv.Avalonia;

public partial class GpsStatusIndicator
{
    public static readonly StyledProperty<GpsFixType> FixTypeProperty = AvaloniaProperty.Register<
        GpsStatusIndicator,
        GpsFixType
    >(nameof(FixType), GpsFixType.GpsFixTypeNoGps, coerce: UpdateValue);

    public GpsFixType FixType
    {
        get => GetValue(FixTypeProperty);
        set => SetValue(FixTypeProperty, value);
    }

    public static readonly StyledProperty<string> ToolTipTextProperty = AvaloniaProperty.Register<
        GpsStatusIndicator,
        string
    >(nameof(ToolTipText), string.Empty);

    public string ToolTipText
    {
        get => GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    public static readonly StyledProperty<DopStatusEnum> DopStatusProperty =
        AvaloniaProperty.Register<GpsStatusIndicator, DopStatusEnum>(
            nameof(DopStatus),
            DopStatusEnum.Unknown,
            coerce: UpdateValue
        );

    private static DopStatusEnum UpdateValue(AvaloniaObject arg1, DopStatusEnum arg2)
    {
        // TODO: AVALONIA11 => implement this
        return DopStatusEnum.Excellent;
    }

    public DopStatusEnum DopStatus
    {
        get => GetValue(DopStatusProperty);
        set => SetValue(DopStatusProperty, value);
    }
}
