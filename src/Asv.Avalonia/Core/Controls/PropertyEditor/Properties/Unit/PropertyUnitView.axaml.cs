using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using R3;

namespace Asv.Avalonia;

public partial class PropertyUnitView : UserControl
{
    private IDisposable? _gotFocusDelayTimer;

    public PropertyUnitView()
    {
        InitializeComponent();
        PART_TextBox.AddHandler(
            PointerPressedEvent,
            PART_TextBox_OnPointerPressed,
            RoutingStrategies.Tunnel
        );
        PART_TextBox.AddHandler(KeyDownEvent, PART_TextBox_OnKeyDown, RoutingStrategies.Tunnel);
        PART_TextBox.AddHandler(TextInputEvent, PART_TextBox_OnTextInput, RoutingStrategies.Tunnel);
    }

    private void PART_TextBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(PART_TextBox).Properties.IsLeftButtonPressed)
        {
            BeginEdit();
        }
    }

    private void PART_TextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape or Key.Tab))
        {
            BeginEdit();
        }
    }

    private void PART_TextBox_OnTextInput(object? sender, TextInputEventArgs e)
    {
        BeginEdit();
    }

    private void BeginEdit()
    {
        if (DataContext is PropertyUnitViewModel { IsBusy: false } prop)
        {
            prop.IsInEditMode = true;
        }
    }

    private void PART_TextBox_OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        BeginEdit();
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
