namespace Asv.Avalonia;

public enum CommandParameterActionType
{
    Add,
    Remove,
    Change,
}

public class CommandParameterAction(string? id, string? value, CommandParameterActionType action)
    : ICommandParameter
{
    public string? Id { get; } = id;
    public string? Value { get; } = value;
    public CommandParameterActionType Action { get; } = action;
}
