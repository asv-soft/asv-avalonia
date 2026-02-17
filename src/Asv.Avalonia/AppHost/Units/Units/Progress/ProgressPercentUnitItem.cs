using Asv.Common;

namespace Asv.Avalonia;

public class ProgressPercentUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{ProgressUnit.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_Progress_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;

    public override ValidationResult ValidateValue(string? value)
    {
        var validationResult = InvariantNumberParser.TryParse(value, out double progress);

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        return progress is < 0 or > 100
            ? ValidationResult.FailAsOutOfRange("0", "100")
            : ValidationResult.Success;
    }
}
