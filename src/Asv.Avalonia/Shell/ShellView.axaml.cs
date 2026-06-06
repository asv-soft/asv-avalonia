using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (
            e.Source is Control { } source
            && (source is Button || source.FindAncestorOfType<Button>() != null)
        )
        {
            return;
        }

        if (sender is Control control && control.DataContext is ShellMessageViewModel message)
        {
            message.CloseCommand.Execute(message);
        }
    }
}
