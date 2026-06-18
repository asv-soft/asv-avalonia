using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public class TelemetryProgressBar : ProgressBar
{
    private const string TelemetryProgressBarClass = "telemetry-progress-bar";

    public TelemetryProgressBar()
    {
        Classes.Add(TelemetryProgressBarClass);
    }

    protected override Type StyleKeyOverride => typeof(ProgressBar);

    public static readonly StyledProperty<AsvColorKind?> StatusProperty = AvaloniaProperty.Register<
        TelemetryProgressBar,
        AsvColorKind?
    >(nameof(Status));

    static TelemetryProgressBar()
    {
        StatusProperty.Changed.AddClassHandler<TelemetryProgressBar>(
            (bar, args) =>
                bar.UpdateStatusClasses(
                    args.OldValue as AsvColorKind?,
                    args.NewValue as AsvColorKind?
                )
        );
    }

    public AsvColorKind? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private void UpdateStatusClasses(AsvColorKind? oldValue, AsvColorKind? newValue)
    {
        foreach (var flag in GetSetFlags(oldValue))
        {
            Classes.Remove(flag.ToString().ToLowerInvariant());
        }

        foreach (var flag in GetSetFlags(newValue))
        {
            Classes.Add(flag.ToString().ToLowerInvariant());
        }
    }

    private static IEnumerable<AsvColorKind> GetSetFlags(AsvColorKind? value)
    {
        if (value is null or AsvColorKind.None)
        {
            yield break;
        }

        foreach (var flag in Enum.GetValues<AsvColorKind>())
        {
            if (flag != AsvColorKind.None && value.Value.HasFlag(flag))
            {
                yield return flag;
            }
        }
    }
}
