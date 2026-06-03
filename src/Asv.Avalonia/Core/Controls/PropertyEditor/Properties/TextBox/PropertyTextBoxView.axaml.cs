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
        if (DataContext is PropertyTextBoxViewModel prop)
        {
            prop.IsInEditMode = true;
        }
        if (sender is TextBox textBox)
        {
            _gotFocusDelayTimer = Observable.TimerFrame(1).Subscribe(_ => textBox.SelectAll());
        }
    }

    private async void PART_TextBox_OnLostFocus(object? sender, FocusChangedEventArgs e)
    {
        try
        {
            if (DataContext is PropertyTextBoxViewModel { IsInEditMode: true } prop)
            {
                if (prop.IsSync)
                {
                    prop.IsInEditMode = false;
                }
                else
                {
                    await prop.ApplyFromUser();
                }
            }
        }
        finally
        {
            _gotFocusDelayTimer?.Dispose();
            _gotFocusDelayTimer = null;
        }
    }
}
