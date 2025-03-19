using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Material.Icons;

namespace Asv.Avalonia;

[PseudoClasses(
    PseudoClassesHelper.Critical,
    PseudoClassesHelper.Warning,
    PseudoClassesHelper.Normal,
    PseudoClassesHelper.Unknown
)]
public partial class ConnectionQuality : IndicatorBase
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            var value = Value;
            PseudoClasses.Set(
                PseudoClassesHelper.Unknown,
                value is null || !double.IsFinite(value.Value) || value > MaxValue
            );
            PseudoClasses.Set(PseudoClassesHelper.Critical, value <= CriticalValue);
            PseudoClasses.Set(
                PseudoClassesHelper.Warning,
                value > CriticalValue && value <= WarningValue
            );
            PseudoClasses.Set(
                PseudoClassesHelper.Normal,
                value > WarningValue && value <= MaxValue
            );
            IconKind = GetIcon(Value / MaxValue);
        }
    }

    private static MaterialIconKind GetIcon(double? normalizedValue)
    {
        return (normalizedValue ?? double.NaN) switch
        {
            (< 0 or > 1 or double.NegativeInfinity or double.PositiveInfinity or double.NaN) =>
                MaterialIconKind.WifiStrengthAlertOutline,
            0 => MaterialIconKind.WifiStrength0,
            (> 0 and <= 0.2) => MaterialIconKind.WifiStrength0,
            (> 0.2 and <= 0.4) => MaterialIconKind.WifiStrength1,
            (> 0.4 and <= 0.6) => MaterialIconKind.WifiStrength2,
            (> 0.6 and <= 0.8) => MaterialIconKind.WifiStrength3,
            (> 0.8 and <= 1.0) => MaterialIconKind.WifiStrength4,
        };
    }
}