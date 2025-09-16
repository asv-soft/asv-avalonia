using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

[ExportUnitItem(AngleBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MsAngleUnit() : UnitItemBase(1)
{
    public const string Id = $"{AngleBase.Id}.ms";

    public override string UnitItemId => Id;
    public override string Name => RS.Ms_UnitItem_Name;
    public override string Description => RS.Ms_Angle_Description;
    public override string Symbol => RS.Ms_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override double Parse(string? value)
    {
        return value != null && AngleMs.TryParse(value, out var result) ? result : double.NaN;
    }

    public override bool IsValid(string? value)
    {
        return value != null && AngleMs.IsValid(value);
    }

    public override ValidationResult ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        var msg = AngleMs.GetErrorMessage(value);

        if (msg is not null)
        {
            return new ValidationResult
            {
                IsSuccess = false,
                ValidationException = new UnitException(msg),
            };
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Prints unit item with units.
    /// </summary>
    /// <param name="value">.</param>
    /// <param name="format">Unused for this unit item.</param>
    /// <returns>Unit item in string format with its units.</returns>
    public override string Print(double value, string? format = null)
    {
        return AngleMs.PrintMs(value);
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
