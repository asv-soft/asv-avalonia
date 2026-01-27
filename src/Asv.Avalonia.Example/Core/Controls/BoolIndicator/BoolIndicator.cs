using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Asv.Avalonia.Example;

public partial class BoolIndicator : UserControl
{
    private readonly MaterialIcon _icon;

    public BoolIndicator()
    {
        _icon = new MaterialIcon { Width = 16, Height = 16 };
        Content = _icon;
        UpdateIcon();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            UpdateIcon();
        }
        else if (change.Property == IconSizeProperty)
        {
            UpdateIconSize();
        }
    }

    private void UpdateIcon()
    {
        (_icon.Kind, _icon.Foreground) = Value switch
        {
            true => (MaterialIconKind.Circle, Brushes.Green),
            false => (MaterialIconKind.Circle, Brushes.Red),
            null => (MaterialIconKind.Circle, Brushes.Gray),
        };
    }

    private void UpdateIconSize()
    {
        _icon.Width = IconSize;
        _icon.Height = IconSize;
    }
}
