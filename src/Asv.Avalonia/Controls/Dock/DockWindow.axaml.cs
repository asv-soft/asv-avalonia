﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// This object represents the window with drag and drop feature.
/// TODO: Has no use now.
/// </summary>
public partial class DockWindow : Window
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CloseRequestedProperty)
        {
            if (DataContext is IPage page)
            {
                page.TryClose.Execute(Unit.Default);
            }

            Close();
        }
    }

    private Point _startDragPosition;
    private PixelPoint _startWindowPosition;

    public DockWindow()
    {
        InitializeComponent();

        var dragArea = this.FindControl<Border>("DragArea");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DragArea_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _startWindowPosition = Position;

            e.Pointer.Capture(sender as IInputElement);
        }
    }

    private void DragArea_PointerMoved(object sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var delta = e.GetPosition(this) - _startDragPosition;

            Position = _startWindowPosition + new PixelPoint((int)delta.X, (int)delta.Y);
        }
    }

    private void DragArea_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
    }
}
