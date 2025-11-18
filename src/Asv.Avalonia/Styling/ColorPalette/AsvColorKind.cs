using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;

namespace Asv.Avalonia;

/// <summary>
/// Visual color/status kinds for UI elements.
/// Supports [Flags] — multiple kinds can be combined.
/// CSS class name = enum member name in lowercase (e.g. Info7 → "info7").
/// </summary>
[Flags]
public enum AsvColorKind : ulong
{
    None = 0,

    // === Статусы ===
    Unknown = 1ul << 0, // 1
    Error = 1ul << 1, // 2
    Warning = 1ul << 2, // 4
    Success = 1ul << 3, // 8

    // === Цвета каналов (Info1–Info20) ===
    Info1 = 1ul << 4,
    Info2 = 1ul << 5,
    Info3 = 1ul << 6,
    Info4 = 1ul << 7,
    Info5 = 1ul << 8,
    Info6 = 1ul << 9,
    Info7 = 1ul << 10,
    Info8 = 1ul << 11,
    Info9 = 1ul << 12,
    Info10 = 1ul << 13,
    Info11 = 1ul << 14,
    Info12 = 1ul << 15,
    Info13 = 1ul << 16,
    Info14 = 1ul << 17,
    Info15 = 1ul << 18,
    Info16 = 1ul << 19,
    Info17 = 1ul << 20,
    Info18 = 1ul << 21,
    Info19 = 1ul << 22,
    Info20 = 1ul << 23,

    // === animation ===
    Blink = 1ul << 30,
    BlinkOnce = 1ul << 31,
    Fadein = 1ul << 32,
    Fadeinblink = 1ul << 33,
    Fadeout = 1ul << 34,

    // === sizes ===
    Small = 1ul << 40,
    Medium = 1ul << 41,
    Large = 1ul << 42,
}

/// <summary>
/// Attached property that automatically synchronizes AsvColorKind (Flags enum)
/// with CSS classes on any StyledElement.
/// Class name = lowercase name of the enum member (e.g. Info7 → "info7").
/// </summary>
public class AsvPallete
{
    public static readonly AttachedProperty<AsvColorKind> ColorProperty =
        AvaloniaProperty.RegisterAttached<AsvPallete, StyledElement, AsvColorKind>(
            "Color",
            defaultValue: AsvColorKind.None,
            inherits: false,
            defaultBindingMode: BindingMode.OneWay
        );

    public static void SetColor(AvaloniaObject target, AsvColorKind value) =>
        target.SetValue(ColorProperty, value);

    public static AsvColorKind GetColor(AvaloniaObject target) => target.GetValue(ColorProperty);

    static AsvPallete()
    {
        ColorProperty.Changed.AddClassHandler<StyledElement>(OnColorChanged);
    }

    private static void OnColorChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
    {
        var oldValue = (AsvColorKind)(e.OldValue ?? AsvColorKind.None);
        var newValue = (AsvColorKind)(e.NewValue ?? AsvColorKind.None);

        // Remove old classes
        foreach (var flag in GetSetFlags(oldValue))
        {
            element.Classes.Remove(flag.ToString().ToLowerInvariant());
        }

        // Add new classes
        foreach (var flag in GetSetFlags(newValue))
        {
            element.Classes.Add(flag.ToString().ToLowerInvariant());
        }
    }

    /// <summary>
    /// Returns all individual enum flags that are currently set (excluding None).
    /// </summary>
    private static IEnumerable<AsvColorKind> GetSetFlags(AsvColorKind value)
    {
        if (value == AsvColorKind.None)
        {
            yield break;
        }

        foreach (AsvColorKind flag in Enum.GetValues(typeof(AsvColorKind)))
        {
            if (flag != AsvColorKind.None && value.HasFlag(flag))
            {
                yield return flag;
            }
        }
    }
}
