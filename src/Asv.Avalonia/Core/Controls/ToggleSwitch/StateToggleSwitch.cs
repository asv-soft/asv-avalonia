using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Asv.Avalonia;

public partial class StateToggleSwitch : ToggleButton
{
    static StateToggleSwitch()
    {
        FocusableProperty.OverrideDefaultValue<StateToggleSwitch>(true);
    }

    public StateToggleSwitch()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        UpdateState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == IsCheckedProperty
            || change.Property == CheckedTextProperty
            || change.Property == UncheckedTextProperty
            || change.Property == CheckedIconProperty
            || change.Property == UncheckedIconProperty
            || change.Property == CheckedStatusProperty
            || change.Property == UncheckedStatusProperty
        )
        {
            UpdateState();
        }
    }

    private void UpdateState()
    {
        var isChecked = IsChecked == true;
        StateText = isChecked ? CheckedText : UncheckedText;
        StateIcon = isChecked ? CheckedIcon : UncheckedIcon;
        StateStatus = isChecked ? CheckedStatus : UncheckedStatus;
        ContentColumn = isChecked ? 0 : 1;
        ThumbColumn = isChecked ? 1 : 0;
    }
}
