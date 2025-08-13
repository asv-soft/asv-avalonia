using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Asv.Avalonia;

public partial class DockControl
{
    private const string PART_MainTabControl = "PART_MainTabControl";

    public static readonly StyledProperty<IDataTemplate?> TabControlStripItemTemplateProperty =
        AvaloniaProperty.Register<DockControl, IDataTemplate?>(nameof(TabControlStripItemTemplate));

    [InheritDataTypeFromItems("ItemsSource")]
    public IDataTemplate? TabControlStripItemTemplate
    {
        get => GetValue(TabControlStripItemTemplateProperty);
        set => SetValue(TabControlStripItemTemplateProperty, value);
    }
}
