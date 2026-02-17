using Material.Icons;

namespace Asv.Avalonia;

public sealed class ChangeMeasureUnitCommand(IUnitService svc) : StatelessCrudCommand<StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.settings.unit.change";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeCurrentUnitItemCommand_CommandInfo_Name,
        Description = RS.ChangeCurrentUnitItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Settings,
        DefaultHotKey = null,
    };

    public static ValueTask ExecuteCommand(IRoutable context, IUnit command, IUnitItem userValue)
    {
        return context.ExecuteCommand(
            Id,
            CommandArg.ChangeAction(command.UnitId, new StringArg(userValue.UnitItemId))
        );
    }

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<string> Update(string subjectId, StringArg options)
    {
        if (!svc.Units.TryGetValue(subjectId, out var unit))
        {
            throw new Exception($"Measure unit with {subjectId} not found");
        }

        if (!unit.AvailableUnits.TryGetValue(options.Value, out var unitItem))
        {
            throw new Exception($"Unit item with {options.Value} not found in unit {subjectId}");
        }

        unit.CurrentUnitItem.Value = unitItem;
        return ValueTask.FromResult(subjectId);
    }

    protected override ValueTask Delete(string? subjectId)
    {
        throw new NotImplementedException(
            $"Delete operation is not supported for {nameof(ChangeMeasureUnitCommand)} command."
        );
    }

    protected override ValueTask<string> Create(StringArg options)
    {
        throw new NotImplementedException(
            $"Create operation is not supported for {nameof(ChangeMeasureUnitCommand)} command."
        );
    }

    protected override ValueTask<StringArg> Read(string subjectId)
    {
        if (!svc.Units.TryGetValue(subjectId, out var unit))
        {
            throw new Exception($"Measure unit with {subjectId} not found");
        }

        return ValueTask.FromResult(new StringArg(unit.CurrentUnitItem.Value.UnitItemId));
    }
}
