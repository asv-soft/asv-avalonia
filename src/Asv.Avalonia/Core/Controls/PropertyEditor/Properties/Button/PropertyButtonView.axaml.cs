using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

public partial class PropertyButtonView : UserControl
{
    public PropertyButtonView()
    {
        InitializeComponent();
    }

    private void ActionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }
}
