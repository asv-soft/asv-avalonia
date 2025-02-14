namespace Asv.Avalonia;

#pragma warning disable RCS1194
public class CommandNotFoundException(string commandId)
#pragma warning restore RCS1194
    : CommandException($"Command with id '{commandId}' not found.")
{
    public string CommandId { get; } = commandId;
}
