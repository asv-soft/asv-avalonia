using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(AngleBase.Id)]
[Shared]
[method: ImportingConstructor]
public class DmsAngleUnit() : UnitItemBase(1)
{
    public const string Id = $"{AngleBase.Id}.dms";

    public override string UnitItemId => Id;
    public override string Name => RS.Dms_UnitItem_Name;
    public override string Description => RS.Dms_Angle_Description;
    public override string Symbol => RS.Dms_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override double Parse(string? value)
    {
        return value != null && Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public override bool IsValid(string? value)
    {
        return value != null && Angle.IsValid(value);
    }

    public override ValidationResult ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        return Angle.ValidateValue(value);
    }

    /// <summary>
    /// Prints unit item with units.
    /// </summary>
    /// <param name="value">.</param>
    /// <param name="format">Unused for this unit item.</param>
    /// <returns>Unit item in string format with its units.</returns>
    public override string Print(double value, string? format = null)
    {
        return Angle.PrintDms(value);
    }

    /// <summary>
    /// Prints unit item with units.
    /// </summary>
    /// <param name="value">.</param>
    /// <param name="format">Unused for this unit item.</param>
    /// <returns>Unit item in string format with its units.</returns>
    public override string PrintWithUnits(double value, string? format = null)
    {
        return Print(value);
    }

    /// <summary>
    /// Method is unusable for this unit item.
    /// </summary>
    /// <param name="siValue">.</param>
    /// <returns>..</returns>
    public override double FromSi(double siValue)
    {
        return siValue;
    }

    /// <summary>
    /// Method is unusable for this unit item.
    /// </summary>
    /// <param name="value">.</param>
    /// <returns>..</returns>
    public override double ToSi(double value)
    {
        return value;
    }
}
