namespace Asv.Avalonia;

public class ExecuteCommandEvent(IRoutable source, string commandId, ICommandArg commandArg)
    : AsyncRoutedEvent(source)
{
    public string CommandId { get; } = commandId;
    public ICommandArg CommandArg { get; } = commandArg;
}

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        ICommandArg commandArg
    )
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandArg));
    }

    public static ValueTask ExecuteCommand(this IRoutable src, string commandId)
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, CommandArg.Empty));
    }
}
