using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

public partial class DockWindow : Window
{

    public static readonly StyledProperty<bool> CloseRequestedProperty =
        AvaloniaProperty.Register<DockWindow, bool>(nameof(CloseRequested));

    public bool CloseRequested
    {
        get => GetValue(CloseRequestedProperty);
        set => SetValue(CloseRequestedProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CloseRequestedProperty)
        {
            Close();
        }
    }
    
    private Point _startDragPosition;
    private PixelPoint _startWindowPosition;

    public DockWindow()
    {
        InitializeComponent();

        // Подписка на события для области перетаскивания
        var dragArea = this.FindControl<Border>("DragArea"); // Предположим, что у вас есть элемент с именем DragArea
        dragArea!.PointerPressed += DragArea_PointerPressed;
        dragArea.PointerMoved += DragArea_PointerMoved;
        dragArea.PointerReleased += DragArea_PointerReleased;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Обработка события нажатия мыши
    private void DragArea_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Проверяем, что мышь была нажата
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Фиксируем начальные позиции мыши и окна
            // _startDragPosition = e.GetPosition(this);
             _startWindowPosition = Position;

            // Захватываем мышь, чтобы следить за ее движением
            e.Pointer.Capture(sender as IInputElement);
        }
    }

    // Обработка события движения мыши
    private void DragArea_PointerMoved(object sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Вычисляем смещение мыши
            var delta = e.GetPosition(this) - _startDragPosition;

            // Изменяем позицию окна в зависимости от движения мыши
            Position = _startWindowPosition + new PixelPoint((int)delta.X, (int)delta.Y);
        }
    }

    // Обработка события отпускания кнопки мыши
    private void DragArea_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        // Освобождаем захват мыши, когда кнопка отпущена
        e.Pointer.Capture(null);
    }
}