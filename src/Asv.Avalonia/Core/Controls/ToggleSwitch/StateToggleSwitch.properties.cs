using Avalonia;
using Material.Icons;

namespace Asv.Avalonia;

public partial class StateToggleSwitch
{
    public static readonly StyledProperty<string?> CheckedTextProperty = AvaloniaProperty.Register<
        StateToggleSwitch,
        string?
    >(nameof(CheckedText), "ON");

    public string? CheckedText
    {
        get => GetValue(CheckedTextProperty);
        set => SetValue(CheckedTextProperty, value);
    }

    public static readonly StyledProperty<string?> UncheckedTextProperty =
        AvaloniaProperty.Register<StateToggleSwitch, string?>(nameof(UncheckedText), "OFF");

    public string? UncheckedText
    {
        get => GetValue(UncheckedTextProperty);
        set => SetValue(UncheckedTextProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind?> CheckedIconProperty =
        AvaloniaProperty.Register<StateToggleSwitch, MaterialIconKind?>(
            nameof(CheckedIcon),
            MaterialIconKind.ToggleSwitch
        );

    public MaterialIconKind? CheckedIcon
    {
        get => GetValue(CheckedIconProperty);
        set => SetValue(CheckedIconProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind?> UncheckedIconProperty =
        AvaloniaProperty.Register<StateToggleSwitch, MaterialIconKind?>(
            nameof(UncheckedIcon),
            MaterialIconKind.ToggleSwitchOff
        );

    public MaterialIconKind? UncheckedIcon
    {
        get => GetValue(UncheckedIconProperty);
        set => SetValue(UncheckedIconProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> CheckedStatusProperty =
        AvaloniaProperty.Register<StateToggleSwitch, AsvColorKind>(
            nameof(CheckedStatus),
            AsvColorKind.Success
        );

    public AsvColorKind CheckedStatus
    {
        get => GetValue(CheckedStatusProperty);
        set => SetValue(CheckedStatusProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> UncheckedStatusProperty =
        AvaloniaProperty.Register<StateToggleSwitch, AsvColorKind>(
            nameof(UncheckedStatus),
            AsvColorKind.Error
        );

    public AsvColorKind UncheckedStatus
    {
        get => GetValue(UncheckedStatusProperty);
        set => SetValue(UncheckedStatusProperty, value);
    }

    private string? _stateText;

    public static readonly DirectProperty<StateToggleSwitch, string?> StateTextProperty =
        AvaloniaProperty.RegisterDirect<StateToggleSwitch, string?>(
            nameof(StateText),
            o => o.StateText
        );

    public string? StateText
    {
        get => _stateText;
        private set => SetAndRaise(StateTextProperty, ref _stateText, value);
    }

    private MaterialIconKind? _stateIcon;

    public static readonly DirectProperty<StateToggleSwitch, MaterialIconKind?> StateIconProperty =
        AvaloniaProperty.RegisterDirect<StateToggleSwitch, MaterialIconKind?>(
            nameof(StateIcon),
            o => o.StateIcon
        );

    public MaterialIconKind? StateIcon
    {
        get => _stateIcon;
        private set => SetAndRaise(StateIconProperty, ref _stateIcon, value);
    }

    private AsvColorKind _stateStatus;

    public static readonly DirectProperty<StateToggleSwitch, AsvColorKind> StateStatusProperty =
        AvaloniaProperty.RegisterDirect<StateToggleSwitch, AsvColorKind>(
            nameof(StateStatus),
            o => o.StateStatus
        );

    public AsvColorKind StateStatus
    {
        get => _stateStatus;
        private set => SetAndRaise(StateStatusProperty, ref _stateStatus, value);
    }

    private int _thumbColumn;

    public static readonly DirectProperty<StateToggleSwitch, int> ThumbColumnProperty =
        AvaloniaProperty.RegisterDirect<StateToggleSwitch, int>(
            nameof(ThumbColumn),
            o => o.ThumbColumn
        );

    public int ThumbColumn
    {
        get => _thumbColumn;
        private set => SetAndRaise(ThumbColumnProperty, ref _thumbColumn, value);
    }

    private int _contentColumn = 1;

    public static readonly DirectProperty<StateToggleSwitch, int> ContentColumnProperty =
        AvaloniaProperty.RegisterDirect<StateToggleSwitch, int>(
            nameof(ContentColumn),
            o => o.ContentColumn
        );

    public int ContentColumn
    {
        get => _contentColumn;
        private set => SetAndRaise(ContentColumnProperty, ref _contentColumn, value);
    }
}
