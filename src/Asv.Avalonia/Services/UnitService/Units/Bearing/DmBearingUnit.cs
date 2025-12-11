using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(BearingBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class DmBearingUnit() : UnitItemBase(1)
{
    public const string Id = $"{BearingBase.Id}.dm";

    public override string UnitItemId => Id;
    public override string Name => RS.DM_UnitItem_Name;
    public override string Description => RS.DM_Bearing_Description;
    public override string Symbol => RS.DM_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override bool IsValid(string? value)
    {
        return value != null && AngleDm.IsValid(value);
    }

    public override double Parse(string? value)
    {
        return value != null && AngleDm.TryParse(value, out var result) ? result : double.NaN;
    }

    public override ValidationResult ValidateValue(string? value)
    {
        var result = AngleDm.ValidateValue(value);
        if (result.IsSuccess)
        {
            return result;
        }

        return new UnitException(
            result.ValidationException?.Message,
            result.ValidationException?.LocalizedMessage
        );
    }

    /// <summary>
    /// Prints unit item.
    /// </summary>
    /// <param name="value">.</param>
    /// <param name="format">Unused for this unit item.</param>
    /// <returns>Unit item in string format.</returns>
    public override string Print(double value, string? format = null)
    {
        return AngleDm.PrintDm(value);
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
