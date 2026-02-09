using System.Composition;
using Asv.Common;

namespace Asv.Avalonia;

public class ProgressInPartsUnitItem() : UnitItemBase(0.01)
{
    public const string Id = $"{ProgressUnit.Id}.in.parts";

    public override string UnitItemId => Id;
    public override string Name => RS.InParts_UnitItem_Name;
    public override string Description => RS.InParts_Progress_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => false;

    public override ValidationResult ValidateValue(string? value)
    {
        var validationResult = InvariantNumberParser.TryParse(value, out double progress);

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        return progress is < 0 or > 1.0
            ? ValidationResult.FailAsOutOfRange("0", "1")
            : ValidationResult.Success;
    }
}
