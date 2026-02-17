using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public partial class SearchBoxView : UserControl
{
    public SearchBoxView()
    {
        InitializeComponent();
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Observable.TimerFrame(1).Subscribe(_ => textBox.SelectAll());
        }
    }
}
