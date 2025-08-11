using Avalonia.Controls;

namespace Asv.Avalonia;

public partial class DockWindow : Window
{
    public NavigationId Id { get; }

    public DockWindow()
        : this(DesignTime.Id)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DockWindow(NavigationId windowId)
    {
        InitializeComponent();
        Id = windowId;
    }
}
