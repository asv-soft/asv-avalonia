using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

public abstract class IntensityUnitItemBase(double minValue, double maxValue, double multiplier)
    : UnitItemBase(multiplier)
{
    public override ValidationResult ValidateValue(string? value)
    {
        var validationResult = InvariantNumberParser.TryParse(value, out double intensity);

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        if (intensity < minValue || intensity > maxValue)
        {
            return ValidationResult.FailAsOutOfRange(
                minValue.ToString(CultureInfo.InvariantCulture),
                maxValue.ToString(CultureInfo.InvariantCulture)
            );
        }

        return ValidationResult.Success;
    }
}
