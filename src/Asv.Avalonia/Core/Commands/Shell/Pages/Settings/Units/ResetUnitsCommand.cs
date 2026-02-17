using Material.Icons;

namespace Asv.Avalonia;

public class ResetUnitsCommand : ContextCommand<SettingsUnitsViewModel, DictArg>
{
    public const string Id = $"{BaseId}.settings.unit.reset";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ResetUnitsCommand_CommandInfo_Name,
        Description = RS.ResetUnitsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Refresh,
        DefaultHotKey = null,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<DictArg?> InternalExecute(
        SettingsUnitsViewModel context,
        DictArg arg,
        CancellationToken cancel
    )
    {
        var previousChangedUnits = new DictArg();

        foreach (var unit in context.Items)
        {
            var currentUnitId = unit.Base.UnitId;
            var currentUnitItemId = unit.Base.CurrentUnitItem.Value.UnitItemId;

            if (currentUnitItemId != unit.Base.InternationalSystemUnit.UnitItemId)
            {
                previousChangedUnits[currentUnitId] = CommandArg.CreateString(currentUnitItemId);
            }

            if (arg.TryGetValue(currentUnitId, out var unitItemIdToSet))
            {
                if (
                    unit.Base.AvailableUnits.TryGetValue(
                        unitItemIdToSet.AsString(),
                        out var unitItemToSet
                    )
                )
                {
                    unit.Base.CurrentUnitItem.Value = unitItemToSet;
                }
            }
        }

        return previousChangedUnits.Count > 0
            ? ValueTask.FromResult<DictArg?>(previousChangedUnits)
            : ValueTask.FromResult<DictArg?>(null);
    }
}
