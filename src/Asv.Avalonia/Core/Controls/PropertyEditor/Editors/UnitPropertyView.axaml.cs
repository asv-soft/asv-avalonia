using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using R3;

namespace Asv.Avalonia;

public partial class UnitPropertyView : UserControl
{
    private IDisposable? _gotFocusDelayTimer;

    public UnitPropertyView()
    {
        InitializeComponent();
    }

    private void PART_ValueTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        _gotFocusDelayTimer?.Dispose();
        if (this.DataContext is UnitPropertyViewModel { IsInEditMode: true } prop)
        {
            prop.CommitValue();
        }
    }

    private void PART_ValueTextBox_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            _gotFocusDelayTimer = Observable.TimerFrame(1).Subscribe(_ => textBox.SelectAll());
        }
    }
}
