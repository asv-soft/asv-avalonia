﻿using Avalonia;
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
public partial class BatteryIndicator : IndicatorBase
{
    public BatteryIndicator() { }

    private static void SetPseudoClass(BatteryIndicator indicator) { }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (
            change.Property == ValueProperty
            || change.Property == MaxValueProperty
            || change.Property == CriticalValueProperty
            || change.Property == WarningValueProperty
        )
        {
            var value = Value;
            PseudoClasses.Set(
                PseudoClassesHelper.Unknown,
                value == null || !double.IsFinite(value.Value) || value > MaxValue
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
            if (MaxValue == 0 || !double.IsFinite(MaxValue)) { }

            IconKind = GetIcon(Value / MaxValue);
        }
    }

    private static MaterialIconKind GetIcon(double? normalizedValue)
    {
        return (normalizedValue ?? double.NaN) switch
        {
            (< 0 or > 1 or double.NegativeInfinity or double.PositiveInfinity or double.NaN) =>
                MaterialIconKind.BatteryUnknown,
            0 => MaterialIconKind.Battery0,
            (> 0 and <= 0.10) => MaterialIconKind.Battery10,
            (> 0.10 and <= 0.20) => MaterialIconKind.Battery20,
            (> 0.20 and <= 0.30) => MaterialIconKind.Battery30,
            (> 0.30 and <= 0.40) => MaterialIconKind.Battery40,
            (> 0.40 and <= 0.50) => MaterialIconKind.Battery50,
            (> 0.50 and <= 0.60) => MaterialIconKind.Battery60,
            (> 0.60 and <= 0.70) => MaterialIconKind.Battery70,
            (> 0.70 and <= 0.80) => MaterialIconKind.Battery80,
            (> 0.80 and <= 0.90) => MaterialIconKind.Battery90,
            (> 0.90 and <= 1) => MaterialIconKind.Battery100,
        };
    }
}
