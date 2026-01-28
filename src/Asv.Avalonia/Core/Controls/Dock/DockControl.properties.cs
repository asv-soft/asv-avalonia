using Asv.Cfg;
using Avalonia;

namespace Asv.Avalonia;

public partial class DockControl
{
    public static readonly StyledProperty<IConfiguration?> ConfigurationProperty =
        AvaloniaProperty.Register<DockControl, IConfiguration?>(nameof(Configuration));

    public IConfiguration? Configuration
    {
        get => GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value);
    }

    public static readonly StyledProperty<ILayoutService?> LayoutServiceProperty =
        AvaloniaProperty.Register<DockControl, ILayoutService?>(nameof(LayoutService));

    public ILayoutService? LayoutService
    {
        get => GetValue(LayoutServiceProperty);
        set => SetValue(LayoutServiceProperty, value);
    }
}
