namespace Asv.Avalonia;

public class ExecuteCommandEvent(
    IRoutable source,
    string commandId,
    ICommandParameter commandParameter
) : AsyncRoutedEvent(source)
{
    public string CommandId { get; } = commandId;
    public ICommandParameter CommandParameter { get; } = commandParameter;
}

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        ICommandParameter commandParameter
    )
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandParameter));
    }

    public static ValueTask ExecuteCommand(this IRoutable src, string commandId)
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, CommandParameter.Empty));
    }
}
