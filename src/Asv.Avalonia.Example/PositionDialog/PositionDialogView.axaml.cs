using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PositionDialogViewModel))]
public partial class PositionDialogView : UserControl
{
    public PositionDialogView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }
}
