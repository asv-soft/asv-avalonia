using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Avalonia;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is ShellMessageViewModel message)
        {
            message.CloseCommand.Execute(message);
        }
    }
}
