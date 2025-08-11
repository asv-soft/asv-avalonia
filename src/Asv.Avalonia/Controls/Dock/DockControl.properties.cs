using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using R3;

namespace Asv.Avalonia;

public partial class DockControl
{
    public static readonly StyledProperty<IDataTemplate?> TabControlStripItemTemplateProperty =
        AvaloniaProperty.Register<DockControl, IDataTemplate?>(nameof(TabControlStripItemTemplate));

    [InheritDataTypeFromItems("ItemsSource")]
    public IDataTemplate? TabControlStripItemTemplate
    {
        get => GetValue(TabControlStripItemTemplateProperty);
        set => SetValue(TabControlStripItemTemplateProperty, value);
    }
}
