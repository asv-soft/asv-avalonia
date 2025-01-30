namespace Asv.Avalonia;

public interface ICommandInfo
{
    string CommandId { get; }
    string Name { get; }
    string Description { get; }
    object? Icon { get; }
    int Order { get; }
}

public class CommandInfo : ICommandInfo
{
    public required string CommandId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required object? Icon { get; set; }
    public required int Order { get; set; }
}
