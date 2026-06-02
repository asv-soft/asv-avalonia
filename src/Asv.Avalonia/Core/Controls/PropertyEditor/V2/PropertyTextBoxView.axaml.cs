using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using R3;

namespace Asv.Avalonia;

public partial class PropertyTextBoxView : UserControl
{
    private IDisposable? _gotFocusDelayTimer;

    public PropertyTextBoxView()
    {
        InitializeComponent();
    }

    private void PART_TextBox_OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            _gotFocusDelayTimer = Observable.TimerFrame(1).Subscribe(_ => textBox.SelectAll());
        }
    }

    private void PART_TextBox_OnLostFocus(object? sender, FocusChangedEventArgs e)
    {
        _gotFocusDelayTimer?.Dispose();
        _gotFocusDelayTimer = null;
    }
}
