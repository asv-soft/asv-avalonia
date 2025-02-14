using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;

namespace Asv.Avalonia.Map;

[PseudoClasses(":pressed", ":selected")]
public class MapControlItem : ContentControl, ISelectable
{
    static MapControlItem()
    {
        SelectableMixin.Attach<MapControlItem>(IsSelectedProperty);
        PressedMixin.Attach<MapControlItem>();
    }

    #region IsSelected

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<
        MapControlItem,
        bool
    >(nameof(IsSelected));
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion
}
