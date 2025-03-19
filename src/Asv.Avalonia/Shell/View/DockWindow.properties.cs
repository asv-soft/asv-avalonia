using Avalonia;

namespace Asv.Avalonia;

public partial class DockWindow
{
    public static readonly StyledProperty<bool> CloseRequestedProperty = AvaloniaProperty.Register<
        DockWindow,
        bool
    >(nameof(CloseRequested));

    public bool CloseRequested
    {
        get => GetValue(CloseRequestedProperty);
        set => SetValue(CloseRequestedProperty, value);
    }
}