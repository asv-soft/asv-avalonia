using Asv.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Media;

namespace Asv.Avalonia.GeoMap;

[PseudoClasses(":pressed", ":selected", ":pointerover")]
public partial class MapItem : ContentControl, ISelectable
{
    static MapItem()
    {
        SelectableMixin.Attach<MapItem>(IsSelectedProperty);
        PressedMixin.Attach<MapItem>();
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        BoundsProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapItem));
    }

    private static void RecalculateRotation(MapItem? sender)
    {
        if (sender == null)
        {
            return;
        }

        sender.RotationCenterX = sender.CenterX.CalculateOffset(sender.Bounds.Width);
        sender.RotationCenterY = sender.CenterY.CalculateOffset(sender.Bounds.Height);
    }

    public MapItem() { }
}

public class GeoPointCollection : AvaloniaList<GeoPoint> { }
