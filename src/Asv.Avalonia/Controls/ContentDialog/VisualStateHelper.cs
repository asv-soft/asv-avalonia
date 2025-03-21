using Avalonia;
using Avalonia.Controls;
using Avalonia.Utilities;

namespace Asv.Avalonia;

/// <summary>
/// Visual State Helper
/// </summary>
public class VisualStateHelper
{
    static VisualStateHelper()
    {
        ForcedClassesProperty.Changed.Subscribe(OnForcedClassesPropertyChanged);
    }

    /// <summary>
    /// Forced Classes
    /// </summary>
    public static readonly AttachedProperty<string> ForcedClassesProperty =
        AvaloniaProperty.RegisterAttached<VisualStateHelper, StyledElement, string>(
            "ForcedClasses"
        );

    /// <summary>
    /// Get value of <see cref="ForcedClassesProperty"/> property.
    /// </summary>
    /// <param name="element">.</param>
    /// <returns></returns>
    public static string GetForcedClassesProperty(StyledElement element) =>
        element.GetValue(ForcedClassesProperty);

    /// <summary>
    /// Set value of <see cref="ForcedClassesProperty"/> property.
    /// </summary>
    /// <param name="element">.</param>
    /// <param name="classes">.</param>
    public static void SetForcedClassesProperty(StyledElement element, string classes) =>
        element.SetValue(ForcedClassesProperty, classes);

    private static void OnForcedClassesPropertyChanged(
        AvaloniaPropertyChangedEventArgs<string> args
    )
    {
        if (args.Sender is StyledElement element)
        {
            SetClasses(element, args.OldValue.GetValueOrDefault<string>(), false);
            SetClasses(element, args.NewValue.GetValueOrDefault<string>(), true);
        }
    }

    private static void SetClasses(StyledElement element, string? classes, bool set)
    {
        if (string.IsNullOrEmpty(classes))
        {
            return;
        }

        CharacterReader cr = new CharacterReader(classes.AsSpan());

        while (!cr.End)
        {
            var @class = cr.TakeUntil(',');

            if (@class[^1] == '!')
            {
                @class = @class.Slice(0, @class.Length - 1);
                ((IPseudoClasses)element.Classes).Set(@class.ToString(), false);
            }
            else
            {
                ((IPseudoClasses)element.Classes).Set(@class.ToString(), set);
            }

            if (!cr.End)
            {
                cr.Skip(1);
            }
        }
    }
}
