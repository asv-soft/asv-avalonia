using Asv.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.GeoMap;

[PseudoClasses(":pressed", ":selected", ":pointerover")]
public partial class MapItem : ContentControl, ISelectable
{
    static MapItem()
    {
        SelectableMixin.Attach<MapItem>(IsSelectedProperty);
        PressedMixin.Attach<MapItem>();
        CenterXProperty
            .Changed.ToObservable()
            .Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        CenterYProperty
            .Changed.ToObservable()
            .Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        BoundsProperty
            .Changed.ToObservable()
            .Subscribe(x => RecalculateRotation(x.Sender as MapItem));
        RotationProperty
            .Changed.ToObservable()
            .Subscribe(x => RecalculateEffectiveRotation(x.Sender as MapItem));
        UseMapRotationProperty
            .Changed.ToObservable()
            .Subscribe(x => RecalculateEffectiveRotation(x.Sender as MapItem));
        MapCanvas
            .RotationProperty.Changed.ToObservable()
            .Subscribe(x => RecalculateEffectiveRotation(x.Sender as MapItem));
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

    private static void RecalculateEffectiveRotation(MapItem? sender)
    {
        if (sender == null)
        {
            return;
        }

        sender.EffectiveRotation =
            sender.Rotation + (sender.UseMapRotation ? MapCanvas.GetRotation(sender) : 0.0);
    }

    public MapItem()
    {
        RecalculateEffectiveRotation(this);
    }
}

public class GeoPointCollection : AvaloniaList<GeoPoint> { }
