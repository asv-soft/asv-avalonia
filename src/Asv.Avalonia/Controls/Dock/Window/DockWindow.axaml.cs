using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public partial class DockWindow : Window
{
    public required string Id { get; init; }

    public DockWindow()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(this, true);
        this.AddHandler(DragDrop.DropEvent, OnFileDrop);
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        Page.Rise(new DesktopDragEvent(Page, e)).SafeFireAndForget();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    public static readonly StyledProperty<IPage> PageProperty = AvaloniaProperty.Register<
        DockWindow,
        IPage
    >(nameof(Page));

    public IPage Page
    {
        get => GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }
}
