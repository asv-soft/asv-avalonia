namespace Asv.Avalonia;

public enum CommandParameterActionType
{
    Add,
    Remove,
    Change,
}

public class ActionCommandArg(string? id, string? value, CommandParameterActionType action)
    : ICommandArg
{
    public string? Id { get; } = id;
    public string? Value { get; } = value;
    public CommandParameterActionType Action { get; } = action;

    public override string ToString()
    {
        return $"[{Action:G}] {Id} {Value}]";
    }
}
