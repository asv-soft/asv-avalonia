using Avalonia.Input;

namespace Asv.Avalonia.GeoMap;

public enum MapInputAction
{
    Pan,
    MoveMarkers,
    SelectRectangle,
    Zoom,
}

public enum MapGesturePreset
{
    None,
    LeftDrag,
    ControlLeftDrag,
    ShiftLeftDrag,
    AltLeftDrag,
    MetaLeftDrag,
    MiddleDrag,
    ControlMiddleDrag,
    ShiftMiddleDrag,
    AltMiddleDrag,
    MetaMiddleDrag,
    Wheel,
    ControlWheel,
    ShiftWheel,
    AltWheel,
    MetaWheel,
}

/// <summary>Immutable map input assignments. Identifiers are persisted, independent of UI language.</summary>
public sealed record MapInputBindings
{
    public static MapInputBindings Default { get; } = new();

    public MapGesturePreset Pan { get; init; } = MapGesturePreset.LeftDrag;
    public MapGesturePreset MoveMarkers { get; init; } = MapGesturePreset.ControlLeftDrag;
    public MapGesturePreset SelectRectangle { get; init; } = MapGesturePreset.ShiftLeftDrag;
    public MapGesturePreset Zoom { get; init; } = MapGesturePreset.Wheel;

    public MapGesturePreset Get(MapInputAction action) =>
        action switch
        {
            MapInputAction.Pan => Pan,
            MapInputAction.MoveMarkers => MoveMarkers,
            MapInputAction.SelectRectangle => SelectRectangle,
            MapInputAction.Zoom => Zoom,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

    public MapInputBindings With(MapInputAction action, MapGesturePreset preset) =>
        action switch
        {
            MapInputAction.Pan => this with { Pan = preset },
            MapInputAction.MoveMarkers => this with { MoveMarkers = preset },
            MapInputAction.SelectRectangle => this with { SelectRectangle = preset },
            MapInputAction.Zoom => this with { Zoom = preset },
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

    public MapInputAction? FindConflict(MapInputAction action, MapGesturePreset preset)
    {
        if (preset == MapGesturePreset.None)
        {
            return null;
        }

        foreach (var other in Enum.GetValues<MapInputAction>())
        {
            if (other != action && Get(other) == preset)
            {
                return other;
            }
        }

        return null;
    }

    public bool IsValid() =>
        Enum.GetValues<MapInputAction>()
            .All(action =>
                MapInputPresets.GetPresets(action).Contains(Get(action))
                && FindConflict(action, Get(action)) is null
            );
}

/// <summary>Available complete gestures and their context-local matching rules.</summary>
public static class MapInputPresets
{
    private static readonly IReadOnlyList<MapGesturePreset> DragPresets = Array.AsReadOnly(
        new[]
        {
            MapGesturePreset.None,
            MapGesturePreset.LeftDrag,
            MapGesturePreset.ControlLeftDrag,
            MapGesturePreset.ShiftLeftDrag,
            MapGesturePreset.AltLeftDrag,
            MapGesturePreset.MetaLeftDrag,
            MapGesturePreset.MiddleDrag,
            MapGesturePreset.ControlMiddleDrag,
            MapGesturePreset.ShiftMiddleDrag,
            MapGesturePreset.AltMiddleDrag,
            MapGesturePreset.MetaMiddleDrag,
        }
    );
    private static readonly IReadOnlyList<MapGesturePreset> ZoomPresets = Array.AsReadOnly(
        DragPresets
            .Concat(
                new[]
                {
                    MapGesturePreset.Wheel,
                    MapGesturePreset.ControlWheel,
                    MapGesturePreset.ShiftWheel,
                    MapGesturePreset.AltWheel,
                    MapGesturePreset.MetaWheel,
                }
            )
            .ToArray()
    );

    public static IReadOnlyList<MapGesturePreset> GetPresets(MapInputAction action) =>
        action switch
        {
            MapInputAction.Zoom => ZoomPresets,
            MapInputAction.Pan or MapInputAction.MoveMarkers or MapInputAction.SelectRectangle =>
                DragPresets,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

    public static KeyModifiers GetModifiers(MapGesturePreset preset) =>
        preset switch
        {
            MapGesturePreset.ControlLeftDrag
            or MapGesturePreset.ControlMiddleDrag
            or MapGesturePreset.ControlWheel => KeyModifiers.Control,
            MapGesturePreset.ShiftLeftDrag
            or MapGesturePreset.ShiftMiddleDrag
            or MapGesturePreset.ShiftWheel => KeyModifiers.Shift,
            MapGesturePreset.AltLeftDrag
            or MapGesturePreset.AltMiddleDrag
            or MapGesturePreset.AltWheel => KeyModifiers.Alt,
            MapGesturePreset.MetaLeftDrag
            or MapGesturePreset.MetaMiddleDrag
            or MapGesturePreset.MetaWheel => KeyModifiers.Meta,
            _ => KeyModifiers.None,
        };

    public static MouseButton GetButton(MapGesturePreset preset) =>
        preset switch
        {
            MapGesturePreset.LeftDrag
            or MapGesturePreset.ControlLeftDrag
            or MapGesturePreset.ShiftLeftDrag
            or MapGesturePreset.AltLeftDrag
            or MapGesturePreset.MetaLeftDrag => MouseButton.Left,
            MapGesturePreset.MiddleDrag
            or MapGesturePreset.ControlMiddleDrag
            or MapGesturePreset.ShiftMiddleDrag
            or MapGesturePreset.AltMiddleDrag
            or MapGesturePreset.MetaMiddleDrag => MouseButton.Middle,
            _ => MouseButton.None,
        };

    public static bool MatchesDrag(
        MapGesturePreset preset,
        MouseButton button,
        KeyModifiers modifiers
    ) =>
        button != MouseButton.None
        && GetButton(preset) == button
        && HasModifiers(preset, modifiers);

    public static bool MatchesWheel(MapGesturePreset preset, KeyModifiers modifiers) =>
        preset
            is MapGesturePreset.Wheel
                or MapGesturePreset.ControlWheel
                or MapGesturePreset.ShiftWheel
                or MapGesturePreset.AltWheel
                or MapGesturePreset.MetaWheel
        && HasModifiers(preset, modifiers);

    private static bool HasModifiers(MapGesturePreset preset, KeyModifiers modifiers)
    {
        var required = GetModifiers(preset);
        return (modifiers & required) == required;
    }

    public static MapInputAction? ResolveDrag(
        MapInputBindings bindings,
        MouseButton button,
        KeyModifiers modifiers,
        bool hasMarker,
        bool canDragWithoutModifier = false
    )
    {
        // More specific modifier gestures win; ties retain legacy Shift-before-Ctrl priority.
        ReadOnlySpan<MapInputAction> priority =
        [
            MapInputAction.Zoom,
            MapInputAction.SelectRectangle,
            MapInputAction.MoveMarkers,
            MapInputAction.Pan,
        ];
        MapInputAction? best = null;
        var specificity = -1;
        foreach (var action in priority)
        {
            if (action == MapInputAction.MoveMarkers && !hasMarker)
            {
                continue;
            }

            var preset = bindings.Get(action);
            var matches = MatchesDrag(preset, button, modifiers);
            var markerOverride =
                action == MapInputAction.MoveMarkers
                && preset != MapGesturePreset.None
                && canDragWithoutModifier
                && button == MouseButton.Left;
            if (!matches && !markerOverride)
            {
                continue;
            }

            var candidateSpecificity = matches && GetModifiers(preset) != KeyModifiers.None ? 1 : 0;
            if (candidateSpecificity > specificity)
            {
                specificity = candidateSpecificity;
                best = action;
            }
        }

        return best;
    }
}
