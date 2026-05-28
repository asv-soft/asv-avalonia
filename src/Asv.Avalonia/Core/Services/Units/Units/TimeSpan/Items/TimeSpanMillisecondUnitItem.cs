using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia;

public sealed class TimeSpanMillisecondUnitItem() : UnitItemBase(1000)
{
    public const string Id = $"{TimeSpanUnit.Id}.millisecond";

    public override string UnitItemId => Id;
    public override string Name => RS.Millisecond_UnitItem_Name;
    public override string Description => RS.Millisecond_TimeSpan_Description;
    public override string Symbol => RS.Millisecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;

    public override bool IsValid(string? value)
    {
        return ValidateMilliseconds(value, out _).IsSuccess;
    }

    public override ValidationResult ValidateValue(string? value)
    {
        var result = ValidateMilliseconds(value, out _);
        if (result.IsSuccess)
        {
            return result;
        }

        return new UnitException(
            result.ValidationException?.Message,
            result.ValidationException,
            result.ValidationException?.LocalizedMessage
        );
    }

    public override double Parse(string? value)
    {
        return ValidateMilliseconds(value, out var milliseconds).IsSuccess
            ? milliseconds
            : double.NaN;
    }

    public override string Print(double value, string? format = null)
    {
        var rounded = Math.Round(value, MidpointRounding.AwayFromZero);
        return rounded.ToString(format ?? "0", CultureInfo.InvariantCulture);
    }

    private static ValidationResult ValidateMilliseconds(string? value, out double milliseconds)
    {
        var result = InvariantNumberParser.TryParse(value, out milliseconds);
        if (result.IsFailed)
        {
            return result;
        }

        return
            double.IsFinite(milliseconds)
            && milliseconds.ApproximatelyEquals(Math.Round(milliseconds))
            ? ValidationResult.Success
            : new ValidationException(
                "Milliseconds value must be an integer",
                RS.Millisecond_UnitItem_NotIntegerException
            );
    }
}
