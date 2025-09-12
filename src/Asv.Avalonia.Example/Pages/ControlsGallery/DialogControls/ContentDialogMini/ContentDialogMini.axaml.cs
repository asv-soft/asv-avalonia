using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia.Example;

public partial class ContentDialogMini : TemplatedControl
{
    private Border? _primaryIndicator;
    private Border? _secondaryIndicator;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _primaryIndicator = e.NameScope.Find<Border>("PART_PrimaryIndicator");
        _secondaryIndicator = e.NameScope.Find<Border>("PART_SecondaryIndicator");

        UpdateIndicators();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DialogResultProperty)
        {
            UpdateIndicators();
        }
    }

    private void UpdateIndicators()
    {
        if (_primaryIndicator == null || _secondaryIndicator == null)
        {
            return;
        }

        var result = DialogResult ?? ContentDialogResult.None;

        _primaryIndicator.Classes.Set("active", result == ContentDialogResult.Primary);
        _secondaryIndicator.Classes.Set("active", result == ContentDialogResult.Secondary);
    }
}
