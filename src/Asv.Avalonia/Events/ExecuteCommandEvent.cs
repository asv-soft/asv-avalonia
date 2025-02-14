namespace Asv.Avalonia;

public class ExecuteCommandEvent(IRoutable source, string commandId, IPersistable commandParameter)
    : AsyncRoutedEvent(source)
{
    public string CommandId { get; } = commandId;
    public IPersistable CommandParameter { get; } = commandParameter;
}

public static class ExecuteCommandEventMixin
{
    public static ValueTask ExecuteCommand(
        this IRoutable src,
        string commandId,
        IPersistable commandParameter
    )
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, commandParameter));
    }

    public static ValueTask ExecuteCommand(this IRoutable src, string commandId)
    {
        return src.Rise(new ExecuteCommandEvent(src, commandId, Persistable.Empty));
    }
}
