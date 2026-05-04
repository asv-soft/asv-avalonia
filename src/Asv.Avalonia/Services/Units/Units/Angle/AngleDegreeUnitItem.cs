using Asv.Common;

namespace Asv.Avalonia;

public sealed class AngleDegreeUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{AngleUnit.Id}.degree";

    public override string UnitItemId => Id;
    public override string Name => RS.Degree_UnitItem_Name;
    public override string Description => RS.Degree_Angle_Description;
    public override string Symbol => RS.Degree_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;

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
        var result = Angle.ValidateValue(value);
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
