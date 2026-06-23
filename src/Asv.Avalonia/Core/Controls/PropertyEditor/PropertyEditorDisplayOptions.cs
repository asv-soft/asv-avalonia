using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;

namespace Asv.Avalonia;

/// <summary>
/// Inherited visual options used by property editor item views.
/// </summary>
public class PropertyEditorDisplayOptions
{
    public static readonly AttachedProperty<bool> ShowPrefixIconProperty =
        AvaloniaProperty.RegisterAttached<PropertyEditorDisplayOptions, StyledElement, bool>(
            "ShowPrefixIcon",
            defaultValue: true,
            inherits: true,
            defaultBindingMode: BindingMode.OneWay
        );

    public static readonly AttachedProperty<bool> ShowShortHeaderProperty =
        AvaloniaProperty.RegisterAttached<PropertyEditorDisplayOptions, StyledElement, bool>(
            "ShowShortHeader",
            defaultValue: true,
            inherits: true,
            defaultBindingMode: BindingMode.OneWay
        );

    public static void SetShowPrefixIcon(AvaloniaObject target, bool value) =>
        target.SetValue(ShowPrefixIconProperty, value);

    public static bool GetShowPrefixIcon(AvaloniaObject target) =>
        target.GetValue(ShowPrefixIconProperty);

    public static void SetShowShortHeader(AvaloniaObject target, bool value) =>
        target.SetValue(ShowShortHeaderProperty, value);

    public static bool GetShowShortHeader(AvaloniaObject target) =>
        target.GetValue(ShowShortHeaderProperty);
}
