using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia.Example;

public partial class ContentDialogMini : TemplatedControl
{
    private Border? _primaryIndicator;
    private Border? _secondaryIndicator;

    private StackPanel? _textPlaceHolder;
    private Grid? _imagePlaceHolder;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _primaryIndicator = e.NameScope.Find<Border>(PrimaryIndicatorElementName);
        _secondaryIndicator = e.NameScope.Find<Border>(SecondaryIndicatorElementName);

        _textPlaceHolder = e.NameScope.Find<StackPanel>(TextPlaceHolderElementName);
        _imagePlaceHolder = e.NameScope.Find<Grid>(ImagePlaceHolderElementName);

        UpdateIndicators();
        UpdateContentType();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DialogResultProperty)
        {
            UpdateIndicators();
        }
        if (change.Property == IsImageContentTypeProperty)
        {
            UpdateContentType();
        }
    }

    private void UpdateIndicators()
    {
        if (_primaryIndicator == null || _secondaryIndicator == null)
        {
            return;
        }

        var result = DialogResult ?? ContentDialogResult.None;

        _primaryIndicator.Classes.Set(ActiveStyleClass, result == ContentDialogResult.Primary);
        _secondaryIndicator.Classes.Set(ActiveStyleClass, result == ContentDialogResult.Secondary);
    }

    private void UpdateContentType()
    {
        if (_textPlaceHolder == null || _imagePlaceHolder == null)
        {
            return;
        }

        _textPlaceHolder.IsVisible = !IsImageContentType;
        _imagePlaceHolder.IsVisible = IsImageContentType;
    }
}
