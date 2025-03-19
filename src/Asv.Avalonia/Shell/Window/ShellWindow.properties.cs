using Avalonia;

namespace Asv.Avalonia;

public partial class ShellWindow
{
    public static readonly StyledProperty<bool> IsMaximizedProperty = AvaloniaProperty.Register<ShellWindow, bool>(nameof(IsMaximized));

    public bool IsMaximized
    {
        get => GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }
}