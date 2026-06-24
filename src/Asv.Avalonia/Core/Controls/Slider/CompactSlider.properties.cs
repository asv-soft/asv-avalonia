using Avalonia;
using Avalonia.Data;

namespace Asv.Avalonia;

public partial class CompactSlider
{
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(Minimum));

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(Maximum), 100);

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<double> TickFrequencyProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(TickFrequency), 1);

    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public static readonly StyledProperty<double> SmallChangeProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(SmallChange), 1);

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public static readonly StyledProperty<double> LargeChangeProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(LargeChange), 10);

    public double LargeChange
    {
        get => GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        AvaloniaProperty.Register<CompactSlider, bool>(nameof(IsSnapToTickEnabled));

    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> SliderColorProperty =
        AvaloniaProperty.Register<CompactSlider, AsvColorKind>(
            nameof(SliderColor),
            AsvColorKind.Info3
        );

    public AsvColorKind SliderColor
    {
        get => GetValue(SliderColorProperty);
        set => SetValue(SliderColorProperty, value);
    }

    public static readonly StyledProperty<string?> ValueTextProperty = AvaloniaProperty.Register<
        CompactSlider,
        string?
    >(nameof(ValueText));

    public string? ValueText
    {
        get => GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public static readonly StyledProperty<string?> UnitsProperty = AvaloniaProperty.Register<
        CompactSlider,
        string?
    >(nameof(Units));

    public string? Units
    {
        get => GetValue(UnitsProperty);
        set => SetValue(UnitsProperty, value);
    }

    public static readonly StyledProperty<double> TrackHeightProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(TrackHeight), 3);

    public double TrackHeight
    {
        get => GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    public static readonly StyledProperty<double> ThumbSizeProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(ThumbSize), 12);

    public double ThumbSize
    {
        get => GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    public static readonly StyledProperty<double> ThumbWidthProperty = AvaloniaProperty.Register<
        CompactSlider,
        double
    >(nameof(ThumbWidth), 56);

    public double ThumbWidth
    {
        get => GetValue(ThumbWidthProperty);
        set => SetValue(ThumbWidthProperty, value);
    }

    private double _thumbLeft;

    public static readonly DirectProperty<CompactSlider, double> ThumbLeftProperty =
        AvaloniaProperty.RegisterDirect<CompactSlider, double>(nameof(ThumbLeft), o => o.ThumbLeft);

    public double ThumbLeft
    {
        get => _thumbLeft;
        private set => SetAndRaise(ThumbLeftProperty, ref _thumbLeft, value);
    }

    private double _thumbTop;

    public static readonly DirectProperty<CompactSlider, double> ThumbTopProperty =
        AvaloniaProperty.RegisterDirect<CompactSlider, double>(nameof(ThumbTop), o => o.ThumbTop);

    public double ThumbTop
    {
        get => _thumbTop;
        private set => SetAndRaise(ThumbTopProperty, ref _thumbTop, value);
    }
}
